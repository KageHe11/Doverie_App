using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// CU-001-014 Ver Detalle de Facturación: historial de pagos de la cuenta,
/// comprobante de cada pago aprobado y su descarga en PDF.
public partial class Facturacion : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerFacturacion"; } }

    protected void Page_Load(object sender, EventArgs e)
    {
        btnDescargarPdf.Text = Traductor.T("fact.descargarPdf");
        btnEnviarEmail.Text = Traductor.T("fact.enviarEmail");
        if (!IsPostBack)
        {
            CargarPlan();
            CargarPagos();
        }
    }
    private void CargarPlan()
    {
        SuscripcionBLL SuscripcionBll = new SuscripcionBLL();
        SuscripcionBE Vigente = SuscripcionBll.ObtenerVigente(UsuarioActual);
        pnlPlanVigente.Visible = Vigente != null;
        pnlSinPlan.Visible = Vigente == null;
        if (Vigente == null) return;
        bool Cancelada = Vigente.Estado == SuscripcionBE.EstadoCancelada;
        litPlan.Text = Traductor.T("plan.nivel." + Vigente.Plan.Nombre) + " - " + Traductor.T("plan.tipo." + Vigente.Plan.Tipo);
        litFechaEtiqueta.Text = Traductor.T(Cancelada ? "susc.accesoHasta" : "susc.proximaRenovacion");
        litFecha.Text = Vigente.FechaRenovacion.ToString("dd/MM/yyyy");
        MedioPagoBE Medio = SuscripcionBll.ObtenerMedioRegistrado(UsuarioActual);
        litMedio.Text = Medio == null ? "-" : Traductor.T("pago.medio." + Medio.Id);
    }
    private void CargarPagos()
    {
        List<PagoBE> Pagos;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().ListarPagos(UsuarioActual, out Pagos);
        gvPagos.DataSource = Pagos;
        gvPagos.DataBind();
        // 2.1 No existen pagos registrados [ERROR-034]
        if (Resultado == ResultadoSuscripcion.SinPagos)
        {
            lblInfo.Text = HttpUtility.HtmlEncode(Traductor.T("error.034"));
            lblInfo.Visible = true;
        }
        else if (Resultado != ResultadoSuscripcion.Exitoso)
        {
            MostrarError(Traductor.T("error.056"));
        }
    }
    protected void gvPagos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.Header) return;
        string[] Encabezados = { "fact.fecha", "fact.concepto", "fact.plan", "fact.medio", "fact.monto", "fact.estado", "" };
        for (int I = 0; I < Encabezados.Length && I < e.Row.Cells.Count; I++)
        {
            e.Row.Cells[I].Text = Encabezados[I] == "" ? "" : HttpUtility.HtmlEncode(Traductor.T(Encabezados[I]));
        }
    }
    protected static string ClaseEstado(string estado)
    {
        switch (estado)
        {
            case PagoBE.EstadoAprobado: return "bg-success";
            case PagoBE.EstadoRechazado: return "bg-danger";
            default: return "bg-secondary";
        }
    }
    /// CU-001-014 pasos 5 a 7: ver el comprobante de un pago.
    protected void gvPagos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Ver") return;
        int PagoId = Convert.ToInt32(e.CommandArgument);
        PagoBE Pago;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().ObtenerComprobante(UsuarioActual, PagoId, out Pago);
        if (Resultado != ResultadoSuscripcion.Exitoso)
        {
            // 6.1 No se puede obtener el comprobante [ERROR-035]
            pnlComprobante.Visible = false;
            MostrarError(Traductor.T("error.035"));
            return;
        }
        UsuarioBE Titular = new UsuarioBLL().ObtenerDatos(Pago.UsuarioId);
        pnlComprobante.Visible = true;
        hdnPagoId.Value = Pago.Id.ToString();
        litNumero.Text = SuscripcionBLL.NumeroComprobante(Pago);
        litCompFecha.Text = Pago.Fecha.ToString("dd/MM/yyyy HH:mm");
        litCompTitular.Text = (Titular.Nombre + " " + Titular.Apellido).Trim() + " (" + Titular.NombreUsuario + ")";
        litCompPlan.Text = Traductor.T("plan.nivel." + Pago.PlanNombre) + " - " + Traductor.T("plan.tipo." + Pago.PlanTipo);
        litCompConcepto.Text = Traductor.T("fact.concepto." + Pago.Concepto);
        litCompMedio.Text = Traductor.T("pago.medio." + Pago.MedioPagoId);
        litCompMonto.Text = SuscripcionBLL.FormatearMonto(Pago.Monto);
        litCompEstado.Text = Traductor.T("fact.estado." + Pago.Estado);
        litCompOperacion.Text = Pago.ComprobanteUrl;
    }
    protected void btnCerrarComprobante_Click(object sender, EventArgs e)
    {
        pnlComprobante.Visible = false;
    }
    /// CU-001-014 pasos 8 y 9: descargar el comprobante en PDF.
    protected void btnDescargarPdf_Click(object sender, EventArgs e)
    {
        int PagoId;
        if (!int.TryParse(hdnPagoId.Value, out PagoId))
        {
            MostrarError(Traductor.T("error.035"));
            return;
        }
        byte[] Pdf;
        string NombreArchivo;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().GenerarComprobantePdf(UsuarioActual, PagoId, Traductor.Contexto, out Pdf, out NombreArchivo);
        if (Resultado != ResultadoSuscripcion.Exitoso)
        {
            // 9.1 No se puede generar el PDF [ERROR-036]
            MostrarError(Traductor.T(Resultado == ResultadoSuscripcion.ErrorPdf ? "error.036" : "error.035"));
            return;
        }
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment; filename=" + NombreArchivo);
        Response.AddHeader("Content-Length", Pdf.Length.ToString());
        Response.BinaryWrite(Pdf);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    /// Reenvía el comprobante en PDF al email del titular.
    protected void btnEnviarEmail_Click(object sender, EventArgs e)
    {
        int PagoId;
        if (!int.TryParse(hdnPagoId.Value, out PagoId))
        {
            MostrarError(Traductor.T("error.035"));
            return;
        }
        string Destino;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().EnviarComprobantePorEmail(UsuarioActual, PagoId, out Destino);
        if (Resultado == ResultadoSuscripcion.Exitoso) Mensaje.Exito(Traductor.T("fact.emailEnviado", Destino));
        else MostrarError(Traductor.T(Resultado == ResultadoSuscripcion.ErrorEmail ? "fact.errorEmail" : "error.035"));
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
