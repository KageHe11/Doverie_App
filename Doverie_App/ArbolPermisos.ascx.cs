using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;

/// Muestra la estructura Composite de los permisos: Familias con sus
/// subfamilias y Permisos. Lo usan Familias (todas las raíces), Usuarios y
/// Mis Empleados (lo que otorga el Perfil elegido).
public partial class ArbolPermisos : UserControl
{
    public void Mostrar(PerfilBE perfil)
    {
        Mostrar(perfil == null ? new List<FamiliaBE>() : perfil.Familias);
    }
    public void Mostrar(IEnumerable<FamiliaBE> familias)
    {
        tvArbol.Nodes.Clear();
        foreach (FamiliaBE Familia in familias)
        {
            tvArbol.Nodes.Add(CrearNodo(Familia));
        }
        lblVacio.Text = HttpUtility.HtmlEncode(Traductor.T("arbol.vacio"));
        lblVacio.Visible = !familias.Any();
    }
    private static TreeNode CrearNodo(ComponentePermiso componente)
    {
        TreeNode Nodo = new TreeNode((componente.EsHoja ? "🔑 " : "📁 ") + componente.Nombre);
        // Solo para ver: un click no hace postback.
        Nodo.SelectAction = TreeNodeSelectAction.None;
        if (!componente.EsHoja)
        {
            Nodo.Expanded = true;
            foreach (ComponentePermiso Hijo in componente.Hijos)
            {
                Nodo.ChildNodes.Add(CrearNodo(Hijo));
            }
        }
        return Nodo;
    }
}
