using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// Integridad de la base (dígitos verificadores): qué tabla, fila o columna
/// no coincide, y las dos salidas para el Administrador: recalcular los
/// dígitos aceptando los datos actuales o restaurar un backup.
public partial class Integridad : PaginaBase
{
    protected override string PermisoRequerido { get { return DigitoVerificadorBLL.PermisoGestion; } }

    /// Los encabezados se traducen en Init, antes de que la grilla se
    /// reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvProblemas.Columns[0].HeaderText = Traductor.T("integ.tabla");
        gvProblemas.Columns[1].HeaderText = Traductor.T("integ.nivel");
        gvProblemas.Columns[2].HeaderText = Traductor.T("integ.detalle");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnRecalcularTodo.Text = Traductor.T("integ.recalcularTodo");
        btnRecalcularTodo.OnClientClick = Confirmacion.Para("integ.confirmarTodo");
        lnkBackup.Text = Traductor.T("integ.irBackup");
        btnVerificar.Text = Traductor.T("integ.verificar");
        if (!IsPostBack) Mostrar(new DigitoVerificadorBLL().Verificar());
    }
    protected void btnVerificar_Click(object sender, EventArgs e)
    {
        Paginador.Reiniciar();
        Mostrar(new DigitoVerificadorBLL().Verificar());
    }
    protected void btnRecalcularTodo_Click(object sender, EventArgs e)
    {
        Mostrar(new DigitoVerificadorBLL().RecalcularTodo(UsuarioActual));
        Mensaje.Exito(Traductor.T("integ.recalculado"));
    }
    protected void gvProblemas_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Recalcular") return;
        string Tabla = e.CommandArgument.ToString();
        Mostrar(new DigitoVerificadorBLL().RecalcularTabla(UsuarioActual, Tabla));
        Mensaje.Exito(Traductor.T("integ.recalculadaTabla", Tabla));
    }
    protected void Paginador_Cambio(object sender, EventArgs e)
    {
        Mostrar(new DigitoVerificadorBLL().Verificar());
    }
    private void Mostrar(List<ProblemaIntegridadBE> problemas)
    {
        bool Integra = problemas.Count == 0;
        pnlIntegra.Visible = Integra;
        pnlComprometida.Visible = !Integra;
        pnlProblemas.Visible = !Integra;
        gvProblemas.DataSource = Paginador.Paginar(problemas, P => P.Tabla + " " + P.Nivel + " " + P.Detalle);
        gvProblemas.DataBind();
    }
}
