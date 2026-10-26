using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SERVICE
{
    /// ConcreteStrategy: única implementación de ITraduccion. Su
    /// comportamiento no depende de la clase sino del archivo JSON que
    /// carga, por eso los idiomas se pueden dar de alta en tiempo de
    /// ejecución sin escribir una clase nueva por idioma.
    [Serializable]
    public class TraduccionJson : ITraduccion
    {
        private Dictionary<string, string> _textos = new Dictionary<string, string>();

        public IEnumerable<string> Claves
        {
            get { return _textos.Keys; }
        }
        public void CargarIdioma(string archivoJson)
        {
            _textos = Parsear(archivoJson);
        }
        public string Traducir(string clave)
        {
            string Texto;
            return _textos.TryGetValue(clave, out Texto) ? Texto : null;
        }
        /// Convierte el JSON en un diccionario clave -> texto. El formato
        /// esperado es un objeto plano: { "clave": "texto", ... }.
        /// Lanza FormatException si el contenido no respeta ese formato.
        public static Dictionary<string, string> Parsear(string archivoJson)
        {
            if (string.IsNullOrWhiteSpace(archivoJson))
            {
                throw new FormatException("El archivo de traducciones está vacío.");
            }
            try
            {
                using (JsonDocument Documento = JsonDocument.Parse(archivoJson))
                {
                    if (Documento.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        throw new FormatException("El archivo de traducciones debe ser un objeto JSON.");
                    }
                    Dictionary<string, string> Textos = new Dictionary<string, string>();
                    foreach (JsonProperty Propiedad in Documento.RootElement.EnumerateObject())
                    {
                        if (Propiedad.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(Propiedad.Name))
                        {
                            throw new FormatException("La clave '" + Propiedad.Name + "' no tiene un texto válido.");
                        }
                        Textos[Propiedad.Name] = Propiedad.Value.GetString();
                    }
                    return Textos;
                }
            }
            catch (JsonException Ex)
            {
                throw new FormatException("El archivo de traducciones no es un JSON válido.", Ex);
            }
        }
    }
}
