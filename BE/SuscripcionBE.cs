using System;

namespace BE
{
    /// Suscripción de una cuenta a un Plan (tabla Suscripcion). UsuarioId es
    /// el titular: la Empresa o el Usuario independiente.
    public class SuscripcionBE
    {
        // Códigos de la columna Estado.
        public const int EstadoPendiente = 1;
        /// Se renueva automáticamente en FechaRenovacion.
        public const int EstadoActiva = 2;
        /// No se renueva: el acceso sigue hasta FechaRenovacion.
        public const int EstadoCancelada = 3;
        public const int EstadoVencida = 4;
        /// El pago inicial no se aprobó.
        public const int EstadoRechazada = 5;
        /// Se cambió por otro plan.
        public const int EstadoReemplazada = 6;

        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PlanId { get; set; }
        public int Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        /// Fin del período abonado: ahí se renueva o vence.
        public DateTime FechaRenovacion { get; set; }

        // No persistidos: los completa la BLL.
        public PlanBE Plan { get; set; }
        /// Si la renovación quedó pendiente de pago (pasarela sin débito
        /// automático), hasta cuándo se mantiene el acceso para pagarla.
        public DateTime? FinGracia { get; set; }

        /// Da acceso a las funcionalidades del plan: dentro del período
        /// abonado o, con la renovación pendiente, dentro del plazo de gracia.
        public bool Vigente
        {
            get
            {
                if (Estado != EstadoActiva && Estado != EstadoCancelada) return false;
                return FechaRenovacion > DateTime.Now || (FinGracia.HasValue && FinGracia.Value > DateTime.Now);
            }
        }
    }
}
