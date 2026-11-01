using BE;

namespace SERVICE
{
    /// Contrato de la pasarela de cobro de las suscripciones. La
    /// implementación actual es un checkout simulado (PasarelaPagoLocal);
    /// la de Mercado Pago se agrega como otra implementación, con el mismo
    /// recorrido: se crea el cobro, el Usuario paga en el checkout de la
    /// pasarela y vuelve a Doverie, que consulta el resultado.
    public interface IPasarelaPago
    {
        /// Crea el cobro de un pago y devuelve la URL del checkout al que
        /// se redirige al Usuario. Al terminar, la pasarela lo devuelve a
        /// urlRetorno.
        string IniciarPago(int referencia, decimal monto, string descripcion, string urlRetorno);
        /// Estado del cobro de una referencia. Es la única fuente válida del
        /// resultado: nunca se confía en lo que diga la URL de retorno.
        OperacionPagoBE ConsultarPago(int referencia);
        /// Débito automático con el medio registrado (renovaciones).
        OperacionPagoBE CobrarRecurrente(int referencia, decimal monto, string descripcion);
    }
}
