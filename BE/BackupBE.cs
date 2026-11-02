using System;

namespace BE
{
    /// Registro de un backup manual (CU-002-013): quién lo generó, cuándo,
    /// si salió bien y qué se copió.
    public class BackupBE
    {
        public const string EstadoExitoso = "Exitoso";
        public const string EstadoFallido = "Fallido";

        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string Detalle { get; set; }
    }

    /// Resultado de la copia de los archivos de los documentos (paso 5).
    public class RespaldoArchivosBE
    {
        public int CantidadArchivos { get; set; }
        public long TamanoBytes { get; set; }
        /// Dónde quedó la copia (prefijo del bucket o carpeta).
        public string Ubicacion { get; set; }
    }

    /// Resultados de CU-002-013, cada uno con su código de error.
    public enum ResultadoBackup
    {
        Exitoso,
        ErrorBaseDatos,   // [ERROR-041]
        ErrorArchivos     // [ERROR-042]
    }
}
