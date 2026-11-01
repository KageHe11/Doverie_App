using System;
using System.Configuration;

namespace SERVICE
{
    /// Decide qué implementación de almacenamiento, de blockchain y de
    /// pasarela de pago usa el sistema, según AppSettings ("Almacenamiento",
    /// "Blockchain" y "PasarelaPago"). Hoy solo existe "Local"; cuando se
    /// sumen S3, Nethereum y Mercado Pago, se agregan acá sus casos y la BLL
    /// no cambia.
    public static class ProveedorServicios
    {
        public static IAlmacenamientoArchivos Almacenamiento()
        {
            string Tipo = ConfigurationManager.AppSettings["Almacenamiento"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return new AlmacenamientoLocal();
                default: throw new ConfigurationErrorsException("Almacenamiento no soportado: " + Tipo);
            }
        }
        public static IRegistroBlockchain Blockchain()
        {
            string Tipo = ConfigurationManager.AppSettings["Blockchain"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return new RegistroBlockchainLocal();
                default: throw new ConfigurationErrorsException("Blockchain no soportada: " + Tipo);
            }
        }
        /// Una sola instancia: la pasarela local guarda los cobros en memoria.
        private static readonly PasarelaPagoLocal PasarelaLocal = new PasarelaPagoLocal();
        public static IPasarelaPago PasarelaPago()
        {
            string Tipo = ConfigurationManager.AppSettings["PasarelaPago"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return PasarelaLocal;
                default: throw new ConfigurationErrorsException("Pasarela de pago no soportada: " + Tipo);
            }
        }
    }
}
