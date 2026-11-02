using System;
using System.Collections.Generic;
using BE;
using BLL;

/// Inicio: sin sesión presenta la plataforma; con sesión muestra un acceso
/// por cada sección que el Perfil del Usuario permite (los mismos permisos
/// que exige cada página).
public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        pnlVisitante.Visible = Usuario == null;
        pnlUsuario.Visible = Usuario != null;
        if (Usuario == null) return;
        litSaludo.Text = Traductor.T("inicio.hola", Usuario.Nombre);
        rptAccesos.DataSource = Accesos(Usuario);
        rptAccesos.DataBind();
    }
    private static List<object> Accesos(UsuarioBE usuario)
    {
        List<object> Lista = new List<object>();
        // Icono (entidad HTML), clave del título, clave de la descripción, página y permiso.
        string[,] Secciones =
        {
            { "&#128196;", "nav.documentos", "inicio.acc.documentos", "/Documentos.aspx", "VerDocumentos" },
            { "&#128276;", "notif.titulo", "inicio.acc.notificaciones", "/Notificaciones.aspx", "VerNotificaciones" },
            { "&#11088;", "nav.suscripcion", "inicio.acc.suscripcion", "/Suscripcion.aspx", "Suscribirse" },
            { "&#129534;", "fact.titulo", "inicio.acc.facturacion", "/Facturacion.aspx", "VerFacturacion" },
            { "&#128101;", "empleados.titulo", "inicio.acc.empleados", "/EmpleadosEmpresa.aspx", "VerEmpleadosPropios" },
            { "&#128100;", "nav.usuarios", "inicio.acc.usuarios", "/Usuarios.aspx", "VerUsuarios" },
            { "&#128273;", "nav.credenciales", "inicio.acc.credenciales", "/Perfiles.aspx", "VerMenuAdmin" },
            { "&#128203;", "nav.bitacora", "inicio.acc.bitacora", "/Bitacora.aspx", "VerBitacora" },
            { "&#128202;", "nav.estadisticas", "inicio.acc.estadisticas", "/Estadisticas.aspx", "VerEstadisticas" },
            { "&#128190;", "nav.backup", "inicio.acc.backup", "/Backup.aspx", "GenerarBackup" },
            { "&#9989;", "nav.integridad", "inicio.acc.integridad", "/Integridad.aspx", DigitoVerificadorBLL.PermisoGestion },
            { "&#127760;", "nav.idiomas", "inicio.acc.idiomas", "/Idiomas.aspx", "VerIdiomas" }
        };
        for (int I = 0; I < Secciones.GetLength(0); I++)
        {
            if (!UsuarioBLL.TienePermiso(usuario, Secciones[I, 4])) continue;
            Lista.Add(new { Icono = Secciones[I, 0], Titulo = Traductor.T(Secciones[I, 1]), Descripcion = Traductor.T(Secciones[I, 2]), Url = Secciones[I, 3] });
        }
        if (!usuario.Empleado)
        {
            Lista.Add(new { Icono = "&#9881;&#65039;", Titulo = Traductor.T("nav.miPerfil"), Descripcion = Traductor.T("inicio.acc.miPerfil"), Url = "/MiPerfil.aspx" });
        }
        return Lista;
    }
}
