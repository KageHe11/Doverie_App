using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SERVICE
{
    public class HashService
    {
        public string HashearContrasena(string contrasenaPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(contrasenaPlano);
        }
        public bool VerificarContrasena(string contrasenaPlano, string hashAlmacenado)
        {
            return BCrypt.Net.BCrypt.Verify(contrasenaPlano, hashAlmacenado);
        }
    }
}
