namespace BE
{
    /// Resultados posibles de Alta / Modificar / Baja Idioma, cada uno
    /// asociado a su código del catálogo de errores.
    public enum ResultadoIdioma
    {
        Exitoso,
        DatosInvalidos,       // [ERROR-004]
        JsonInvalido,         // [ERROR-050]
        YaExiste,             // [ERROR-051]
        ErrorAlta,            // [ERROR-052]
        ErrorModificacion,    // [ERROR-053]
        NoEliminable,         // [ERROR-054]
        ErrorBaja             // [ERROR-055]
    }
}
