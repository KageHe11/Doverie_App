using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// CU-002-012 Ver Estadísticas de la Plataforma. El panel general se arma
/// en el servidor; las métricas por período las pide el navegador a los
/// webservices WS_Documentos, WS_Usuarios y WS_Blockchain.
public partial class Estadisticas : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerEstadisticas"; } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        CargarResumen();
        txtHasta.Text = DateTime.Today.ToString("yyyy-MM-dd");
        txtDesde.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        foreach (UsuarioBE Usuario in new UsuarioBLL().Listar())
        {
            ddlUsuario.Items.Add(new ListItem(Usuario.NombreUsuario, Usuario.Id.ToString()));
        }
        // Una tarjeta por método de webservice, con el archivo de salida
        // XML que define el anexo 10.12.2.
        rptMetricas.DataSource = new[]
        {
            Metrica("WS_Documentos.asmx", "ObtenerDocumentosPorPeriodo", "WS_DocumentosPorPeriodo.xml", "est.documentosSubidos", "entero"),
            Metrica("WS_Documentos.asmx", "ObtenerVerificacionesPorPeriodo", "WS_VerificacionesPorPeriodo.xml", "est.verificaciones", "entero"),
            Metrica("WS_Usuarios.asmx", "ObtenerUsuariosPorPeriodo", "WS_UsuariosPorPeriodo.xml", "est.usuariosRegistrados", "entero"),
            Metrica("WS_Usuarios.asmx", "ObtenerPlanMasContratado", "WS_PlanMasContratado.xml", "est.planMasContratado", "plan"),
            Metrica("WS_Blockchain.asmx", "ObtenerOperacionesPorPeriodo", "WS_OperacionesPorPeriodo.xml", "est.operacionesPeriodo", "entero"),
            Metrica("WS_Blockchain.asmx", "ObtenerTransaccionesPorUsuario", "WS_TransaccionesPorUsuario.xml", "est.transaccionesUsuario", "usuario")
        };
        rptMetricas.DataBind();
        new BitacoraBLL().Registrar("Ver Estadísticas", null, UsuarioActual.Id);
    }
    /// Pasos 2, 3 y 4 del CU. Si la base no responde [ERROR-040].
    private void CargarResumen()
    {
        try
        {
            EstadisticasBE Resumen = new EstadisticaBLL().ObtenerResumen();
            litUsuariosActivos.Text = Resumen.UsuariosActivos.ToString();
            litDocumentosAlmacenados.Text = Resumen.DocumentosAlmacenados.ToString();
            litOperacionesBlockchain.Text = Resumen.OperacionesBlockchain.ToString();
        }
        catch (Exception)
        {
            pnlResumen.Visible = false;
            lblError.Text = HttpUtility.HtmlEncode(Traductor.T("error.040"));
            lblError.Visible = true;
        }
    }
    private static object Metrica(string servicio, string metodo, string archivo, string clave, string tipo)
    {
        return new { Servicio = servicio, Url = servicio + "/" + metodo, Archivo = archivo, Titulo = Traductor.T(clave), Tipo = tipo };
    }
    /// Textos que usa el script de la página, ya traducidos y escapados.
    protected string TextosJs()
    {
        Dictionary<string, string> Textos = new Dictionary<string, string>
        {
            { "error040", Traductor.T("error.040") },
            { "periodoInvalido", Traductor.T("error.004") },
            { "sinPlan", Traductor.T("est.sinPlan") }
        };
        List<string> Pares = new List<string>();
        foreach (KeyValuePair<string, string> Par in Textos)
        {
            Pares.Add(Par.Key + ": " + HttpUtility.JavaScriptStringEncode(Par.Value, true));
        }
        return "{ " + string.Join(", ", Pares) + " }";
    }
}
