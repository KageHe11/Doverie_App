using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Xml.Serialization;
using BE;

namespace SERVICE
{
    [XmlRoot("Bitacora")]
    public class BitacoraArchivoXml
    {
        [XmlElement("Evento")]
        public List<BitacoraBE> Eventos { get; set; }
        public BitacoraArchivoXml()
        {
            Eventos = new List<BitacoraBE>();
        }
    }
    /// Guarda y lee los eventos de Bitácora que ya salieron de la tabla
    /// persistidos en un XML dentro
    /// de App_Data.
    public class BitacoraArchivoService
    {
        private string ObtenerRutaArchivo()
        {
            string Carpeta = HostingEnvironment.MapPath("~/App_Data");
            if (!Directory.Exists(Carpeta)) Directory.CreateDirectory(Carpeta);
            return Path.Combine(Carpeta, "BitacoraArchivo.xml");
        }
        public void Archivar(List<BitacoraBE> registros)
        {
            if (registros == null || registros.Count == 0) return;

            string Ruta = ObtenerRutaArchivo();
            BitacoraArchivoXml Archivo = File.Exists(Ruta) ? Leer(Ruta) : new BitacoraArchivoXml();
            Archivo.Eventos.AddRange(registros);
            Guardar(Ruta, Archivo);
        }
        public List<BitacoraBE> ObtenerTodos()
        {
            string Ruta = ObtenerRutaArchivo();
            if (!File.Exists(Ruta)) return new List<BitacoraBE>();
            return Leer(Ruta).Eventos;
        }
        private BitacoraArchivoXml Leer(string ruta)
        {
            XmlSerializer Serializer = new XmlSerializer(typeof(BitacoraArchivoXml));
            using (FileStream Stream = new FileStream(ruta, FileMode.Open, FileAccess.Read))
            {
                return (BitacoraArchivoXml)Serializer.Deserialize(Stream);
            }
        }
        private void Guardar(string ruta, BitacoraArchivoXml archivo)
        {
            XmlSerializer Serializer = new XmlSerializer(typeof(BitacoraArchivoXml));
            using (FileStream Stream = new FileStream(ruta, FileMode.Create, FileAccess.Write))
            {
                Serializer.Serialize(Stream, archivo);
            }
        }
    }
}