namespace BE
{
    /// Resultados posibles de las operaciones sobre Documentos, cada uno
    /// asociado a su código del catálogo de errores.
    public enum ResultadoDocumento
    {
        Exitoso,
        ArchivoInvalido,      // [ERROR-010]
        ErrorAlmacenamiento,  // [ERROR-011]
        ErrorBlockchain,      // [ERROR-012]
        YaEliminado,          // [ERROR-016]
        ErrorObtener,         // [ERROR-017]
        Modificado,           // [ERROR-018]
        ErrorEliminar,        // [ERROR-006]
        SinAcceso             // [ERROR-056]: no existe o es de otra cuenta
    }
}
