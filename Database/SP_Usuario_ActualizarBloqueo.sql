-- ========================================
-- SP_Usuario_ActualizarBloqueo
-- Usado por ActualizarBloqueo(UsuarioBE Pusuario) -> CU-002-001 Login
-- Actualiza solo el contador de intentos fallidos y el estado de
-- bloqueo, sin tocar el resto de los datos del Usuario.
-- ========================================
CREATE OR ALTER PROCEDURE SP_Usuario_ActualizarBloqueo
    @Id INT,
    @Intentos INT,
    @Suspendido BIT,
    @Motivo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuario
    SET Intentos = @Intentos,
        Suspendido = @Suspendido,
        Motivo = @Motivo
    WHERE Id = @Id;
END
