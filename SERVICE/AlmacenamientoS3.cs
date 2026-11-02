using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace SERVICE
{
    /// Implementación de IAlmacenamientoArchivos sobre un bucket privado de
    /// AWS S3. Igual que en disco, cada archivo se guarda con un GUID como
    /// nombre (clave "documentos/<guid>"), así que Documento.UrlDocumento
    /// no cambia de formato y los archivos locales se migran subiéndolos
    /// con la misma clave. El bucket no es público: los archivos solo salen
    /// a través de la app, después de validar permisos.
    ///
    /// Configuración (AppSettings.config): S3Bucket, S3Region, S3AccessKey,
    /// S3SecretKey y, opcionales, S3Prefijo y S3ServiceUrl (un emulador de
    /// S3 para pruebas locales).
    public class AlmacenamientoS3 : IAlmacenamientoArchivos
    {
        public string Guardar(byte[] contenido)
        {
            string Ubicacion = Guid.NewGuid().ToString("N");
            using (AmazonS3Client Cliente = CrearCliente())
            using (MemoryStream Flujo = new MemoryStream(contenido))
            {
                PutObjectRequest Pedido = new PutObjectRequest
                {
                    BucketName = Configuracion("S3Bucket"),
                    Key = Clave(Ubicacion),
                    InputStream = Flujo,
                    ContentType = "application/octet-stream",
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };
                Ejecutar(() => Cliente.PutObjectAsync(Pedido));
            }
            return Ubicacion;
        }
        public byte[] Obtener(string ubicacion)
        {
            using (AmazonS3Client Cliente = CrearCliente())
            using (GetObjectResponse Respuesta = Ejecutar(() => Cliente.GetObjectAsync(Configuracion("S3Bucket"), Clave(ubicacion))))
            using (MemoryStream Flujo = new MemoryStream())
            {
                Respuesta.ResponseStream.CopyTo(Flujo);
                return Flujo.ToArray();
            }
        }
        public void Eliminar(string ubicacion)
        {
            // En S3 borrar una clave que no existe no es un error.
            using (AmazonS3Client Cliente = CrearCliente())
            {
                Ejecutar(() => Cliente.DeleteObjectAsync(Configuracion("S3Bucket"), Clave(ubicacion)));
            }
        }
        /// El SDK es asíncrono; se ejecuta fuera del contexto de ASP.NET para
        /// poder esperar el resultado sin bloquear el request.
        private static T Ejecutar<T>(Func<Task<T>> operacion)
        {
            return Task.Run(operacion).GetAwaiter().GetResult();
        }
        private static string Clave(string ubicacion)
        {
            if (ubicacion == null || !Regex.IsMatch(ubicacion, "^[0-9a-f]{32}$"))
            {
                throw new ArgumentException("Ubicación de archivo inválida.");
            }
            return (ConfigurationManager.AppSettings["S3Prefijo"] ?? "documentos/") + ubicacion;
        }
        private static AmazonS3Client CrearCliente()
        {
            string Region = Configuracion("S3Region");
            AmazonS3Config Config = new AmazonS3Config();
            string Servicio = ConfigurationManager.AppSettings["S3ServiceUrl"];
            if (string.IsNullOrWhiteSpace(Servicio))
            {
                Config.RegionEndpoint = RegionEndpoint.GetBySystemName(Region);
            }
            else
            {
                Config.ServiceURL = Servicio;
                Config.AuthenticationRegion = Region;
                Config.ForcePathStyle = true;
            }
            BasicAWSCredentials Credenciales = new BasicAWSCredentials(Configuracion("S3AccessKey"), Configuracion("S3SecretKey"));
            return new AmazonS3Client(Credenciales, Config);
        }
        private static string Configuracion(string clave)
        {
            string Valor = ConfigurationManager.AppSettings[clave];
            if (string.IsNullOrWhiteSpace(Valor)) throw new ConfigurationErrorsException("Falta configurar " + clave + ".");
            return Valor;
        }
    }
}
