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
    }
}
