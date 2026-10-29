using System;

namespace BE
{
    /// Notificación entregada dentro de la plataforma (canal Push).
    public class NotificacionRecibidaBE
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Evento { get; set; }
        /// Dato del evento (ej: nombre del documento). El texto final se arma
        /// al mostrarla, en el idioma de quien la lee.
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
    }
}
