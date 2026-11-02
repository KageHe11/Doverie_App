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

namespace SERVICE
{
    /// Implementación de IRegistroNFT sobre Ethereum, con el contrato ERC-721
    /// DoverieNFT (carpeta Blockchain). Usa la misma red y la misma wallet que
    /// RegistroBlockchainNethereum, más la clave BlockchainContratoNFT. Los
    /// tokens quedan en la wallet de Doverie; el identificador que se guarda
    /// en la base incluye la dirección del contrato, así un NFT se sigue
    /// encontrando aunque más adelante se despliegue otro contrato.
    public class RegistroNFTNethereum : IRegistroNFT
    {
        [Function("generar", "uint256")]
        private class GenerarFuncion : FunctionMessage
        {
            [Parameter("bytes32", "hash", 1)]
            public byte[] Hash { get; set; }
        }

        [Function("tokenDeHash", "uint256")]
        private class TokenDeHashFuncion : FunctionMessage
        {
            [Parameter("bytes32", "", 1)]
            public byte[] Hash { get; set; }
        }

        [Function("totalGenerados", "uint256")]
        private class TotalGeneradosFuncion : FunctionMessage { }

        [Function("consultar", typeof(CertificadoSalida))]
        private class ConsultarFuncion : FunctionMessage
        {
            [Parameter("uint256", "tokenId", 1)]
            public BigInteger TokenId { get; set; }
        }

        [FunctionOutput]
        private class CertificadoSalida : IFunctionOutputDTO
        {
            [Parameter("bytes32", "hash", 1)]
            public byte[] Hash { get; set; }
            [Parameter("uint256", "fecha", 2)]
            public BigInteger Fecha { get; set; }
            [Parameter("address", "propietario", 3)]
            public string Propietario { get; set; }
        }

        [Event("NFTGenerado")]
        private class NFTGeneradoEvento : IEventDTO
        {
            [Parameter("uint256", "tokenId", 1, true)]
            public BigInteger TokenId { get; set; }
            [Parameter("bytes32", "hash", 2, true)]
            public byte[] Hash { get; set; }
            [Parameter("uint256", "fecha", 3, false)]
            public BigInteger Fecha { get; set; }
        }

        public NFTBE Generar(string hash)
        {
            // Nethereum es asíncrono; se ejecuta fuera del contexto de ASP.NET
            // para poder esperar el resultado sin bloquear el request.
            return Task.Run(() => GenerarAsync(hash)).GetAwaiter().GetResult();
        }
        public NFTBE Consultar(string identificador)
        {
            // Los NFT emitidos con el registro local se siguen leyendo de ahí.
            if (identificador.StartsWith(RegistroNFTLocal.Prefijo, StringComparison.Ordinal))
            {
                return new RegistroNFTLocal().Consultar(identificador);
            }
            return Task.Run(() => ConsultarAsync(identificador)).GetAwaiter().GetResult();
        }

        private async Task<NFTBE> GenerarAsync(string hash)
        {
            Web3 Web3 = RegistroBlockchainNethereum.CrearWeb3(true);
            string Contrato = RegistroBlockchainNethereum.Configuracion("BlockchainContratoNFT");
            byte[] Hash = hash.HexToByteArray();
            // El contrato revertiría igual; consultarlo antes evita pagar una
            // transacción fallida y permite informar ERROR-019.
            BigInteger Existente = await Web3.Eth.GetContractQueryHandler<TokenDeHashFuncion>()
                .QueryAsync<BigInteger>(Contrato, new TokenDeHashFuncion { Hash = Hash }).ConfigureAwait(false);
            if (Existente != 0) throw new NFTExistenteException("El contenido ya tiene el NFT " + Existente + ".");

            var Manejador = Web3.Eth.GetContractTransactionHandler<GenerarFuncion>();
            string Transaccion;
            lock (RegistroBlockchainNethereum.BloqueoEnvio)
            {
                Transaccion = Manejador.SendRequestAsync(Contrato, new GenerarFuncion { Hash = Hash }).GetAwaiter().GetResult();
            }
            TransactionReceipt Recibo;
            using (CancellationTokenSource Cancelacion = new CancellationTokenSource(RegistroBlockchainNethereum.EsperaMaxima))
            {
                Recibo = await Web3.TransactionReceiptPolling.PollForReceiptAsync(Transaccion, Cancelacion.Token).ConfigureAwait(false);
            }
            if (Recibo.Status == null || Recibo.Status.Value != 1) throw new InvalidOperationException("La transacción " + Transaccion + " falló.");
            var Evento = Recibo.DecodeAllEvents<NFTGeneradoEvento>()
                .FirstOrDefault(E => string.Equals(E.Log.Address, Contrato, StringComparison.OrdinalIgnoreCase));
            if (Evento == null) throw new InvalidOperationException("La transacción " + Transaccion + " no emitió el NFT.");
            NFTBE Nft = ArmarNFT(Contrato, (long)Evento.Event.TokenId, Evento.Event.Hash, Evento.Event.Fecha, Web3.TransactionManager.Account.Address);
            Nft.Transaccion = Transaccion;
            return Nft;
        }
        private async Task<NFTBE> ConsultarAsync(string identificador)
        {
            string[] Partes = identificador.Split(':');
            long TokenId;
            if (Partes.Length != 2 || !long.TryParse(Partes[1], out TokenId)) return null;
            string Contrato = Partes[0];
            Web3 Web3 = RegistroBlockchainNethereum.CrearWeb3(false);
            // consultar() revierte si el token no existe: se compara antes
            // contra el total emitido para distinguirlo de un error de red.
            BigInteger Total = await Web3.Eth.GetContractQueryHandler<TotalGeneradosFuncion>()
                .QueryAsync<BigInteger>(Contrato, new TotalGeneradosFuncion()).ConfigureAwait(false);
            if (TokenId < 1 || TokenId > Total) return null;
            CertificadoSalida Certificado = await Web3.Eth.GetContractQueryHandler<ConsultarFuncion>()
                .QueryDeserializingToObjectAsync<CertificadoSalida>(new ConsultarFuncion { TokenId = TokenId }, Contrato).ConfigureAwait(false);
            return ArmarNFT(Contrato, TokenId, Certificado.Hash, Certificado.Fecha, Certificado.Propietario);
        }
        private static NFTBE ArmarNFT(string contrato, long tokenId, byte[] hash, BigInteger fecha, string propietario)
        {
            string Explorador = ConfigurationManager.AppSettings["BlockchainExplorador"];
            return new NFTBE
            {
                Identificador = contrato + ":" + tokenId,
                Contrato = contrato,
                TokenId = tokenId,
                Hash = hash.ToHex(),
                FechaEmision = DateTimeOffset.FromUnixTimeSeconds((long)fecha).LocalDateTime,
                Propietario = propietario,
                Red = ConfigurationManager.AppSettings["BlockchainRed"] ?? "Ethereum",
                UrlExplorador = string.IsNullOrWhiteSpace(Explorador) ? null : Explorador.TrimEnd('/') + "/nft/" + contrato + "/" + tokenId
            };
        }
    }
}
