using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BE;
using BLL;

public partial class Bitacora : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerBitacora"; } }
    /// Eliminar y restaurar (baja lógica) requiere un permiso aparte.
    protected bool PuedeEliminar
    {
        get { return UsuarioBLL.TienePermiso(UsuarioActual, BitacoraBLL.PermisoEliminar); }
    }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvBitacora.Columns[0].HeaderText = Traductor.T("bitacora.fecha");
        gvBitacora.Columns[1].HeaderText = Traductor.T("bitacora.evento");
        gvBitacora.Columns[2].HeaderText = Traductor.T("comun.usuario");
        gvBitacora.Columns[3].HeaderText = Traductor.T("bitacora.detalle");
        gvBitacora.Columns[4].HeaderText = Traductor.T("comun.estado");
        gvBitacora.Columns[5].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        ddlUsuario.Items[0].Text = Traductor.T("comun.todos");
        ddlAccion.Items[0].Text = Traductor.T("comun.todos");
        btnFiltrar.Text = Traductor.T("bitacora.filtrar");
        btnCargarMas.Text = Traductor.T("bitacora.cargarMas");
        lblSinResultados.Text = Traductor.T("bitacora.sinResultados");
        if (!IsPostBack)
        {
            CargarFiltros();
            CargarGrilla();
        }
    }
    private void CargarFiltros()
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        foreach (UsuarioBE Usuario in UsuarioBll.Listar())
        {
            ddlUsuario.Items.Add(new ListItem(Usuario.NombreUsuario, Usuario.Id.ToString()));
        }
        BitacoraBLL BitacoraBll = new BitacoraBLL();
        foreach (string Accion in BitacoraBll.ListarAcciones())
        {
            ddlAccion.Items.Add(new ListItem(Accion, Accion));
        }
    }
    /// Trae la lista base (últimos 30 días, o el historial completo si ya
    /// se tocó "Cargar más") y le aplica los filtros vigentes.
    /// La búsqueda o la página del Paginador cambiaron.
    protected void Paginador_Cambio(object sender, EventArgs e)
    {
        CargarGrilla();
    }
    private void CargarGrilla()
    {
        BitacoraBLL BitacoraBll = new BitacoraBLL();
        bool HistorialCompleto = hdnHistorialCompleto.Value == "1";
        List<BitacoraBE> Base = HistorialCompleto
            ? BitacoraBll.ObtenerHistorialCompleto()
            : BitacoraBll.ObtenerRecientes();
        int? UsuarioId = string.IsNullOrEmpty(ddlUsuario.SelectedValue) ? (int?)null : Convert.ToInt32(ddlUsuario.SelectedValue);
        string Accion = string.IsNullOrEmpty(ddlAccion.SelectedValue) ? null : ddlAccion.SelectedValue;
        DateTime? Desde = string.IsNullOrEmpty(txtDesde.Text) ? (DateTime?)null : Convert.ToDateTime(txtDesde.Text);
        DateTime? Hasta = string.IsNullOrEmpty(txtHasta.Text) ? (DateTime?)null : Convert.ToDateTime(txtHasta.Text).AddDays(1).AddSeconds(-1);
        List<BitacoraBE> Filtrado = BitacoraBll.Filtrar(Base, UsuarioId, Accion, Desde, Hasta, chkEliminados.Checked);
        gvBitacora.DataSource = Paginador.Paginar(Filtrado, B => B.Accion + " " + B.Detalle + " " + B.NombreUsuario);
        gvBitacora.DataBind();
        lblSinResultados.Visible = Filtrado.Count == 0;
        // Una vez cargado todo el historial, no tiene sentido seguir
        // mostrando el botón (ya no hay "más" para traer).
        btnCargarMas.Visible = !HistorialCompleto;
    }
    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        Paginador.Reiniciar();
        CargarGrilla();
    }
    protected void gvBitacora_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Eliminar" && e.CommandName != "Restaurar") return;
        if (!PuedeEliminar)
        {
            Response.Redirect("~/AccesoDenegado.aspx");
            return;
        }
        int Id = Convert.ToInt32(e.CommandArgument);
        BitacoraBLL BitacoraBll = new BitacoraBLL();
        bool Hecho = e.CommandName == "Eliminar" ? BitacoraBll.EliminarLogico(Id, UsuarioActual) : BitacoraBll.Restaurar(Id, UsuarioActual);
        if (Hecho) Mensaje.Exito(Traductor.T(e.CommandName == "Eliminar" ? "bitacora.eliminadoOk" : "bitacora.restauradoOk", Id));
        else Mensaje.Error(Traductor.T("bitacora.noDisponible"));
        CargarGrilla();
    }
    /// Los registros eliminados se ven atenuados.
    protected void gvBitacora_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        if (((BitacoraBE)e.Row.DataItem).Eliminado) e.Row.CssClass = "text-muted opacity-75";
    }
    protected void btnCargarMas_Click(object sender, EventArgs e)
    {
        hdnHistorialCompleto.Value = "1";
        Paginador.Reiniciar();
        CargarGrilla();
    }
}