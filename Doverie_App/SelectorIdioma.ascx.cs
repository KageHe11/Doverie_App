using System;
using System.Collections.Generic;
using System.Web.UI;
using BE;
using BLL;

/// Permite elegir el idioma de la interfaz desde cualquier pantalla
/// (está en Site.Master). Cambia la estrategia del ContextoIdioma de la
/// sesión y, si hay un Usuario logueado, lo guarda como su preferido.
public partial class SelectorIdioma : UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            IdiomaBLL IdiomaBll = new IdiomaBLL();
            List<IdiomaBE> Idiomas = IdiomaBll.Listar();
            // Con un solo idioma no hay nada para elegir.
            Visible = Idiomas.Count > 1;
            ddlIdioma.DataSource = Idiomas;
            ddlIdioma.DataBind();
            int? Actual = Traductor.Contexto.IdiomaId;
            if (Actual.HasValue && ddlIdioma.Items.FindByValue(Actual.Value.ToString()) != null)
            {
                ddlIdioma.SelectedValue = Actual.Value.ToString();
            }
        }
    }
    protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
    {
        int IdiomaId = Convert.ToInt32(ddlIdioma.SelectedValue);
        IdiomaBLL IdiomaBll = new IdiomaBLL();
        Traductor.Contexto = IdiomaBll.CrearContexto(IdiomaId);
        UsuarioBE Usuario = Session["Usuario"] as UsuarioBE;
        if (Usuario != null)
        {
            UsuarioBLL UsuarioBll = new UsuarioBLL();
            UsuarioBll.ActualizarIdioma(Usuario, IdiomaId);
        }
        // Se recarga la página para que todos los textos (incluidos los
        // que ya se asignaron en el Page_Load) salgan en el idioma nuevo.
        Response.Redirect(Request.RawUrl, false);
        Context.ApplicationInstance.CompleteRequest();
    }
}
