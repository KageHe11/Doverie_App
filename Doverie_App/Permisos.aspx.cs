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
            ResultadoAbm Resultado = PermisoBll.Eliminar(Id);
            if (Resultado == ResultadoAbm.EnUso)
            {
                // [ERROR-023] con el motivo: alguna Familia lo contiene.
                MostrarError(Traductor.T("error.023") + " " + Traductor.T("permisos.enUso"));
            }
            else if (Resultado != ResultadoAbm.Exitoso)
            {
                MostrarError(Traductor.T("error.023"));
            }
            CargarGrilla();
        }
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = System.Web.HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        int Id = Convert.ToInt32(hdnId.Value);
        PermisoBLL PermisoBll = new PermisoBLL();
        PermisoBE Permiso = new PermisoBE { Id = Id, Nombre = txtNombre.Text, Descripcion = txtDescripcion.Text };
        ResultadoAbm Resultado = Id == 0 ? PermisoBll.Registrar(Permiso) : PermisoBll.ActualizarDatos(Permiso);
        if (Resultado != ResultadoAbm.Exitoso)
        {
            // [ERROR-004] / [ERROR-022] / [ERROR-024]
            string Clave = Resultado == ResultadoAbm.DatosInvalidos ? "comun.nombreObligatorio"
                : Resultado == ResultadoAbm.Duplicado ? "error.022" : "error.024";
            lblErrorModal.Text = Traductor.T(Clave);
            lblErrorModal.Visible = true;
            AbrirModal();
            return;
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