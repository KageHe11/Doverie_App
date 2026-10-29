using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Configuration;


namespace SERVICE
{
    /// Genera y valida los JWT usados para la sesión, según lo especificado
    /// en la documentación (Gestión de sesión mediante JWT con expiración).
    public class TokenService
    {
        private const int MinutosExpiracion = 30;
        private SymmetricSecurityKey ObtenerClave()
        {
            string ClaveSecreta = ConfigurationManager.AppSettings["JwtClaveSecreta"];
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ClaveSecreta));
        }
        // Genera el JWT de sesión: incluye el Id del Usuario y expira a los
        // 30 minutos. Este token es lo único que identifica al Usuario en cada
        // request posterior (vía la cookie TokenSesion).
        public string GenerarToken(UsuarioBE usuario)
        {
            SymmetricSecurityKey Clave = ObtenerClave();
            SigningCredentials Credenciales = new SigningCredentials(Clave, SecurityAlgorithms.HmacSha256);
            List<Claim> Claims = new List<Claim>
            {
                new Claim("Id", usuario.Id.ToString()),
                new Claim("NombreUsuario", usuario.NombreUsuario),
                new Claim("PerfilId", usuario.Perfil.Id.ToString())
            };

            JwtSecurityToken Token = new JwtSecurityToken(
                claims: Claims,
                expires: DateTime.UtcNow.AddMinutes(MinutosExpiracion),
                signingCredentials: Credenciales
            );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
        public bool ValidarToken(string token, out int idUsuario)
        {
            idUsuario = 0;
            try
            {
                JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();
                TokenValidationParameters Parametros = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = ObtenerClave()
                };
                ClaimsPrincipal Principal = Handler.ValidateToken(token, Parametros, out SecurityToken TokenValidado);
                idUsuario = int.Parse(Principal.FindFirst("Id").Value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public string GenerarTokenRecuperacion(UsuarioBE usuario)
        {
            SymmetricSecurityKey Clave = ObtenerClave();
            SigningCredentials Credenciales = new SigningCredentials(Clave, SecurityAlgorithms.HmacSha256);
            List<Claim> Claims = new List<Claim>
            {
                new Claim("Id", usuario.Id.ToString()),
                new Claim("Tipo", "Recuperacion"),
                new Claim("Huella", ObtenerHuella(usuario.Contrasena))
            };
            JwtSecurityToken Token = new JwtSecurityToken(
                claims: Claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: Credenciales
            );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
        public bool ValidarTokenRecuperacion(string token, out int idUsuario, out string huella)
        {
            idUsuario = 0;
            huella = null;
            try
            {
                JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();
                TokenValidationParameters Parametros = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = ObtenerClave()
                };
                ClaimsPrincipal Principal = Handler.ValidateToken(token, Parametros, out SecurityToken TokenValidado);
                string Tipo = Principal.FindFirst("Tipo")?.Value;
                if (Tipo != "Recuperacion") return false;
                idUsuario = int.Parse(Principal.FindFirst("Id").Value);
                huella = Principal.FindFirst("Huella")?.Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// Token para un enlace de documento compartido: 32 bytes de un
        /// generador criptográficamente seguro, en Base64 apto para URL. No
        /// es un JWT: no lleva datos adentro, se valida contra su hash en la
        /// base (y así se puede revocar).
        public string GenerarTokenEnlace()
        {
            byte[] Bytes = new byte[32];
            using (System.Security.Cryptography.RandomNumberGenerator Generador = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                Generador.GetBytes(Bytes);
            }
            return Convert.ToBase64String(Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        /// corta del hash de contraseña actual. Sirve para detectar si
        /// el link de recuperación ya fue usado: si la contraseña cambió desde
        /// que se generó el token, la huella guardada en el token ya no
        /// coincide con la huella de la contraseña actual del Usuario.
        public string ObtenerHuella(string contrasenaHasheada)
        {
            return contrasenaHasheada.Substring(0, Math.Min(20, contrasenaHasheada.Length));
        }
    }
}
