using System;
using System.Web;
using System.Web.UI;
using BE;
using SERVICE;
using BLL;

/// Clase base de la que heredan las páginas que requieren sesión iniciada.
/// Valida el token JWT en cada carga de página y, opcionalmente, un
/// Permiso puntual que cada página puede declarar.
public class PaginaBase : Page
{
    /// Nombre del Permiso que esta página necesita para poder cargar.
    /// Cada página hija puede sobreescribir esto (ej: "VerUsuarios").
    /// Si no lo sobreescribe (queda null), solo exige tener sesión
    /// válida, sin pedir ningún permiso extra.
    protected virtual string PermisoRequerido { get { return null; } }
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        // 1) Sin cookie de token, no hay nada que validar: corta acá.
        HttpCookie CookieToken = Request.Cookies["TokenSesion"];
        if (CookieToken == null || string.IsNullOrEmpty(CookieToken.Value))
        {
            CerrarSesionYRedirigir();
            return;
        }
        // 2) Token presente pero vencido o inválido.
        TokenService TokenServ = new TokenService();
        int IdUsuario;
        bool EsValido = TokenServ.ValidarToken(CookieToken.Value, out IdUsuario);
        if (!EsValido)
        {
            CerrarSesionYRedirigir();
            return;
        }
        // 3) Token válido, pero sin datos de Usuario en Session
        // tampoco puede continuar.
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario == null)
        {
            CerrarSesionYRedirigir();
            return;
        }
        // 4) Todo válido: renovamos el token (sesión, 30 min
        // más desde ahora), para que no expire mientras el Usuario
        // sigue navegando activamente.
        string NuevoToken = TokenServ.GenerarToken(Usuario);
        HttpCookie NuevaCookie = new HttpCookie("TokenSesion", NuevoToken);
        NuevaCookie.HttpOnly = true;
        NuevaCookie.Expires = DateTime.Now.AddMinutes(30);
        Response.Cookies.Add(NuevaCookie);
        // 5) Si la página pidió un Permiso puntual (PermisoRequerido) y
        // el Usuario no lo tiene, lo mandamos a Acceso Denegado.
        if (PermisoRequerido != null && !TienePermiso(Usuario, PermisoRequerido))
        {
            Response.Redirect("~/AccesoDenegado.aspx");
            return;
        }
    }
    /// Busca el Permiso por nombre y chequea si el árbol de Familias del
    /// Perfil del Usuario lo contiene (recorrido recursivo vía el
    /// patrón Composite armado en BE).
    private bool TienePermiso(UsuarioBE usuario, string nombrePermiso)
    {
        if (usuario.Perfil == null) return false;
        PermisoBLL PermisoBll = new PermisoBLL();
        PermisoBE Permiso = PermisoBll.ObtenerPorNombre(nombrePermiso);
        if (Permiso == null) return false;
        return usuario.Perfil.TienePermiso(Permiso.Id);
    }
    ///Limpia la sesión y manda al Usuario de vuelta al Login.
    private void CerrarSesionYRedirigir()
    {
        Session.Clear();
        Response.Redirect("~/Login.aspx");
    }
}