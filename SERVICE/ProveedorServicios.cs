using System;
using System.Configuration;

namespace SERVICE
{
    /// Decide qué implementación de almacenamiento y de blockchain usa el
    /// sistema, según AppSettings ("Almacenamiento" y "Blockchain"). Hoy
    /// solo existe "Local"; cuando se sumen S3 y Nethereum, se agregan acá
    /// sus casos y la BLL no cambia.
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
    }
}
