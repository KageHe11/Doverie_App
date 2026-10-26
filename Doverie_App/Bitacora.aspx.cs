using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BE;
using BLL;

public partial class Bitacora : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerBitacora"; } }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvBitacora.Columns[0].HeaderText = Traductor.T("bitacora.fecha");
        gvBitacora.Columns[1].HeaderText = Traductor.T("bitacora.evento");
        gvBitacora.Columns[2].HeaderText = Traductor.T("comun.usuario");
        gvBitacora.Columns[3].HeaderText = Traductor.T("bitacora.detalle");
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
    /// Trae la lista base (30 recientes, o el historial completo si ya
    /// se tocó "Cargar más") y le aplica los filtros vigentes.
    private void CargarGrilla()
    {
        BitacoraBLL BitacoraBll = new BitacoraBLL();
        bool HistorialCompleto = hdnHistorialCompleto.Value == "1";
        List<BitacoraBE> Base = HistorialCompleto
            ? BitacoraBll.ObtenerHistorialCompleto()
            : BitacoraBll.ObtenerUltimos30();
        int? UsuarioId = string.IsNullOrEmpty(ddlUsuario.SelectedValue) ? (int?)null : Convert.ToInt32(ddlUsuario.SelectedValue);
        string Accion = string.IsNullOrEmpty(ddlAccion.SelectedValue) ? null : ddlAccion.SelectedValue;
        DateTime? Desde = string.IsNullOrEmpty(txtDesde.Text) ? (DateTime?)null : Convert.ToDateTime(txtDesde.Text);
        DateTime? Hasta = string.IsNullOrEmpty(txtHasta.Text) ? (DateTime?)null : Convert.ToDateTime(txtHasta.Text).AddDays(1).AddSeconds(-1);
        List<BitacoraBE> Filtrado = BitacoraBll.Filtrar(Base, UsuarioId, Accion, Desde, Hasta);
        gvBitacora.DataSource = Filtrado;
        gvBitacora.DataBind();
        lblSinResultados.Visible = Filtrado.Count == 0;
        // Una vez cargado todo el historial, no tiene sentido seguir
        // mostrando el botón (ya no hay "más" para traer).
        btnCargarMas.Visible = !HistorialCompleto;
    }
    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        CargarGrilla();
    }
    protected void btnCargarMas_Click(object sender, EventArgs e)
    {
        hdnHistorialCompleto.Value = "1";
        CargarGrilla();
    }
}