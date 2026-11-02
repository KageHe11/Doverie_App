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
        rblTipo.Items[0].Text = Traductor.T("registro.usuarioIndividual");
        rblTipo.Items[1].Text = Traductor.T("registro.empresa");
        btnRegistrar.Text = Traductor.T("registro.boton");
    }
    protected void btnRegistrar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        if (txtContrasena.Text != txtConfirmar.Text)
        {
            MostrarError(Traductor.T("comun.contrasenasNoCoinciden"));
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
                Mensaje.Exito(Traductor.T("registro.exito"));
                break;
            case ResultadoRegistro.DatosInvalidos:
                MostrarError(Traductor.T("registro.errorDatos"));
                break;
            case ResultadoRegistro.YaRegistrado:
                MostrarError(Traductor.T("registro.errorYaRegistrado"));
                break;
            case ResultadoRegistro.ContrasenaCorta:
                MostrarError(Traductor.T("error.contrasenaCorta", UsuarioBLL.LargoMinimoContrasena));
                break;
        }
    }
    private void MostrarError(string mensaje)
    {
        Mensaje.Error(mensaje);
    }
}