using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class PermisoBE : ComponentePermiso
    {
        public string Descripcion { get; set; }
        public PermisoBE()
        {
            EsHoja = true;
        }
        public override IList<ComponentePermiso> Hijos
        {
            get
            {
                return null;
            }
        }
        public override void AgregarHijo(ComponentePermiso c)
        {
        }
        public override bool TienePermiso(int idPermiso)
        {
            return Id == idPermiso;
        }
    }
}
