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

    /// Resultado de la copia de los archivos de los documentos (paso 5), o
    /// de su restauración.
    public class RespaldoArchivosBE
    {
        public int CantidadArchivos { get; set; }
        public long TamanoBytes { get; set; }
        /// Solo en un restore: archivos que no estaban en el respaldo y se
        /// borraron (siguen en el backup de seguridad previo).
        public int CantidadEliminados { get; set; }
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

    /// Resultados del restore de un backup.
    public enum ResultadoRestauracion
    {
        Exitoso,
        ExitosoSinArchivos,  // la base se restauró; el .bak no tiene copia de archivos asociada
        NoEncontrado,        // el backup del historial no existe o no fue exitoso
        ArchivoNoDisponible, // no está el .bak ni en el servidor ni en el almacenamiento
        ArchivoInvalido,     // no es un .bak de esta base o está dañado
        ErrorRespaldoPrevio, // no se pudo generar el backup de seguridad: no se tocó nada
        ErrorBaseDatos,      // falló el restore de la base: quedó como estaba
        ErrorArchivos        // la base se restauró pero los archivos no
    }
}
