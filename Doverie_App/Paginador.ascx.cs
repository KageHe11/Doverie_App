using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

/// Búsqueda y paginado de un listado, reutilizable en cualquier pantalla.
/// La página le pasa la lista completa a Paginar(), junto con qué texto de
/// cada elemento se busca (eso lo parametriza según la entidad), y enlaza
/// la grilla con lo que devuelve: los elementos de la página actual. Cuando
/// cambia la búsqueda o la página, el control avisa con el evento Cambio y
/// la página vuelve a cargar su grilla.
public partial class Paginador : UserControl
{
    public event EventHandler Cambio;

    private int tamanoPagina = 10;
    private bool mostrarBuscador = true;
    /// Elementos por página.
    public int TamanoPagina
    {
        get { return tamanoPagina; }
        set { tamanoPagina = value; }
    }
    /// Sin buscador, para listados que ya tienen sus propios filtros.
    public bool MostrarBuscador
    {
        get { return mostrarBuscador; }
        set { mostrarBuscador = value; }
    }

    protected int PaginaActual
    {
        get { return ViewState["Pagina"] as int? ?? 0; }
        private set { ViewState["Pagina"] = value; }
    }
    protected int TotalPaginas
    {
        get { return ViewState["TotalPaginas"] as int? ?? 1; }
        private set { ViewState["TotalPaginas"] = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        pnlBuscador.Visible = MostrarBuscador;
        txtBuscar.Attributes["placeholder"] = Traductor.T("pag.placeholder");
        btnBuscar.Text = Traductor.T("pag.buscar");
        btnLimpiar.Text = Traductor.T("pag.limpiar");
        btnAnterior.Attributes["aria-label"] = Traductor.T("pag.anterior");
        btnSiguiente.Attributes["aria-label"] = Traductor.T("pag.siguiente");
    }
    /// Vuelve a la primera página; la página lo llama cuando cambian sus
    /// propios filtros.
    public void Reiniciar()
    {
        PaginaActual = 0;
    }
    /// Aplica la búsqueda (sin distinguir mayúsculas ni acentos) sobre el
    /// texto que devuelve textoBuscable y devuelve la página actual.
    public List<T> Paginar<T>(IEnumerable<T> lista, Func<T, string> textoBuscable = null)
    {
        string Busqueda = MostrarBuscador ? txtBuscar.Text.Trim() : "";
        btnLimpiar.Visible = Busqueda != "";
        CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;
        List<T> Filtrados = (Busqueda == "" || textoBuscable == null ? lista : lista.Where(E =>
            Comparador.IndexOf(textoBuscable(E) ?? "", Busqueda, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)).ToList();

        TotalPaginas = Math.Max(1, (Filtrados.Count + TamanoPagina - 1) / TamanoPagina);
        if (PaginaActual >= TotalPaginas) PaginaActual = TotalPaginas - 1;
        int Desde = PaginaActual * TamanoPagina;
        List<T> Pagina = Filtrados.Skip(Desde).Take(TamanoPagina).ToList();

        litResumen.Text = Filtrados.Count == 0 ? "" :
            Traductor.T("pag.mostrando", Desde + 1, Desde + Pagina.Count, Filtrados.Count);
        phPaginas.Visible = TotalPaginas > 1;
        // Hasta 7 números alrededor de la página actual.
        int Primera = Math.Max(0, Math.Min(PaginaActual - 3, TotalPaginas - 7));
        rptPaginas.DataSource = Enumerable.Range(Primera, Math.Min(7, TotalPaginas));
        rptPaginas.DataBind();
        return Pagina;
    }
    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        IrA(0);
    }
    protected void btnLimpiar_Click(object sender, EventArgs e)
    {
        txtBuscar.Text = "";
        IrA(0);
    }
    protected void btnAnterior_Click(object sender, EventArgs e)
    {
        IrA(PaginaActual - 1);
    }
    protected void btnSiguiente_Click(object sender, EventArgs e)
    {
        IrA(PaginaActual + 1);
    }
    protected void rptPaginas_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "Pagina") IrA(Convert.ToInt32(e.CommandArgument));
    }
    private void IrA(int pagina)
    {
        PaginaActual = Math.Max(0, Math.Min(pagina, TotalPaginas - 1));
        if (Cambio != null) Cambio(this, EventArgs.Empty);
    }
}
