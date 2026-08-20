using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class UsuarioBE
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Gmail { get; set; }
        public string Dni { get; set; }
        public string Telefono { get; set; }
        public bool Empleado { get; set; }
        public bool Empresa { get; set; }
        public bool Independiente { get; set; }
        public bool Local { get; set; }
        public bool Suspendido { get; set; }
        public string Motivo { get; set; }
        public int Intentos { get; set; }
        public PerfilBE Perfil { get; set; }

    }
}
