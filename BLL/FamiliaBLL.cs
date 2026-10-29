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
        /// CU-002-015 Alta Familia: nombre obligatorio [ERROR-004] y sin
        /// repetir [ERROR-045].
        public ResultadoAbm Registrar(FamiliaBE familia)
        {
            ResultadoAbm Validacion = ValidarNombre(familia);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new DAL.FamiliaDAL().Registrar(familia);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-016 Modificar Familia [ERROR-004 / ERROR-047]. Las
        /// referencias circulares [ERROR-046] se validan con ValidarSinCiclos.
        public ResultadoAbm ActualizarDatos(FamiliaBE familia)
        {
            ResultadoAbm Validacion = ValidarNombre(familia);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new DAL.FamiliaDAL().ActualizarDatos(familia);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-017 Baja Familia. No se puede eliminar si está asignada a
        /// algún Perfil o contenida en otra Familia [ERROR-048]: dejaría
        /// referencias rotas en el árbol de permisos.
        public ResultadoAbm Eliminar(int id)
        {
            DAL.FamiliaDAL FamiliaDal = new DAL.FamiliaDAL();
            if (FamiliaDal.ContarUsos(id) > 0) return ResultadoAbm.EnUso;
            try
            {
                FamiliaDal.Eliminar(id);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        private static ResultadoAbm ValidarNombre(FamiliaBE familia)
        {
            familia.Nombre = (familia.Nombre ?? "").Trim();
            if (familia.Nombre.Length == 0 || familia.Nombre.Length > 50) return ResultadoAbm.DatosInvalidos;
            if (new DAL.FamiliaDAL().ExistePorNombre(familia.Nombre, familia.Id)) return ResultadoAbm.Duplicado;
            return ResultadoAbm.Exitoso;
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
