using System;
using System.Web;
using System.Web.UI;
using BE;
using BLL;

public partial class Site : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btnCerrarSesion.Text = Traductor.T("nav.cerrarSesion");
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario != null)
        {
            pnlLogueado.Visible = true;
            pnlNoLogueado.Visible = false;
            litSaludo.Text = ArmarSaludo(Usuario);
            pnlAdmin.Visible = TienePermiso(Usuario, "VerMenuAdmin");
            pnlIdiomas.Visible = TienePermiso(Usuario, "VerIdiomas");
            pnlEmpresa.Visible = Usuario.Empresa;
            pnlMiPerfil.Visible = !Usuario.Empleado;
            pnlEmpleadoOFreelance.Visible = Usuario.Empleado || Usuario.Independiente;
        }
        else
        {
            pnlLogueado.Visible = false;
            pnlNoLogueado.Visible = true;
            pnlAdmin.Visible = false;
            pnlIdiomas.Visible = false;
            pnlEmpresa.Visible = false;
            pnlEmpleadoOFreelance.Visible = false;
        }
    }
    /// Arma el texto de bienvenida según el tipo de cuenta. El orden de
    private string ArmarSaludo(UsuarioBE usuario)
    {
        if (usuario.Empresa) return Traductor.T("saludo.empresa", usuario.Nombre);
        if (usuario.Empleado) return Traductor.T("saludo.empleado", usuario.Nombre);
        if (usuario.Independiente) return Traductor.T("saludo.freelance", usuario.Nombre);
        return Traductor.T("saludo.general", usuario.Nombre);
    }
    private bool TienePermiso(UsuarioBE usuario, string nombrePermiso)
    {
        if (usuario.Perfil == null) return false;
        PermisoBLL PermisoBll = new PermisoBLL();
        PermisoBE Permiso = PermisoBll.ObtenerPorNombre(nombrePermiso);
        if (Permiso == null) return false;
        return usuario.Perfil.TienePermiso(Permiso.Id);
    }
    protected void btnCerrarSesion_Click(object sender, EventArgs e)
    {
        Session.Clear();

        HttpCookie Cookie = new HttpCookie("TokenSesion", "");
        Cookie.Expires = DateTime.Now.AddDays(-1);
        Response.Cookies.Add(Cookie);

        Response.Redirect("~/Login.aspx");
    }
}