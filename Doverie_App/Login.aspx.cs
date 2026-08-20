using BE;
using BLL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        UsuarioBE Usuario;
        string Token;
        UsuarioBLL BllUsuario = new UsuarioBLL();
        ResultadoLogin Resultado =  BllUsuario.LogIn(txtUsuario.Text, txtContrasena.Text, out Usuario, out Token);
        switch(Resultado)
        {
            case ResultadoLogin.Exitoso:
                Session["Usuario"] = Usuario;
                HttpCookie Cookie = new HttpCookie("TokenSesion", Token);
                Cookie.HttpOnly = true;
                Cookie.Expires = DateTime.Now.AddMinutes(30);
                Response.Cookies.Add(Cookie);
                Response.Redirect("Default.aspx");
                break;
            case ResultadoLogin.UsuarioBloqueado:
                lblError.Text = "Tu cuenta se encuentra bloqueada.";
                lblError.Visible = true;
                break;
            case ResultadoLogin.CredencialesInvalidas:
                lblError.Text = "Usuario o contraseña incorrectos.";
                lblError.Visible = true;
                break;
        }
    }
}