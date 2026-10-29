using System;
using System.IO;
using System.Web;
using BE;
using BLL;

/// CU-001-018 Acceder a Documento Compartido. Página pública (no requiere
/// sesión): el acceso lo da el token del enlace, de solo lectura.
///   ?token=X                 muestra el documento (cuenta como acceso)
///   ?token=X&modo=contenido  el archivo en línea, para la vista previa
///   ?token=X&modo=descargar  el archivo como descarga
public partial class DocumentoCompartido : System.Web.UI.Page
{
    private string Token
    {
        get { return Request.QueryString["token"]; }
    }
    private string Modo
    {
        get { return Request.QueryString["modo"] ?? ""; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        DocumentoCompartidoBLL CompartidoBll = new DocumentoCompartidoBLL();
        DocumentoCompartidoBE Compartido;
        DocumentoBE Documento;
        // 2. y 3. Validar el token [ERROR-044] y el documento [ERROR-016]
        ResultadoDocumento Resultado = CompartidoBll.Acceder(Token, out Compartido, out Documento);
        if (Resultado != ResultadoDocumento.Exitoso)
        {
            if (Modo != "") { NoEncontrado(); return; }
            MostrarError(Resultado == ResultadoDocumento.YaEliminado ? "error.016" : "error.044");
            return;
        }
        // 4. Obtener el archivo [ERROR-017]
        byte[] Contenido;
        if (CompartidoBll.ObtenerContenido(Documento, out Contenido) != ResultadoDocumento.Exitoso)
        {
            if (Modo != "") { NoEncontrado(); return; }
            MostrarError("error.017");
            return;
        }
        string ClaveVisto = "CompartidoVisto_" + Compartido.Id;
        if (Modo == "contenido")
        {
            // La vista previa solo se sirve si antes se abrió la página del
            // enlace en esta sesión: el acceso real queda registrado ahí.
            if (Session[ClaveVisto] == null) { NoEncontrado(); return; }
            EnviarArchivo(Documento.Nombre, Contenido, false);
            return;
        }
        if (Modo == "descargar")
        {
            // 6. Descarga opcional
            CompartidoBll.RegistrarAcceso(Compartido, Documento, true, false);
            EnviarArchivo(Documento.Nombre, Contenido, true);
            return;
        }
        // 5. Mostrar el documento en modo de solo lectura. 7. y 8. Registrar
        // el acceso y avisar al titular (una vez por sesión del visitante,
        // para no avisar en cada recarga de la página).
        bool PrimeraVez = Session[ClaveVisto] == null;
        Session[ClaveVisto] = true;
        CompartidoBll.RegistrarAcceso(Compartido, Documento, false, PrimeraVez);
        MostrarDocumento(Compartido, Documento);
    }
    private void MostrarDocumento(DocumentoCompartidoBE compartido, DocumentoBE documento)
    {
        pnlDocumento.Visible = true;
        litNombre.Text = documento.Nombre;
        litInfo.Text = Traductor.T("compartido.info", compartido.NombreUsuario, compartido.FechaVencimiento.ToString("dd/MM/yyyy HH:mm"));
        string Base = "DocumentoCompartido.aspx?token=" + HttpUtility.UrlEncode(Token);
        lnkDescargar.Text = Traductor.T("doc.descargar");
        lnkDescargar.NavigateUrl = Base + "&modo=descargar";
        string Url = Base + "&modo=contenido";
        switch (Path.GetExtension(documento.Nombre).ToLowerInvariant())
        {
            case ".pdf":
            case ".txt":
                pnlVistaPdf.Visible = true;
                ifrPdf.Attributes["src"] = Url;
                ifrPdf.Attributes["title"] = documento.Nombre;
                break;
            case ".png":
            case ".jpg":
            case ".jpeg":
                pnlVistaImagen.Visible = true;
                imgVista.ImageUrl = Url;
                imgVista.AlternateText = documento.Nombre;
                break;
            default:
                // Word, Excel, etc.: el navegador no los muestra; se ofrece
                // la descarga.
                pnlSinVista.Visible = true;
                break;
        }
    }
    private void EnviarArchivo(string nombre, byte[] contenido, bool descarga)
    {
        Response.Clear();
        string Tipo = MimeMapping.GetMimeMapping(nombre);
        if (Tipo == "text/plain") Tipo += "; charset=utf-8";
        Response.ContentType = Tipo;
        Response.AddHeader("Content-Disposition", (descarga ? "attachment" : "inline") + "; filename*=UTF-8''" + Uri.EscapeDataString(nombre));
        Response.AddHeader("Content-Length", contenido.Length.ToString());
        Response.AddHeader("X-Content-Type-Options", "nosniff");
        Response.AddHeader("Cache-Control", "no-store");
        Response.BinaryWrite(contenido);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    private void NoEncontrado()
    {
        Response.Clear();
        Response.StatusCode = 404;
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }
    private void MostrarError(string clave)
    {
        lblError.Text = HttpUtility.HtmlEncode(Traductor.T(clave));
        lblError.Visible = true;
        pnlDocumento.Visible = false;
    }
}
