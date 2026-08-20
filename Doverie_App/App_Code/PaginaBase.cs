using System;
using System.Web;
using System.Web.UI;
using BE;
using SERVICE;

public class PaginaBase : Page
{
    /// Clase base de la que heredan las páginas que requieren tener una sesión
    /// iniciada (todas excepto Login.aspx).
    /// en cada carga de página, antes de que se ejecute cualquier otra
    /// cosa, valida que exista un token de sesión (JWT) vigente. Si no hay token,
    /// o si expiró, corta el acceso y redirige al Login. Si el token es válido,
    /// lo renueva por 30 minutos más (sesión "sliding": mientras el usuario esté
    /// activo navegando, la sesión no se corta).
    protected override void OnInit(EventArgs e)
    {
        /// OnInit se ejecuta automáticamente al principio del ciclo de vida de
        /// CUALQUIER página que herede de esta clase (antes que el Page_Load
        /// propio de cada página) — por eso es el lugar correcto para el chequeo
        /// de sesión: se garantiza que se valide siempre, sin tener que repetir
        /// este código en cada .aspx.cs del sistema.
        base.OnInit(e);
        // 1) Buscamos la cookie que guarda el JWT. La generamos nosotros
        //    mismos al hacer login (ver UsuarioBLL.Login / Login.aspx.cs).
        HttpCookie CookieToken = Request.Cookies["TokenSesion"];
        // 2) Si no existe la cookie (nunca inició sesión, o el navegador
        //    la borró), no hay nada que validar: directo a Login.
        if (CookieToken == null || string.IsNullOrEmpty(CookieToken.Value))
        {
            CerrarSesionYRedirigir();
            return;
        }
        // 3) Si la cookie existe, validamos el JWT en sí: firma correcta
        //    (que no fue alterado) y que no haya vencido su expiración.
        TokenService TokenServ = new TokenService();
        int IdUsuario;
        bool EsValido = TokenServ.ValidarToken(CookieToken.Value, out IdUsuario);
        if (!EsValido)
        {
            // Token vencido o inválido → se acabó la sesión.
            CerrarSesionYRedirigir();
            return;
        }
        // 4) Token válido: como la sesión es "sliding", le regalamos 30
        //    minutos más generando un token NUEVO y pisando la cookie vieja.
        //    Para armar el nuevo token necesitamos los datos del usuario,
        //    que ya están guardados en Session desde el login (evita tener
        //    que ir a buscar el usuario a la base de datos en cada request).
        UsuarioBE Usuario = (UsuarioBE)Session["Usuario"];
        if (Usuario != null)
        {
            string NuevoToken = TokenServ.GenerarToken(Usuario);
            HttpCookie NuevaCookie = new HttpCookie("TokenSesion", NuevoToken);
            NuevaCookie.HttpOnly = true;
            NuevaCookie.Expires = DateTime.Now.AddMinutes(30);
            Response.Cookies.Add(NuevaCookie);
        }
        else
        {
            // Caso raro pero posible: el token es válido pero la Session
            // se perdió (por ejemplo, se reinició el servidor/IIS, o
            // expiró la Session aunque el token todavía no). Sin el
            // objeto Usuario no podemos regenerar el token, así que
            // tratamos esto como sesión inválida.
            CerrarSesionYRedirigir();
        }
    }
    private void CerrarSesionYRedirigir()
    {
        Session.Clear();
        Response.Redirect("~/Login.aspx");
    }
}