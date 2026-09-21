using BE;
using BLL;
using DAL;
using SERVICE;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

/// Pantalla de administración de Usuarios: grilla con listado, y un modal
/// único que sirve tanto para alta como para edición (según si hdnId es 0
/// o tiene un Id cargado).
public partial class Usuarios : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerUsuarios"; } }
    protected void Page_Load(object sender, EventArgs e)
    {
        // Solo la primera carga (no en cada postback, para no perder
        // selecciones/estado del formulario innecesariamente).
        if (!IsPostBack)
        {
            CargarPerfiles();
            CargarGrilla();
        }
    }
    ///Trae todos los usuarios y los pinta en la grilla.
    private void CargarGrilla()
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        gvUsuarios.DataSource = UsuarioBll.Listar();
        gvUsuarios.DataBind();
    }

    ///Llena el combo de Perfiles del modal (Administrador, etc.).
    private void CargarPerfiles()
    {
        PerfilBLL PerfilBll = new PerfilBLL();
        ddlPerfil.DataSource = PerfilBll.Listar();
        ddlPerfil.DataBind();
    }
    ///Botón "+ Agregar Usuario": limpia el formulario y abre el modal en modo alta.
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
        litTituloModal.Text = "Agregar Usuario";
        AbrirModal();
    }
    /// Maneja los dos botones de acción de cada fila de la grilla:
    /// "Editar" (abre el modal cargado con los datos de ese usuario) y
    /// "ToggleEstado" (Suspende o Reactiva, según el estado actual).
    protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "Editar")
        {
            CargarFormulario(Id);
            litTituloModal.Text = "Editar Usuario";
            AbrirModal();
        }
        else if (e.CommandName == "ToggleEstado")
        {
            // Suspender/Reactivar reutiliza el mismo campo Suspendido:
            // si estaba activo, lo suspende (y guarda un motivo genérico);
            // si estaba suspendido, lo reactiva (y limpia el motivo).
            UsuarioBLL UsuarioBll = new UsuarioBLL();
            UsuarioBE Usuario = UsuarioBll.ObtenerDatos(Id);
            Usuario.Suspendido = !Usuario.Suspendido;
            Usuario.Motivo = Usuario.Suspendido ? "Suspendido desde panel de administración" : null;
            UsuarioBll.ActualizarEstado(Usuario);
            CargarGrilla();
        }
    }
    ///Trae un usuario existente y precarga sus datos en los campos del modal.
    private void CargarFormulario(int id)
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        UsuarioBE Usuario = UsuarioBll.ObtenerDatos(id);
        hdnId.Value = Usuario.Id.ToString();
        txtNombre.Text = Usuario.Nombre;
        txtApellido.Text = Usuario.Apellido;
        txtNombreUsuario.Text = Usuario.NombreUsuario;
        txtContrasena.Text = ""; // nunca se muestra el hash existente
        txtGmail.Text = Usuario.Gmail;
        txtDni.Text = Usuario.Dni;
        txtTelefono.Text = Usuario.Telefono;
        ddlPerfil.SelectedValue = Usuario.Perfil.Id.ToString();
    }
    ///Vacía todos los campos del modal, para dejarlo listo en modo alta.
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
    /// Botón "Guardar" del modal. hdnId = 0 indica alta; cualquier otro
    /// valor indica edición de ese Id puntual.
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        int Id = Convert.ToInt32(hdnId.Value);
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        if (Id == 0)
        {
            // --- ALTA ---
            // La contraseña es obligatoria al crear un usuario nuevo.
            HashService Hasheo = new HashService();
            if (UsuarioBll.ExistePorNombreUsuario(txtNombreUsuario.Text))
            {
                lblErrorModal.Text = "Ya existe un usuario con ese nombre de usuario.";
                lblErrorModal.Visible = true;
                AbrirModal();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                lblErrorModal.Text = "La contraseña es obligatoria para un usuario nuevo.";
                lblErrorModal.Visible = true;
                AbrirModal();
                return;
            }
            string ContrasenaHash = Hasheo.HashearContrasena(txtContrasena.Text);
            UsuarioBE NuevoUsuario = new UsuarioBE
            {
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                NombreUsuario = txtNombreUsuario.Text,
                Contrasena = ContrasenaHash, // texto plano: se hashea dentro de UsuarioBll.Registrar()
                Gmail = txtGmail.Text,
                Dni = txtDni.Text,
                Telefono = txtTelefono.Text,
                Perfil = new PerfilBE { Id = Convert.ToInt32(ddlPerfil.SelectedValue) },
                Local = true
            };
            UsuarioBll.Registrar(NuevoUsuario);
        }
        else
        {
            // --- EDICIÓN ---
            // Se parte del usuario existente (para no perder campos que
            // este formulario no edita) y se pisan solo los que sí cambian.
            UsuarioBE Usuario = UsuarioBll.ObtenerDatos(Id);
            Usuario.Nombre = txtNombre.Text;
            Usuario.Apellido = txtApellido.Text;
            Usuario.Gmail = txtGmail.Text;
            Usuario.Dni = txtDni.Text;
            Usuario.Telefono = txtTelefono.Text;
            Usuario.Perfil = new PerfilBE { Id = Convert.ToInt32(ddlPerfil.SelectedValue) };
            // La contraseña solo se toca si el admin escribió una nueva;
            // si el campo quedó vacío, se conserva el hash actual.
            if (!string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                HashService HashServ = new HashService();
                Usuario.Contrasena = HashServ.HashearContrasena(txtContrasena.Text);
            }
            UsuarioBll.ActualizarDatos(Usuario);
        }
        CargarGrilla();
        CerrarModal();
    }
    ///Le pide al navegador (vía JS) que abra el modal de Bootstrap.
    private void AbrirModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModal",
            "var m = new bootstrap.Modal(document.getElementById('modalUsuario')); m.show();", true);
    }
    ///Le pide al navegador (vía JS) que cierre el modal de Bootstrap.
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalUsuario'));
          if (m) m.hide();
          document.body.classList.remove('modal-open');
          document.body.style.removeProperty('padding-right');
          document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}