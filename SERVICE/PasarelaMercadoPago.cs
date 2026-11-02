using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BE;

namespace SERVICE
{
    /// Implementación de IPasarelaPago con Mercado Pago Checkout Pro (API
    /// REST). IniciarPago crea una "preferencia" con el monto y la referencia
    /// del Pago y devuelve el link al checkout de Mercado Pago; al volver,
    /// ConsultarPago busca en Mercado Pago los pagos de esa referencia. No
    /// hacen falta webhooks: el resultado siempre se consulta a la API, así
    /// que funciona igual con la app en localhost.
    /// Checkout Pro no debita solo: las renovaciones las paga el Usuario
    /// dentro del plazo de gracia (DebitoAutomatico = false).
    ///
    /// Configuración (AppSettings.config): MercadoPagoAccessToken. Con las
    /// credenciales de producción los cobros son reales.
    public class PasarelaMercadoPago : IPasarelaPago
    {
        private static readonly HttpClient Cliente = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public bool DebitoAutomatico { get { return false; } }

        public string IniciarPago(int referencia, DateTime fecha, decimal monto, string descripcion, string urlRetorno)
        {
            Dictionary<string, object> Preferencia = new Dictionary<string, object>
            {
                { "items", new[] { new Dictionary<string, object>
                    {
                        { "id", "DOVERIE-SUSCRIPCION" },
                        { "title", descripcion },
                        { "quantity", 1 },
                        { "currency_id", "ARS" },
                        { "unit_price", monto }
                    } } },
                { "external_reference", ReferenciaExterna(referencia, fecha) },
                { "back_urls", new Dictionary<string, string> { { "success", urlRetorno }, { "failure", urlRetorno }, { "pending", urlRetorno } } },
                // Solo aprobado o rechazado: sin medios que quedan pendientes
                // días (efectivo en un local de pago).
                { "binary_mode", true },
                { "statement_descriptor", "DOVERIE" }
            };
            // La vuelta automática solo se acepta con https; en localhost el
            // Usuario vuelve con el botón "Volver al sitio" de Mercado Pago.
            if (urlRetorno.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) Preferencia.Add("auto_return", "approved");
            using (JsonDocument Respuesta = Enviar(HttpMethod.Post, "checkout/preferences", JsonSerializer.Serialize(Preferencia)))
            {
                // Con credenciales de prueba, MercadoPagoSandbox=true usa el
                // checkout de pruebas de Mercado Pago.
                bool Sandbox = string.Equals(ConfigurationManager.AppSettings["MercadoPagoSandbox"], "true", StringComparison.OrdinalIgnoreCase);
                return Respuesta.RootElement.GetProperty(Sandbox ? "sandbox_init_point" : "init_point").GetString();
            }
        }
        /// Aprobado si algún pago de la referencia se aprobó (el Usuario puede
        /// reintentar con otra tarjeta en el mismo checkout); pendiente si
        /// todavía no pagó o el pago está en proceso; si no, rechazado.
        public OperacionPagoBE ConsultarPago(int referencia, DateTime fecha)
        {
            string Ruta = "v1/payments/search?sort=date_created&criteria=desc&external_reference=" + Uri.EscapeDataString(ReferenciaExterna(referencia, fecha));
            using (JsonDocument Respuesta = Enviar(HttpMethod.Get, Ruta, null))
            {
                bool EnProceso = false;
                JsonElement? Rechazado = null;
                foreach (JsonElement Pago in Respuesta.RootElement.GetProperty("results").EnumerateArray())
                {
                    string Estado = Pago.GetProperty("status").GetString();
                    if (Estado == "approved") return Armar(referencia, OperacionPagoBE.EstadoAprobado, Pago);
                    if (Estado == "pending" || Estado == "in_process" || Estado == "authorized") EnProceso = true;
                    else if (Rechazado == null) Rechazado = Pago;
                }
                if (!EnProceso && Rechazado.HasValue) return Armar(referencia, OperacionPagoBE.EstadoRechazado, Rechazado.Value);
                return new OperacionPagoBE { Referencia = referencia, Estado = OperacionPagoBE.EstadoPendiente };
            }
        }
        public OperacionPagoBE CobrarRecurrente(int referencia, decimal monto, string descripcion)
        {
            throw new NotSupportedException("Mercado Pago Checkout Pro no hace débitos automáticos.");
        }

        /// Referencia del cobro en Mercado Pago: el Id y la fecha del Pago, para
        /// que un Id reusado (por ejemplo después de restaurar un backup) no
        /// encuentre los pagos de otro.
        private static string ReferenciaExterna(int referencia, DateTime fecha)
        {
            return "DOVERIE-" + referencia + "-" + fecha.ToString("yyyyMMddHHmmss");
        }
        private static OperacionPagoBE Armar(int referencia, string estado, JsonElement pago)
        {
            return new OperacionPagoBE
            {
                Referencia = referencia,
                Estado = estado,
                Operacion = "MP-" + pago.GetProperty("id").GetRawText(),
                Monto = pago.GetProperty("transaction_amount").GetDecimal()
            };
        }
        /// Llama a la API con el Access Token. Se ejecuta fuera del contexto de
        /// ASP.NET para poder esperar la respuesta sin bloquear el request.
        private static JsonDocument Enviar(HttpMethod metodo, string ruta, string cuerpo)
        {
            string Token = ConfigurationManager.AppSettings["MercadoPagoAccessToken"];
            if (string.IsNullOrWhiteSpace(Token)) throw new ConfigurationErrorsException("Falta configurar MercadoPagoAccessToken.");
            return Task.Run(async () =>
            {
                using (HttpRequestMessage Pedido = new HttpRequestMessage(metodo, ruta))
                {
                    Pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                    if (cuerpo != null) Pedido.Content = new StringContent(cuerpo, Encoding.UTF8, "application/json");
                    using (HttpResponseMessage Respuesta = await Cliente.SendAsync(Pedido).ConfigureAwait(false))
                    {
                        string Texto = await Respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!Respuesta.IsSuccessStatusCode)
                        {
                            throw new InvalidOperationException("Mercado Pago respondió " + (int)Respuesta.StatusCode + ": " + Texto);
                        }
                        return JsonDocument.Parse(Texto);
                    }
                }
            }).GetAwaiter().GetResult();
        }
    }
}
