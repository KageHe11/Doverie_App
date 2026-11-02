-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ========================================
-- NFT de documentos: CU-001-007 Generar NFT Documento y
-- CU-001-013 Consultar Estado de NFT.
-- El NFT vive en la blockchain (contrato DoverieNFT); en la base solo se
-- guarda su identificador en Documento.NFTId ("<contrato>:<token>").
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- Requiere haber ejecutado antes Database/Documentos.sql.
-- ========================================

-- Usado por RegistrarNFT -> CU-001-007 paso 6. Solo asocia el NFT si el
-- documento sigue activo y todavía no tenía uno; devuelve las filas
-- actualizadas (0 si otro pedido se adelantó).
CREATE OR ALTER PROCEDURE SP_Documento_RegistrarNFT
    @Id INT,
    @NFTId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Documento SET NFTId = @NFTId
    WHERE Id = @Id AND NFTId IS NULL AND Estado = 'Activo';
    SELECT @@ROWCOUNT AS Filas;
END
GO

-- ----------------------------------------
-- Permisos (Composite)
--   GenerarNFT en GestionDocumentos: Freelance, Empresa y EmpleadoAvanzado.
--   ConsultarNFT en AccesoDocumentos: también EmpleadoEmpresa (solo lectura).
-- ----------------------------------------
DECLARE @Permisos TABLE (Nombre NVARCHAR(50), Descripcion NVARCHAR(MAX), Familia NVARCHAR(50));
INSERT INTO @Permisos VALUES
    ('GenerarNFT', 'Permite generar el NFT de utilidad de un documento (CU-001-007)', 'GestionDocumentos'),
    ('ConsultarNFT', 'Permite consultar el estado del NFT de un documento (CU-001-013)', 'AccesoDocumentos');

DECLARE @Nombre NVARCHAR(50), @Descripcion NVARCHAR(MAX), @Familia NVARCHAR(50), @Id INT, @FamiliaId INT;
DECLARE CursorPermisos CURSOR LOCAL FAST_FORWARD FOR SELECT Nombre, Descripcion, Familia FROM @Permisos;
OPEN CursorPermisos;
FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion, @Familia;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Id = (SELECT Id FROM Permiso WHERE Nombre = @Nombre);
    IF @Id IS NULL
    BEGIN
        SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
        INSERT INTO Permiso (Id, Nombre, Descripcion) VALUES (@Id, @Nombre, @Descripcion);
    END
    SET @FamiliaId = (SELECT Id FROM Familia WHERE Nombre = @Familia);
    IF @FamiliaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @Id)
        INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @Id, NULL);
    FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion, @Familia;
END
CLOSE CursorPermisos;
DEALLOCATE CursorPermisos;
GO
