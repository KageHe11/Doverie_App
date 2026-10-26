using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// Documentos: CU-001-012 Listado, 002 Subir, 003 Descargar, 004 Eliminar
/// y 006 Verificar Integridad. Entrar exige VerDocumentos; cada acción
/// exige además su propio Permiso. Que el documento sea de la cuenta del
/// Usuario lo controla DocumentoBLL.
public partial class Documentos : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerDocumentos"; } }

    /// Los encabezados de las grillas se traducen en Init, antes de que se
    /// reconstruyan desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvDocumentos.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvDocumentos.Columns[1].HeaderText = Traductor.T("doc.tipo");
        gvDocumentos.Columns[2].HeaderText = Traductor.T("doc.subidoPor");
        gvDocumentos.Columns[3].HeaderText = Traductor.T("doc.fecha");
        gvDocumentos.Columns[4].HeaderText = Traductor.T("comun.acciones");
        gvHistorial.Columns[0].HeaderText = Traductor.T("bitacora.fecha");
        gvHistorial.Columns[1].HeaderText = Traductor.T("bitacora.evento");
        gvHistorial.Columns[2].HeaderText = Traductor.T("comun.usuario");
    }
    // Se calculan una vez por request: TienePermiso consulta la base y la
    // grilla lo necesita en cada fila.
    private bool PuedeDescargar, PuedeVerificar, PuedeEliminar;
    protected void Page_Load(object sender, EventArgs e)
    {
        PuedeDescargar = TienePermiso("DescargarDocumento");
        PuedeVerificar = TienePermiso("VerificarDocumento");
        PuedeEliminar = TienePermiso("EliminarDocumento");
        pnlSubir.Visible = TienePermiso("SubirDocumento");
        btnSubir.Text = Traductor.T("doc.subir");
        btnFiltrar.Text = Traductor.T("bitacora.filtrar");
        ddlTipo.Items[0].Text = Traductor.T("comun.todos");
        litAyudaArchivo.Text = Traductor.T("doc.ayudaArchivo",
            string.Join(", ", DocumentoBLL.ExtensionesPermitidas), DocumentoBLL.TamanoMaximoBytes / (1024 * 1024));
        if (!IsPostBack)
        {
            CargarTipos();
            CargarGrilla();
        }
    }
    private void CargarTipos()
    {
        string Seleccionado = ddlTipo.SelectedValue;
        while (ddlTipo.Items.Count > 1) ddlTipo.Items.RemoveAt(1);
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        foreach (string Tipo in DocumentoBll.ListarTipos(UsuarioActual))
        {
            ddlTipo.Items.Add(new ListItem(Tipo, Tipo));
        }
        if (ddlTipo.Items.FindByValue(Seleccionado) != null) ddlTipo.SelectedValue = Seleccionado;
    }
    private void CargarGrilla()
    {
        DateTime? Desde = string.IsNullOrEmpty(txtDesde.Text) ? (DateTime?)null : Convert.ToDateTime(txtDesde.Text);
        DateTime? Hasta = string.IsNullOrEmpty(txtHasta.Text) ? (DateTime?)null : Convert.ToDateTime(txtHasta.Text).AddDays(1).AddSeconds(-1);
        bool HayFiltros = !string.IsNullOrWhiteSpace(txtFiltroNombre.Text) || ddlTipo.SelectedValue != "" || Desde.HasValue || Hasta.HasValue;
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        List<DocumentoBE> Lista = DocumentoBll.Listar(UsuarioActual, txtFiltroNombre.Text, ddlTipo.SelectedValue, Desde, Hasta);
        gvDocumentos.DataSource = Lista;
        gvDocumentos.DataBind();
        // 2.1 La cuenta no tiene documentos / 4.1 El filtro no arroja
        // resultados [ERROR-033].
        lblSinResultados.Visible = Lista.Count == 0;
        lblSinResultados.Text = Traductor.T(HayFiltros ? "error.033" : "doc.sinDocumentos");
    }
    protected void gvDocumentos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        e.Row.FindControl("btnDescargar").Visible = PuedeDescargar;
        e.Row.FindControl("btnVerificar").Visible = PuedeVerificar;
        e.Row.FindControl("btnEliminar").Visible = PuedeEliminar;
    }
    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        pnlVerificacion.Visible = false;
        CargarGrilla();
    }
    protected void btnSubir_Click(object sender, EventArgs e)
    {
        pnlVerificacion.Visible = false;
        if (!TienePermiso("SubirDocumento"))
        {
            MostrarError(Traductor.T("error.056"));
            return;
        }
        if (!fuArchivo.HasFile)
        {
            MostrarError(Traductor.T("error.010"));
            return;
        }
        byte[] Contenido;
        using (MemoryStream Memoria = new MemoryStream())
        {
            fuArchivo.PostedFile.InputStream.CopyTo(Memoria);
            Contenido = Memoria.ToArray();
        }
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        DocumentoBE Documento;
        ResultadoDocumento Resultado = DocumentoBll.Subir(UsuarioActual, fuArchivo.FileName, Contenido, out Documento);
        if (Resultado == ResultadoDocumento.Exitoso)
        {
            MostrarMensaje(Traductor.T("doc.subidaExitosa", Documento.Nombre));
        }
        else
        {
            MostrarError(MensajeResultado(Resultado));
        }
        CargarTipos();
        CargarGrilla();
    }
    protected void gvDocumentos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        pnlVerificacion.Visible = false;
        if (e.CommandName == "Descargar")
        {
            if (!PuedeDescargar) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            byte[] Contenido;
            ResultadoDocumento Resultado = DocumentoBll.Descargar(UsuarioActual, Id, out Documento, out Contenido);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                EnviarArchivo(Documento.Nombre, Contenido);
                return;
            }
            MostrarError(MensajeResultado(Resultado));
            CargarGrilla();
        }
        else if (e.CommandName == "Verificar")
        {
            if (!PuedeVerificar) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            VerificacionDocumentoBE Verificacion;
            ResultadoDocumento Resultado = DocumentoBll.VerificarIntegridad(UsuarioActual, Id, out Documento, out Verificacion);
            if (Verificacion != null)
            {
                MostrarVerificacion(Documento, Verificacion);
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
        }
        else if (e.CommandName == "Eliminar")
        {
            if (!PuedeEliminar) { MostrarError(Traductor.T("error.056")); return; }
            ResultadoDocumento Resultado = DocumentoBll.Eliminar(UsuarioActual, Id);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                MostrarMensaje(Traductor.T("doc.eliminacionExitosa"));
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
            CargarTipos();
            CargarGrilla();
        }
    }
    private void MostrarVerificacion(DocumentoBE documento, VerificacionDocumentoBE verificacion)
    {
        pnlVerificacion.Visible = true;
        litVerificacionNombre.Text = documento.Nombre;
        // 8. Íntegro, o 7.1 modificado [ERROR-018]
        lblVerificacionResultado.Text = HttpUtility.HtmlEncode(Traductor.T(verificacion.Integro ? "doc.integro" : "error.018"));
        lblVerificacionResultado.CssClass = "alert d-block " + (verificacion.Integro ? "alert-success" : "alert-danger");
        litHashRegistrado.Text = verificacion.Sello.Hash;
        litHashActual.Text = verificacion.HashActual;
        litFechaSellado.Text = verificacion.Sello.FechaSellado.ToString("dd/MM/yyyy HH:mm:ss");
        litTransaccion.Text = verificacion.Sello.Transaccion;
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        gvHistorial.DataSource = DocumentoBll.Historial(documento.Id);
        gvHistorial.DataBind();
    }
    /// Manda el archivo al navegador como descarga. El nombre se codifica
    /// para que acentos o espacios no rompan el encabezado.
    private void EnviarArchivo(string nombre, byte[] contenido)
    {
        Response.Clear();
        Response.ContentType = MimeMapping.GetMimeMapping(nombre);
        Response.AddHeader("Content-Disposition", "attachment; filename*=UTF-8''" + Uri.EscapeDataString(nombre));
        Response.AddHeader("Content-Length", contenido.Length.ToString());
        Response.BinaryWrite(contenido);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    private string MensajeResultado(ResultadoDocumento resultado)
    {
        switch (resultado)
        {
            case ResultadoDocumento.ArchivoInvalido: return Traductor.T("error.010");
            case ResultadoDocumento.ErrorAlmacenamiento: return Traductor.T("error.011");
            case ResultadoDocumento.ErrorBlockchain: return Traductor.T("error.012");
            case ResultadoDocumento.YaEliminado: return Traductor.T("error.016");
            case ResultadoDocumento.ErrorObtener: return Traductor.T("error.017");
            case ResultadoDocumento.Modificado: return Traductor.T("error.018");
            case ResultadoDocumento.ErrorEliminar: return Traductor.T("error.006");
            default: return Traductor.T("error.056");
        }
    }
    private void MostrarMensaje(string mensaje)
    {
        lblError.Visible = false;
        lblMensaje.Text = HttpUtility.HtmlEncode(mensaje);
        lblMensaje.Visible = true;
    }
    private void MostrarError(string mensaje)
    {
        lblMensaje.Visible = false;
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
