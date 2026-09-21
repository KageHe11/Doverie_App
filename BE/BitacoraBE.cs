using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BitacoraBE
    {
        public int Id { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; }
        public int? UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
    }
}
