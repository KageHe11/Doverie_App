using System;
using System.Xml.Serialization;

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
        /// Eliminación lógica: el registro sigue guardado, marcado con
        /// quién y cuándo lo eliminó, y se puede restaurar.
        public bool Eliminado { get; set; }
        public DateTime? FechaEliminacion { get; set; }
        public string NombreEliminadoPor { get; set; }
        /// true si viene del archivo histórico (más de 30 días): ahí ya no
        /// se puede eliminar ni restaurar. No se guarda en el XML.
        [XmlIgnore]
        public bool Archivado { get; set; }
    }
}
