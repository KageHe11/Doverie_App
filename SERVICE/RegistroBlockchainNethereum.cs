using System;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BE;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace SERVICE
{
    /// Implementación de IRegistroBlockchain sobre Ethereum, con Nethereum y
    /// el smart contract DoverieRegistro (carpeta Blockchain del repositorio).
    /// Registrar envía una transacción firmada con la wallet de Doverie y
    /// espera a que entre en un bloque; Consultar lee esa transacción de la
    /// red, decodifica el evento DocumentoRegistrado y toma la fecha del
    /// bloque. Los documentos sellados antes con el registro local siguen
    /// verificándose contra ese registro.
    ///
    /// Configuración (AppSettings.config): BlockchainRpcUrl,
    /// BlockchainClavePrivada, BlockchainContrato, BlockchainChainId y,
    /// opcionales, BlockchainRed y BlockchainExplorador.
    public class RegistroBlockchainNethereum : IRegistroBlockchain
    {
        /// Las transacciones de una misma wallet se numeran (nonce): se
        /// envían de a una para que dos subidas simultáneas no tomen el mismo.
        private static readonly object BloqueoEnvio = new object();
        private static readonly TimeSpan EsperaMaxima = TimeSpan.FromMinutes(3);

        [Function("registrar")]
        private class RegistrarFuncion : FunctionMessage
        {
            [Parameter("bytes32", "hash", 1)]
            public byte[] Hash { get; set; }
        }

        [Event("DocumentoRegistrado")]
        private class DocumentoRegistradoEvento : IEventDTO
        {
            [Parameter("bytes32", "hash", 1, true)]
            public byte[] Hash { get; set; }
            [Parameter("uint256", "fecha", 2, false)]
            public BigInteger Fecha { get; set; }
        }

        public SelloBlockchainBE Registrar(string hash)
        {
            // Nethereum es asíncrono; se ejecuta fuera del contexto de ASP.NET
            // para poder esperar el resultado sin bloquear el request.
            return Task.Run(() => RegistrarAsync(hash)).GetAwaiter().GetResult();
        }
        public SelloBlockchainBE Consultar(string transaccion)
        {
            SelloBlockchainBE Sello = Task.Run(() => ConsultarAsync(transaccion)).GetAwaiter().GetResult();
            return Sello ?? new RegistroBlockchainLocal().Consultar(transaccion);
        }

        private async Task<SelloBlockchainBE> RegistrarAsync(string hash)
        {
            Web3 Web3 = CrearWeb3(true);
            var Manejador = Web3.Eth.GetContractTransactionHandler<RegistrarFuncion>();
            RegistrarFuncion Funcion = new RegistrarFuncion { Hash = hash.HexToByteArray() };
            string Transaccion;
            lock (BloqueoEnvio)
            {
                Transaccion = Manejador.SendRequestAsync(Contrato(), Funcion).GetAwaiter().GetResult();
            }
            using (CancellationTokenSource Cancelacion = new CancellationTokenSource(EsperaMaxima))
            {
                TransactionReceipt Recibo = await Web3.TransactionReceiptPolling.PollForReceiptAsync(Transaccion, Cancelacion.Token).ConfigureAwait(false);
                SelloBlockchainBE Sello = await SelloDesdeRecibo(Web3, Recibo).ConfigureAwait(false);
                if (Sello == null) throw new InvalidOperationException("La transacción " + Transaccion + " no registró el documento.");
                return Sello;
            }
        }
        private async Task<SelloBlockchainBE> ConsultarAsync(string transaccion)
        {
            Web3 Web3 = CrearWeb3(false);
            TransactionReceipt Recibo = await Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaccion).ConfigureAwait(false);
            return Recibo == null ? null : await SelloDesdeRecibo(Web3, Recibo).ConfigureAwait(false);
        }
        /// Arma el sello a partir de una transacción ya minada. Devuelve null
        /// si falló o si no es un registro de nuestro contrato.
        private async Task<SelloBlockchainBE> SelloDesdeRecibo(Web3 web3, TransactionReceipt recibo)
        {
            if (recibo.Status == null || recibo.Status.Value != 1) return null;
            if (!string.Equals(recibo.To, Contrato(), StringComparison.OrdinalIgnoreCase)) return null;
            var Evento = recibo.DecodeAllEvents<DocumentoRegistradoEvento>()
                .FirstOrDefault(E => string.Equals(E.Log.Address, Contrato(), StringComparison.OrdinalIgnoreCase));
            if (Evento == null) return null;
            BlockWithTransactionHashes Bloque = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                .SendRequestAsync(new HexBigInteger(recibo.BlockNumber.Value)).ConfigureAwait(false);
            DateTime Fecha = DateTimeOffset.FromUnixTimeSeconds((long)Bloque.Timestamp.Value).LocalDateTime;
            string Explorador = ConfigurationManager.AppSettings["BlockchainExplorador"];
            return new SelloBlockchainBE
            {
                Transaccion = recibo.TransactionHash,
                Hash = Evento.Event.Hash.ToHex(),
                FechaSellado = Fecha,
                Red = ConfigurationManager.AppSettings["BlockchainRed"] ?? "Ethereum",
                Bloque = (long)recibo.BlockNumber.Value,
                UrlExplorador = string.IsNullOrWhiteSpace(Explorador) ? null : Explorador.TrimEnd('/') + "/tx/" + recibo.TransactionHash
            };
        }
        private static Web3 CrearWeb3(bool conCuenta)
        {
            string Url = Configuracion("BlockchainRpcUrl");
            if (!conCuenta) return new Web3(Url);
            Account Cuenta = new Account(Configuracion("BlockchainClavePrivada"), BigInteger.Parse(Configuracion("BlockchainChainId")));
            return new Web3(Cuenta, Url);
        }
        private static string Contrato()
        {
            return Configuracion("BlockchainContrato");
        }
        private static string Configuracion(string clave)
        {
            string Valor = ConfigurationManager.AppSettings[clave];
            if (string.IsNullOrWhiteSpace(Valor)) throw new ConfigurationErrorsException("Falta configurar " + clave + ".");
            return Valor;
        }
    }
}
