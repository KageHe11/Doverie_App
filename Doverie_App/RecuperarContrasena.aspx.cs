using System;
using BLL;

public partial class RecuperarContrasena : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btnEnviar.Text = Traductor.T("recuperar.enviar");
    }
    protected void btnEnviar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            Mensaje.Error(Traductor.T("comun.completarAmbos"));
            return;
        }
        string UrlBase = Request.Url.GetLeftPart(UriPartial.Authority);
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        BE.ResultadoRecuperacion Resultado = UsuarioBll.EnviarRecuperacion(txtEmail.Text, txtNombreUsuario.Text, UrlBase);
        // Mostramos el mismo mensaje de éxito exista o no la cuenta, por
        // seguridad (no confirmamos ni negamos si el usuario/email existe).
        pnlFormulario.Visible = false;
        Mensaje.Exito(Traductor.T("recuperar.exito"));
    }
}