using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class FamiliaBE : ComponentePermiso
    {
        private IList<ComponentePermiso> _hijos;
        public FamiliaBE()
        {
            EsHoja = false;
            _hijos = new List<ComponentePermiso>();
        }

        public override IList<ComponentePermiso> Hijos
        {
            get
            {
                return _hijos.ToArray();
            }
        }
        public override void AgregarHijo(ComponentePermiso c)
        {
            _hijos.Add(c);
        }
        public override bool TienePermiso(int idPermiso)
        {
            foreach (ComponentePermiso Hijo in _hijos)
            {
                if (Hijo.TienePermiso(idPermiso))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
