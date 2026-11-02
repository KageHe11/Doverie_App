using BE;
using BLL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btnIngresar.Text = Traductor.T("login.ingresar");
    }
    /// Identificador de este navegador: un valor aleatorio en una cookie de
    /// larga duración. Si no existe todavía, se crea. Sirve para detectar
    /// inicios de sesión desde un dispositivo nuevo.
    private string TokenDispositivo()
    {
        HttpCookie Cookie = Request.Cookies["DispositivoDoverie"];
        if (Cookie != null && !string.IsNullOrWhiteSpace(Cookie.Value)) return Cookie.Value;
        string Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        HttpCookie Nueva = new HttpCookie("DispositivoDoverie", Token);
        Nueva.HttpOnly = true;
        Nueva.Expires = DateTime.Now.AddYears(1);
        Response.Cookies.Add(Nueva);
        return Token;
    }
    /// Descripción legible del dispositivo, para el aviso: navegador,
    /// sistema operativo y dirección IP.
    private string DescribirDispositivo()
    {
        HttpBrowserCapabilities Navegador = Request.Browser;
        return Navegador.Browser + " " + Navegador.Version + " / " + Navegador.Platform + " (" + Request.UserHostAddress + ")";
    }
    //delega toda la lógica real en UsuarioBLL.LogIn(),
    // y acá solo se ocupa de la parte "web" (cookie, Session, redirección).
    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        UsuarioBE Usuario;
        string Token;
        ContextoIdioma Idioma;
        // Toda la validación (usuario existe, no está bloqueado, contraseña
        // correcta) vive en la BLL. Acá solo recibimos el resultado.
        UsuarioBLL BllUsuario = new UsuarioBLL();
        ResultadoLogin Resultado =  BllUsuario.LogIn(txtUsuario.Text, txtContrasena.Text, out Usuario, out Token, out Idioma);
        switch(Resultado)
        {
            case ResultadoLogin.Exitoso:
                // Guardamos el Usuario completo (con su Perfil y permisos ya
                // cargados) en Session, para no tener que volver a consultar
                // la base en cada página mientras dure la sesión.
                Session["Usuario"] = Usuario;
                // RC-004: la interfaz pasa al idioma preferido del Usuario.
                Traductor.Contexto = Idioma;
                // El token JWT viaja en una cookie HttpOnly (no accesible
                // desde JavaScript, por seguridad) con 30 minutos de validez.
                // PaginaBase se encarga de renovarla en cada request (sesión
                // "sliding") mientras el Usuario siga navegando.
                HttpCookie Cookie = new HttpCookie("TokenSesion", Token);
                Cookie.HttpOnly = true;
                Cookie.Expires = DateTime.Now.AddMinutes(30);
                Response.Cookies.Add(Cookie);
                // Queda registro del evento en la Bitácora del sistema.
                new BitacoraBLL().Registrar("Login", null, Usuario.Id);
                BllUsuario.RegistrarDispositivo(Usuario, TokenDispositivo(), DescribirDispositivo());
                // Con la base inconsistente solo llega hasta acá el
                // Administrador: se lo lleva directo a revisar la integridad.
                Response.Redirect(DigitoVerificadorBLL.BaseComprometida ? "Integridad.aspx" : "Default.aspx");
                break;
            case ResultadoLogin.BaseInconsistente:
                lblError.Text = Traductor.T("login.baseInconsistente");
                lblError.Visible = true;
                break;
            case ResultadoLogin.UsuarioBloqueado:
                // [ERROR-002] Bloqueo automático por intentos fallidos.
                lblError.Text = Traductor.T("login.cuentaBloqueada");
                lblError.Visible = true;
                break;
            case ResultadoLogin.UsuarioSuspendido:
                // Suspensión manual (Administrador o Empresa): recuperar la
                // contraseña no la levanta.
                lblError.Text = Traductor.T("login.cuentaSuspendida");
                lblError.Visible = true;
                break;
            case ResultadoLogin.CredencialesInvalidas:
                // Mismo mensaje tanto si el usuario no existe como si la
                // contraseña está mal — por seguridad, no se revela cuál de
                // las dos cosas falló (evita que alguien pueda "adivinar"
                // qué nombres de usuario existen en el sistema).
                lblError.Text = Traductor.T("login.credencialesInvalidas");
                lblError.Visible = true;
                break;
        }
    }
}