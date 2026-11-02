using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// CU-002-013 Generar Backup Manual: copia de la base de datos y de los
/// archivos de los documentos en AWS S3, más el historial de backups. Con
/// el permiso RestaurarBackup, también el restore de un backup del
/// historial o de un .bak subido.
public partial class Backup : PaginaBase
{
    protected override string PermisoRequerido { get { return "GenerarBackup"; } }

    private bool PuedeRestaurar;

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
        PuedeRestaurar = TienePermiso("RestaurarBackup");
        pnlRestaurarArchivo.Visible = PuedeRestaurar;
        gvBackups.Columns[4].Visible = PuedeRestaurar;
        btnGenerar.Text = Traductor.T("backup.generar");
        // 2. Solicitar confirmación
        btnGenerar.OnClientClick = Traductor.Confirmar("backup.confirmar");
        btnRestaurarArchivo.Text = Traductor.T("restore.subir");
        btnRestaurarArchivo.OnClientClick = Traductor.Confirmar("restore.confirmar");
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
                MostrarMensaje(Traductor.T("backup.exito"));
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
    protected void gvBackups_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        // Solo se restaura lo que salió bien.
        e.Row.FindControl("btnRestaurar").Visible = ((BackupBE)e.Row.DataItem).Estado == BackupBE.EstadoExitoso;
    }
    protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Restaurar") return;
        if (!PuedeRestaurar) { MostrarError(Traductor.T("error.056")); return; }
        BackupBE Previo;
        ResultadoRestauracion Resultado = new BackupBLL().RestaurarDesdeHistorial(UsuarioActual, Convert.ToInt32(e.CommandArgument), out Previo);
        MostrarRestauracion(Resultado, Previo);
    }
    protected void btnRestaurarArchivo_Click(object sender, EventArgs e)
    {
        if (!PuedeRestaurar) { MostrarError(Traductor.T("error.056")); return; }
        if (!fuBak.HasFile)
        {
            MostrarError(Traductor.T("restore.errorArchivo"));
            return;
        }
        byte[] Contenido;
        using (MemoryStream Memoria = new MemoryStream())
        {
            fuBak.PostedFile.InputStream.CopyTo(Memoria);
            Contenido = Memoria.ToArray();
        }
        BackupBE Previo;
        ResultadoRestauracion Resultado = new BackupBLL().RestaurarDesdeArchivo(UsuarioActual, fuBak.FileName, Contenido, out Previo);
        MostrarRestauracion(Resultado, Previo);
    }
    private void MostrarRestauracion(ResultadoRestauracion resultado, BackupBE previo)
    {
        string Previo = previo == null ? "" : previo.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
        switch (resultado)
        {
            case ResultadoRestauracion.Exitoso: MostrarMensaje(Traductor.T("restore.exito", Previo)); break;
            case ResultadoRestauracion.ExitosoSinArchivos: MostrarMensaje(Traductor.T("restore.exitoSinArchivos", Previo)); break;
            case ResultadoRestauracion.ArchivoNoDisponible: MostrarError(Traductor.T("restore.errorNoDisponible")); break;
            case ResultadoRestauracion.ArchivoInvalido: MostrarError(Traductor.T("restore.errorArchivo")); break;
            case ResultadoRestauracion.ErrorRespaldoPrevio: MostrarError(Traductor.T("restore.errorPrevio")); break;
            case ResultadoRestauracion.ErrorBaseDatos: MostrarError(Traductor.T("restore.errorBase", Previo)); break;
            case ResultadoRestauracion.ErrorArchivos: MostrarError(Traductor.T("restore.errorArchivos", Previo)); break;
            default: MostrarError(Traductor.T("error.056")); break;
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
    private void MostrarMensaje(string mensaje)
    {
        lblMensaje.Text = HttpUtility.HtmlEncode(mensaje);
        lblMensaje.Visible = true;
    }
    private void MostrarError(string mensaje)
    {
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
