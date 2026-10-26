namespace SERVICE
{
    /// Contrato del almacenamiento de archivos de los documentos. La
    /// implementación actual guarda en disco (AlmacenamientoLocal); la de
    /// AWS S3 se agrega como otra implementación sin tocar la BLL.
    public interface IAlmacenamientoArchivos
    {
        /// Guarda el contenido y devuelve la ubicación con la que se lo
        /// recupera después (lo que se persiste en Documento.UrlDocumento).
        string Guardar(byte[] contenido);
        byte[] Obtener(string ubicacion);
        void Eliminar(string ubicacion);
    }
}
