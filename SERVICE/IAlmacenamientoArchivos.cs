using BE;

namespace SERVICE
{
    /// Contrato del almacenamiento de archivos de los documentos: en disco
    /// (AlmacenamientoLocal) o en AWS S3 (AlmacenamientoS3), sin que la BLL
    /// sepa cuál.
    public interface IAlmacenamientoArchivos
    {
        /// Guarda el contenido y devuelve la ubicación con la que se lo
        /// recupera después (lo que se persiste en Documento.UrlDocumento).
        string Guardar(byte[] contenido);
        byte[] Obtener(string ubicacion);
        void Eliminar(string ubicacion);
        /// CU-002-013 paso 5: copia todos los archivos de los documentos al
        /// respaldo "etiqueta" y, si se indica, también el .bak de la base.
        RespaldoArchivosBE Respaldar(string etiqueta, string archivoBaseDatos);
        /// Restore: deja los archivos como estaban en el respaldo "etiqueta"
        /// (copia los del respaldo y borra los que no estaban). Devuelve null,
        /// sin tocar nada, si ese respaldo no tiene copia de archivos.
        RespaldoArchivosBE RestaurarArchivos(string etiqueta);
        /// Trae el .bak del respaldo "etiqueta" a rutaDestino. Devuelve false
        /// si el respaldo no lo tiene.
        bool ObtenerBaseDatos(string etiqueta, string nombreArchivo, string rutaDestino);
    }
}
