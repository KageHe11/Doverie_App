using System;

namespace BE
{
    /// Datos que el GestorEventos le pasa a cada observador junto con el
    /// evento (patrón Observer): a quién notificar y el dato del evento.
    public class DatosEventoBE
    {
        public UsuarioBE Destinatario { get; set; }
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; }
    }
}
