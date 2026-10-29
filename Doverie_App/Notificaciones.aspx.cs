using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// Notificaciones: bandeja de lo recibido dentro de la plataforma y
/// CU-001-015 Configurar Notificaciones.
public partial class Notificaciones : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerNotificaciones"; } }

    private string CategoriaAnterior;

    protected void Page_Load(object sender, EventArgs e)
    {
        btnGuardar.Text = Traductor.T("notif.guardar");
        if (!IsPostBack)
        {
            CargarBandeja();
            CargarPreferencias();
        }
    }
    /// Muestra lo recibido y lo marca como leído: las nuevas quedan
    /// resaltadas en esta visita y la campana de la navbar vuelve a cero
    /// (la master page se carga después que esta página).
    private void CargarBandeja()
    {
        NotificacionBLL NotificacionBll = new NotificacionBLL();
        List<NotificacionRecibidaBE> Recibidas = NotificacionBll.ListarRecibidas(UsuarioActual.Id);
        rptRecibidas.DataSource = Recibidas;
        rptRecibidas.DataBind();
        pnlBandejaVacia.Visible = Recibidas.Count == 0;
        if (Recibidas.Any(R => !R.Leida)) NotificacionBll.MarcarLeidas(UsuarioActual.Id);
    }
    private void CargarPreferencias()
    {
        CategoriaAnterior = null;
        NotificacionBLL NotificacionBll = new NotificacionBLL();
        rptPreferencias.DataSource = NotificacionBll.ObtenerPreferencias(UsuarioActual.Id);
        rptPreferencias.DataBind();
    }
    protected void rptPreferencias_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
        PreferenciaNotificacionBE Preferencia = (PreferenciaNotificacionBE)e.Item.DataItem;
        // Encabezado de categoría al empezar cada grupo (paso 3 del CU).
        if (Preferencia.Categoria != CategoriaAnterior)
        {
            e.Item.FindControl("phCategoria").Visible = true;
            ((Literal)e.Item.FindControl("litCategoria")).Text = Traductor.T("notif.categoria." + Preferencia.Categoria);
            CategoriaAnterior = Preferencia.Categoria;
        }
        ((Literal)e.Item.FindControl("litEvento")).Text = Traductor.T("notif.evento." + Preferencia.Evento);
        // Los eventos de suscripción todavía no se disparan (no hay
        // suscripciones): se pueden configurar, pero se aclara.
        if (Preferencia.Categoria == CatalogoEventos.CategoriaSuscripcion)
        {
            Label Proximamente = (Label)e.Item.FindControl("lblProximamente");
            Proximamente.Text = Traductor.T("nav.proximamente");
            Proximamente.Visible = true;
        }
    }
    /// Los atributos que usa el script de la página (data-rol, data-canal)
    /// no se guardan en el ViewState: se ponen en ItemCreated, que corre en
    /// cada request (también cuando el Repeater se recrea en un postback).
    protected void rptPreferencias_ItemCreated(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
        PrepararCheck((CheckBox)e.Item.FindControl("chkHabilitado"), "habilitado", null);
        PrepararCheck((CheckBox)e.Item.FindControl("chkEmail"), "canal", CatalogoEventos.CanalEmail);
        PrepararCheck((CheckBox)e.Item.FindControl("chkPush"), "canal", CatalogoEventos.CanalPush);
    }
    private static void PrepararCheck(CheckBox check, string rol, string canal)
    {
        check.InputAttributes["class"] = "form-check-input";
        check.InputAttributes["data-rol"] = rol;
        if (canal != null) check.InputAttributes["data-canal"] = canal;
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        List<PreferenciaNotificacionBE> Preferencias = new List<PreferenciaNotificacionBE>();
        foreach (RepeaterItem Item in rptPreferencias.Items)
        {
            Preferencias.Add(new PreferenciaNotificacionBE
            {
                Evento = ((HiddenField)Item.FindControl("hdnEvento")).Value,
                Habilitado = ((CheckBox)Item.FindControl("chkHabilitado")).Checked,
                Email = ((CheckBox)Item.FindControl("chkEmail")).Checked,
                Push = ((CheckBox)Item.FindControl("chkPush")).Checked
            });
        }
        NotificacionBLL NotificacionBll = new NotificacionBLL();
        List<string> SinCanal;
        if (!NotificacionBll.GuardarPreferencias(UsuarioActual.Id, Preferencias, out SinCanal))
        {
            // 8.1 Evento habilitado sin canal [ERROR-004]
            lblError.Text = HttpUtility.HtmlEncode(Traductor.T("error.004") + " " +
                Traductor.T("notif.sinCanal", string.Join(", ", SinCanal.Select(Ev => Traductor.T("notif.evento." + Ev)))));
            lblError.Visible = true;
            CargarBandeja();
            return;
        }
        // 9. Mensaje de éxito con el resumen de lo configurado.
        pnlExito.Visible = true;
        litResumen.Text = ArmarResumen(NotificacionBll.ObtenerPreferencias(UsuarioActual.Id));
        CargarBandeja();
        CargarPreferencias();
    }
    private static string ArmarResumen(List<PreferenciaNotificacionBE> preferencias)
    {
        List<PreferenciaNotificacionBE> Habilitadas = preferencias.Where(P => P.Habilitado).ToList();
        if (Habilitadas.Count == 0)
        {
            return "<div class=\"mt-1\">" + HttpUtility.HtmlEncode(Traductor.T("notif.resumenNinguno")) + "</div>";
        }
        StringBuilder Html = new StringBuilder("<ul class=\"mb-0 mt-1 small\">");
        foreach (PreferenciaNotificacionBE P in Habilitadas)
        {
            List<string> Canales = new List<string>();
            if (P.Email) Canales.Add(Traductor.T("notif.canal.Email"));
            if (P.Push) Canales.Add(Traductor.T("notif.canal.Push"));
            Html.Append("<li>").Append(HttpUtility.HtmlEncode(Traductor.T("notif.evento." + P.Evento) + ": " + string.Join(" + ", Canales))).Append("</li>");
        }
        return Html.Append("</ul>").ToString();
    }
}
