using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL
{
    public class UsuarioBLL
    {
        public void Registrar(UsuarioBE Pusuario)
        {
            HashService HashServ = new HashService();
            Pusuario.Contrasena = HashServ.HashearContrasena(Pusuario.Contrasena);
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.Registrar(Pusuario);
        }
        public bool ExistePorNombreUsuario(string nombreUsuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ExistePorNombreUsuario(nombreUsuario);
        }
        public void ActualizarEstado(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarEstado(Pusuario);
        }
        public void ActualizarDatos(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Pusuario);
        }
        public UsuarioBE ObtenerDatos(int Pid)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ObtenerDatos(Pid);
        }
        public List<UsuarioBE> Listar()
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.Listar();
        }
        public UsuarioBE ObtenerPorNombreUsuario(string PnombreUsuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ObtenerPorNombreUsuario(PnombreUsuario);
        }
        public bool EstaBloqueado(UsuarioBE Pusuario)
        {
            return Pusuario.Suspendido;
        }
        public void ReiniciarIntentos(UsuarioBE Pusuario)
        {
            Pusuario.Intentos = 0;
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Pusuario);
        }
        public List<UsuarioBE> ListarPorEmpresa(int empresaId)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ListarPorEmpresa(empresaId);
        }
        public ResultadoRecuperacion EnviarRecuperacion(string email, string nombreUsuario, string urlBase)
        {
            UsuarioBE Usuario = ObtenerPorNombreUsuario(nombreUsuario);

            // Mismo criterio de seguridad que el Login: no revelamos cuál de los
            // dos datos está mal, solo si la combinación es válida o no.
            if (Usuario == null || string.IsNullOrWhiteSpace(Usuario.Gmail) ||
                !Usuario.Gmail.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoRecuperacion.DatosInvalidos;
            }

            TokenService TokenServ = new TokenService();
            string Token = TokenServ.GenerarTokenRecuperacion(Usuario);
            string Link = urlBase + "/RestablecerContrasena.aspx?token=" + Uri.EscapeDataString(Token);

            string Cuerpo = "<p>Hola " + Usuario.Nombre + ",</p>" +
                            "<p>Recibimos una solicitud para restablecer tu contraseña en Doverie.</p>" +
                            "<p><a href=\"" + Link + "\">Hacé clic acá para elegir una nueva contraseña</a></p>" +
                            "<p>Este enlace vence en 30 minutos. Si no fuiste vos, podés ignorar este email.</p>";

            EmailService EmailServ = new EmailService();
            EmailServ.EnviarEmail(Usuario.Gmail, "Recuperar contraseña - Doverie", Cuerpo);

            return ResultadoRecuperacion.Exitoso;
        }

        public ResultadoRestablecimiento RestablecerContrasena(string token, string nuevaContrasena, string confirmarContrasena)
        {
            if (nuevaContrasena != confirmarContrasena)
            {
                return ResultadoRestablecimiento.ContrasenasNoCoinciden;
            }
            TokenService TokenServ = new TokenService();
            int IdUsuario;
            string Huella;
            bool EsValido = TokenServ.ValidarTokenRecuperacion(token, out IdUsuario, out Huella);
            if (!EsValido)
            {
                return ResultadoRestablecimiento.TokenInvalido;
            }
            UsuarioBE Usuario = ObtenerDatos(IdUsuario);
            if (Usuario == null || Huella != TokenServ.ObtenerHuella(Usuario.Contrasena))
            {
                // El link ya fue usado antes (la contraseña cambió), o el
                // Usuario ya no existe.
                return ResultadoRestablecimiento.TokenInvalido;
            }
            HashService HashServ = new HashService();
            Usuario.Contrasena = HashServ.HashearContrasena(nuevaContrasena);
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Usuario);
            return ResultadoRestablecimiento.Exitoso;
        }
        public ResultadoModificarPerfil ModificarDatosPropios(int idUsuario, string nombre, string apellido, string gmail, string nuevaContrasena)
        {
            // 4. Validación de campos [ERROR-004]
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(gmail))
            {
                return ResultadoModificarPerfil.DatosInvalidos;
            }
            if (!Regex.IsMatch(gmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return ResultadoModificarPerfil.DatosInvalidos;
            }
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            // 5. Verificar que el email no esté registrado por OTRO usuario [ERROR-021]
            if (UsuarioDal.ExistePorGmailExcluyendo(gmail, idUsuario))
            {
                return ResultadoModificarPerfil.EmailYaRegistrado;
            }
            UsuarioBE Usuario = UsuarioDal.ObtenerDatos(idUsuario);
            Usuario.Nombre = nombre;
            Usuario.Apellido = apellido;
            Usuario.Gmail = gmail;
            if (!string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                HashService HashServ = new HashService();
                Usuario.Contrasena = HashServ.HashearContrasena(nuevaContrasena);
            }
            UsuarioDal.ActualizarDatos(Usuario);
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Modificar Datos Cuenta", "Usuario Id: " + idUsuario, idUsuario);
            return ResultadoModificarPerfil.Exitoso;
        }
        public ResultadoRegistro RegistrarPublico(UsuarioBE nuevoUsuario, string nombrePerfil)
        {
            // 7. Validación de campos obligatorios [ERROR-004]
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Apellido) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.NombreUsuario) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Contrasena) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Gmail) ||
                string.IsNullOrWhiteSpace(nuevoUsuario.Dni))
            {
                return ResultadoRegistro.DatosInvalidos;
            }
            // 7. Validación de formato: email válido
            if (!Regex.IsMatch(nuevoUsuario.Gmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return ResultadoRegistro.DatosInvalidos;
            }
            // 7. Validación de formato: DNI solo números, entre 7 y 8 dígitos
            if (!Regex.IsMatch(nuevoUsuario.Dni, @"^\d{7,8}$"))
            {
                return ResultadoRegistro.DatosInvalidos;
            }
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            // 8. Verificar que no esté registrado previamente [ERROR-005]
            if (UsuarioDal.ExistePorNombreUsuario(nuevoUsuario.NombreUsuario) ||
                UsuarioDal.ExistePorGmail(nuevoUsuario.Gmail))
            {
                return ResultadoRegistro.YaRegistrado;
            }
            // 9. Encriptar la contraseña
            HashService HashServ = new HashService();
            nuevoUsuario.Contrasena = HashServ.HashearContrasena(nuevoUsuario.Contrasena);
            PerfilBLL PerfilBll = new PerfilBLL();
            PerfilBE Perfil = PerfilBll.ObtenerPorNombre(nombrePerfil);
            nuevoUsuario.Perfil = new PerfilBE { Id = Perfil.Id };
            UsuarioDal.Registrar(nuevoUsuario);
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Alta Usuario (registro público)", "Usuario: " + nuevoUsuario.NombreUsuario + " (" + nombrePerfil + ")", nuevoUsuario.Id);
            return ResultadoRegistro.Exitoso;
        }
        /// Se encarga de todo el caso de uso Login: busca al Usuario, valida que no
        /// esté bloqueado, verifica la contraseña, reinicia el contador de
        /// intentos fallidos, carga su Perfil completo (con el árbol de permisos)
        /// y genera el token de sesión.
        public ResultadoLogin LogIn(string PnombreUsuario, string Pcontrasena, out UsuarioBE UsuarioLogueado, out string Token)
        {
            UsuarioLogueado = null;
            Token = null;
            UsuarioBE Usuario = ObtenerPorNombreUsuario(PnombreUsuario);
            if (Usuario == null)
            {
                // No se distingue "no existe" de "contraseña mal" (ver nota de
                // seguridad en Login.aspx.cs) — acá simplemente cortamos.
                return ResultadoLogin.CredencialesInvalidas;
            }
            if (EstaBloqueado(Usuario))
            {
                return ResultadoLogin.UsuarioBloqueado;
            }
            // La contraseña se compara hasheada, nunca en texto plano. BCrypt
            // internamente extrae el salt guardado en el hash y recalcula para
            // comparar (por eso no se puede comparar con == directo).
            HashService Hasheo = new HashService();
            if (!Hasheo.VerificarContrasena(Pcontrasena, Usuario.Contrasena))
            {
                return ResultadoLogin.CredencialesInvalidas;
            }
            // Login exitoso: se resetea el contador de intentos fallidos.
            ReiniciarIntentos(Usuario);
            // Hasta acá, Usuario.Perfil solo tiene el Id (así viene desde el
            // mapeo de la fila SQL). Acá se reemplaza por el objeto completo,
            // con el árbol de Familias/Permisos ya armado (patrón Composite),
            // necesario para que el resto de la app pueda usar TienePermiso().
            PerfilBLL PerfilBll = new PerfilBLL();
            Usuario.Perfil = PerfilBll.ObtenerDatos(Usuario.Perfil.Id);
            // Se genera el JWT que va a viajar en la cookie de sesión.
            TokenService ServioToken = new TokenService();
            Token = ServioToken.GenerarToken(Usuario);
            UsuarioLogueado = Usuario;
            return ResultadoLogin.Exitoso;
        }
    }
}
