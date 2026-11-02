using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BLL;

/// Inicio. Sin sesión: la página pública de Doverie (cómo funciona,
/// funcionalidades, planes con los precios de la base, sobre nosotros y
/// contacto). Con sesión: un acceso por cada sección que el Perfil del
/// Usuario permite (los mismos permisos que exige cada página).
public partial class _Default : System.Web.UI.Page
{
    protected const string UrlLinkedIn = "https://www.linkedin.com/in/doverie-app-833930448/";

    protected void Page_Load(object sender, EventArgs e)
    {
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        pnlVisitante.Visible = Usuario == null;
        pnlUsuario.Visible = Usuario != null;
        if (Usuario != null)
        {
            litSaludo.Text = Traductor.T("inicio.hola", Usuario.Nombre);
            rptAccesos.DataSource = Accesos(Usuario);
            rptAccesos.DataBind();
            return;
        }
        btnEnviarContacto.Text = Traductor.T("home.contacto.enviar");
        rptFunciones.DataSource = Funciones();
        rptFunciones.DataBind();
        rptPlanes.DataSource = Planes();
        rptPlanes.DataBind();
    }
    protected void btnEnviarContacto_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        string Nombre = txtNombreContacto.Text.Trim();
        // Si el campo trampa vino completo es un bot: se le responde como
        // si se hubiera enviado, sin mandar nada.
        ResultadoContacto Resultado = txtSitioWeb.Text.Length > 0
            ? ResultadoContacto.Exitoso
            : new ContactoBLL().Enviar(Nombre, txtEmailContacto.Text, txtMensajeContacto.Text);
        switch (Resultado)
        {
            case ResultadoContacto.Exitoso:
                Mensaje.Exito(Traductor.T("home.contacto.ok", Nombre, txtEmailContacto.Text.Trim()));
                txtNombreContacto.Text = "";
                txtEmailContacto.Text = "";
                txtMensajeContacto.Text = "";
                break;
            case ResultadoContacto.DatosInvalidos:
                Mensaje.Error(Traductor.T("error.004"));
                break;
            default:
                Mensaje.Error(Traductor.T("home.contacto.error", ContactoBLL.EmailContacto));
                break;
        }
    }
    private static List<object> Funciones()
    {
        // Icono (entidad HTML), clave del título y clave del texto.
        string[,] Lista =
        {
            { "&#128279;", "inicio.blockchainTitulo", "inicio.blockchainTexto" },
            { "&#127894;&#65039;", "home.f.nftTitulo", "home.f.nftTexto" },
            { "&#128228;", "inicio.compartirTitulo", "inicio.compartirTexto" },
            { "&#128270;", "home.f.trazaTitulo", "home.f.trazaTexto" },
            { "&#128274;", "inicio.seguridadTitulo", "inicio.seguridadTexto" },
            { "&#128101;", "home.f.equiposTitulo", "home.f.equiposTexto" }
        };
        List<object> Resultado = new List<object>();
        for (int I = 0; I < Lista.GetLength(0); I++)
        {
            Resultado.Add(new { Icono = Lista[I, 0], Titulo = Traductor.T(Lista[I, 1]), Texto = Traductor.T(Lista[I, 2]) });
        }
        return Resultado;
    }
    /// Un plan por nivel con su precio mensual más bajo (el del plan anual).
    private static List<object> Planes()
    {
        List<object> Resultado = new List<object>();
        foreach (IGrouping<string, PlanBE> Nivel in new SuscripcionBLL().ListarPlanes().GroupBy(P => P.Nombre))
        {
            decimal Desde = Nivel.Min(P => P.PrecioMensual);
            Resultado.Add(new
            {
                Nombre = Traductor.T("plan.nivel." + Nivel.Key),
                Subtitulo = Traductor.T("plan.subtitulo." + Nivel.Key),
                Precio = SuscripcionBLL.FormatearMonto(Desde),
                Caracteristicas = Traductor.T("plan.caracteristicas." + Nivel.Key).Split('|'),
                Destacado = Nivel.Key == "Oro"
            });
        }
        return Resultado;
    }
    private static List<object> Accesos(UsuarioBE usuario)
    {
        List<object> Lista = new List<object>();
        // Icono (entidad HTML), clave del título, clave de la descripción, página y permiso.
        string[,] Secciones =
        {
            { "&#128196;", "nav.documentos", "inicio.acc.documentos", "/Documentos.aspx", "VerDocumentos" },
            { "&#128276;", "notif.titulo", "inicio.acc.notificaciones", "/Notificaciones.aspx", "VerNotificaciones" },
            { "&#11088;", "nav.suscripcion", "inicio.acc.suscripcion", "/Suscripcion.aspx", "Suscribirse" },
            { "&#129534;", "fact.titulo", "inicio.acc.facturacion", "/Facturacion.aspx", "VerFacturacion" },
            { "&#128101;", "empleados.titulo", "inicio.acc.empleados", "/EmpleadosEmpresa.aspx", "VerEmpleadosPropios" },
            { "&#128100;", "nav.usuarios", "inicio.acc.usuarios", "/Usuarios.aspx", "VerUsuarios" },
            { "&#128273;", "nav.credenciales", "inicio.acc.credenciales", "/Perfiles.aspx", "VerMenuAdmin" },
            { "&#128203;", "nav.bitacora", "inicio.acc.bitacora", "/Bitacora.aspx", "VerBitacora" },
            { "&#128202;", "nav.estadisticas", "inicio.acc.estadisticas", "/Estadisticas.aspx", "VerEstadisticas" },
            { "&#128190;", "nav.backup", "inicio.acc.backup", "/Backup.aspx", "GenerarBackup" },
            { "&#9989;", "nav.integridad", "inicio.acc.integridad", "/Integridad.aspx", DigitoVerificadorBLL.PermisoGestion },
            { "&#127760;", "nav.idiomas", "inicio.acc.idiomas", "/Idiomas.aspx", "VerIdiomas" }
        };
        for (int I = 0; I < Secciones.GetLength(0); I++)
        {
            if (!UsuarioBLL.TienePermiso(usuario, Secciones[I, 4])) continue;
            Lista.Add(new { Icono = Secciones[I, 0], Titulo = Traductor.T(Secciones[I, 1]), Descripcion = Traductor.T(Secciones[I, 2]), Url = Secciones[I, 3] });
        }
        if (!usuario.Empleado)
        {
            Lista.Add(new { Icono = "&#9881;&#65039;", Titulo = Traductor.T("nav.miPerfil"), Descripcion = Traductor.T("inicio.acc.miPerfil"), Url = "/MiPerfil.aspx" });
        }
        return Lista;
    }
}
