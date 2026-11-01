using System;
using BE;
using BLL;
using SERVICE;

/// Checkout simulado de PasarelaPagoLocal: permite aprobar o rechazar el
/// pago para probar los dos caminos del CU (paso 7 y 7.1 [ERROR-014]).
/// Solo muestra pagos pendientes de la cuenta del Usuario logueado.
public partial class PagoSimulado : PaginaBase
{
    protected override string PermisoRequerido { get { return "Suscribirse"; } }

    private PasarelaPagoLocal Pasarela
    {
        get { return ProveedorServicios.PasarelaPago() as PasarelaPagoLocal; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        btnAprobar.Text = Traductor.T("pagoSim.aprobar");
        btnRechazar.Text = Traductor.T("pagoSim.rechazar");
        PagoBE Pago;
        OperacionPagoBE Cobro = ObtenerCobro(out Pago);
        if (Cobro == null)
        {
            pnlCobro.Visible = false;
            pnlSinCobro.Visible = true;
            return;
        }
        if (!IsPostBack)
        {
            litDescripcion.Text = Cobro.Descripcion;
            litMedio.Text = Traductor.T("pago.medio." + Pago.MedioPagoId);
            litMonto.Text = SuscripcionBLL.FormatearMonto(Cobro.Monto);
        }
    }
    /// El cobro pendiente de la referencia, si la pasarela es la local y el
    /// pago es de la cuenta del Usuario.
    private OperacionPagoBE ObtenerCobro(out PagoBE pago)
    {
        pago = null;
        int Referencia;
        if (Pasarela == null || !int.TryParse(Request.QueryString["ref"], out Referencia)) return null;
        pago = new SuscripcionBLL().ObtenerPagoPendiente(UsuarioActual, Referencia);
        if (pago == null) return null;
        OperacionPagoBE Cobro = Pasarela.ObtenerCobro(Referencia);
        return Cobro != null && Cobro.Estado == OperacionPagoBE.EstadoPendiente ? Cobro : null;
    }
    protected void btnAprobar_Click(object sender, EventArgs e)
    {
        Resolver(true);
    }
    protected void btnRechazar_Click(object sender, EventArgs e)
    {
        Resolver(false);
    }
    /// Resuelve el cobro y vuelve a Doverie, como hace la pasarela real.
    private void Resolver(bool aprobado)
    {
        PagoBE Pago;
        OperacionPagoBE Cobro = ObtenerCobro(out Pago);
        if (Cobro == null) return;
        Pasarela.Resolver(Cobro.Referencia, aprobado);
        Response.Redirect(Cobro.UrlRetorno, false);
        Context.ApplicationInstance.CompleteRequest();
    }
}
