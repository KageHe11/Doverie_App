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
        // Política de bloqueo de cuenta: al llegar a esta cantidad de
        // intentos fallidos consecutivos, la cuenta se bloquea [ERROR-002].
        public const int MaxIntentosFallidos = 3;
        // Motivo que distingue un bloqueo automático por intentos de una
        // suspensión manual del Administrador o la Empresa: solo el primero
        // se levanta al recuperar la contraseña.
        public const string MotivoBloqueoIntentos = "Bloqueo por intentos fallidos";
        public void Registrar(UsuarioBE Pusuario)
        {
            HashService HashServ = new HashService();
            Pusuario.Contrasena = HashServ.HashearContrasena(Pusuario.Contrasena);
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.Registrar(Pusuario);
        }
        /// Motivo con el que SP_Usuario_RevocarAcceso marca a un empleado
        /// eliminado por su Empresa (CU-001-016): baja permanente.
        public const string MotivoRevocado = "Acceso revocado por la empresa";

        /// Validaciones de datos de un Usuario, compartidas por el Registro
        /// público, la gestión del Administrador y la de la Empresa:
        /// obligatorios, formato de email y DNI [ERROR-004], y nombre de
        /// usuario o email ya registrados [ERROR-005 / ERROR-021].
        public ResultadoRegistro ValidarDatos(UsuarioBE usuario, bool esAlta)
        {
            usuario.Nombre = (usuario.Nombre ?? "").Trim();
            usuario.Apellido = (usuario.Apellido ?? "").Trim();
            usuario.NombreUsuario = (usuario.NombreUsuario ?? "").Trim();
            usuario.Gmail = (usuario.Gmail ?? "").Trim();
            usuario.Dni = (usuario.Dni ?? "").Trim();
            usuario.Telefono = string.IsNullOrWhiteSpace(usuario.Telefono) ? null : usuario.Telefono.Trim();
            if (usuario.Nombre.Length == 0 || usuario.Apellido.Length == 0 || usuario.Gmail.Length == 0 || usuario.Dni.Length == 0 ||
                usuario.Nombre.Length > 50 || usuario.Apellido.Length > 50 || usuario.Gmail.Length > 50 ||
                (esAlta && (usuario.NombreUsuario.Length == 0 || usuario.NombreUsuario.Length > 50 || string.IsNullOrWhiteSpace(usuario.Contrasena))) ||
                !Regex.IsMatch(usuario.Gmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ||
                !Regex.IsMatch(usuario.Dni, @"^\d{7,8}$") ||
                (usuario.Telefono != null && !Regex.IsMatch(usuario.Telefono, @"^[0-9+\-\s()]{6,20}$")))
            {
                return ResultadoRegistro.DatosInvalidos;
            }
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            bool Repetido = esAlta
                ? UsuarioDal.ExistePorNombreUsuario(usuario.NombreUsuario) || UsuarioDal.ExistePorGmail(usuario.Gmail)
                : UsuarioDal.ExistePorGmailExcluyendo(usuario.Gmail, usuario.Id);
            return Repetido ? ResultadoRegistro.YaRegistrado : ResultadoRegistro.Exitoso;
        }
        /// Alta de un Usuario desde la gestión del Administrador o de una
        /// Empresa (CU-001-010). accionBitacora distingue quién la hizo.
        public ResultadoRegistro RegistrarDesdeGestion(UsuarioBE usuario, int actorId, string accionBitacora)
        {
            ResultadoRegistro Validacion = ValidarDatos(usuario, true);
            if (Validacion != ResultadoRegistro.Exitoso) return Validacion;
            Registrar(usuario);
            new BitacoraBLL().Registrar(accionBitacora, "Usuario: " + usuario.NombreUsuario, actorId);
            return ResultadoRegistro.Exitoso;
        }
        /// Modificación de un Usuario desde la gestión. nuevaContrasena vacía
        /// conserva la actual.
        public ResultadoRegistro ModificarDesdeGestion(UsuarioBE usuario, string nuevaContrasena, int actorId, string accionBitacora)
        {
            ResultadoRegistro Validacion = ValidarDatos(usuario, false);
            if (Validacion != ResultadoRegistro.Exitoso) return Validacion;
            if (!string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                usuario.Contrasena = new HashService().HashearContrasena(nuevaContrasena);
            }
            new UsuarioDAL().ActualizarDatos(usuario);
            new BitacoraBLL().Registrar(accionBitacora, "Usuario: " + usuario.NombreUsuario, actorId);
            return ResultadoRegistro.Exitoso;
        }
        /// CU-002-010 Suspender Usuario: el motivo es obligatorio [ERROR-004]
        /// (hasta 50 caracteres, el largo de la columna).
        public ResultadoEstadoUsuario Suspender(int usuarioId, string motivo, int actorId)
        {
            motivo = (motivo ?? "").Trim();
            if (motivo.Length == 0 || motivo.Length > 50) return ResultadoEstadoUsuario.MotivoInvalido;
            UsuarioBE Usuario = ObtenerDatos(usuarioId);
            if (Usuario == null) return ResultadoEstadoUsuario.SinAcceso;
            try
            {
                Usuario.Suspendido = true;
                Usuario.Motivo = motivo;
                new UsuarioDAL().ActualizarEstado(Usuario);
            }
            catch (Exception)
            {
                return ResultadoEstadoUsuario.Error;
            }
            new BitacoraBLL().Registrar("Suspender Usuario", "Usuario: " + Usuario.NombreUsuario + " | Motivo: " + motivo, actorId);
            return ResultadoEstadoUsuario.Exitoso;
        }
        /// CU-002-011 Reactivar Usuario: tiene que estar suspendido
        /// [ERROR-039]. Reinicia los intentos y avisa al titular (ver
        /// ActualizarEstado).
        public ResultadoEstadoUsuario Reactivar(int usuarioId, int actorId)
        {
            UsuarioBE Usuario = ObtenerDatos(usuarioId);
            if (Usuario == null) return ResultadoEstadoUsuario.SinAcceso;
            if (!Usuario.Suspendido) return ResultadoEstadoUsuario.NoSuspendido;
            try
            {
                Usuario.Suspendido = false;
                Usuario.Motivo = null;
                ActualizarEstado(Usuario);
            }
            catch (Exception)
            {
                return ResultadoEstadoUsuario.Error;
            }
            new BitacoraBLL().Registrar("Reactivar Usuario", "Usuario: " + Usuario.NombreUsuario, actorId);
            return ResultadoEstadoUsuario.Exitoso;
        }
        /// CU-001-016 Eliminar Usuario Empresa: revoca el acceso del
        /// empleado de forma permanente [ERROR-037]. Solo sobre empleados de
        /// la propia Empresa.
        public ResultadoEstadoUsuario RevocarAccesoEmpleado(int empleadoId, int empresaId)
        {
            UsuarioBE Empleado = ObtenerDatos(empleadoId);
            if (Empleado == null || Empleado.EmpresaId != empresaId) return ResultadoEstadoUsuario.SinAcceso;
            try
            {
                new UsuarioDAL().RevocarAcceso(empleadoId);
            }
            catch (Exception)
            {
                return ResultadoEstadoUsuario.Error;
            }
            new BitacoraBLL().Registrar("Eliminar Usuario Empresa", "Empleado: " + Empleado.NombreUsuario, empresaId);
            return ResultadoEstadoUsuario.Exitoso;
        }
        public bool EstaRevocado(UsuarioBE usuario)
        {
            return usuario.Suspendido && usuario.Motivo == MotivoRevocado;
        }
        public bool ExistePorNombreUsuario(string nombreUsuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ExistePorNombreUsuario(nombreUsuario);
        }
        public void ActualizarEstado(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            if (!Pusuario.Suspendido)
            {
                // Al reactivar se reinicia el contador: si no, un Usuario
                // bloqueado por intentos volvería a bloquearse con el
                // primer error.
                Pusuario.Intentos = 0;
                UsuarioDal.ActualizarBloqueo(Pusuario);
                // CU-002-011 paso 8: se avisa al titular de la cuenta, si
                // está suscripto al evento.
                GestorEventos.Notificar(CatalogoEventos.CuentaReactivada, Pusuario.Id, null);
                return;
            }
            UsuarioDal.ActualizarEstado(Pusuario);
        }
        /// Registra el dispositivo desde el que el Usuario inició sesión. Si
        /// es nuevo y el Usuario ya tenía otros dispositivos registrados, se
        /// dispara "inicio de sesión desde un nuevo dispositivo". El primer
        /// dispositivo se registra sin avisar.
        /// tokenDispositivo es el valor de la cookie del navegador; se
        /// guarda solo su SHA-256.
        public void RegistrarDispositivo(UsuarioBE usuario, string tokenDispositivo, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(tokenDispositivo)) return;
            HashService HashServ = new HashService();
            string Identificador = HashServ.CalcularHashArchivo(Encoding.UTF8.GetBytes(tokenDispositivo));
            DispositivoDAL DispositivoDal = new DispositivoDAL();
            if (DispositivoDal.Existe(usuario.Id, Identificador)) return;
            bool TeniaOtros = DispositivoDal.Contar(usuario.Id) > 0;
            if (descripcion != null && descripcion.Length > 200) descripcion = descripcion.Substring(0, 200);
            DispositivoDal.Registrar(usuario.Id, Identificador, descripcion);
            if (TeniaOtros)
            {
                GestorEventos.Notificar(CatalogoEventos.InicioSesionNuevoDispositivo, usuario.Id, descripcion);
            }
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
        public bool EstaBloqueadoPorIntentos(UsuarioBE Pusuario)
        {
            return Pusuario.Suspendido && Pusuario.Motivo == MotivoBloqueoIntentos;
        }
        public void ReiniciarIntentos(UsuarioBE Pusuario)
        {
            if (Pusuario.Intentos == 0) return;
            Pusuario.Intentos = 0;
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarBloqueo(Pusuario);
        }
        /// Suma un intento fallido y, si se alcanzó el máximo, bloquea la
        /// cuenta. Devuelve true si este intento provocó el bloqueo.
        public bool RegistrarIntentoFallido(UsuarioBE Pusuario)
        {
            Pusuario.Intentos += 1;
            bool Bloquear = Pusuario.Intentos >= MaxIntentosFallidos;
            if (Bloquear)
            {
                Pusuario.Suspendido = true;
                Pusuario.Motivo = MotivoBloqueoIntentos;
            }
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarBloqueo(Pusuario);
            if (Bloquear)
            {
                BitacoraBLL BitacoraBll = new BitacoraBLL();
                BitacoraBll.Registrar("Bloqueo Cuenta", "Usuario: " + Pusuario.NombreUsuario + " (" + Pusuario.Intentos + " intentos fallidos)", Pusuario.Id);
            }
            return Bloquear;
        }
        /// Guarda el idioma preferido elegido desde el SelectorIdioma.
        public void ActualizarIdioma(UsuarioBE Pusuario, int? idiomaId)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarIdioma(Pusuario.Id, idiomaId);
            Pusuario.IdiomaId = idiomaId;
        }
        /// Empleados de una Empresa. Los eliminados (CU-001-016, acceso
        /// revocado) ya no forman parte de la cuenta y no se listan.
        public List<UsuarioBE> ListarPorEmpresa(int empresaId)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ListarPorEmpresa(empresaId).Where(U => !EstaRevocado(U)).ToList();
        }
        public ResultadoRecuperacion EnviarRecuperacion(string email, string nombreUsuario, string urlBase)
        {
            UsuarioBE Usuario = ObtenerPorNombreUsuario(nombreUsuario);

            // Mismo criterio de seguridad que el Login: no revelamos cuál de los
            // dos datos está mal, solo si la combinación es válida o no.
            if (Usuario == null || string.IsNullOrWhiteSpace(Usuario.Gmail) ||
                !Usuario.Gmail.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                // Queda registro del intento (sin decir cuál dato estaba mal).
                new BitacoraBLL().Registrar("Recuperar Contraseña (datos inválidos)", "Usuario ingresado: " + (nombreUsuario ?? "").Trim(), Usuario != null ? (int?)Usuario.Id : null);
                return ResultadoRecuperacion.DatosInvalidos;
            }
            new BitacoraBLL().Registrar("Recuperar Contraseña", "Link de recuperación generado para " + Usuario.Gmail, Usuario.Id);

            TokenService TokenServ = new TokenService();
            string Token = TokenServ.GenerarTokenRecuperacion(Usuario);
            string Link = urlBase + "/RestablecerContrasena.aspx?token=" + Uri.EscapeDataString(Token);

            string Cuerpo = "<p>Hola " + Usuario.Nombre + ",</p>" +
                            "<p>Recibimos una solicitud para restablecer tu contraseña en Doverie.</p>" +
                            "<p><a href=\"" + Link + "\">Hacé clic acá para elegir una nueva contraseña</a></p>" +
                            "<p>Este enlace vence en 30 minutos. Si no fuiste vos, podés ignorar este email.</p>";

            EmailService EmailServ = new EmailService();
            try
            {
                EmailServ.EnviarEmail(Usuario.Gmail, "Recuperar contraseña - Doverie", Cuerpo);
            }
            catch (Exception)
            {
                // Si el servidor de correo falla, la pantalla igual muestra el
                // mensaje neutro de siempre: avisar del error confirmaría que
                // el usuario y el email son correctos. La falla queda en la
                // Bitácora para que la vea el Administrador.
                new BitacoraBLL().Registrar("Error Envío Email", "Recuperación de contraseña de " + Usuario.NombreUsuario, Usuario.Id);
            }
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
            // Recuperar la contraseña desbloquea la cuenta [ERROR-002], pero
            // solo si el bloqueo fue por intentos fallidos: una suspensión
            // manual sigue vigente hasta que la levante quien la aplicó.
            bool Desbloquear = EstaBloqueadoPorIntentos(Usuario);
            if (Desbloquear)
            {
                Usuario.Suspendido = false;
                Usuario.Motivo = null;
            }
            Usuario.Intentos = 0;
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Usuario);
            if (Desbloquear)
            {
                BitacoraBLL BitacoraBll = new BitacoraBLL();
                BitacoraBll.Registrar("Desbloqueo Cuenta", "Usuario: " + Usuario.NombreUsuario + " (recuperación de contraseña)", Usuario.Id);
            }
            new BitacoraBLL().Registrar("Restablecer Contraseña", "Usuario: " + Usuario.NombreUsuario, Usuario.Id);
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
            // 7. Validación de campos y formatos [ERROR-004] y 8. que no esté
            // registrado previamente [ERROR-005]
            ResultadoRegistro Validacion = ValidarDatos(nuevoUsuario, true);
            if (Validacion != ResultadoRegistro.Exitoso) return Validacion;
            UsuarioDAL UsuarioDal = new UsuarioDAL();
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
        public ResultadoLogin LogIn(string PnombreUsuario, string Pcontrasena, out UsuarioBE UsuarioLogueado, out string Token, out ContextoIdioma Idioma)
        {
            UsuarioLogueado = null;
            Token = null;
            Idioma = null;
            UsuarioBE Usuario = ObtenerPorNombreUsuario(PnombreUsuario);
            if (Usuario == null)
            {
                // No se distingue "no existe" de "contraseña mal" (ver nota de
                // seguridad en Login.aspx.cs) — acá simplemente cortamos.
                return ResultadoLogin.CredencialesInvalidas;
            }
            if (EstaBloqueado(Usuario))
            {
                // Bloqueo automático (se levanta recuperando la contraseña)
                // vs. suspensión manual (solo la levanta un Administrador).
                return EstaBloqueadoPorIntentos(Usuario) ? ResultadoLogin.UsuarioBloqueado : ResultadoLogin.UsuarioSuspendido;
            }
            // La contraseña se compara hasheada, nunca en texto plano. BCrypt
            // internamente extrae el salt guardado en el hash y recalcula para
            // comparar (por eso no se puede comparar con == directo).
            HashService Hasheo = new HashService();
            if (!Hasheo.VerificarContrasena(Pcontrasena, Usuario.Contrasena))
            {
                // Contraseña incorrecta: se suma el intento fallido. Si con
                // este se llega al máximo, la cuenta queda bloqueada.
                if (RegistrarIntentoFallido(Usuario))
                {
                    return ResultadoLogin.UsuarioBloqueado;
                }
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
            // RC-004 Traducir: se establece el idioma preferido del Usuario
            // (o el predeterminado) mediante ContextoIdioma (patrón Strategy).
            IdiomaBLL IdiomaBll = new IdiomaBLL();
            Idioma = IdiomaBll.CrearContexto(Usuario.IdiomaId);
            UsuarioLogueado = Usuario;
            return ResultadoLogin.Exitoso;
        }
    }
}
