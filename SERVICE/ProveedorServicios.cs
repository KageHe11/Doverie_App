using System;
using System.Configuration;

namespace SERVICE
{
    /// Decide qué implementación de almacenamiento, de blockchain y de
    /// pasarela de pago usa el sistema, según AppSettings ("Almacenamiento",
    /// "Blockchain" y "PasarelaPago"). Todas tienen una versión "Local" para
    /// desarrollo; además el almacenamiento tiene "S3" (AWS), la blockchain
    /// "Nethereum" (Ethereum real) y la pasarela "MercadoPago".
    public static class ProveedorServicios
    {
        public static IAlmacenamientoArchivos Almacenamiento()
        {
            string Tipo = ConfigurationManager.AppSettings["Almacenamiento"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return new AlmacenamientoLocal();
                case "S3": return new AlmacenamientoS3();
                default: throw new ConfigurationErrorsException("Almacenamiento no soportado: " + Tipo);
            }
        }
        public static IRegistroBlockchain Blockchain()
        {
            string Tipo = ConfigurationManager.AppSettings["Blockchain"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return new RegistroBlockchainLocal();
                case "Nethereum": return new RegistroBlockchainNethereum();
                default: throw new ConfigurationErrorsException("Blockchain no soportada: " + Tipo);
            }
        }
        /// Los NFT siguen a la blockchain configurada.
        public static IRegistroNFT NFT()
        {
            string Tipo = ConfigurationManager.AppSettings["Blockchain"] ?? "Local";
            switch (Tipo)
            {
                case "Local": return new RegistroNFTLocal();
                case "Nethereum": return new RegistroNFTNethereum();
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
                case "MercadoPago": return new PasarelaMercadoPago();
                default: throw new ConfigurationErrorsException("Pasarela de pago no soportada: " + Tipo);
            }
        }
    }
}
