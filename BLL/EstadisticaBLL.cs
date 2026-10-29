using BE;
using DAL;
using System;

namespace BLL
{
    /// CU-002-012 Ver Estadísticas de la Plataforma y la lógica detrás de
    /// los webservices WS_Documentos, WS_Usuarios y WS_Blockchain.
    /// Los períodos son de días completos: [desde 00:00, hasta 23:59:59].
    public class EstadisticaBLL
    {
        public EstadisticasBE ObtenerResumen()
        {
            return new EstadisticaDAL().ObtenerResumen();
        }
        public int ObtenerDocumentosPorPeriodo(DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return new EstadisticaDAL().ObtenerDocumentosPorPeriodo(desde.Date, hasta.Date.AddDays(1));
        }
        public int ObtenerVerificacionesPorPeriodo(DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return new EstadisticaDAL().ObtenerVerificacionesPorPeriodo(desde.Date, hasta.Date.AddDays(1));
        }
        public int ObtenerUsuariosPorPeriodo(DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return new EstadisticaDAL().ObtenerUsuariosPorPeriodo(desde.Date, hasta.Date.AddDays(1));
        }
        /// Todavía no hay suscripciones (CU-001-008/017): no hay plan más
        /// contratado. Cuando existan las tablas Plan y Suscripcion, se
        /// resuelve con un SP_Estadistica_ObtenerPlanMasContratado.
        public PlanBE ObtenerPlanMasContratado(DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return null;
        }
        public int ObtenerOperacionesPorPeriodo(DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return new EstadisticaDAL().ObtenerOperacionesPorPeriodo(desde.Date, hasta.Date.AddDays(1));
        }
        public int ObtenerTransaccionesPorUsuario(int usuarioId, DateTime desde, DateTime hasta)
        {
            ValidarPeriodo(desde, hasta);
            return new EstadisticaDAL().ObtenerTransaccionesPorUsuario(usuarioId, desde.Date, hasta.Date.AddDays(1));
        }
        private static void ValidarPeriodo(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
            {
                throw new ArgumentException("El período es inválido: 'desde' es posterior a 'hasta'.");
            }
        }
    }
}
