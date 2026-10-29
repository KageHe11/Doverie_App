using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class PerfilBE
    {
        public int Id { get; set; }
        public string NombrePerfil { get; set; }
        public bool Activo { get; set; }
        public bool AsignablePorEmpresa { get; set; }
        public IList<FamiliaBE> Familias { get; set; }
        public PerfilBE()
        {
            Familias = new List<FamiliaBE>();
        }
        public bool TienePermiso(int idPermiso)
        {
            // Un Perfil dado de baja (CU-002-008) no otorga ningún permiso.
            if (!Activo) return false;
            foreach (FamiliaBE Familia in Familias)
            {
                if (Familia.TienePermiso(idPermiso))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
