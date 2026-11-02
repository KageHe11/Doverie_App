using System;
using System.Collections.Generic;
using System.Web;
using BE;
using BLL;

/// CU-002-013 Generar Backup Manual: copia de la base de datos y de los
/// archivos de los documentos en AWS S3, más el historial de backups.
public partial class Backup : PaginaBase
{
    protected override string PermisoRequerido { get { return "GenerarBackup"; } }

    /// Los encabezados se traducen en Init, antes de que la grilla se
    /// reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvBackups.Columns[0].HeaderText = Traductor.T("bitacora.fecha");
        gvBackups.Columns[1].HeaderText = Traductor.T("comun.usuario");
        gvBackups.Columns[2].HeaderText = Traductor.T("comun.estado");
        gvBackups.Columns[3].HeaderText = Traductor.T("backup.detalle");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnGenerar.Text = Traductor.T("backup.generar");
        // 2. Solicitar confirmación
        btnGenerar.OnClientClick = Traductor.Confirmar("backup.confirmar");
        if (!IsPostBack) CargarHistorial();
    }
    /// 3. a 6. Generar la copia de la base y de los archivos.
    protected void btnGenerar_Click(object sender, EventArgs e)
    {
        BackupBE Generado;
        ResultadoBackup Resultado = new BackupBLL().Generar(UsuarioActual, out Generado);
        switch (Resultado)
        {
            case ResultadoBackup.Exitoso:
                lblMensaje.Text = HttpUtility.HtmlEncode(Traductor.T("backup.exito"));
                lblMensaje.Visible = true;
                break;
            case ResultadoBackup.ErrorBaseDatos:
                MostrarError(Traductor.T("error.041"));
                break;
            default:
                MostrarError(Traductor.T("error.042"));
                break;
        }
        CargarHistorial();
    }
    private void CargarHistorial()
    {
        List<BackupBE> Backups = new BackupBLL().Listar();
        gvBackups.DataSource = Backups;
        gvBackups.DataBind();
        lblSinBackups.Visible = Backups.Count == 0;
        lblSinBackups.Text = HttpUtility.HtmlEncode(Traductor.T("backup.sinBackups"));
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
