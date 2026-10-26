using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web.Hosting;
using BE;

namespace SERVICE
{
    /// Implementación local de IRegistroBlockchain, para desarrollo y
    /// pruebas sin red. Simula lo esencial de una blockchain: un registro
    /// de solo agregado donde cada entrada incluye el identificador de la
    /// anterior, y su propio identificador (la "transacción") es el
    /// SHA-256 de todo eso. Si alguien edita o borra una entrada, la
    /// cadena deja de cerrar y la consulta falla en lugar de mentir.
    public class RegistroBlockchainLocal : IRegistroBlockchain
    {
        private static readonly object Bloqueo = new object();
        private const string Genesis = "0000000000000000000000000000000000000000000000000000000000000000";

        private class Entrada
        {
            public string Transaccion { get; set; }
            public string Anterior { get; set; }
            public string Hash { get; set; }
            public string Fecha { get; set; }
        }

        public SelloBlockchainBE Registrar(string hash)
        {
            lock (Bloqueo)
            {
                List<Entrada> Cadena = LeerCadena();
                string Anterior = Cadena.Count == 0 ? Genesis : Cadena[Cadena.Count - 1].Transaccion.Substring(2);
                DateTime Fecha = DateTime.UtcNow;
                Entrada Nueva = new Entrada
                {
                    Anterior = Anterior,
                    Hash = hash,
                    Fecha = Fecha.ToString("o", CultureInfo.InvariantCulture)
                };
                Nueva.Transaccion = CalcularTransaccion(Nueva);
                File.AppendAllText(RutaRegistro(), JsonSerializer.Serialize(Nueva) + Environment.NewLine, Encoding.UTF8);
                return new SelloBlockchainBE { Transaccion = Nueva.Transaccion, Hash = hash, FechaSellado = Fecha.ToLocalTime() };
            }
        }
        public SelloBlockchainBE Consultar(string transaccion)
        {
            lock (Bloqueo)
            {
                foreach (Entrada E in LeerCadena())
                {
                    if (E.Transaccion == transaccion)
                    {
                        DateTime Fecha = DateTime.Parse(E.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                        return new SelloBlockchainBE { Transaccion = E.Transaccion, Hash = E.Hash, FechaSellado = Fecha.ToLocalTime() };
                    }
                }
                return null;
            }
        }
        /// Lee todo el registro y verifica que la cadena esté intacta.
        private List<Entrada> LeerCadena()
        {
            List<Entrada> Cadena = new List<Entrada>();
            string Ruta = RutaRegistro();
            if (!File.Exists(Ruta)) return Cadena;
            string Anterior = Genesis;
            foreach (string Linea in File.ReadAllLines(Ruta, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(Linea)) continue;
                Entrada E = JsonSerializer.Deserialize<Entrada>(Linea);
                if (E.Anterior != Anterior || E.Transaccion != CalcularTransaccion(E))
                {
                    throw new InvalidOperationException("El registro de blockchain local fue alterado.");
                }
                Cadena.Add(E);
                Anterior = E.Transaccion.Substring(2);
            }
            return Cadena;
        }
        private static string CalcularTransaccion(Entrada e)
        {
            using (SHA256 Sha = SHA256.Create())
            {
                byte[] Bytes = Sha.ComputeHash(Encoding.UTF8.GetBytes(e.Anterior + "|" + e.Hash + "|" + e.Fecha));
                return "0x" + HashService.AHexadecimal(Bytes);
            }
        }
        private static string RutaRegistro()
        {
            string Ruta = ConfigurationManager.AppSettings["BlockchainLocalRuta"];
            if (string.IsNullOrWhiteSpace(Ruta))
            {
                Ruta = HostingEnvironment.MapPath("~/App_Data/Blockchain/registro.jsonl");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta));
            return Ruta;
        }
    }
}
