namespace BE
{
    /// Un dígito verificador que no coincide con los datos: en qué tabla y a
    /// qué nivel (una fila, una columna o la tabla entera).
    public class ProblemaIntegridadBE
    {
        public const string NivelRegistro = "Registro";
        public const string NivelColumna = "Columna";
        public const string NivelTabla = "Tabla";

        public string Tabla { get; set; }
        public string Nivel { get; set; }
        /// Qué fila (su clave) o qué columna; vacío para la tabla entera.
        public string Detalle { get; set; }
    }
}
