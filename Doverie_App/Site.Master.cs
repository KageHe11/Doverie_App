using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using BE;
using BLL;

public partial class Site : MasterPage
{
    /// Título de la pestaña de cada página: el mismo texto que su encabezado.
    private static readonly Dictionary<string, string> Titulos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Login", "login.titulo" }, { "Registro", "registro.titulo" },
        { "RecuperarContrasena", "recuperar.titulo" }, { "RestablecerContrasena", "restablecer.titulo" },
        { "MiPerfil", "miPerfil.titulo" }, { "Usuarios", "usuarios.titulo" }, { "EmpleadosEmpresa", "empleados.titulo" },
        { "Perfiles", "perfiles.titulo" }, { "Familias", "familias.titulo" }, { "Permisos", "permisos.titulo" },
        { "Bitacora", "bitacora.titulo" }, { "AccesoDenegado", "acceso.titulo" }, { "Idiomas", "idiomas.titulo" },
        { "Documentos", "doc.titulo" }, { "Notificaciones", "notif.titulo" }, { "Estadisticas", "est.titulo" },
        { "Suscripcion", "susc.titulo" }, { "PagoSimulado", "pagoSim.titulo" }, { "Facturacion", "fact.titulo" },
        { "Backup", "backup.titulo" }, { "Integridad", "integ.titulo" }
    };
    protected void Page_Load(object sender, EventArgs e)
    {
        string Clave;
        Page.Title = Titulos.TryGetValue(Path.GetFileNameWithoutExtension(Request.Path), out Clave)
            ? Traductor.T(Clave) + " - Doverie"
            : "Doverie";
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario != null)
        {
            pnlLogueado.Visible = true;
            pnlNoLogueado.Visible = false;
            litSaludo.Text = ArmarSaludo(Usuario);
            pnlAdmin.Visible = TienePermiso(Usuario, "VerMenuAdmin");
            pnlIdiomas.Visible = TienePermiso(Usuario, "VerIdiomas");
            pnlEstadisticas.Visible = TienePermiso(Usuario, "VerEstadisticas");
            pnlBackup.Visible = TienePermiso(Usuario, "GenerarBackup");
            pnlIntegridad.Visible = TienePermiso(Usuario, DigitoVerificadorBLL.PermisoGestion);
            pnlDocumentos.Visible = TienePermiso(Usuario, "VerDocumentos");
            pnlSuscripcion.Visible = TienePermiso(Usuario, "Suscribirse");
            phFacturacion.Visible = TienePermiso(Usuario, "VerFacturacion");
            pnlEmpresa.Visible = Usuario.Empresa;
            pnlMiPerfil.Visible = !Usuario.Empleado;
            pnlCampana.Visible = TienePermiso(Usuario, "VerNotificaciones");
        }
        else
        {
            pnlLogueado.Visible = false;
            pnlNoLogueado.Visible = true;
            pnlNavPublico.Visible = true;
            pnlAdmin.Visible = false;
            pnlIdiomas.Visible = false;
            pnlEstadisticas.Visible = false;
            pnlBackup.Visible = false;
            pnlIntegridad.Visible = false;
            pnlDocumentos.Visible = false;
            pnlSuscripcion.Visible = false;
            pnlEmpresa.Visible = false;
            pnlCampana.Visible = false;
        }
    }
    /// El contador de la campana se calcula al final del ciclo de la página,
    /// después de los eventos: así refleja lo que la página haya hecho en
    /// este mismo request (ej: Notificaciones.aspx marca todo como leído).
    protected void Page_PreRender(object sender, EventArgs e)
    {
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario == null || !pnlCampana.Visible) return;
        int NoLeidas = new NotificacionBLL().ContarNoLeidas(Usuario.Id);
        lblNoLeidas.Visible = NoLeidas > 0;
        lblNoLeidas.Text = NoLeidas > 99 ? "99+" : NoLeidas.ToString();
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
}