-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ========================================
-- Corrección: SP_Perfil_ObtenerPorNombre no devolvía AsignablePorEmpresa,
-- y PerfilDAL.ObtenerPorNombre lo lee (CU-001-001 Registrarse busca así el
-- Perfil "Freelance" o "Empresa"). Devuelve las mismas columnas que
-- SP_Perfil_Listar. El script se puede ejecutar más de una vez.
-- ========================================
CREATE OR ALTER PROCEDURE SP_Perfil_ObtenerPorNombre
    @NombrePerfil NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, NombrePerfil, Activo, AsignablePorEmpresa FROM Perfil WHERE NombrePerfil = @NombrePerfil;
END
GO
