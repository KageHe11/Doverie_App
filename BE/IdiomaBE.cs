using System;

namespace BE
{
    [Serializable]
    public class IdiomaBE
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        /// Contenido del archivo JSON de traducciones (clave -> texto).
        /// Viene vacío en los listados, que no lo necesitan.
        public string ArchivoJson { get; set; }
        public bool EsPredeterminado { get; set; }
    }
}
