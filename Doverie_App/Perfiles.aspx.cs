using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;

public partial class Perfiles : PaginaBase
{
    protected override string PermisoRequerido { get { return "VerMenuAdmin"; } }
    /// Los encabezados de la grilla se traducen en Init, antes de que la
    /// grilla se reconstruya desde el ViewState en los postbacks.
    protected void Page_Init(object sender, EventArgs e)
    {
        gvPerfiles.Columns[0].HeaderText = Traductor.T("comun.nombre");
        gvPerfiles.Columns[1].HeaderText = Traductor.T("comun.estado");
        gvPerfiles.Columns[2].HeaderText = Traductor.T("comun.acciones");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        btnAgregar.Text = Traductor.T("perfiles.agregar");
        btnGuardar.Text = Traductor.T("comun.guardar");
        if (!IsPostBack)
        {
            CargarGrilla();
        }
    }
    private void CargarGrilla()
    {
        PerfilBLL PerfilBll = new PerfilBLL();
        gvPerfiles.DataSource = PerfilBll.Listar();
        gvPerfiles.DataBind();
    }
    protected void btnAgregar_Click(object sender, EventArgs e)
    {
        hdnId.Value = "0";
        txtNombrePerfil.Text = "";
        chkAsignablePorEmpresa.Checked = false;
        litTituloModal.Text = Traductor.T("perfiles.tituloAgregar");
        lblErrorModal.Visible = false;
        CargarListasFamilias(new List<int>());
        AbrirModal();
    }
    protected void gvPerfiles_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int Id = Convert.ToInt32(e.CommandArgument);
        PerfilBLL PerfilBll = new PerfilBLL();
        if (e.CommandName == "Editar")
        {
            PerfilBE Perfil = PerfilBll.ObtenerDatos(Id);
            hdnId.Value = Perfil.Id.ToString();
            txtNombrePerfil.Text = Perfil.NombrePerfil;
            // Sin esto, guardar la edición pisaba AsignablePorEmpresa con
            // lo que hubiera quedado tildado de la última vez.
            chkAsignablePorEmpresa.Checked = Perfil.AsignablePorEmpresa;
            litTituloModal.Text = Traductor.T("perfiles.tituloEditar");
            lblErrorModal.Visible = false;
            List<int> IdsSeleccionados = new List<int>();
            foreach (FamiliaBE Familia in Perfil.Familias)
            {
                IdsSeleccionados.Add(Familia.Id);
            }
            CargarListasFamilias(IdsSeleccionados);
            AbrirModal();
        }
        else if (e.CommandName == "ToggleEstado")
        {
            PerfilBE Perfil = PerfilBll.ObtenerDatos(Id);
            Perfil.Activo = !Perfil.Activo;
            PerfilBll.ActualizarEstado(Perfil);
            CargarGrilla();
        }
    }
    /// Llena las dos listas (Disponibles/Seleccionadas) con todas las
    /// Familias existentes, separándolas según si ya están asignadas
    /// a este Perfil o no.
    private void CargarListasFamilias(List<int> idsSeleccionados)
    {
        lstFamiliasDisponibles.Items.Clear();
        lstFamiliasSeleccionadas.Items.Clear();
        FamiliaBLL FamiliaBll = new FamiliaBLL();
        foreach (FamiliaBE Familia in FamiliaBll.Listar())
        {
            ListItem Item = new ListItem(Familia.Nombre, Familia.Id.ToString());
            if (idsSeleccionados.Contains(Familia.Id))
                lstFamiliasSeleccionadas.Items.Add(Item);
            else
                lstFamiliasDisponibles.Items.Add(Item);
        }
    }
    protected void btnAgregarFamilia_Click(object sender, EventArgs e)
    {
        MoverItems(lstFamiliasDisponibles, lstFamiliasSeleccionadas);
    }
    protected void btnQuitarFamilia_Click(object sender, EventArgs e)
    {
        MoverItems(lstFamiliasSeleccionadas, lstFamiliasDisponibles);
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
        PerfilBLL PerfilBll = new PerfilBLL();
        if (string.IsNullOrWhiteSpace(txtNombrePerfil.Text))
        {
            lblErrorModal.Text = Traductor.T("comun.nombreObligatorio");
            lblErrorModal.Visible = true;
            AbrirModal();
            return;
        }
        if (Id == 0)
        {
            PerfilBE NuevoPerfil = new PerfilBE
            {
                NombrePerfil = txtNombrePerfil.Text,
                AsignablePorEmpresa = chkAsignablePorEmpresa.Checked
            };
            PerfilBll.Registrar(NuevoPerfil);
            foreach (ListItem Item in lstFamiliasSeleccionadas.Items)
            {
                NuevoPerfil.Familias.Add(new FamiliaBE { Id = Convert.ToInt32(Item.Value) });
            }
            PerfilBll.ActualizarPermisos(NuevoPerfil);
        }
        else
        {
            PerfilBE Perfil = new PerfilBE
            {
                Id = Id,
                NombrePerfil = txtNombrePerfil.Text,
                AsignablePorEmpresa = chkAsignablePorEmpresa.Checked
            };
            PerfilBll.ActualizarDatos(Perfil);
            foreach (ListItem Item in lstFamiliasSeleccionadas.Items)
            {
                Perfil.Familias.Add(new FamiliaBE { Id = Convert.ToInt32(Item.Value) });
            }
            PerfilBll.ActualizarPermisos(Perfil);
        }
        CargarGrilla();
        CerrarModal();
    }
    private void AbrirModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "abrirModal",
            "var m = new bootstrap.Modal(document.getElementById('modalPerfil')); m.show();", true);
    }
    private void CerrarModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModal",
            @"var m = bootstrap.Modal.getInstance(document.getElementById('modalPerfil'));
              if (m) m.hide();
              document.body.classList.remove('modal-open');
              document.body.style.removeProperty('padding-right');
              document.querySelectorAll('.modal-backdrop').forEach(function(b) { b.remove(); });",
            true);
    }
}