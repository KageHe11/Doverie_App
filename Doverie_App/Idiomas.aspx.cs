using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// Gestionar Idiomas: CU-002-018 Alta, CU-002-019 Modificar y
/// CU-002-020 Baja. Entrar a la pantalla exige VerIdiomas; cada acción
/// exige además su propio Permiso (AltaIdioma, ModificarIdioma, BajaIdioma).
public partial class Idiomas : PaginaBase
{
    private const int TamanoMaximoArchivo = 1024 * 1024;

    protected override string PermisoRequerido { get { return "VerIdiomas"; } }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvIdiomas.Columns[0].HeaderText = Traductor.T("idiomas.codigo");
        gvIdiomas.Columns[1].HeaderText = Traductor.T("idiomas.nombre");
        gvIdiomas.Columns[2].HeaderText = Traductor.T("idiomas.predeterminado");
        gvIdiomas.Columns[3].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        TraducirPantalla();
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }
    private void TraducirPantalla()
    {
        btnAgregar.Text = Traductor.T("idiomas.nuevo");
        btnAgregar.Visible = TienePermiso("AltaIdioma");
        btnGuardar.Text = Traductor.T("comun.guardar");
        chkPredeterminado.InputAttributes["class"] = "form-check-input";
    }
    /// La búsqueda o la página del Paginador cambiaron.
    protected void Paginador_Cambio(object sender, EventArgs e)
    {
        CargarGrilla();
    }
    private void CargarGrilla()
    {
        IdiomaBLL IdiomaBll = new IdiomaBLL();
        gvIdiomas.DataSource = Paginador.Paginar(IdiomaBll.Listar(), I => I.Codigo + " " + I.Nombre);
        gvIdiomas.DataBind();
    }
    protected void gvIdiomas_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        IdiomaBE Idioma = (IdiomaBE)e.Row.DataItem;
        ((Label)e.Row.FindControl("lblPredeterminado")).Text = Traductor.T("idiomas.predeterminado");
        LinkButton BtnEditar = (LinkButton)e.Row.FindControl("btnEditar");
        BtnEditar.Text = Traductor.T("comun.editar");
        BtnEditar.Visible = TienePermiso("ModificarIdioma");
        // El predeterminado no se puede eliminar [ERROR-054]: ni se ofrece.
        LinkButton BtnEliminar = (LinkButton)e.Row.FindControl("btnEliminar");
        BtnEliminar.Text = Traductor.T("comun.eliminar");
        BtnEliminar.Visible = TienePermiso("BajaIdioma") && !Idioma.EsPredeterminado;
        BtnEliminar.OnClientClick = Confirmacion.Para("idiomas.confirmarEliminar");
    }
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        OcultarMensajes();
        if (!TienePermiso("AltaIdioma"))
        {
            MostrarError(Traductor.T("error.056"));
            return;
        }
        hdnId.Value = "0";
        txtCodigo.Text = "";
        txtCodigo.Enabled = true;
        txtNombre.Text = "";
        // Un idioma nuevo no puede nacer como predeterminado: primero se
        // crea y después se lo marca desde Modificar.
        chkPredeterminado.Checked = false;
        chkPredeterminado.Enabled = false;
        pnlPredeterminado.Visible = false;
        pnlJsonActual.Visible = false;
        litTituloModal.Text = Traductor.T("idiomas.tituloNuevo");
        litAyudaArchivo.Text = Traductor.T("idiomas.archivoAyuda");
        lblErrorModal.Visible = false;
        AbrirModal();
    }
    protected void gvIdiomas_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        OcultarMensajes();
        int Id = Convert.ToInt32(e.CommandArgument);
        IdiomaBLL IdiomaBll = new IdiomaBLL();
        if (e.CommandName == "Editar")
        {
            if (!TienePermiso("ModificarIdioma"))
            {
                MostrarError(Traductor.T("error.056"));
                return;
            }
            IdiomaBE Idioma = IdiomaBll.ObtenerDatos(Id);
            if (Idioma == null)
            {
                MostrarError(Traductor.T("error.004"));
                CargarGrilla();
                return;
            }
            // 4. Se carga el formulario con los datos actuales y su JSON.
            hdnId.Value = Idioma.Id.ToString();
            txtCodigo.Text = Idioma.Codigo;
            txtCodigo.Enabled = false;
            txtNombre.Text = Idioma.Nombre;
            pnlPredeterminado.Visible = true;
            chkPredeterminado.Checked = Idioma.EsPredeterminado;
            // Siempre tiene que haber un predeterminado: no se lo puede
            // desmarcar, solo marcar otro idioma en su lugar.
            chkPredeterminado.Enabled = !Idioma.EsPredeterminado;
            pnlJsonActual.Visible = true;
            litJsonActual.Text = Idioma.ArchivoJson;
            litTituloModal.Text = Traductor.T("idiomas.tituloEditar");
            litAyudaArchivo.Text = Traductor.T("idiomas.archivoAyuda") + " " + Traductor.T("idiomas.archivoAyudaEditar");
            lblErrorModal.Visible = false;
            AbrirModal();
        }
        else if (e.CommandName == "Eliminar")
        {
            if (!TienePermiso("BajaIdioma"))
            {
                MostrarError(Traductor.T("error.056"));
                return;
            }
            ResultadoIdioma Resultado = IdiomaBll.Eliminar(Id, UsuarioActual.Id);
            if (Resultado == ResultadoIdioma.Exitoso)
            {
                // Si era el idioma del propio Administrador, la base ya lo
                // pasó al predeterminado: se refleja también en la sesión.
                if (UsuarioActual.IdiomaId == Id) UsuarioActual.IdiomaId = null;
                RecargarIdiomaSesion();
                MostrarMensaje(Traductor.T("idiomas.bajaExitosa"));
            }
            else
            {
                MostrarError(MensajeResultado(Resultado, null));
            }
            CargarGrilla();
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) { AbrirModal(); return; }
        OcultarMensajes();
        int Id = Convert.ToInt32(hdnId.Value);
        if (!TienePermiso(Id == 0 ? "AltaIdioma" : "ModificarIdioma"))
        {
            MostrarError(Traductor.T("error.056"));
            return;
        }
        string ArchivoJson;
        if (!LeerArchivo(out ArchivoJson))
        {
            MostrarErrorModal(Traductor.T("error.050"));
            return;
        }
        if (Id == 0 && ArchivoJson == null)
        {
            // En el alta el archivo es obligatorio [ERROR-004].
            MostrarErrorModal(Traductor.T("error.004"));
            return;
        }
        IdiomaBLL IdiomaBll = new IdiomaBLL();
        ResultadoIdioma Resultado;
        List<string> ClavesFaltantes;
        if (Id == 0)
        {
            IdiomaBE Nuevo = new IdiomaBE
            {
                Codigo = txtCodigo.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                ArchivoJson = ArchivoJson
            };
            Resultado = IdiomaBll.Registrar(Nuevo, UsuarioActual.Id, out ClavesFaltantes);
        }
        else
        {
            Resultado = IdiomaBll.Modificar(Id, txtNombre.Text.Trim(), ArchivoJson,
                chkPredeterminado.Enabled && chkPredeterminado.Checked, UsuarioActual.Id, out ClavesFaltantes);
        }
        if (Resultado != ResultadoIdioma.Exitoso)
        {
            MostrarErrorModal(MensajeResultado(Resultado, ClavesFaltantes));
            return;
        }
        // Las traducciones pudieron cambiar: se recarga el contexto de la
        // sesión para que el cambio se vea sin volver a iniciar sesión.
        RecargarIdiomaSesion();
        TraducirPantalla();
        MostrarMensaje(Traductor.T(Id == 0 ? "idiomas.altaExitosa" : "idiomas.modificacionExitosa"));
        CargarGrilla();
        CerrarModal();
    }
    /// Lee el JSON subido. Devuelve false si el archivo no es válido
    /// (extensión o tamaño); ArchivoJson queda en null si no se subió nada.
    private bool LeerArchivo(out string archivoJson)
    {
        archivoJson = null;
        if (!fuArchivo.HasFile) return true;
        if (!string.Equals(Path.GetExtension(fuArchivo.FileName), ".json", StringComparison.OrdinalIgnoreCase) ||
            fuArchivo.PostedFile.ContentLength > TamanoMaximoArchivo)
        {
            return false;
        }
        using (StreamReader Lector = new StreamReader(fuArchivo.PostedFile.InputStream, Encoding.UTF8, true))
        {
            archivoJson = Lector.ReadToEnd();
        }
        return true;
    }
    private string MensajeResultado(ResultadoIdioma resultado, List<string> clavesFaltantes)
    {
        switch (resultado)
        {
            case ResultadoIdioma.DatosInvalidos: return Traductor.T("error.004");
            case ResultadoIdioma.JsonInvalido:
                string Mensaje = Traductor.T("error.050");
                if (clavesFaltantes != null && clavesFaltantes.Count > 0)
                {
                    Mensaje += " " + Traductor.T("idiomas.clavesFaltantes", string.Join(", ", clavesFaltantes));
                }
                return Mensaje;
            case ResultadoIdioma.YaExiste: return Traductor.T("error.051");
            case ResultadoIdioma.ErrorAlta: return Traductor.T("error.052");
            case ResultadoIdioma.ErrorModificacion: return Traductor.T("error.053");
            case ResultadoIdioma.NoEliminable: return Traductor.T("error.054");
            default: return Traductor.T("error.055");
        }
    }
    private void RecargarIdiomaSesion()
    {
        IdiomaBLL IdiomaBll = new IdiomaBLL();
        Traductor.Contexto = IdiomaBll.CrearContexto(UsuarioActual.IdiomaId);
    }
    private void OcultarMensajes()
    {
        lblMensaje.Visible = false;
        lblError.Visible = false;
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
    private void MostrarErrorModal(string mensaje)
    {
        lblErrorModal.Text = HttpUtility.HtmlEncode(mensaje);
        lblErrorModal.Visible = true;
        AbrirModal();
    }
    private void AbrirModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModal",
            "var m = new bootstrap.Modal(document.getElementById('modalIdioma')); m.show();", true);
    }
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalIdioma'));
          if (m) m.hide();
          document.body.classList.remove('modal-open');
          document.body.style.removeProperty('padding-right');
          document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}
