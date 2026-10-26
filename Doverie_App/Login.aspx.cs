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

    }
    //delega toda la lógica real en UsuarioBLL.LogIn(),
    // y acá solo se ocupa de la parte "web" (cookie, Session, redirección).
    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        UsuarioBE Usuario;
        string Token;
        // Toda la validación (usuario existe, no está bloqueado, contraseña
        // correcta) vive en la BLL. Acá solo recibimos el resultado.
        UsuarioBLL BllUsuario = new UsuarioBLL();
        ResultadoLogin Resultado =  BllUsuario.LogIn(txtUsuario.Text, txtContrasena.Text, out Usuario, out Token);
        switch(Resultado)
        {
            case ResultadoLogin.Exitoso:
                // Guardamos el Usuario completo (con su Perfil y permisos ya
                // cargados) en Session, para no tener que volver a consultar
                // la base en cada página mientras dure la sesión.
                Session["Usuario"] = Usuario;
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
                Response.Redirect("Default.aspx");
                break;
            case ResultadoLogin.UsuarioBloqueado:
                // [ERROR-002] Bloqueo automático por intentos fallidos.
                lblError.Text = "Tu cuenta se encuentra bloqueada. Recuperá tu contraseña para desbloquearla.";
                lblError.Visible = true;
                break;
            case ResultadoLogin.UsuarioSuspendido:
                // Suspensión manual (Administrador o Empresa): recuperar la
                // contraseña no la levanta.
                lblError.Text = "Tu cuenta se encuentra suspendida. Contactá al administrador.";
                lblError.Visible = true;
                break;
            case ResultadoLogin.CredencialesInvalidas:
                // Mismo mensaje tanto si el usuario no existe como si la
                // contraseña está mal — por seguridad, no se revela cuál de
                // las dos cosas falló (evita que alguien pueda "adivinar"
                // qué nombres de usuario existen en el sistema).
                lblError.Text = "Usuario o contraseña incorrectos.";
                lblError.Visible = true;
                break;
        }
    }
}