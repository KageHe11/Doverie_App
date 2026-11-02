using System;
using BE;
using BLL;

public partial class MiPerfil : PaginaBase
{
    private UsuarioBE UsuarioLogueado
    {
        get { return Session["Usuario"] as UsuarioBE; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        // Los Usuario Empresa (empleados) no gestionan sus propios datos,
        // los gestiona la Empresa — no forman parte de los actores de este CU.
        if (UsuarioLogueado.Empleado)
        {
            Response.Redirect("~/AccesoDenegado.aspx");
            return;
        }
        btnGuardar.Text = Traductor.T("miPerfil.guardarCambios");
        if (!IsPostBack)
        {
            txtNombreUsuario.Text = UsuarioLogueado.NombreUsuario;
            txtNombre.Text = UsuarioLogueado.Nombre;
            txtApellido.Text = UsuarioLogueado.Apellido;
            txtGmail.Text = UsuarioLogueado.Gmail;
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        ResultadoModificarPerfil Resultado = UsuarioBll.ModificarDatosPropios(
            UsuarioLogueado.Id, txtNombre.Text, txtApellido.Text, txtGmail.Text, txtContrasena.Text);
        switch (Resultado)
        {
            case ResultadoModificarPerfil.Exitoso:
                // Actualizamos la Session para que el navbar (saludo, etc.)
                // refleje el cambio sin necesitar volver a loguearse.
                UsuarioBE Actualizado = UsuarioLogueado;
                Actualizado.Nombre = txtNombre.Text;
                Actualizado.Apellido = txtApellido.Text;
                Actualizado.Gmail = txtGmail.Text;
                Session["Usuario"] = Actualizado;
                txtContrasena.Text = "";
                Mensaje.Exito(Traductor.T("miPerfil.exito"));
                break;
            case ResultadoModificarPerfil.DatosInvalidos:
                MostrarError(Traductor.T("miPerfil.errorDatos"));
                break;
            case ResultadoModificarPerfil.EmailYaRegistrado:
                MostrarError(Traductor.T("miPerfil.errorEmail"));
                break;
            case ResultadoModificarPerfil.ContrasenaCorta:
                MostrarError(Traductor.T("error.contrasenaCorta", UsuarioBLL.LargoMinimoContrasena));
                break;
        }
    }
    private void MostrarError(string mensaje)
    {
        Mensaje.Error(mensaje);
    }
}