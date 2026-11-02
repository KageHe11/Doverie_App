using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DAL
{
    /// Cifrado de los datos personales antes de guardarlos (email, DNI,
    /// teléfono, email de los destinatarios y datos del dispositivo).
    /// AES-256 en modo CBC con un IV aleatorio por valor: el mismo dato
    /// cifrado dos veces da textos distintos. La clave es CifradoClave de
    /// AppSettings y nunca se guarda en la base, así que una copia de la base
    /// sola no alcanza para leer los datos.
    ///
    /// Como un dato cifrado no se puede buscar, el email también se guarda
    /// como índice (HMAC-SHA256 del email normalizado): sirve para "¿ya existe
    /// este email?" sin descifrar nada.
    public static class CifradoDatos
    {
        /// Marca de los valores cifrados. Un valor sin la marca es anterior al
        /// cifrado (por ejemplo, de un backup viejo) y se devuelve tal cual.
        private const string Prefijo = "v1:";

        public static string Cifrar(string texto)
        {
            return Cifrar(texto, ClaveConfigurada());
        }
        public static string Descifrar(string valor)
        {
            return Descifrar(valor, ClaveConfigurada());
        }
        public static string Indice(string texto)
        {
            return Indice(texto, ClaveConfigurada());
        }

        // Las versiones con la clave explícita las usa también la migración
        // de los datos existentes (Database/CifrarDatos.ps1).
        public static string Cifrar(string texto, string clave)
        {
            if (texto == null) return null;
            using (Aes Algoritmo = Aes.Create())
            {
                Algoritmo.Key = Derivar(clave, "cifrado");
                Algoritmo.GenerateIV();
                using (MemoryStream Salida = new MemoryStream())
                {
                    Salida.Write(Algoritmo.IV, 0, Algoritmo.IV.Length);
                    using (CryptoStream Cifrador = new CryptoStream(Salida, Algoritmo.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] Bytes = Encoding.UTF8.GetBytes(texto);
                        Cifrador.Write(Bytes, 0, Bytes.Length);
                    }
                    return Prefijo + Convert.ToBase64String(Salida.ToArray());
                }
            }
        }
        public static string Descifrar(string valor, string clave)
        {
            if (valor == null || !valor.StartsWith(Prefijo, StringComparison.Ordinal)) return valor;
            byte[] Datos = Convert.FromBase64String(valor.Substring(Prefijo.Length));
            using (Aes Algoritmo = Aes.Create())
            {
                Algoritmo.Key = Derivar(clave, "cifrado");
                byte[] IV = new byte[Algoritmo.BlockSize / 8];
                Array.Copy(Datos, IV, IV.Length);
                Algoritmo.IV = IV;
                using (ICryptoTransform Descifrador = Algoritmo.CreateDecryptor())
                {
                    byte[] Texto = Descifrador.TransformFinalBlock(Datos, IV.Length, Datos.Length - IV.Length);
                    return Encoding.UTF8.GetString(Texto);
                }
            }
        }
        /// Mismo email (sin espacios y sin distinguir mayúsculas), mismo índice.
        public static string Indice(string texto, string clave)
        {
            if (texto == null) return null;
            using (HMACSHA256 Algoritmo = new HMACSHA256(Derivar(clave, "indice")))
            {
                byte[] Resultado = Algoritmo.ComputeHash(Encoding.UTF8.GetBytes(texto.Trim().ToLowerInvariant()));
                return BitConverter.ToString(Resultado).Replace("-", "").ToLowerInvariant();
            }
        }
        /// Una clave de 256 bits por uso, derivada de la configurada: el índice
        /// y el cifrado no comparten clave.
        private static byte[] Derivar(string clave, string uso)
        {
            using (SHA256 Algoritmo = SHA256.Create())
            {
                return Algoritmo.ComputeHash(Encoding.UTF8.GetBytes(uso + ":" + clave));
            }
        }
        private static string ClaveConfigurada()
        {
            string Clave = ConfigurationManager.AppSettings["CifradoClave"];
            if (string.IsNullOrWhiteSpace(Clave)) throw new ConfigurationErrorsException("Falta configurar CifradoClave.");
            return Clave;
        }
    }
}
