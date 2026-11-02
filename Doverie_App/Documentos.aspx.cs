using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;

/// Documentos: CU-001-012 Listado, 002 Subir, 003 Descargar, 004 Eliminar,
/// 006 Verificar Integridad, 007 Generar NFT y 013 Consultar Estado de NFT.
/// Entrar exige VerDocumentos; cada acción
/// exige además su propio Permiso. Que el documento sea de la cuenta del
/// Usuario lo controla DocumentoBLL.
public partial class Documentos : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerDocumentos"; } }

    /// Los encabezados de las grillas se traducen en Init, antes de que se
    /// reconstruyan desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvDocumentos.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvDocumentos.Columns[1].HeaderText = Traductor.T("doc.tipo");
        gvDocumentos.Columns[2].HeaderText = Traductor.T("doc.subidoPor");
        gvDocumentos.Columns[3].HeaderText = Traductor.T("doc.fecha");
        gvDocumentos.Columns[4].HeaderText = Traductor.T("comun.acciones");
        gvHistorial.Columns[0].HeaderText = Traductor.T("bitacora.fecha");
        gvHistorial.Columns[1].HeaderText = Traductor.T("bitacora.evento");
        gvHistorial.Columns[2].HeaderText = Traductor.T("comun.usuario");
        gvCompartidos.Columns[0].HeaderText = Traductor.T("compartir.destinatario");
        gvCompartidos.Columns[1].HeaderText = Traductor.T("compartir.compartidoPor");
        gvCompartidos.Columns[2].HeaderText = Traductor.T("compartir.creado");
        gvCompartidos.Columns[3].HeaderText = Traductor.T("compartir.vence");
        gvCompartidos.Columns[4].HeaderText = Traductor.T("compartir.accesos");
        gvCompartidos.Columns[5].HeaderText = Traductor.T("comun.estado");
    }
    // Se calculan una vez por request: TienePermiso consulta la base y la
    // grilla lo necesita en cada fila.
    private bool PuedeDescargar, PuedeVerificar, PuedeEliminar, PuedeCompartir, PuedeGenerarNFT, PuedeConsultarNFT;
    protected void Page_Load(object sender, EventArgs e)
    {
        PuedeDescargar = TienePermiso("DescargarDocumento");
        PuedeVerificar = TienePermiso("VerificarDocumento");
        PuedeEliminar = TienePermiso("EliminarDocumento");
        PuedeCompartir = TienePermiso("CompartirDocumento");
        PuedeGenerarNFT = TienePermiso("GenerarNFT");
        PuedeConsultarNFT = TienePermiso("ConsultarNFT");
        pnlSubir.Visible = TienePermiso("SubirDocumento");
        btnCompartir.Text = Traductor.T("compartir.boton");
        CargarVigencias();
        btnSubir.Text = Traductor.T("doc.subir");
        btnFiltrar.Text = Traductor.T("bitacora.filtrar");
        ddlTipo.Items[0].Text = Traductor.T("comun.todos");
        litAyudaArchivo.Text = Traductor.T("doc.ayudaArchivo",
            string.Join(", ", DocumentoBLL.ExtensionesPermitidas), DocumentoBLL.TamanoMaximoBytes / (1024 * 1024));
        if (!IsPostBack)
        {
            CargarTipos();
            CargarGrilla();
        }
    }
    private void CargarTipos()
    {
        string Seleccionado = ddlTipo.SelectedValue;
        while (ddlTipo.Items.Count > 1) ddlTipo.Items.RemoveAt(1);
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        foreach (string Tipo in DocumentoBll.ListarTipos(UsuarioActual))
        {
            ddlTipo.Items.Add(new ListItem(Tipo, Tipo));
        }
        if (ddlTipo.Items.FindByValue(Seleccionado) != null) ddlTipo.SelectedValue = Seleccionado;
    }
    /// La búsqueda o la página del Paginador cambiaron.
    protected void Paginador_Cambio(object sender, EventArgs e)
    {
        CargarGrilla();
    }
    private void CargarGrilla()
    {
        DateTime? Desde = string.IsNullOrEmpty(txtDesde.Text) ? (DateTime?)null : Convert.ToDateTime(txtDesde.Text);
        DateTime? Hasta = string.IsNullOrEmpty(txtHasta.Text) ? (DateTime?)null : Convert.ToDateTime(txtHasta.Text).AddDays(1).AddSeconds(-1);
        bool HayFiltros = !string.IsNullOrWhiteSpace(txtFiltroNombre.Text) || ddlTipo.SelectedValue != "" || Desde.HasValue || Hasta.HasValue;
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        List<DocumentoBE> Lista = DocumentoBll.Listar(UsuarioActual, txtFiltroNombre.Text, ddlTipo.SelectedValue, Desde, Hasta);
        gvDocumentos.DataSource = Paginador.Paginar(Lista);
        gvDocumentos.DataBind();
        // 2.1 La cuenta no tiene documentos / 4.1 El filtro no arroja
        // resultados [ERROR-033].
        lblSinResultados.Visible = Lista.Count == 0;
        lblSinResultados.Text = Traductor.T(HayFiltros ? "error.033" : "doc.sinDocumentos");
    }
    protected void gvDocumentos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        e.Row.FindControl("btnDescargar").Visible = PuedeDescargar;
        e.Row.FindControl("btnVerificar").Visible = PuedeVerificar;
        e.Row.FindControl("btnEliminar").Visible = PuedeEliminar;
        e.Row.FindControl("btnCompartirFila").Visible = PuedeCompartir;
        // Generar solo si todavía no tiene NFT; consultar solo si ya tiene.
        bool TieneNFT = !string.IsNullOrEmpty(((DocumentoBE)e.Row.DataItem).NFTId);
        e.Row.FindControl("btnGenerarNFT").Visible = PuedeGenerarNFT && !TieneNFT;
        e.Row.FindControl("btnConsultarNFT").Visible = PuedeConsultarNFT && TieneNFT;
    }
    protected void btnFiltrar_Click(object sender, EventArgs e)
    {
        Paginador.Reiniciar();
        pnlVerificacion.Visible = false;
        pnlNFT.Visible = false;
        pnlCompartir.Visible = false;
        CargarGrilla();
    }
    protected void btnSubir_Click(object sender, EventArgs e)
    {
        pnlVerificacion.Visible = false;
        pnlNFT.Visible = false;
        pnlCompartir.Visible = false;
        if (!TienePermiso("SubirDocumento"))
        {
            MostrarError(Traductor.T("error.056"));
            return;
        }
        if (!fuArchivo.HasFile)
        {
            MostrarError(Traductor.T("error.010"));
            return;
        }
        byte[] Contenido;
        using (MemoryStream Memoria = new MemoryStream())
        {
            fuArchivo.PostedFile.InputStream.CopyTo(Memoria);
            Contenido = Memoria.ToArray();
        }
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        DocumentoBE Documento;
        ResultadoDocumento Resultado = DocumentoBll.Subir(UsuarioActual, fuArchivo.FileName, Contenido, out Documento);
        if (Resultado == ResultadoDocumento.Exitoso)
        {
            MostrarMensaje(Traductor.T("doc.subidaExitosa", Documento.Nombre));
        }
        else
        {
            MostrarError(MensajeResultado(Resultado));
        }
        CargarTipos();
        CargarGrilla();
    }
    protected void gvDocumentos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        pnlVerificacion.Visible = false;
        pnlNFT.Visible = false;
        pnlCompartir.Visible = false;
        if (e.CommandName == "Compartir")
        {
            if (!PuedeCompartir) { MostrarError(Traductor.T("error.056")); return; }
            AbrirCompartir(Id);
        }
        else if (e.CommandName == "Descargar")
        {
            if (!PuedeDescargar) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            byte[] Contenido;
            ResultadoDocumento Resultado = DocumentoBll.Descargar(UsuarioActual, Id, out Documento, out Contenido);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                EnviarArchivo(Documento.Nombre, Contenido);
                return;
            }
            MostrarError(MensajeResultado(Resultado));
            CargarGrilla();
        }
        else if (e.CommandName == "Verificar")
        {
            if (!PuedeVerificar) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            VerificacionDocumentoBE Verificacion;
            ResultadoDocumento Resultado = DocumentoBll.VerificarIntegridad(UsuarioActual, Id, out Documento, out Verificacion);
            if (Verificacion != null)
            {
                MostrarVerificacion(Documento, Verificacion);
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
        }
        else if (e.CommandName == "GenerarNFT")
        {
            if (!PuedeGenerarNFT) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            NFTBE Nft;
            ResultadoDocumento Resultado = DocumentoBll.GenerarNFT(UsuarioActual, Id, out Documento, out Nft);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                // 7. Mensaje de generación exitosa con el identificador del NFT
                MostrarMensaje(Traductor.T("doc.nftGenerado", Documento.Nombre, Nft.Identificador));
                Nft.Vigente = true;
                MostrarNFT(Documento, Nft);
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
            CargarGrilla();
        }
        else if (e.CommandName == "ConsultarNFT")
        {
            if (!PuedeConsultarNFT) { MostrarError(Traductor.T("error.056")); return; }
            DocumentoBE Documento;
            NFTBE Nft;
            ResultadoDocumento Resultado = DocumentoBll.ConsultarNFT(UsuarioActual, Id, out Documento, out Nft);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                MostrarNFT(Documento, Nft);
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
        }
        else if (e.CommandName == "Eliminar")
        {
            if (!PuedeEliminar) { MostrarError(Traductor.T("error.056")); return; }
            ResultadoDocumento Resultado = DocumentoBll.Eliminar(UsuarioActual, Id);
            if (Resultado == ResultadoDocumento.Exitoso)
            {
                MostrarMensaje(Traductor.T("doc.eliminacionExitosa"));
            }
            else
            {
                MostrarError(MensajeResultado(Resultado));
            }
            CargarTipos();
            CargarGrilla();
        }
    }
    private void MostrarVerificacion(DocumentoBE documento, VerificacionDocumentoBE verificacion)
    {
        pnlVerificacion.Visible = true;
        litVerificacionNombre.Text = documento.Nombre;
        // 8. Íntegro, o 7.1 modificado [ERROR-018]
        lblVerificacionResultado.Text = HttpUtility.HtmlEncode(Traductor.T(verificacion.Integro ? "doc.integro" : "error.018"));
        lblVerificacionResultado.CssClass = "alert d-block " + (verificacion.Integro ? "alert-success" : "alert-danger");
        litHashRegistrado.Text = verificacion.Sello.Hash;
        litHashActual.Text = verificacion.HashActual;
        litFechaSellado.Text = verificacion.Sello.FechaSellado.ToString("dd/MM/yyyy HH:mm:ss");
        litTransaccion.Text = verificacion.Sello.Transaccion;
        SelloBlockchainBE Sello = verificacion.Sello;
        litRed.Text = Sello.Red == null ? Traductor.T("doc.registroLocal")
            : Sello.Red + (Sello.Bloque.HasValue ? " (" + Traductor.T("doc.bloque") + " " + Sello.Bloque.Value + ")" : "");
        lnkExplorador.Visible = Sello.UrlExplorador != null;
        lnkExplorador.NavigateUrl = Sello.UrlExplorador;
        lnkExplorador.Text = HttpUtility.HtmlEncode(Traductor.T("doc.verExplorador"));
        DocumentoBLL DocumentoBll = new DocumentoBLL();
        gvHistorial.DataSource = DocumentoBll.Historial(documento.Id);
        gvHistorial.DataBind();
    }
    /// CU-001-007 paso 7 y CU-001-013 paso 6: datos del NFT y su estado.
    private void MostrarNFT(DocumentoBE documento, NFTBE nft)
    {
        pnlNFT.Visible = true;
        litNFTNombre.Text = documento.Nombre;
        lblNFTEstado.Text = HttpUtility.HtmlEncode(Traductor.T(nft.Vigente ? "doc.nftVigente" : "doc.nftNoCoincide"));
        lblNFTEstado.CssClass = "alert d-block " + (nft.Vigente ? "alert-success" : "alert-danger");
        litNFTIdentificador.Text = nft.Identificador;
        litNFTToken.Text = "#" + nft.TokenId;
        litNFTHash.Text = nft.Hash;
        litNFTFecha.Text = nft.FechaEmision.ToString("dd/MM/yyyy HH:mm:ss");
        litNFTRed.Text = nft.Red ?? Traductor.T("doc.registroLocal");
        phNFTPropietario.Visible = nft.Propietario != null;
        litNFTPropietario.Text = nft.Propietario;
        phNFTTransaccion.Visible = nft.Transaccion != null;
        litNFTTransaccion.Text = nft.Transaccion;
        lnkNFTExplorador.Visible = nft.UrlExplorador != null;
        lnkNFTExplorador.NavigateUrl = nft.UrlExplorador;
        lnkNFTExplorador.Text = HttpUtility.HtmlEncode(Traductor.T("doc.nftVerExplorador"));
    }
    protected void btnCerrarNFT_Click(object sender, EventArgs e)
    {
        pnlNFT.Visible = false;
    }
    /// Manda el archivo al navegador como descarga. El nombre se codifica
    /// para que acentos o espacios no rompan el encabezado.
    private void EnviarArchivo(string nombre, byte[] contenido)
    {
        Response.Clear();
        Response.ContentType = MimeMapping.GetMimeMapping(nombre);
        Response.AddHeader("Content-Disposition", "attachment; filename*=UTF-8''" + Uri.EscapeDataString(nombre));
        Response.AddHeader("Content-Length", contenido.Length.ToString());
        Response.BinaryWrite(contenido);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    // ---------- CU-001-005 Compartir Documento ----------

    private void CargarVigencias()
    {
        string Seleccionada = ddlVigencia.SelectedValue;
        ddlVigencia.Items.Clear();
        foreach (int Dias in DocumentoCompartidoBLL.VigenciasDias)
        {
            ddlVigencia.Items.Add(new ListItem(Traductor.T(Dias == 1 ? "compartir.dia" : "compartir.dias", Dias), Dias.ToString()));
        }
        ddlVigencia.SelectedValue = ddlVigencia.Items.FindByValue(Seleccionada) != null ? Seleccionada : "7";
    }
    /// 1. y 2. Abre el panel de compartir para un documento de la cuenta.
    private void AbrirCompartir(int documentoId)
    {
        DocumentoBE Documento;
        ResultadoDocumento Acceso = new DocumentoBLL().ObtenerAccesible(UsuarioActual, documentoId, out Documento);
        if (Acceso != ResultadoDocumento.Exitoso)
        {
            MostrarError(MensajeResultado(Acceso));
            CargarGrilla();
            return;
        }
        hdnCompartirDocumentoId.Value = Documento.Id.ToString();
        litCompartirNombre.Text = Documento.Nombre;
        txtEmailDestinatario.Text = "";
        pnlCompartir.Visible = true;
        CargarCompartidos();
    }
    private void CargarCompartidos()
    {
        List<DocumentoCompartidoBE> Compartidos = new DocumentoCompartidoBLL().Listar(UsuarioActual, Convert.ToInt32(hdnCompartirDocumentoId.Value));
        gvCompartidos.DataSource = Compartidos;
        gvCompartidos.DataBind();
        lblSinEnlaces.Visible = Compartidos.Count == 0;
        lblSinEnlaces.Text = Traductor.T("compartir.sinEnlaces");
    }
    protected void btnCompartir_Click(object sender, EventArgs e)
    {
        pnlVerificacion.Visible = false;
        pnlNFT.Visible = false;
        if (!PuedeCompartir) { MostrarError(Traductor.T("error.056")); return; }
        string Enlace;
        string UrlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Request.ApplicationPath;
        ResultadoDocumento Resultado = new DocumentoCompartidoBLL().Compartir(UsuarioActual,
            Convert.ToInt32(hdnCompartirDocumentoId.Value), txtEmailDestinatario.Text, Convert.ToInt32(ddlVigencia.SelectedValue), UrlBase, out Enlace);
        if (Resultado == ResultadoDocumento.Exitoso)
        {
            // 7. Mensaje de compartir exitoso
            lblCompartirMensaje.Text = HttpUtility.HtmlEncode(Traductor.T("compartir.exito", txtEmailDestinatario.Text.Trim()));
            lblCompartirMensaje.Visible = true;
            txtEmailDestinatario.Text = "";
        }
        else if (Resultado == ResultadoDocumento.EnlaceNoEnviado)
        {
            // [ERROR-013] El email falló, pero el enlace existe: se muestra
            // para copiarlo y mandarlo por otro medio.
            lblCompartirError.Text = HttpUtility.HtmlEncode(Traductor.T("error.013"));
            lblCompartirError.Visible = true;
            txtEnlace.Text = Enlace;
            pnlEnlace.Visible = true;
            txtEmailDestinatario.Text = "";
        }
        else
        {
            lblCompartirError.Text = HttpUtility.HtmlEncode(MensajeResultado(Resultado));
            lblCompartirError.Visible = true;
        }
        CargarCompartidos();
    }
    protected void btnCerrarCompartir_Click(object sender, EventArgs e)
    {
        pnlCompartir.Visible = false;
    }
    protected void gvCompartidos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        DocumentoCompartidoBE Compartido = (DocumentoCompartidoBE)e.Row.DataItem;
        Label Estado = (Label)e.Row.FindControl("lblEstadoEnlace");
        if (Compartido.Revocado)
        {
            Estado.Text = Traductor.T("compartir.estadoRevocado");
            Estado.CssClass = "badge bg-secondary";
        }
        else if (!Compartido.Vigente)
        {
            Estado.Text = Traductor.T("compartir.estadoVencido");
            Estado.CssClass = "badge bg-warning text-dark";
        }
        else
        {
            Estado.Text = Traductor.T("compartir.estadoVigente");
            Estado.CssClass = "badge bg-success";
        }
        // Solo se puede revocar lo que todavía da acceso.
        e.Row.FindControl("btnRevocar").Visible = PuedeCompartir && Compartido.Vigente;
    }
    protected void gvCompartidos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Revocar") return;
        pnlVerificacion.Visible = false;
        pnlNFT.Visible = false;
        if (!PuedeCompartir) { MostrarError(Traductor.T("error.056")); return; }
        ResultadoDocumento Resultado = new DocumentoCompartidoBLL().Revocar(UsuarioActual, Convert.ToInt32(e.CommandArgument));
        if (Resultado == ResultadoDocumento.Exitoso)
        {
            lblCompartirMensaje.Text = HttpUtility.HtmlEncode(Traductor.T("compartir.revocado"));
            lblCompartirMensaje.Visible = true;
        }
        else
        {
            lblCompartirError.Text = HttpUtility.HtmlEncode(MensajeResultado(Resultado));
            lblCompartirError.Visible = true;
        }
        CargarCompartidos();
    }

    private string MensajeResultado(ResultadoDocumento resultado)
    {
        switch (resultado)
        {
            case ResultadoDocumento.DatosInvalidos: return Traductor.T("error.004");
            case ResultadoDocumento.ErrorEnlace: return Traductor.T("error.013");
            case ResultadoDocumento.EnlaceNoEnviado: return Traductor.T("error.013");
            case ResultadoDocumento.ArchivoInvalido: return Traductor.T("error.010");
            case ResultadoDocumento.ErrorAlmacenamiento: return Traductor.T("error.011");
            case ResultadoDocumento.ErrorBlockchain: return Traductor.T("error.012");
            case ResultadoDocumento.YaEliminado: return Traductor.T("error.016");
            case ResultadoDocumento.ErrorObtener: return Traductor.T("error.017");
            case ResultadoDocumento.Modificado: return Traductor.T("error.018");
            case ResultadoDocumento.ErrorEliminar: return Traductor.T("error.006");
            case ResultadoDocumento.NFTExistente: return Traductor.T("error.019");
            case ResultadoDocumento.ErrorNFT: return Traductor.T("error.020");
            case ResultadoDocumento.SinNFT: return Traductor.T("error.031");
            case ResultadoDocumento.ErrorConsultaNFT: return Traductor.T("error.032");
            case ResultadoDocumento.SinSuscripcion: return Traductor.T(UsuarioActual.EmpresaId == null ? "susc.requerida" : "susc.requeridaEmpresa");
            default: return Traductor.T("error.056");
        }
    }
    private void MostrarMensaje(string mensaje)
    {
        lblError.Visible = false;
        lblMensaje.Text = HttpUtility.HtmlEncode(mensaje);
        lblMensaje.Visible = true;
    }
    private void MostrarError(string mensaje)
    {
        lblMensaje.Visible = false;
        lblError.Text = HttpUtility.HtmlEncode(mensaje);
        lblError.Visible = true;
    }
}
