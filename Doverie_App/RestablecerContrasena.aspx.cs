using System;
using BE;
using BLL;

public partial class RestablecerContrasena : System.Web.UI.Page
{
    private string Token
    {
        get { return Request.QueryString["token"]; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            MostrarError("El link de recuperación no es válido.");
        }
    }
    protected void btnRestablecer_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtContrasena.Text) || string.IsNullOrWhiteSpace(txtConfirmar.Text))
        {
            lblError.Text = "Completá ambos campos.";
            lblError.Visible = true;
            return;
        }
        UsuarioBLL UsuarioBll = new UsuarioBLL();
        ResultadoRestablecimiento Resultado = UsuarioBll.RestablecerContrasena(Token, txtContrasena.Text, txtConfirmar.Text);
        switch (Resultado)
        {
            case ResultadoRestablecimiento.Exitoso:
                Response.Redirect("~/Login.aspx?restablecido=1");
                break;
            case ResultadoRestablecimiento.ContrasenasNoCoinciden:
                lblError.Text = "Las contraseñas no coinciden.";
                lblError.Visible = true;
                break;
            case ResultadoRestablecimiento.TokenInvalido:
                MostrarError("El link es inválido o ya expiró. Solicitá uno nuevo.");
                break;
        }
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.Visible = true;
        pnlFormulario.Visible = false;
    }
}