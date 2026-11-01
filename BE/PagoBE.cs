using System;

namespace BE
{
    /// Pago de una suscripción (tabla Pago).
    public class PagoBE
    {
        public const string EstadoPendiente = "Pendiente";
        public const string EstadoAprobado = "Aprobado";
        public const string EstadoRechazado = "Rechazado";
        /// Pago pendiente que se abandonó al iniciar otro.
        public const string EstadoAnulado = "Anulado";

        public const string ConceptoAlta = "Alta";
        public const string ConceptoRenovacion = "Renovacion";
        public const string ConceptoCambioPlan = "CambioPlan";

        public int Id { get; set; }
        public int SuscripcionId { get; set; }
        public int MedioPagoId { get; set; }
        /// Identificador de la operación en la pasarela de pago.
        public string ComprobanteUrl { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; }

        // No persistidos: vienen calculados en las consultas.
        public string MedioPago { get; set; }
        /// Titular de la cuenta.
        public int UsuarioId { get; set; }
        public int PlanId { get; set; }
        public string PlanNombre { get; set; }
        public string PlanTipo { get; set; }
    }
}
