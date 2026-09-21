using System;
using System.Web;
using System.Web.UI;
using BE;
using BLL;

public partial class Site : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario != null)
        {
            pnlLogueado.Visible = true;
            pnlNoLogueado.Visible = false;
            litSaludo.Text = ArmarSaludo(Usuario);
            pnlAdmin.Visible = TienePermisoDeAdmin(Usuario);
            pnlEmpresa.Visible = Usuario.Empresa;
            pnlMiPerfil.Visible = !Usuario.Empleado;
            pnlEmpleadoOFreelance.Visible = Usuario.Empleado || Usuario.Independiente;
        }
        else
        {
            pnlLogueado.Visible = false;
            pnlNoLogueado.Visible = true;
            pnlAdmin.Visible = false;
            pnlEmpresa.Visible = false;
            pnlEmpleadoOFreelance.Visible = false;
        }
    }
    /// Arma el texto de bienvenida según el tipo de cuenta. El orden de
    private string ArmarSaludo(UsuarioBE usuario)
    {
        if (usuario.Empresa) return "Bienvenido Empresa " + usuario.Nombre;
        if (usuario.Empleado) return "Bienvenido Empleado Empresa " + usuario.Nombre;
        if (usuario.Independiente) return "Bienvenido Freelance " + usuario.Nombre;
        return "Hola, " + usuario.Nombre;
    }
    private bool TienePermisoDeAdmin(UsuarioBE usuario)
    {
        if (usuario.Perfil == null) return false;
        PermisoBLL PermisoBll = new PermisoBLL();
        PermisoBE PermisoAdmin = PermisoBll.ObtenerPorNombre("VerMenuAdmin");
        if (PermisoAdmin == null) return false;
        return usuario.Perfil.TienePermiso(PermisoAdmin.Id);
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