using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public abstract class ComponentePermiso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool EsHoja { get; set; }
        public abstract IList<ComponentePermiso> Hijos { get; }
        public abstract void AgregarHijo(ComponentePermiso c);
        public abstract bool TienePermiso(int idPermiso);
    }
}
