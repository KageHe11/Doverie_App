using System;
using BE;
using BLL;

public partial class Registro : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // si ya tiene sesión activa, no tiene sentido que se registre de nuevo.
        if (!IsPostBack && Session["Usuario"] != null)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
    protected void btnRegistrar_Click(object sender, EventArgs e)
    {
        if (txtContrasena.Text != txtConfirmar.Text)
        {
            MostrarError("Las contraseñas no coinciden.");
            return;
        }
        bool EsEmpresa = rblTipo.SelectedValue == "Empresa";
        UsuarioBE NuevoUsuario = new UsuarioBE
        {
            Nombre = txtNombre.Text,
            Apellido = txtApellido.Text,
            NombreUsuario = txtNombreUsuario.Text,
            Contrasena = txtContrasena.Text,
            Gmail = txtGmail.Text,
            Dni = txtDni.Text,
            Telefono = txtTelefono.Text,
            Empleado = false,
            Empresa = EsEmpresa,
            Independiente = !EsEmpresa,
            Local = false
        };
        string NombrePerfil = EsEmpresa ? "Empresa" : "Freelance";
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        ResultadoRegistro Resultado = UsuarioBll.RegistrarPublico(NuevoUsuario, NombrePerfil);
        switch (Resultado)
        {
            case ResultadoRegistro.Exitoso:
                pnlFormulario.Visible = false;
                lblExito.Text = "¡Registro exitoso! Ya podés iniciar sesión.";
                lblExito.Visible = true;
                break;
            case ResultadoRegistro.DatosInvalidos:
                MostrarError("Completá todos los campos obligatorios.");
                break;
            case ResultadoRegistro.YaRegistrado:
                MostrarError("Ya existe una cuenta con ese nombre de usuario o email.");
                break;
        }
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.Visible = true;
    }
}