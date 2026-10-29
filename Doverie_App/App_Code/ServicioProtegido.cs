using System;
using System.Globalization;
using System.Web;
using System.Web.Services;
using BE;
using BLL;
using SERVICE;

/// Base de los webservices de estadísticas (WS_Documentos, WS_Usuarios,
/// WS_Blockchain). Exponen datos de toda la plataforma, así que exigen lo
/// mismo que PaginaBase: token JWT válido, sesión iniciada y el Permiso
/// VerEstadisticas. Si falta algo responden 403 sin contenido.
public abstract class ServicioProtegido : WebService
{
    private const string PermisoRequerido = "VerEstadisticas";

    /// Valida el acceso y el período recibido ("yyyy-MM-dd"). Si algo falla
    /// deja la respuesta en 403 (sin acceso) o 400 (período inválido) y
    /// devuelve false: el método no tiene que devolver datos.
    protected bool Preparar(string desde, string hasta, out DateTime fechaDesde, out DateTime fechaHasta)
    {
        fechaDesde = DateTime.MinValue;
        fechaHasta = DateTime.MinValue;
        if (!AccesoPermitido())
        {
            Cortar(403);
            return false;
        }
        if (!DateTime.TryParseExact(desde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaDesde) ||
            !DateTime.TryParseExact(hasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaHasta) ||
            fechaDesde > fechaHasta)
        {
            Cortar(400);
            return false;
        }
        return true;
    }
    private bool AccesoPermitido()
    {
        HttpCookie CookieToken = Context.Request.Cookies["TokenSesion"];
        int IdUsuario;
        if (CookieToken == null || !new TokenService().ValidarToken(CookieToken.Value, out IdUsuario)) return false;
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario == null || Usuario.Id != IdUsuario || Usuario.Perfil == null) return false;
        PermisoBE Permiso = new PermisoBLL().ObtenerPorNombre(PermisoRequerido);
        return Permiso != null && Usuario.Perfil.TienePermiso(Permiso.Id);
    }
    private void Cortar(int estado)
    {
        Context.Response.StatusCode = estado;
        // Sin esto IIS reemplaza la respuesta vacía por su página de error.
        Context.Response.TrySkipIisCustomErrors = true;
        Context.Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
}
