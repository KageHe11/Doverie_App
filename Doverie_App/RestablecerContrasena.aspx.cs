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
        btnRestablecer.Text = Traductor.T("restablecer.boton");
        if (string.IsNullOrWhiteSpace(Token))
        {
            MostrarError(Traductor.T("restablecer.linkInvalido"));
        }
    }
    protected void btnRestablecer_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        if (string.IsNullOrWhiteSpace(txtContrasena.Text) || string.IsNullOrWhiteSpace(txtConfirmar.Text))
        {
            Mensaje.Error(Traductor.T("comun.completarAmbos"));
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
                Mensaje.Error(Traductor.T("comun.contrasenasNoCoinciden"));
                break;
            case ResultadoRestablecimiento.ContrasenaCorta:
                Mensaje.Error(Traductor.T("error.contrasenaCorta", UsuarioBLL.LargoMinimoContrasena));
                break;
            case ResultadoRestablecimiento.TokenInvalido:
                MostrarError(Traductor.T("restablecer.linkExpirado"));
                break;
        }
    }
    private void MostrarError(string mensaje)
    {
        Mensaje.Error(mensaje);
        pnlFormulario.Visible = false;
    }
}