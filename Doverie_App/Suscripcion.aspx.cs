using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// CU-001-017 Suscribirse y CU-001-008 Gestionar Suscripción. Sin
/// suscripción vigente se muestran los planes; con una, el plan actual y
/// las opciones de cambiarlo o cancelarlo. El pago se hace en el checkout de
/// la pasarela, que vuelve a esta página con ?pago=Id.
public partial class Suscripcion : PaginaBase
{
    protected override string PermisoRequerido { get { return "Suscribirse"; } }

    /// Modalidad de pago elegida para mostrar los precios de los planes.
    protected string ModalidadElegida
    {
        get { return (ViewState["Modalidad"] as string) ?? "Mensual"; }
        set { ViewState["Modalidad"] = value; }
    }
    private bool PuedeGestionar
    {
        get { return TienePermiso("GestionarSuscripcion"); }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        lnkFacturacion.Text = Traductor.T("fact.titulo");
        lnkFacturacion.Visible = TienePermiso("VerFacturacion");
        btnCambiarPlan.Text = Traductor.T("susc.cambiarPlan");
        btnCancelar.Text = Traductor.T("susc.cancelar");
        btnCancelar.OnClientClick = Traductor.Confirmar("susc.confirmarCancelar");
        btnPagar.Text = Traductor.T("susc.irAPagar");
        btnVolverPlanes.Text = Traductor.T("susc.volverPlanes");
        btnVolverActual.Text = Traductor.T("susc.volverActual");
        if (!IsPostBack)
        {
            int PagoId;
            if (int.TryParse(Request.QueryString["pago"], out PagoId)) ProcesarRetorno(PagoId);
            MostrarEstado();
        }
    }
    /// Arma la pantalla según tenga o no una suscripción vigente.
    private void MostrarEstado()
    {
        pnlPago.Visible = false;
        SuscripcionBE Vigente = new SuscripcionBLL().ObtenerVigente(UsuarioActual);
        if (Vigente == null)
        {
            pnlActual.Visible = false;
            MostrarPlanes(null);
            litPlanesAyuda.Text = Traductor.T("susc.elegirPlan");
            btnVolverActual.Visible = false;
            return;
        }
        pnlPlanes.Visible = false;
        MostrarActual(Vigente);
    }
    private void MostrarActual(SuscripcionBE suscripcion)
    {
        SuscripcionBLL SuscripcionBll = new SuscripcionBLL();
        PlanBE Plan = suscripcion.Plan;
        bool Cancelada = suscripcion.Estado == SuscripcionBE.EstadoCancelada;
        pnlActual.Visible = true;
        litActualNivel.Text = Traductor.T("plan.nivel." + Plan.Nombre);
        litActualModalidad.Text = Traductor.T("plan.tipo." + Plan.Tipo);
        lblActualEstado.Text = Traductor.T(Cancelada ? "susc.estado.Cancelada" : "susc.estado.Activa");
        lblActualEstado.CssClass = "badge " + (Cancelada ? "bg-warning text-dark" : "bg-success");
        litActualPrecio.Text = DetallePrecio(Plan);
        litActualInicio.Text = suscripcion.FechaInicio.ToString("dd/MM/yyyy");
        litActualFechaEtiqueta.Text = Traductor.T(Cancelada ? "susc.accesoHasta" : "susc.proximaRenovacion");
        litActualFecha.Text = suscripcion.FechaRenovacion.ToString("dd/MM/yyyy");
        MedioPagoBE Medio = SuscripcionBll.ObtenerMedioRegistrado(UsuarioActual);
        litActualMedio.Text = Medio == null ? "-" : Traductor.T("pago.medio." + Medio.Id);
        int Usuarios = 1 + (UsuarioActual.Empresa ? new UsuarioBLL().ListarPorEmpresa(UsuarioActual.Id).Count : 0);
        litActualUsuarios.Text = Plan.MaxUsuarios.HasValue
            ? Traductor.T("susc.usuariosDe", Usuarios, Plan.MaxUsuarios.Value)
            : Traductor.T("susc.usuariosSinLimite", Usuarios);
        litActualCaracteristicas.Text = Caracteristicas(Plan.Nombre);
        btnCambiarPlan.Visible = PuedeGestionar;
        btnCancelar.Visible = PuedeGestionar && !Cancelada;
    }
    /// Planes de la modalidad elegida. planActualId marca el plan que ya
    /// tiene (cambio de plan).
    private void MostrarPlanes(int? planActualId)
    {
        pnlPlanes.Visible = true;
        List<PlanBE> Planes = new SuscripcionBLL().ListarPlanes();
        rptModalidades.DataSource = Planes.GroupBy(P => P.Tipo).Select(G => new
        {
            Tipo = G.Key,
            Meses = G.First().Meses,
            Descuento = G.Max(P => SuscripcionBLL.Descuento(P, Planes))
        }).OrderBy(M => M.Meses).ToList();
        rptModalidades.DataBind();
        rptPlanes.DataSource = Planes.Where(P => P.Tipo == ModalidadElegida).Select(P => new
        {
            P.Id,
            Nivel = P.Nombre,
            PrecioMensual = SuscripcionBLL.FormatearMonto(P.PrecioMensual),
            DetallePrecio = P.Meses == 1
                ? Traductor.T("susc.cobroMensual")
                : Traductor.T("susc.cobroPeriodo", SuscripcionBLL.FormatearMonto(P.Total), P.Meses),
            Caracteristicas = Caracteristicas(P.Nombre),
            Destacado = P.Nombre == "Oro",
            EsActual = planActualId == P.Id
        }).ToList();
        rptPlanes.DataBind();
    }
    /// Lista HTML (ya codificada) de las características de un nivel.
    private static string Caracteristicas(string nivel)
    {
        StringBuilder Html = new StringBuilder();
        foreach (string Item in Traductor.T("plan.caracteristicas." + nivel).Split('|'))
        {
            Html.Append("<li>").Append(HttpUtility.HtmlEncode(Item)).Append("</li>");
        }
        return Html.ToString();
    }
    /// Precio por mes y lo que se cobra en cada período.
    private static string DetallePrecio(PlanBE plan)
    {
        string PorMes = Traductor.T("susc.porMes", SuscripcionBLL.FormatearMonto(plan.PrecioMensual));
        if (plan.Meses == 1) return PorMes;
        return PorMes + " · " + Traductor.T("susc.cobroPeriodo", SuscripcionBLL.FormatearMonto(plan.Total), plan.Meses);
    }
    protected void rptModalidades_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        ModalidadElegida = e.CommandArgument.ToString();
        SuscripcionBE Vigente = new SuscripcionBLL().ObtenerVigente(UsuarioActual);
        MostrarPlanes(Vigente == null ? (int?)null : Vigente.PlanId);
        if (Vigente != null) MostrarActual(Vigente);
    }
    protected void btnCambiarPlan_Click(object sender, EventArgs e)
    {
        if (!PuedeGestionar) { MostrarError(Traductor.T("error.056")); return; }
        SuscripcionBE Vigente = new SuscripcionBLL().ObtenerVigente(UsuarioActual);
        if (Vigente == null) { MostrarEstado(); return; }
        // CU-001-008 paso 3: se muestran los planes, empezando por la
        // modalidad que ya tiene.
        ModalidadElegida = Vigente.Plan.Tipo;
        MostrarActual(Vigente);
        MostrarPlanes(Vigente.PlanId);
        litPlanesAyuda.Text = Traductor.T("susc.cambiarPlanAyuda");
        btnVolverActual.Visible = true;
    }
    protected void btnVolverActual_Click(object sender, EventArgs e)
    {
        MostrarEstado();
    }
    /// CU-001-017 paso 3 / CU-001-008 paso 4: plan elegido, formulario de pago.
    protected void rptPlanes_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        int PlanId = Convert.ToInt32(e.CommandArgument);
        SuscripcionBLL SuscripcionBll = new SuscripcionBLL();
        PlanBE Plan = SuscripcionBll.ListarPlanes().FirstOrDefault(P => P.Id == PlanId);
        if (Plan == null) { MostrarError(Traductor.T("error.004")); MostrarEstado(); return; }
        SuscripcionBE Vigente = SuscripcionBll.ObtenerVigente(UsuarioActual);
        if (Vigente != null && !PuedeGestionar) { MostrarError(Traductor.T("error.056")); MostrarEstado(); return; }
        pnlActual.Visible = false;
        pnlPlanes.Visible = false;
        pnlPago.Visible = true;
        hdnPlanId.Value = Plan.Id.ToString();
        litPagoPlan.Text = Traductor.T("plan.nivel." + Plan.Nombre) + " - " + Traductor.T("plan.tipo." + Plan.Tipo);
        litPagoPrecio.Text = DetallePrecio(Plan);
        litPagoTotal.Text = SuscripcionBLL.FormatearMonto(Plan.Total);
        litPagoRenovacion.Text = Traductor.T("susc.renovacionAutoDetalle", DateTime.Now.AddMonths(Plan.Meses).ToString("dd/MM/yyyy"));
        if (Vigente != null)
        {
            MostrarInfo(Traductor.T("susc.cambioAviso", Traductor.T("plan.nivel." + Vigente.Plan.Nombre) + " - " + Traductor.T("plan.tipo." + Vigente.Plan.Tipo)));
        }
        ddlMedioPago.Items.Clear();
        foreach (MedioPagoBE Medio in SuscripcionBll.ListarMediosPago())
        {
            ddlMedioPago.Items.Add(new ListItem(Traductor.T("pago.medio." + Medio.Id), Medio.Id.ToString()));
        }
    }
    protected void btnVolverPlanes_Click(object sender, EventArgs e)
    {
        SuscripcionBE Vigente = new SuscripcionBLL().ObtenerVigente(UsuarioActual);
        if (Vigente == null)
        {
            MostrarEstado();
            return;
        }
        MostrarActual(Vigente);
        MostrarPlanes(Vigente.PlanId);
        litPlanesAyuda.Text = Traductor.T("susc.cambiarPlanAyuda");
        btnVolverActual.Visible = true;
        pnlPago.Visible = false;
    }
    /// CU-001-017 paso 6 / CU-001-008 paso 5: redirige al proceso de pago.
    protected void btnPagar_Click(object sender, EventArgs e)
    {
        int PlanId, MedioId;
        if (!int.TryParse(hdnPlanId.Value, out PlanId) || !int.TryParse(ddlMedioPago.SelectedValue, out MedioId))
        {
            MostrarError(Traductor.T("error.004"));
            MostrarEstado();
            return;
        }
        SuscripcionBLL SuscripcionBll = new SuscripcionBLL();
        if (SuscripcionBll.ObtenerVigente(UsuarioActual) != null && !PuedeGestionar)
        {
            MostrarError(Traductor.T("error.056"));
            MostrarEstado();
            return;
        }
        string UrlRetorno = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Suscripcion.aspx");
        string UrlCheckout;
        ResultadoSuscripcion Resultado = SuscripcionBll.IniciarPago(UsuarioActual, PlanId, MedioId, UrlRetorno, out UrlCheckout);
        if (Resultado == ResultadoSuscripcion.Exitoso)
        {
            Response.Redirect(UrlCheckout, false);
            Context.ApplicationInstance.CompleteRequest();
            return;
        }
        MostrarError(MensajeResultado(Resultado));
        MostrarEstado();
    }
    /// CU-001-017 pasos 7 a 10 / CU-001-008 pasos 7 a 9: vuelta del checkout.
    private void ProcesarRetorno(int pagoId)
    {
        PagoBE Pago;
        bool EmailEnviado;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().ConfirmarPago(UsuarioActual, pagoId, out Pago, out EmailEnviado);
        switch (Resultado)
        {
            case ResultadoSuscripcion.Exitoso:
                string Plan = Traductor.T("plan.nivel." + Pago.PlanNombre) + " - " + Traductor.T("plan.tipo." + Pago.PlanTipo);
                string Mensaje = Traductor.T(Pago.Concepto == PagoBE.ConceptoCambioPlan ? "susc.exitoCambio" : "susc.exitoAlta", Plan);
                // Solo se dice que salió el correo si salió en este request.
                if (EmailEnviado) Mensaje += " " + Traductor.T("susc.emailEnviado");
                MostrarMensaje(Mensaje);
                break;
            case ResultadoSuscripcion.PagoPendiente:
                MostrarInfo(Traductor.T("susc.pagoPendiente"));
                break;
            default:
                MostrarError(MensajeResultado(Resultado));
                break;
        }
    }
    /// CU-001-008 4.1: cancelar la suscripción.
    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        if (!PuedeGestionar) { MostrarError(Traductor.T("error.056")); MostrarEstado(); return; }
        DateTime? AccesoHasta;
        ResultadoSuscripcion Resultado = new SuscripcionBLL().Cancelar(UsuarioActual, out AccesoHasta);
        if (Resultado == ResultadoSuscripcion.Exitoso)
        {
            // 6.1 / 7.1: mantiene el acceso hasta el fin del período abonado.
            MostrarMensaje(Traductor.T("susc.canceladaExito", AccesoHasta.Value.ToString("dd/MM/yyyy")));
        }
        else
        {
            MostrarError(MensajeResultado(Resultado));
        }
        MostrarEstado();
    }
    private static string MensajeResultado(ResultadoSuscripcion resultado)
    {
        switch (resultado)
        {
            case ResultadoSuscripcion.ErrorPago: return Traductor.T("error.014");
            case ResultadoSuscripcion.ErrorCancelar: return Traductor.T("error.015");
            case ResultadoSuscripcion.DatosInvalidos: return Traductor.T("error.004");
            case ResultadoSuscripcion.MismoPlan: return Traductor.T("susc.mismoPlan");
            case ResultadoSuscripcion.LimiteUsuarios: return Traductor.T("susc.limiteBaja");
            case ResultadoSuscripcion.SinSuscripcion: return Traductor.T("susc.sinSuscripcion");
            case ResultadoSuscripcion.PagoPendiente: return Traductor.T("susc.pagoPendiente");
            default: return Traductor.T("error.056");
        }
    }
    private void MostrarMensaje(string mensaje)
    {
        lblMensaje.Text = HttpUtility.HtmlEncode(mensaje);
        lblMensaje.Visible = true;
    }
    private void MostrarInfo(string mensaje)
    {
        lblInfo.Text = HttpUtility.HtmlEncode(mensaje);
        lblInfo.Visible = true;
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
