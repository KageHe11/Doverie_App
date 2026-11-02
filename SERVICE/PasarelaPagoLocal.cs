using System;
using System.Collections.Generic;
using BE;

namespace SERVICE
{
    /// Implementación local de IPasarelaPago, para desarrollo y pruebas sin
    /// cuenta de Mercado Pago. El checkout es PagoSimulado.aspx, donde se
    /// elige aprobar o rechazar el pago. Los cobros se guardan en memoria:
    /// si la aplicación se reinicia, un cobro sin resolver queda pendiente y
    /// el Usuario tiene que iniciar el pago de nuevo.
    /// Los débitos automáticos se aprueban siempre.
    public class PasarelaPagoLocal : IPasarelaPago
    {
        private static readonly object Bloqueo = new object();
        private static readonly Dictionary<int, OperacionPagoBE> Cobros = new Dictionary<int, OperacionPagoBE>();

        public bool DebitoAutomatico { get { return true; } }
        public string IniciarPago(int referencia, DateTime fecha, decimal monto, string descripcion, string urlRetorno)
        {
            lock (Bloqueo)
            {
                Cobros[referencia] = new OperacionPagoBE
                {
                    Referencia = referencia,
                    Estado = OperacionPagoBE.EstadoPendiente,
                    Monto = monto,
                    Descripcion = descripcion,
                    UrlRetorno = urlRetorno
                };
            }
            return "~/PagoSimulado.aspx?ref=" + referencia;
        }
        public OperacionPagoBE ConsultarPago(int referencia, DateTime fecha)
        {
            lock (Bloqueo)
            {
                OperacionPagoBE Cobro;
                if (!Cobros.TryGetValue(referencia, out Cobro))
                {
                    return new OperacionPagoBE { Referencia = referencia, Estado = OperacionPagoBE.EstadoPendiente };
                }
                return Copiar(Cobro);
            }
        }
        public OperacionPagoBE CobrarRecurrente(int referencia, decimal monto, string descripcion)
        {
            OperacionPagoBE Cobro = new OperacionPagoBE
            {
                Referencia = referencia,
                Estado = OperacionPagoBE.EstadoAprobado,
                Operacion = NuevaOperacion(referencia),
                Monto = monto,
                Descripcion = descripcion
            };
            lock (Bloqueo)
            {
                Cobros[referencia] = Cobro;
            }
            return Copiar(Cobro);
        }
        /// Para el checkout simulado: los datos del cobro a mostrar, o null
        /// si no existe.
        public OperacionPagoBE ObtenerCobro(int referencia)
        {
            lock (Bloqueo)
            {
                OperacionPagoBE Cobro;
                return Cobros.TryGetValue(referencia, out Cobro) ? Copiar(Cobro) : null;
            }
        }
        /// Para el checkout simulado: el Usuario aprobó o rechazó el pago.
        /// Un cobro ya resuelto no cambia.
        public void Resolver(int referencia, bool aprobado)
        {
            lock (Bloqueo)
            {
                OperacionPagoBE Cobro;
                if (!Cobros.TryGetValue(referencia, out Cobro) || Cobro.Estado != OperacionPagoBE.EstadoPendiente) return;
                Cobro.Estado = aprobado ? OperacionPagoBE.EstadoAprobado : OperacionPagoBE.EstadoRechazado;
                Cobro.Operacion = NuevaOperacion(referencia);
            }
        }
        private static string NuevaOperacion(int referencia)
        {
            return "SIM-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + referencia;
        }
        private static OperacionPagoBE Copiar(OperacionPagoBE cobro)
        {
            return new OperacionPagoBE
            {
                Referencia = cobro.Referencia,
                Estado = cobro.Estado,
                Operacion = cobro.Operacion,
                Monto = cobro.Monto,
                Descripcion = cobro.Descripcion,
                UrlRetorno = cobro.UrlRetorno
            };
        }
    }
}
