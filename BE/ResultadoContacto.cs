namespace BE
{
    /// Resultado del formulario "Contactanos" de la página de inicio.
    public enum ResultadoContacto
    {
        Exitoso,
        DatosInvalidos,   // [ERROR-004]
        ErrorEnvio        // no se pudo enviar el correo
    }
}
