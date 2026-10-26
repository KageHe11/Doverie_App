using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;
using System.Web.UI.WebControls;

public partial class Familias : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerMenuAdmin"; } }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvFamilias.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvFamilias.Columns[1].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnAgregar.Text = Traductor.T("familias.agregar");
        btnGuardar.Text = Traductor.T("comun.guardar");
        if (!IsPostBack)
        {
            CargarGrilla();
            CargarArbol();

        }
    }
    private void CargarGrilla()
    {
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        gvFamilias.DataSource = FamiliaBll.Listar();
        gvFamilias.DataBind();
    }
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        hdnId.Value = "0";
        txtNombre.Text = "";
        litTituloModal.Text = Traductor.T("familias.tituloAgregar");
        lblErrorModal.Visible = false;
        CargarListas(idFamiliaActual: 0, idsSeleccionados: new List<string>());
        AbrirModal();
    }
    protected void gvFamilias_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        if (e.CommandName == "Editar")
        {
            FamiliaBE Familia = FamiliaBll.ObtenerDatos(Id);
            hdnId.Value = Familia.Id.ToString();
            txtNombre.Text = Familia.Nombre;
            litTituloModal.Text = Traductor.T("familias.tituloEditar");
            lblErrorModal.Visible = false;
            List<string> Seleccionados = new List<string>();
            foreach (ComponentePermiso Hijo in Familia.Hijos)
            {
                Seleccionados.Add((Hijo.EsHoja ? "P|" : "F|") + Hijo.Id);
            }
            CargarListas(idFamiliaActual: Id, idsSeleccionados: Seleccionados);
            AbrirModal();
        }
        else if (e.CommandName == "Eliminar")
        {
            FamiliaBll.Eliminar(Id);
            CargarGrilla();
        }
    }
    /// Llena las dos listas (Disponibles/Seleccionados) a partir de todos
    /// los Permisos y Familias existentes. idFamiliaActual se excluye de
    /// la lista de Familias disponibles, para no permitir que una Familia
    /// se contenga a sí misma.
    private void CargarListas(int idFamiliaActual, List<string> idsSeleccionados)
    {
        lstDisponibles.Items.Clear();
        lstSeleccionados.Items.Clear();
        PermisoBLL PermisoBll = new PermisoBLL();
        foreach (PermisoBE Permiso in PermisoBll.Listar())
        {
            string Valor = "P|" + Permiso.Id;
            ListItem Item = new ListItem("[" + Traductor.T("familias.tipoPermiso") + "] " + Permiso.Nombre, Valor);
            if (idsSeleccionados.Contains(Valor))
                lstSeleccionados.Items.Add(Item);
            else
                lstDisponibles.Items.Add(Item);
        }
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        foreach (FamiliaBE Familia in FamiliaBll.Listar())
        {
            if (Familia.Id == idFamiliaActual) continue; // evita autorreferencia directa
            string Valor = "F|" + Familia.Id;
            ListItem Item = new ListItem("[" + Traductor.T("familias.tipoFamilia") + "] " + Familia.Nombre, Valor);
            if (idsSeleccionados.Contains(Valor))
                lstSeleccionados.Items.Add(Item);
            else
                lstDisponibles.Items.Add(Item);
        }
    }
    protected void btnAgregarComponente_Click(object sender, EventArgs e)
    {
        MoverItems(lstDisponibles, lstSeleccionados);
    }
    protected void btnQuitarComponente_Click(object sender, EventArgs e)
    {
        MoverItems(lstSeleccionados, lstDisponibles);
    }
    private void MoverItems(ListBox Origen, ListBox Destino)
    {
        List<ListItem> Seleccionados = new List<ListItem>();
        foreach (ListItem Item in Origen.Items)
        {
            if (Item.Selected) Seleccionados.Add(Item);
        }
        foreach (ListItem Item in Seleccionados)
        {
            Origen.Items.Remove(Item);
            Item.Selected = false;
            Destino.Items.Add(Item);
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        int Id = Convert.ToInt32(hdnId.Value);
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            lblErrorModal.Text = Traductor.T("comun.nombreObligatorio");
            lblErrorModal.Visible = true;
            AbrirModal();
            return;
        }
        FamiliaBE Familia = new FamiliaBE { Id = Id, Nombre = txtNombre.Text };
        foreach (ListItem Item in lstSeleccionados.Items)
        {
            string[] Partes = Item.Value.Split('|');
            string Tipo = Partes[0];
            int IdComponente = Convert.ToInt32(Partes[1]);
            ComponentePermiso Hijo = Tipo == "P"
                ? (ComponentePermiso)new PermisoBE { Id = IdComponente }
                : (ComponentePermiso)new FamiliaBE { Id = IdComponente };
            Familia.AgregarHijo(Hijo);
        }
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        if (Id != 0)
        {
            string NombreConflicto = FamiliaBll.ValidarSinCiclos(Familia);
            if (NombreConflicto != null)
            {
                // [ERROR-046] Referencia circular en la jerarquía de Familias.
                lblErrorModal.Text = Traductor.T("familias.errorCiclo", NombreConflicto);
                lblErrorModal.Visible = true;
                AbrirModal();
                return;
            }
        }
        if (Id == 0)
        {
            FamiliaBll.Registrar(Familia);
        }
        else
        {
            FamiliaBll.ActualizarDatos(Familia);
        }
        CargarGrilla();
        CargarArbol();
        CerrarModal();
    }
    private void CargarArbol()
    {
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        List<FamiliaBE> Raices = FamiliaBll.ObtenerRaices();

        tvArbol.Nodes.Clear();
        foreach (FamiliaBE Raiz in Raices)
        {
            TreeNode NodoRaiz = new TreeNode("📁 " + Raiz.Nombre);
            AgregarHijosAlNodo(NodoRaiz, Raiz);
            NodoRaiz.Expanded = true;
            tvArbol.Nodes.Add(NodoRaiz);
        }
    }
    private void AgregarHijosAlNodo(TreeNode NodoPadre, FamiliaBE Familia)
    {
        foreach (ComponentePermiso Hijo in Familia.Hijos)
        {
            if (Hijo.EsHoja)
            {
                TreeNode NodoHoja = new TreeNode("🔑 " + Hijo.Nombre);
                NodoPadre.ChildNodes.Add(NodoHoja);
            }
            else
            {
                FamiliaBE SubFamilia = (FamiliaBE)Hijo;
                TreeNode NodoSub = new TreeNode("📁 " + SubFamilia.Nombre);
                NodoSub.Expanded = true;
                AgregarHijosAlNodo(NodoSub, SubFamilia);
                NodoPadre.ChildNodes.Add(NodoSub);
            }
        }
    }
    private void AbrirModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModal",
            "var m = new bootstrap.Modal(document.getElementById('modalFamilia')); m.show();", true);
    }
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalFamilia'));
          if (m) m.hide();
          document.body.classList.remove('modal-open');
          document.body.style.removeProperty('padding-right');
          document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}