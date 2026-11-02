using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using BE;

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
        /// Copia los documentos a "backups/<etiqueta>/documentos/" dentro del
        /// mismo bucket, del lado de S3 (no pasan por el servidor de la app),
        /// y sube el .bak de la base a "backups/<etiqueta>/".
        public RespaldoArchivosBE Respaldar(string etiqueta, string archivoBaseDatos)
        {
            string Bucket = Configuracion("S3Bucket");
            string Prefijo = ConfigurationManager.AppSettings["S3Prefijo"] ?? "documentos/";
            string Destino = "backups/" + etiqueta + "/";
            RespaldoArchivosBE Respaldo = new RespaldoArchivosBE { Ubicacion = "s3://" + Bucket + "/" + Destino };
            using (AmazonS3Client Cliente = CrearCliente())
            {
                foreach (S3Object Objeto in Listar(Cliente, Bucket, Prefijo))
                {
                    Copiar(Cliente, Bucket, Objeto.Key, Destino + "documentos/" + Objeto.Key.Substring(Prefijo.Length));
                    Respaldo.CantidadArchivos++;
                    Respaldo.TamanoBytes += Objeto.Size ?? 0;
                }
                if (archivoBaseDatos != null)
                {
                    PutObjectRequest Subida = new PutObjectRequest
                    {
                        BucketName = Bucket,
                        Key = Destino + Path.GetFileName(archivoBaseDatos),
                        FilePath = archivoBaseDatos,
                        ContentType = "application/octet-stream",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    };
                    Ejecutar(() => Cliente.PutObjectAsync(Subida));
                }
            }
            return Respaldo;
        }
        /// Restore: vuelve a copiar "backups/<etiqueta>/documentos/" a
        /// "documentos/" y borra de "documentos/" lo que no estaba en el
        /// respaldo. Si el respaldo no tiene archivos no toca nada: borrar todo
        /// por un respaldo vacío o inexistente sería peor que no restaurar.
        public RespaldoArchivosBE RestaurarArchivos(string etiqueta)
        {
            string Bucket = Configuracion("S3Bucket");
            string Prefijo = ConfigurationManager.AppSettings["S3Prefijo"] ?? "documentos/";
            string Origen = "backups/" + etiqueta + "/documentos/";
            using (AmazonS3Client Cliente = CrearCliente())
            {
                List<S3Object> Respaldados = Listar(Cliente, Bucket, Origen);
                if (Respaldados.Count == 0) return null;
                RespaldoArchivosBE Resultado = new RespaldoArchivosBE { Ubicacion = "s3://" + Bucket + "/" + Origen };
                HashSet<string> Nombres = new HashSet<string>();
                foreach (S3Object Objeto in Respaldados)
                {
                    string Nombre = Objeto.Key.Substring(Origen.Length);
                    Nombres.Add(Nombre);
                    Copiar(Cliente, Bucket, Objeto.Key, Prefijo + Nombre);
                    Resultado.CantidadArchivos++;
                    Resultado.TamanoBytes += Objeto.Size ?? 0;
                }
                foreach (S3Object Objeto in Listar(Cliente, Bucket, Prefijo))
                {
                    if (Nombres.Contains(Objeto.Key.Substring(Prefijo.Length))) continue;
                    Ejecutar(() => Cliente.DeleteObjectAsync(Bucket, Objeto.Key));
                    Resultado.CantidadEliminados++;
                }
                return Resultado;
            }
        }
        public bool ObtenerBaseDatos(string etiqueta, string nombreArchivo, string rutaDestino)
        {
            using (AmazonS3Client Cliente = CrearCliente())
            {
                GetObjectResponse Respuesta;
                try
                {
                    Respuesta = Ejecutar(() => Cliente.GetObjectAsync(Configuracion("S3Bucket"), "backups/" + etiqueta + "/" + nombreArchivo));
                }
                catch (AmazonS3Exception Error) when (Error.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                using (Respuesta)
                using (FileStream Archivo = File.Create(rutaDestino))
                {
                    Respuesta.ResponseStream.CopyTo(Archivo);
                }
                return true;
            }
        }
        private static List<S3Object> Listar(AmazonS3Client cliente, string bucket, string prefijo)
        {
            List<S3Object> Objetos = new List<S3Object>();
            ListObjectsV2Request Pedido = new ListObjectsV2Request { BucketName = bucket, Prefix = prefijo };
            ListObjectsV2Response Pagina;
            do
            {
                Pagina = Ejecutar(() => cliente.ListObjectsV2Async(Pedido));
                if (Pagina.S3Objects != null) Objetos.AddRange(Pagina.S3Objects);
                Pedido.ContinuationToken = Pagina.NextContinuationToken;
            } while (Pagina.IsTruncated == true);
            return Objetos;
        }
        /// Copia del lado de S3: el archivo no pasa por el servidor de la app.
        private static void Copiar(AmazonS3Client cliente, string bucket, string origen, string destino)
        {
            CopyObjectRequest Copia = new CopyObjectRequest
            {
                SourceBucket = bucket,
                SourceKey = origen,
                DestinationBucket = bucket,
                DestinationKey = destino,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };
            Ejecutar(() => cliente.CopyObjectAsync(Copia));
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
