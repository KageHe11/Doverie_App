using System;
using System.Web;
using BLL;
using SERVICE;

/// Punto de acceso de la capa web al ContextoIdioma de la sesión
/// (RC-004 Traducir). Las páginas usan Traductor.T("clave") en el markup
/// (<%: Traductor.T("clave") %>) o en el code-behind.
public static class Traductor
{
    private const string ClaveSesion = "ContextoIdioma";
    /// Cada cuánto una sesión vuelve a leer las traducciones de su idioma,
    /// para ver los cambios hechos en Idiomas o por script sin volver a entrar.
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(1);

    /// Contexto de la sesión actual. Si todavía no hay uno (visitante sin
    /// login, o sesión recién creada), se arma con el idioma predeterminado.
    public static ContextoIdioma Contexto
    {
        get
        {
            HttpSessionStateBase Sesion = SesionActual();
            ContextoIdioma Actual = Sesion != null ? Sesion[ClaveSesion] as ContextoIdioma : null;
            if (Actual == null)
            {
                Actual = new IdiomaBLL().CrearContexto(null);
                if (Sesion != null) Sesion[ClaveSesion] = Actual;
            }
            else if (DateTime.UtcNow - Actual.Cargado > Vigencia)
            {
                try
                {
                    Actual = new IdiomaBLL().CrearContexto(Actual.IdiomaId);
                    Sesion[ClaveSesion] = Actual;
                }
                catch (Exception)
                {
                    // Base no disponible (por ejemplo, durante un restore):
                    // se sigue con las traducciones que ya estaban.
                }
            }
            return Actual;
        }
        set
        {
            HttpSessionStateBase Sesion = SesionActual();
            if (Sesion != null) Sesion[ClaveSesion] = value;
        }
    }
    public static string T(string clave)
    {
        return Contexto.Traducir(clave);
    }
    public static string T(string clave, params object[] valores)
    {
        return Contexto.Traducir(clave, valores);
    }
    private static HttpSessionStateBase SesionActual()
    {
        if (HttpContext.Current == null || HttpContext.Current.Session == null) return null;
        return new HttpSessionStateWrapper(HttpContext.Current.Session);
    }
}
