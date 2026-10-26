using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;

public partial class Permisos : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerMenuAdmin"; } }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvPermisos.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvPermisos.Columns[1].HeaderText = Traductor.T("comun.descripcion");
        gvPermisos.Columns[2].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnAgregar.Text = Traductor.T("permisos.agregar");
        btnGuardar.Text = Traductor.T("comun.guardar");
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }
    private void CargarGrilla()
    {
        PermisoBLL PermisoBll = new PermisoBLL();
        gvPermisos.DataSource = PermisoBll.Listar();
        gvPermisos.DataBind();
    }
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        hdnId.Value = "0";
        txtNombre.Text = "";
        txtDescripcion.Text = "";
        litTituloModal.Text = Traductor.T("permisos.tituloAgregar");
        lblErrorModal.Visible = false;
        AbrirModal();
    }
    protected void gvPermisos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        PermisoBLL PermisoBll = new PermisoBLL();
        if (e.CommandName == "Editar")
        {
            PermisoBE Permiso = PermisoBll.ObtenerDatos(Id);
            hdnId.Value = Permiso.Id.ToString();
            txtNombre.Text = Permiso.Nombre;
            txtDescripcion.Text = Permiso.Descripcion;
            litTituloModal.Text = Traductor.T("permisos.tituloEditar");
            lblErrorModal.Visible = false;
            AbrirModal();
        }
        else if (e.CommandName == "Eliminar")
        {
            PermisoBll.Eliminar(Id);
            CargarGrilla();
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        int Id = Convert.ToInt32(hdnId.Value);
        PermisoBLL PermisoBll = new PermisoBLL();
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            lblErrorModal.Text = Traductor.T("comun.nombreObligatorio");
            lblErrorModal.Visible = true;
            AbrirModal();
            return;
        }
        if (Id == 0)
        {
            PermisoBE Nuevo = new PermisoBE { Nombre = txtNombre.Text, Descripcion = txtDescripcion.Text };
            PermisoBll.Registrar(Nuevo);
        }
        else
        {
            PermisoBE Permiso = new PermisoBE { Id = Id, Nombre = txtNombre.Text, Descripcion = txtDescripcion.Text };
            PermisoBll.ActualizarDatos(Permiso);
        }
        CargarGrilla();
        CerrarModal();
    }
    private void AbrirModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModal",
            "var m = new bootstrap.Modal(document.getElementById('modalPermiso')); m.show();", true);
    }
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalPermiso'));
          if (m) m.hide();
          document.body.classList.remove('modal-open');
          document.body.style.removeProperty('padding-right');
          document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}