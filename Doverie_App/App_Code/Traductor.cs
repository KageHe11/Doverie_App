using System.Web;
using BLL;
using SERVICE;

/// Punto de acceso de la capa web al ContextoIdioma de la sesión
/// (RC-004 Traducir). Las páginas usan Traductor.T("clave") en el markup
/// (<%: Traductor.T("clave") %>) o en el code-behind.
public static class Traductor
{
    private const string ClaveSesion = "ContextoIdioma";

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
    /// Script para el OnClientClick de un botón que pide confirmación,
    /// con el mensaje traducido y escapado para JavaScript.
    public static string Confirmar(string clave)
    {
        return "return confirm(" + HttpUtility.JavaScriptStringEncode(T(clave), true) + ");";
    }
    private static HttpSessionStateBase SesionActual()
    {
        if (HttpContext.Current == null || HttpContext.Current.Session == null) return null;
        return new HttpSessionStateWrapper(HttpContext.Current.Session);
    }
}
