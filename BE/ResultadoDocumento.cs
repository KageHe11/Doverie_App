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
        SinAcceso,            // [ERROR-056]: no existe o es de otra cuenta
        DatosInvalidos,       // [ERROR-004]
        ErrorEnlace,          // [ERROR-013]: no se pudo generar el enlace
        EnlaceNoEnviado,      // [ERROR-013]: el enlace se generó pero el email falló
        EnlaceInvalido,       // [ERROR-044]: el token no existe, venció o fue revocado
        SinSuscripcion,       // la cuenta no tiene una suscripción vigente
        NFTExistente,         // [ERROR-019]
        ErrorNFT,             // [ERROR-020]
        SinNFT,               // [ERROR-031]
        ErrorConsultaNFT      // [ERROR-032]
    }
}
