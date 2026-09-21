using System;
using BLL;

public partial class RecuperarContrasena : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }
    protected void btnEnviar_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            lblError.Text = "Completá ambos campos.";
            lblError.Visible = true;
            return;
        }
        string UrlBase = Request.Url.GetLeftPart(UriPartial.Authority);
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        BE.ResultadoRecuperacion Resultado = UsuarioBll.EnviarRecuperacion(txtEmail.Text, txtNombreUsuario.Text, UrlBase);
        // Mostramos el mismo mensaje de éxito exista o no la cuenta, por
        // seguridad (no confirmamos ni negamos si el usuario/email existe).
        lblError.Visible = false;
        pnlFormulario.Visible = false;
        lblExito.Text = "Si los datos son correctos, vas a recibir un email con el link de recuperación en los próximos minutos.";
        lblExito.Visible = true;
    }
}