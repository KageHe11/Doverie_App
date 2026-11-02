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
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvUsuarios.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvUsuarios.Columns[1].HeaderText = Traductor.T("comun.apellido");
        gvUsuarios.Columns[2].HeaderText = Traductor.T("comun.usuario");
        gvUsuarios.Columns[3].HeaderText = Traductor.T("comun.email");
        gvUsuarios.Columns[4].HeaderText = Traductor.T("comun.estado");
        gvUsuarios.Columns[5].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnAgregar.Text = Traductor.T("usuarios.agregar");
        btnGuardar.Text = Traductor.T("comun.guardar");
        btnConfirmarSuspension.Text = Traductor.T("usuarios.suspender");
        // Solo la primera carga (no en cada postback, para no perder
        // selecciones/estado del formulario innecesariamente).
        if (!IsPostBack)
        {
            CargarPerfiles();
            CargarGrilla();
        }
    }
    ///Trae todos los usuarios y los pinta en la grilla.
    /// La búsqueda o la página del Paginador cambiaron.
    protected void Paginador_Cambio(object sender, EventArgs e)
    {
        CargarGrilla();
    }
    private void CargarGrilla()
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        gvUsuarios.DataSource = Paginador.Paginar(UsuarioBll.Listar(), U => U.Nombre + " " + U.Apellido + " " + U.NombreUsuario + " " + U.Gmail);
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
        litTituloModal.Text = Traductor.T("usuarios.tituloAgregar");
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
            litTituloModal.Text = Traductor.T("usuarios.tituloEditar");
            AbrirModal();
        }
        else if (e.CommandName == "ToggleEstado")
        {
            UsuarioBLL UsuarioBll = new UsuarioBLL();
            UsuarioBE Usuario = UsuarioBll.ObtenerDatos(Id);
            if (Usuario == null) { CargarGrilla(); return; }
            if (!Usuario.Suspendido)
            {
                // CU-002-010 paso 5: se pide el motivo de la suspensión.
                hdnSuspenderId.Value = Usuario.Id.ToString();
                litSuspenderNombre.Text = Usuario.NombreUsuario;
                txtMotivo.Text = "";
                lblErrorSuspender.Visible = false;
                AbrirModalSuspender();
                return;
            }
            // CU-002-011 Reactivar (la confirmación se pide en el navegador).
            ResultadoEstadoUsuario Resultado = UsuarioBll.Reactivar(Id, UsuarioActual.Id);
            if (Resultado == ResultadoEstadoUsuario.Exitoso) MostrarMensaje(Traductor.T("usuarios.reactivado", Usuario.NombreUsuario));
            else MostrarError(Traductor.T(Resultado == ResultadoEstadoUsuario.NoSuspendido ? "error.039" : "error.038"));
            CargarGrilla();
        }
    }
    /// CU-002-010 pasos 6 y 7: motivo obligatorio [ERROR-004] y suspensión
    /// [ERROR-038].
    protected void btnConfirmarSuspension_Click(object sender, EventArgs e)
    {
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        ResultadoEstadoUsuario Resultado = UsuarioBll.Suspender(Convert.ToInt32(hdnSuspenderId.Value), txtMotivo.Text, UsuarioActual.Id);
        if (Resultado == ResultadoEstadoUsuario.MotivoInvalido)
        {
            lblErrorSuspender.Text = Traductor.T("error.004") + " " + Traductor.T("usuarios.motivoAyuda");
            lblErrorSuspender.Visible = true;
            AbrirModalSuspender();
            return;
        }
        if (Resultado == ResultadoEstadoUsuario.Exitoso) MostrarMensaje(Traductor.T("usuarios.suspendido", litSuspenderNombre.Text));
        else MostrarError(Traductor.T("error.038"));
        CargarGrilla();
        CerrarModal("modalSuspender");
    }
    private void MostrarMensaje(string mensaje)
    {
        lblMensaje.Text = System.Web.HttpUtility.HtmlEncode(mensaje);
        lblMensaje.Visible = true;
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = System.Web.HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
    private void AbrirModalSuspender()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModalSuspender",
            "var m = new bootstrap.Modal(document.getElementById('modalSuspender')); m.show();", true);
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
        ResultadoRegistro Resultado;
        if (Id == 0)
        {
            // --- ALTA ---
            // La contraseña es obligatoria al crear un usuario nuevo.
            if (string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                lblErrorModal.Text = Traductor.T("usuarios.errorContrasenaObligatoria");
                lblErrorModal.Visible = true;
                AbrirModal();
                return;
            }
            UsuarioBE NuevoUsuario = new UsuarioBE
            {
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                NombreUsuario = txtNombreUsuario.Text,
                Contrasena = txtContrasena.Text, // texto plano: se hashea dentro de UsuarioBll.Registrar()
                Gmail = txtGmail.Text,
                Dni = txtDni.Text,
                Telefono = txtTelefono.Text,
                Perfil = new PerfilBE { Id = Convert.ToInt32(ddlPerfil.SelectedValue) },
                Local = true
            };
            Resultado = UsuarioBll.RegistrarDesdeGestion(NuevoUsuario, UsuarioActual.Id, "Alta Usuario");
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
            Resultado = UsuarioBll.ModificarDesdeGestion(Usuario, txtContrasena.Text, UsuarioActual.Id, "Modificar Usuario");
        }
        if (Resultado != ResultadoRegistro.Exitoso)
        {
            // [ERROR-004] datos o formato; ya registrado: [ERROR-005] en el
            // alta, [ERROR-021] email de otro usuario en la edición.
            lblErrorModal.Text = Traductor.T(Resultado == ResultadoRegistro.DatosInvalidos ? "error.004" : Id == 0 ? "error.005" : "error.021");
            lblErrorModal.Visible = true;
            AbrirModal();
            return;
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
    private void CerrarModal(string idModal = "modalUsuario")
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('" + idModal + @"'));
          if (m) m.hide();
          document.body.classList.remove('modal-open');
          document.body.style.removeProperty('padding-right');
          document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}