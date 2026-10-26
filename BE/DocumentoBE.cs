using System;
using System.IO;

namespace BE
{
    public class DocumentoBE
    {
        public const string EstadoActivo = "Activo";
        public const string EstadoEliminado = "Eliminado";

        public int Id { get; set; }
        /// Usuario que subió el documento.
        public int UsuarioId { get; set; }
        /// Nombre original del archivo (solo para mostrar y descargar).
        public string Nombre { get; set; }
        /// Ubicación interna del archivo en el almacenamiento (uso exclusivo
        /// del sistema; nunca se muestra al usuario).
        public string UrlDocumento { get; set; }
        /// SHA-256 del contenido al momento de subirlo.
        public string Hash { get; set; }
        public string TransaccionBlockchain { get; set; }
        public string NFTId { get; set; }
        public string Estado { get; set; }
        public DateTime FechaSubida { get; set; }

        // No persistidos: vienen calculados en las consultas.
        public string NombreUsuario { get; set; }
        /// Cuenta dueña del documento: la Empresa, o el Usuario independiente.
        public int CuentaId { get; set; }

        /// Tipo de documento según su extensión (ej: "PDF").
        public string Tipo
        {
            get
            {
                string Extension = Path.GetExtension(Nombre ?? "");
                return Extension.Length > 1 ? Extension.Substring(1).ToUpperInvariant() : "";
            }
        }
    }
}
