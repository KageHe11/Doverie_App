using System;
using BE;

namespace SERVICE
{
    /// Contrato de la pasarela de cobro de las suscripciones: el checkout
    /// simulado (PasarelaPagoLocal) o Mercado Pago (PasarelaMercadoPago). El
    /// recorrido es el mismo: se crea el cobro, el Usuario paga en el
    /// checkout de la pasarela y vuelve a Doverie, que consulta el resultado.
    /// Cada cobro se identifica por el Id del Pago y su fecha de creación:
    /// juntos no se repiten aunque la base se restaure y los Id se reusen.
    public interface IPasarelaPago
    {
        /// Crea el cobro de un pago y devuelve la URL del checkout al que
        /// se redirige al Usuario. Al terminar, la pasarela lo devuelve a
        /// urlRetorno.
        string IniciarPago(int referencia, DateTime fecha, decimal monto, string descripcion, string urlRetorno);
        /// Estado del cobro de una referencia. Es la única fuente válida del
        /// resultado: nunca se confía en lo que diga la URL de retorno.
        OperacionPagoBE ConsultarPago(int referencia, DateTime fecha);
        /// Si la pasarela puede debitar sola las renovaciones con el medio
        /// registrado. Si no, el Usuario las paga desde Suscripción, dentro
        /// del plazo de gracia.
        bool DebitoAutomatico { get; }
        /// Débito automático con el medio registrado (renovaciones). Solo si
        /// DebitoAutomatico.
        OperacionPagoBE CobrarRecurrente(int referencia, decimal monto, string descripcion);
    }
}
