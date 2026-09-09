using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
    public class FamiliaBLL
    {
        public List<FamiliaBE> Listar()
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            return FamiliaDal.Listar();
        }
        /// Trae una Familia puntual con su árbol de hijos completo
        /// (Permisos y/o Sub-Familias ya resueltos), para editarla.
        public FamiliaBE ObtenerDatos(int id)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            Dictionary<int, ComponentePermiso> Arbol = FamiliaDal.ArmarArbol();
            if (!Arbol.ContainsKey(id)) return null;
            return Arbol[id] as FamiliaBE;
        }
        public void Registrar(FamiliaBE familia)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            FamiliaDal.Registrar(familia);
        }
        public void ActualizarDatos(FamiliaBE familia)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            FamiliaDal.ActualizarDatos(familia);
        }
        public void Eliminar(int id)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            FamiliaDal.Eliminar(id);
        }
        /// Devuelve el árbol completo (todas las Familias y Permisos, ya
        /// enlazados entre sí). Lo usan, por ejemplo, las pantallas donde
        /// hay que elegir qué componentes forman parte de una Familia o
        /// de un Perfil.
        public Dictionary<int, ComponentePermiso> ArmarArbol()
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            return FamiliaDal.ArmarArbol();
        }
        /// <summary>
        /// Devuelve solo las Familias "raíz" (las que no son sub-Familia de ninguna
        /// otra), cada una con su árbol completo ya armado. Sirve para mostrar el
        /// bosque completo sin repetir una misma sub-Familia en varios lugares.
        /// </summary>
        public List<FamiliaBE> ObtenerRaices()
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            Dictionary<int, ComponentePermiso> Arbol = FamiliaDal.ArmarArbol();
            HashSet<int> UsadasComoHijas = new HashSet<int>();
            foreach (ComponentePermiso Nodo in Arbol.Values)
            {
                FamiliaBE Familia = Nodo as FamiliaBE;
                if (Familia != null)
                {
                    foreach (ComponentePermiso Hijo in Familia.Hijos)
                    {
                        if (!Hijo.EsHoja)
                        {
                            UsadasComoHijas.Add(Hijo.Id);
                        }
                    }
                }
            }
            List<FamiliaBE> Raices = new List<FamiliaBE>();
            foreach (ComponentePermiso Nodo in Arbol.Values)
            {
                FamiliaBE Familia = Nodo as FamiliaBE;
                if (Familia != null && !UsadasComoHijas.Contains(Familia.Id))
                {
                    Raices.Add(Familia);
                }
            }
            return Raices;
        }
        /// Verifica que, al guardar los componentes de una Familia, ninguna
        /// Sub-Familia seleccionada termine formando un ciclo (que la Familia
        /// editada aparezca, directa o indirectamente, dentro de sí misma).
        /// Devuelve null si está todo bien, o el nombre de la Sub-Familia
        /// conflictiva si detecta un ciclo.
        public string ValidarSinCiclos(FamiliaBE familiaEditada)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            Dictionary<int, ComponentePermiso> Arbol = FamiliaDal.ArmarArbol();
            foreach (ComponentePermiso Hijo in familiaEditada.Hijos)
            {
                if (Hijo.EsHoja) continue; // los Permisos nunca generan ciclos
                if (!Arbol.ContainsKey(Hijo.Id)) continue;
                FamiliaBE SubFamiliaReal = (FamiliaBE)Arbol[Hijo.Id];
                if (SubFamiliaReal.Id == familiaEditada.Id || EsDescendiente(familiaEditada.Id, SubFamiliaReal))
                {
                    return SubFamiliaReal.Nombre;
                }
            }
            return null;
        }
        private bool EsDescendiente(int idBuscado, FamiliaBE nodo)
        {
            foreach (ComponentePermiso Hijo in nodo.Hijos)
            {
                if (Hijo.EsHoja) continue;

                FamiliaBE SubFamilia = (FamiliaBE)Hijo;
                if (SubFamilia.Id == idBuscado) return true;
                if (EsDescendiente(idBuscado, SubFamilia)) return true;
            }
            return false;
        }
    }
}
