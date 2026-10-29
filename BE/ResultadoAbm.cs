namespace BE
{
    /// Resultado de las altas, modificaciones y bajas de Permisos, Familias
    /// y Perfiles. Cada pantalla lo traduce al código de error de su CU.
    public enum ResultadoAbm
    {
        Exitoso,
        DatosInvalidos,   // [ERROR-004]
        Duplicado,        // Permiso [ERROR-022], Perfil [ERROR-025], Familia [ERROR-045/047]
        EnUso,            // Familia [ERROR-048], Perfil [ERROR-027], Permiso [ERROR-023]
        Error             // Falla al guardar o eliminar
    }
}
