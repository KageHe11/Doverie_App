using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Web.Hosting;
using BE;

namespace SERVICE
{
    /// Implementación local de IRegistroNFT, para desarrollo sin red: guarda
    /// los tokens emitidos en App_Data/Blockchain/nft.jsonl, con las mismas
    /// reglas que el contrato (numeración en orden y un token por hash).
    public class RegistroNFTLocal : IRegistroNFT
    {
        internal const string Prefijo = "local:";
        private static readonly object Bloqueo = new object();

        private class Token
        {
            public long TokenId { get; set; }
            public string Hash { get; set; }
            public string Fecha { get; set; }
        }

        public NFTBE Generar(string hash)
        {
            lock (Bloqueo)
            {
                List<Token> Tokens = LeerTokens();
                Token Existente = Tokens.Find(T => string.Equals(T.Hash, hash, StringComparison.OrdinalIgnoreCase));
                if (Existente != null) throw new NFTExistenteException("El contenido ya tiene el NFT " + Existente.TokenId + ".");
                Token Nuevo = new Token
                {
                    TokenId = Tokens.Count + 1,
                    Hash = hash,
                    Fecha = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
                };
                File.AppendAllText(RutaRegistro(), JsonSerializer.Serialize(Nuevo) + Environment.NewLine, Encoding.UTF8);
                return ArmarNFT(Nuevo);
            }
        }
        public NFTBE Consultar(string identificador)
        {
            long TokenId;
            if (!identificador.StartsWith(Prefijo, StringComparison.Ordinal) ||
                !long.TryParse(identificador.Substring(Prefijo.Length), out TokenId)) return null;
            lock (Bloqueo)
            {
                Token Encontrado = LeerTokens().Find(T => T.TokenId == TokenId);
                return Encontrado == null ? null : ArmarNFT(Encontrado);
            }
        }
        private static NFTBE ArmarNFT(Token token)
        {
            return new NFTBE
            {
                Identificador = Prefijo + token.TokenId,
                TokenId = token.TokenId,
                Hash = token.Hash,
                FechaEmision = DateTime.Parse(token.Fecha, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime()
            };
        }
        private static List<Token> LeerTokens()
        {
            List<Token> Tokens = new List<Token>();
            string Ruta = RutaRegistro();
            if (!File.Exists(Ruta)) return Tokens;
            foreach (string Linea in File.ReadAllLines(Ruta, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(Linea)) Tokens.Add(JsonSerializer.Deserialize<Token>(Linea));
            }
            return Tokens;
        }
        private static string RutaRegistro()
        {
            string Ruta = ConfigurationManager.AppSettings["NFTLocalRuta"];
            if (string.IsNullOrWhiteSpace(Ruta))
            {
                Ruta = HostingEnvironment.MapPath("~/App_Data/Blockchain/nft.jsonl");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Ruta));
            return Ruta;
        }
    }
}
