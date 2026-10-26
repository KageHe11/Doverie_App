using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SERVICE
{
    // Encapsula el hasheo de contraseñas con BCrypt. Es la única clase
    // del sistema que sabe cómo se guardan/comparan las contraseñas —
    // el resto del código nunca maneja texto plano ni el algoritmo en sí.
    public class HashService
    {
        // Genera un hash nuevo (con salt aleatorio incluido) para guardar en la base
        public string HashearContrasena(string contrasenaPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(contrasenaPlano);
        }
        // Compara una contraseña en texto plano contra un hash ya guardado.
        // BCrypt extrae el salt del propio hash para poder comparar.
        public bool VerificarContrasena(string contrasenaPlano, string hashAlmacenado)
        {
            return BCrypt.Net.BCrypt.Verify(contrasenaPlano, hashAlmacenado);
        }
        // Huella digital de un documento: SHA-256 del contenido, en
        // hexadecimal. A diferencia de BCrypt, es determinístico (mismo
        // archivo -> mismo hash), que es justo lo que se necesita para
        // verificar la integridad comparando contra la blockchain.
        public string CalcularHashArchivo(byte[] contenido)
        {
            using (System.Security.Cryptography.SHA256 Sha = System.Security.Cryptography.SHA256.Create())
            {
                return AHexadecimal(Sha.ComputeHash(contenido));
            }
        }
        public static string AHexadecimal(byte[] bytes)
        {
            StringBuilder Hex = new StringBuilder(bytes.Length * 2);
            foreach (byte B in bytes) Hex.Append(B.ToString("x2"));
            return Hex.ToString();
        }
    }
}
