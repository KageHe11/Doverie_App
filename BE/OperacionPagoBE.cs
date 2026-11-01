namespace BE
{
    /// Estado de un cobro según la pasarela de pago.
    public class OperacionPagoBE
    {
        public const string EstadoPendiente = "Pendiente";
        public const string EstadoAprobado = "Aprobado";
        public const string EstadoRechazado = "Rechazado";

        /// Id del Pago en Doverie (la "referencia externa" para la pasarela).
        public int Referencia { get; set; }
        public string Estado { get; set; }
        /// Identificador de la operación en la pasarela. Null si no se cobró.
        public string Operacion { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
        /// A dónde vuelve el Usuario al terminar en el checkout.
        public string UrlRetorno { get; set; }
    }
}
