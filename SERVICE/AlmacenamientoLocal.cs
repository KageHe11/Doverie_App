using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace SERVICE
{
    /// Implementación de IAlmacenamientoArchivos sobre el disco del
    /// servidor. Por defecto usa App_Data/Documentos, que IIS no sirve
    /// nunca de forma directa: los archivos solo salen a través de la app,
    /// después de validar permisos.
    public class AlmacenamientoLocal : IAlmacenamientoArchivos
    {
        public string Guardar(byte[] contenido)
        {
            // El nombre en disco es un GUID: el nombre original del usuario
            // nunca llega al sistema de archivos (evita colisiones y rutas
            // armadas a mano del tipo "..\..\web.config").
            string Ubicacion = Guid.NewGuid().ToString("N");
            File.WriteAllBytes(RutaCompleta(Ubicacion), contenido);
            return Ubicacion;
        }
        public byte[] Obtener(string ubicacion)
        {
            return File.ReadAllBytes(RutaCompleta(ubicacion));
        }
        public void Eliminar(string ubicacion)
        {
            string Ruta = RutaCompleta(ubicacion);
            if (File.Exists(Ruta)) File.Delete(Ruta);
        }
        private string RutaCompleta(string ubicacion)
        {
            if (ubicacion == null || !Regex.IsMatch(ubicacion, "^[0-9a-f]{32}$"))
            {
                throw new ArgumentException("Ubicación de archivo inválida.");
            }
            return Path.Combine(Carpeta(), ubicacion);
        }
        private static string Carpeta()
        {
            string Carpeta = ConfigurationManager.AppSettings["DocumentosRuta"];
            if (string.IsNullOrWhiteSpace(Carpeta))
            {
                Carpeta = HostingEnvironment.MapPath("~/App_Data/Documentos");
            }
            Directory.CreateDirectory(Carpeta);
            return Carpeta;
        }
    }
}
