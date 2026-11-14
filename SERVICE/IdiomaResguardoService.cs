using System;
using System.IO;
using System.Web.Hosting;
using System.Xml.Serialization;
using BE;

namespace SERVICE
{
    /// Copia del idioma predeterminado en App_Data. Si se borra la tabla
    /// Idioma, la interfaz se sigue mostrando traducida con esta copia, para
    /// que el Administrador pueda entrar y restaurar un backup.
    public class IdiomaResguardoService
    {
        private static readonly object Bloqueo = new object();
        /// Lo último que se guardó, para no escribir el archivo en cada uso.
        private static string UltimoGuardado;

        private static string ObtenerRutaArchivo()
        {
            string Carpeta = HostingEnvironment.MapPath("~/App_Data");
            if (Carpeta == null) return null;
            if (!Directory.Exists(Carpeta)) Directory.CreateDirectory(Carpeta);
            return Path.Combine(Carpeta, "IdiomaResguardo.xml");
        }
        public void Guardar(IdiomaBE idioma)
        {
            string Clave = idioma.Id + "|" + idioma.Codigo + "|" + idioma.ArchivoJson;
            lock (Bloqueo)
            {
                if (Clave == UltimoGuardado) return;
                string Ruta = ObtenerRutaArchivo();
                if (Ruta == null) return;
                try
                {
                    using (StreamWriter Escritor = new StreamWriter(Ruta))
                    {
                        new XmlSerializer(typeof(IdiomaBE)).Serialize(Escritor, idioma);
                    }
                    UltimoGuardado = Clave;
                }
                catch (Exception)
                {
                    // Sin permiso de escritura no hay copia, pero la interfaz
                    // sigue funcionando con la base.
                }
            }
        }
        /// La copia guardada, o null si no hay.
        public IdiomaBE Leer()
        {
            string Ruta = ObtenerRutaArchivo();
            if (Ruta == null || !File.Exists(Ruta)) return null;
            try
            {
                using (StreamReader Lector = new StreamReader(Ruta))
                {
                    return (IdiomaBE)new XmlSerializer(typeof(IdiomaBE)).Deserialize(Lector);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
