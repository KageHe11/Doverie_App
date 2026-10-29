using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;
using SERVICE;

public partial class EmpleadosEmpresa : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerEmpleadosPropios"; } }
    private UsuarioBE UsuarioLogueado
    {
        get { return Session["Usuario"] as UsuarioBE; }
    }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvEmpleados.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvEmpleados.Columns[1].HeaderText = Traductor.T("comun.apellido");
        gvEmpleados.Columns[2].HeaderText = Traductor.T("comun.usuario");
        gvEmpleados.Columns[3].HeaderText = Traductor.T("comun.email");
        gvEmpleados.Columns[4].HeaderText = Traductor.T("comun.estado");
        gvEmpleados.Columns[5].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnAgregar.Text = Traductor.T("empleados.agregar");
        btnGuardar.Text = Traductor.T("comun.guardar");
        if (!IsPostBack)
        {
            CargarPerfiles();
            CargarGrilla();
        }
    }
    private void CargarPerfiles()
    {
        PerfilBLL PerfilBll = new PerfilBLL();
        ddlPerfil.DataSource = PerfilBll.ListarAsignablesPorEmpresa();
        ddlPerfil.DataBind();
    }
    private void CargarGrilla()
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        gvEmpleados.DataSource = UsuarioBll.ListarPorEmpresa(UsuarioLogueado.Id);
        gvEmpleados.DataBind();
    }
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
        litTituloModal.Text = Traductor.T("empleados.tituloAgregar");
        AbrirModal();
    }
    protected void gvEmpleados_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        UsuarioBE Empleado = UsuarioBll.ObtenerDatos(Id);
        // Chequeo de seguridad: el empleado tiene que pertenecer a ESTA
        // Empresa. Sin esto, alguien podría forzar el CommandArgument
        // (por ejemplo, con las herramientas de desarrollador del
        // navegador) y editar empleados de otra Empresa.
        if (Empleado == null || Empleado.EmpresaId != UsuarioLogueado.Id)
        {
            Response.Redirect("~/AccesoDenegado.aspx");
            return;
        }
        if (e.CommandName == "Editar")
        {
            hdnId.Value = Empleado.Id.ToString();
            txtNombre.Text = Empleado.Nombre;
            txtApellido.Text = Empleado.Apellido;
            txtNombreUsuario.Text = Empleado.NombreUsuario;
            txtContrasena.Text = "";
            txtGmail.Text = Empleado.Gmail;
            ddlPerfil.SelectedValue = Empleado.Perfil.Id.ToString();
            txtDni.Text = Empleado.Dni;
            txtTelefono.Text = Empleado.Telefono;
            litTituloModal.Text = Traductor.T("empleados.tituloEditar");
            lblErrorModal.Visible = false;
            AbrirModal();
        }
        else if (e.CommandName == "Eliminar")
        {
            // CU-001-016: baja permanente del empleado (la confirmación se
            // pide en el navegador) [ERROR-037].
            ResultadoEstadoUsuario Resultado = UsuarioBll.RevocarAccesoEmpleado(Empleado.Id, UsuarioLogueado.Id);
            if (Resultado == ResultadoEstadoUsuario.Exitoso)
            {
                lblMensaje.Text = System.Web.HttpUtility.HtmlEncode(Traductor.T("empleados.eliminado", Empleado.NombreUsuario));
                lblMensaje.Visible = true;
            }
            else
            {
                lblError.Text = System.Web.HttpUtility.HtmlEncode(Traductor.T("error.037"));
                lblError.Visible = true;
            }
            CargarGrilla();
        }
    }
    private void LimpiarFormulario()
    {
        hdnId.Value = "0";
        txtNombre.Text = "";
        txtApellido.Text = "";
        txtNombreUsuario.Text = "";
        txtContrasena.Text = "";
        txtGmail.Text = "";
        txtDni.Text = "";
        txtTelefono.Text = "";
        lblErrorModal.Visible = false;
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        int Id = Convert.ToInt32(hdnId.Value);
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        ResultadoRegistro Resultado;
        if (Id == 0)
        {
            if (string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                lblErrorModal.Text = Traductor.T("empleados.errorContrasenaObligatoria");
                lblErrorModal.Visible = true;
                AbrirModal();
                return;
            }
            UsuarioBE NuevoEmpleado = new UsuarioBE
            {
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                NombreUsuario = txtNombreUsuario.Text,
                Contrasena = txtContrasena.Text,
                Gmail = txtGmail.Text,
                Dni = txtDni.Text,
                Telefono = txtTelefono.Text,
                Empleado = true,
                Empresa = false,
                Independiente = false,
                Local = false,
                Perfil = new PerfilBE { Id = Convert.ToInt32(ddlPerfil.SelectedValue) },
                EmpresaId = UsuarioLogueado.Id
            };
            // CU-001-010: validación de datos [ERROR-004] y duplicados [ERROR-005].
            Resultado = UsuarioBll.RegistrarDesdeGestion(NuevoEmpleado, UsuarioLogueado.Id, "Alta Empleado Empresa");
        }
        else
        {
            UsuarioBE Empleado = UsuarioBll.ObtenerDatos(Id);
            if (Empleado == null || Empleado.EmpresaId != UsuarioLogueado.Id)
            {
                Response.Redirect("~/AccesoDenegado.aspx");
                return;
            }
            bool CambioPermisos = Empleado.Perfil.Id != Convert.ToInt32(ddlPerfil.SelectedValue);
            Empleado.Nombre = txtNombre.Text;
            Empleado.Apellido = txtApellido.Text;
            Empleado.Gmail = txtGmail.Text;
            Empleado.Dni = txtDni.Text;
            Empleado.Telefono = txtTelefono.Text;
            Empleado.Perfil = new PerfilBE { Id = Convert.ToInt32(ddlPerfil.SelectedValue) };
            Resultado = UsuarioBll.ModificarDesdeGestion(Empleado, txtContrasena.Text, UsuarioLogueado.Id, "Modificar Empleado Empresa");
            if (Resultado == ResultadoRegistro.Exitoso && CambioPermisos)
            {
                new BitacoraBLL().Registrar("Modificar Permisos Usuario Empresa", "Empleado Id: " + Empleado.Id, UsuarioLogueado.Id);
                // Se avisa al empleado de su nuevo perfil de permisos.
                GestorEventos.Notificar(CatalogoEventos.PermisosModificados, Empleado.Id, ddlPerfil.SelectedItem.Text);
            }
        }
        if (Resultado != ResultadoRegistro.Exitoso)
        {
            lblErrorModal.Text = Traductor.T(Resultado == ResultadoRegistro.DatosInvalidos ? "error.004" : Id == 0 ? "error.005" : "error.021");
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
            "var m = new bootstrap.Modal(document.getElementById('modalEmpleado')); m.show();", true);
    }
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalEmpleado'));
              if (m) m.hide();
              document.body.classList.remove('modal-open');
              document.body.style.removeProperty('padding-right');
              document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}