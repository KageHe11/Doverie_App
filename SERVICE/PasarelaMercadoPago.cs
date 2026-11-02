using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BE;

namespace SERVICE
{
    /// Implementación de IPasarelaPago con Mercado Pago Checkout Pro sobre la
    /// API de Orders. IniciarPago crea una order con el monto y la referencia
    /// del Pago y devuelve su checkout_url; Doverie guarda el Id de la order y,
    /// al volver, ConsultarPago la lee de la API. No hacen falta webhooks: el
    /// resultado siempre se consulta a Mercado Pago, así que funciona igual
    /// con la app en localhost.
    /// Checkout Pro no debita solo: las renovaciones las paga el Usuario
    /// dentro del plazo de gracia (DebitoAutomatico = false).
    ///
    /// Configuración (AppSettings.config): MercadoPagoAccessToken. Con las
    /// credenciales de producción los cobros son reales. MercadoPagoSandbox =
    /// true lleva al checkout de pruebas (credenciales de prueba).
    public class PasarelaMercadoPago : IPasarelaPago
    {
        private static readonly HttpClient Cliente = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public bool DebitoAutomatico { get { return false; } }

        public string IniciarPago(int referencia, DateTime fecha, decimal monto, string descripcion, string urlRetorno, out string cobro)
        {
            string Monto = monto.ToString("0.00", CultureInfo.InvariantCulture);
            Dictionary<string, object> Online = new Dictionary<string, object>
            {
                { "success_url", urlRetorno },
                { "failure_url", urlRetorno },
                { "pending_url", urlRetorno }
            };
            // La vuelta automática solo se pide con https; en localhost el
            // Usuario vuelve con el botón "Volver al sitio" de Mercado Pago.
            if (urlRetorno.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) Online.Add("auto_return", "approved");
            Dictionary<string, object> Order = new Dictionary<string, object>
            {
                { "type", "online" },
                { "processing_mode", "manual" },
                { "total_amount", Monto },
                { "external_reference", ReferenciaExterna(referencia, fecha) },
                { "description", descripcion },
                { "items", new[] { new Dictionary<string, object>
                    {
                        { "title", descripcion },
                        { "unit_price", Monto },
                        { "quantity", 1 }
                    } } },
                { "config", new Dictionary<string, object> { { "online", Online } } }
            };
            using (JsonDocument Respuesta = Enviar(HttpMethod.Post, "v1/orders", JsonSerializer.Serialize(Order)))
            {
                cobro = Respuesta.RootElement.GetProperty("id").GetString();
                string Url = Respuesta.RootElement.GetProperty("checkout_url").GetString();
                bool Sandbox = string.Equals(ConfigurationManager.AppSettings["MercadoPagoSandbox"], "true", StringComparison.OrdinalIgnoreCase);
                return Sandbox ? Url.Replace("://www.mercadopago.", "://sandbox.mercadopago.") : Url;
            }
        }
        /// Estado de la order: processed es aprobado; failed, canceled,
        /// expired o refunded, rechazado; el resto (creada, en proceso),
        /// pendiente.
        public OperacionPagoBE ConsultarPago(int referencia, DateTime fecha, string cobro)
        {
            OperacionPagoBE Pendiente = new OperacionPagoBE { Referencia = referencia, Estado = OperacionPagoBE.EstadoPendiente };
            if (string.IsNullOrEmpty(cobro)) return Pendiente;
            using (JsonDocument Respuesta = Enviar(HttpMethod.Get, "v1/orders/" + Uri.EscapeDataString(cobro), null))
            {
                JsonElement Order = Respuesta.RootElement;
                // La order tiene que ser la de este Pago y no otra.
                if (Order.GetProperty("external_reference").GetString() != ReferenciaExterna(referencia, fecha))
                {
                    throw new InvalidOperationException("La order " + cobro + " no corresponde al pago " + referencia + ".");
                }
                string Estado;
                switch (Order.GetProperty("status").GetString())
                {
                    case "processed": Estado = OperacionPagoBE.EstadoAprobado; break;
                    case "failed":
                    case "canceled":
                    case "expired":
                    case "refunded": Estado = OperacionPagoBE.EstadoRechazado; break;
                    default: return Pendiente;
                }
                JsonElement Pagado;
                return new OperacionPagoBE
                {
                    Referencia = referencia,
                    Estado = Estado,
                    Operacion = "MP-" + cobro,
                    Monto = Order.TryGetProperty("total_paid_amount", out Pagado)
                        ? decimal.Parse(Pagado.GetString(), CultureInfo.InvariantCulture)
                        : 0
                };
            }
        }
        public OperacionPagoBE CobrarRecurrente(int referencia, decimal monto, string descripcion)
        {
            throw new NotSupportedException("Mercado Pago Checkout Pro no hace débitos automáticos.");
        }

        /// Referencia del cobro en Mercado Pago: el Id y la fecha del Pago, para
        /// que un Id reusado (por ejemplo después de restaurar un backup) no se
        /// confunda con un cobro viejo.
        private static string ReferenciaExterna(int referencia, DateTime fecha)
        {
            return "DOVERIE-" + referencia + "-" + fecha.ToString("yyyyMMddHHmmss");
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
                    if (cuerpo != null)
                    {
                        Pedido.Content = new StringContent(cuerpo, Encoding.UTF8, "application/json");
                        // Mercado Pago lo pide para no crear dos veces la misma order.
                        Pedido.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
                    }
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
