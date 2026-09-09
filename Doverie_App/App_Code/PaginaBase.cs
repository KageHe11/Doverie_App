using System;
using System.Web;
using System.Web.UI;
using BE;
using SERVICE;
using BLL;

public class PaginaBase : Page
{
    /// Cada página hija puede sobreescribir esto para declarar qué Permiso
    /// necesita para poder cargar. Si no lo sobreescribe (queda null),
    /// solo exige tener una sesión válida, sin ningún permiso extra.
    protected virtual string PermisoRequerido { get { return null; } }
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        HttpCookie CookieToken = Request.Cookies["TokenSesion"];
        if (CookieToken == null || string.IsNullOrEmpty(CookieToken.Value))
        {
            CerrarSesionYRedirigir();
            return;
        }
        TokenService TokenServ = new TokenService();
        int IdUsuario;
        bool EsValido = TokenServ.ValidarToken(CookieToken.Value, out IdUsuario);
        if (!EsValido)
        {
            CerrarSesionYRedirigir();
            return;
        }
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario == null)
        {
            CerrarSesionYRedirigir();
            return;
        }
        // Sliding: renovamos el token por 30 minutos más.
        string NuevoToken = TokenServ.GenerarToken(Usuario);
        HttpCookie NuevaCookie = new HttpCookie("TokenSesion", NuevoToken);
        NuevaCookie.HttpOnly = true;
        NuevaCookie.Expires = DateTime.Now.AddMinutes(30);
        Response.Cookies.Add(NuevaCookie);
        // Chequeo de permiso específico de la página, si declaró uno.
        if (PermisoRequerido != null && !TienePermiso(Usuario, PermisoRequerido))
        {
            Response.Redirect("~/AccesoDenegado.aspx");
            return;
        }
    }
    private bool TienePermiso(UsuarioBE usuario, string nombrePermiso)
    {
        if (usuario.Perfil == null) return false;

        PermisoBLL PermisoBll = new PermisoBLL();
        PermisoBE Permiso = PermisoBll.ObtenerPorNombre(nombrePermiso);
        if (Permiso == null) return false;
        return usuario.Perfil.TienePermiso(Permiso.Id);
    }
    private void CerrarSesionYRedirigir()
    {
        Session.Clear();
        Response.Redirect("~/Login.aspx");
    }
}