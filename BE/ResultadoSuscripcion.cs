namespace BE
{
    /// Resultados posibles de las operaciones de Suscripción y Facturación,
    /// cada uno asociado a su código del catálogo de errores.
    public enum ResultadoSuscripcion
    {
        Exitoso,
        PagoPendiente,        // la pasarela todavía no informó el resultado
        ErrorPago,            // [ERROR-014]
        ErrorCancelar,        // [ERROR-015]
        SinPagos,             // [ERROR-034]
        ErrorComprobante,     // [ERROR-035]
        ErrorPdf,             // [ERROR-036]
        DatosInvalidos,       // [ERROR-004]
        MismoPlan,            // el plan elegido es el que ya tiene
        LimiteUsuarios,       // la cuenta tiene más usuarios de los que permite el plan
        SinSuscripcion,       // no hay suscripción vigente
        SinAcceso,            // [ERROR-056]: no es titular de la cuenta o el pago es de otra
        ErrorEmail            // no se pudo enviar el comprobante por correo
    }
}
