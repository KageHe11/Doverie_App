namespace BE
{
    /// Resultado de Suspender (CU-002-010), Reactivar (CU-002-011) y
    /// Eliminar Usuario Empresa (CU-001-016).
    public enum ResultadoEstadoUsuario
    {
        Exitoso,
        MotivoInvalido,   // [ERROR-004]
        NoSuspendido,     // [ERROR-039]
        SinAcceso,        // el usuario no existe o no es de la Empresa
        Error             // [ERROR-038] / [ERROR-037]
    }
}
