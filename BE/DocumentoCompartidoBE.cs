using System;

namespace BE
{
    /// Enlace de solo lectura a un documento, para un destinatario externo
    /// (CU-001-005 / CU-001-018). El token del enlace no se guarda: solo
    /// su SHA-256.
    public class DocumentoCompartidoBE
    {
        public int Id { get; set; }
        public int DocumentoId { get; set; }
        /// Usuario que compartió el documento.
        public int UsuarioId { get; set; }
        public string EmailDestinatario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public bool Revocado { get; set; }
        public int CantidadAccesos { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        // No persistido: viene calculado en las consultas.
        public string NombreUsuario { get; set; }

        /// Si el enlace todavía permite el acceso.
        public bool Vigente
        {
            get { return !Revocado && FechaVencimiento > DateTime.Now; }
        }
    }
}
