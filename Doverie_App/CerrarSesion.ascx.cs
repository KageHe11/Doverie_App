using System;
using System.Web;
using System.Web.UI;
using BE;
using BLL;

/// CU-002-002 Logout: registra el cierre en la Bitácora (RC-002), vacía la
/// Session, vence la cookie con el token JWT y vuelve al Login. Está en
/// Site.Master, así que se puede salir desde cualquier pantalla.
public partial class CerrarSesion : UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btnCerrarSesion.Text = Traductor.T("nav.cerrarSesion");
    }
    protected void btnCerrarSesion_Click(object sender, EventArgs e)
    {
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario != null) new BitacoraBLL().Registrar("Logout", null, Usuario.Id);
        Session.Clear();

        HttpCookie Cookie = new HttpCookie("TokenSesion", "");
        Cookie.Expires = DateTime.Now.AddDays(-1);
        Response.Cookies.Add(Cookie);

        Response.Redirect("~/Login.aspx");
    }
}
