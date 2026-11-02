using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using BE;

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
        /// Copia los documentos a "<BackupsRuta>/<etiqueta>/documentos" (por
        /// defecto App_Data/Backups) y el .bak de la base a "<etiqueta>".
        public RespaldoArchivosBE Respaldar(string etiqueta, string archivoBaseDatos)
        {
            string Destino = CarpetaRespaldo(etiqueta);
            string DestinoDocumentos = Path.Combine(Destino, "documentos");
            Directory.CreateDirectory(DestinoDocumentos);
            RespaldoArchivosBE Respaldo = new RespaldoArchivosBE { Ubicacion = Destino };
            foreach (string Archivo in Directory.GetFiles(Carpeta()))
            {
                File.Copy(Archivo, Path.Combine(DestinoDocumentos, Path.GetFileName(Archivo)));
                Respaldo.CantidadArchivos++;
                Respaldo.TamanoBytes += new FileInfo(Archivo).Length;
            }
            if (archivoBaseDatos != null)
            {
                File.Copy(archivoBaseDatos, Path.Combine(Destino, Path.GetFileName(archivoBaseDatos)));
            }
            return Respaldo;
        }
        /// Restore: vuelve a copiar los documentos del respaldo y borra los que
        /// no estaban. Sin copia de archivos en ese respaldo no toca nada.
        public RespaldoArchivosBE RestaurarArchivos(string etiqueta)
        {
            string Origen = Path.Combine(CarpetaRespaldo(etiqueta), "documentos");
            if (!Directory.Exists(Origen) || Directory.GetFiles(Origen).Length == 0) return null;
            RespaldoArchivosBE Resultado = new RespaldoArchivosBE { Ubicacion = Origen };
            foreach (string Archivo in Directory.GetFiles(Origen))
            {
                File.Copy(Archivo, Path.Combine(Carpeta(), Path.GetFileName(Archivo)), true);
                Resultado.CantidadArchivos++;
                Resultado.TamanoBytes += new FileInfo(Archivo).Length;
            }
            foreach (string Archivo in Directory.GetFiles(Carpeta()))
            {
                if (File.Exists(Path.Combine(Origen, Path.GetFileName(Archivo)))) continue;
                File.Delete(Archivo);
                Resultado.CantidadEliminados++;
            }
            return Resultado;
        }
        public bool ObtenerBaseDatos(string etiqueta, string nombreArchivo, string rutaDestino)
        {
            string Origen = Path.Combine(CarpetaRespaldo(etiqueta), nombreArchivo);
            if (!File.Exists(Origen)) return false;
            File.Copy(Origen, rutaDestino, true);
            return true;
        }
        /// "<BackupsRuta>/<etiqueta>", por defecto bajo App_Data/Backups.
        private static string CarpetaRespaldo(string etiqueta)
        {
            string Raiz = ConfigurationManager.AppSettings["BackupsRuta"];
            if (string.IsNullOrWhiteSpace(Raiz)) Raiz = HostingEnvironment.MapPath("~/App_Data/Backups");
            return Path.Combine(Raiz, etiqueta);
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
