SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Bitácora (checklist 26): eliminación lógica y mantenimiento de los
-- últimos 30 días.
-- * Eliminación lógica: un registro "eliminado" no se borra; queda marcado
--   con quién y cuándo, se oculta por defecto y se puede restaurar.
-- * 30 días: la tabla guarda los eventos de los últimos 30 días. Los más
--   viejos se pasan al archivo histórico (App_Data/BitacoraArchivo.xml)
--   y recién ahí salen de la tabla: se archivan, no se pierden.
-- Reemplaza el criterio anterior de "30 filas" (SP_Bitacora_ListarUltimos30,
-- SP_Bitacora_ObtenerExcedente y SP_Bitacora_EliminarPorId).
-- Después de ejecutarlo: Integridad -> Recalcular todos los dígitos.
-- Se puede ejecutar más de una vez.
-- ========================================

IF COL_LENGTH('Bitacora', 'Eliminado') IS NULL
    ALTER TABLE Bitacora ADD Eliminado BIT NOT NULL CONSTRAINT DF_Bitacora_Eliminado DEFAULT 0;
IF COL_LENGTH('Bitacora', 'FechaEliminacion') IS NULL
    ALTER TABLE Bitacora ADD FechaEliminacion DATETIME NULL;
IF COL_LENGTH('Bitacora', 'EliminadoPor') IS NULL
    ALTER TABLE Bitacora ADD EliminadoPor INT NULL CONSTRAINT FK_Bitacora_EliminadoPor REFERENCES Usuario (Id);
GO

DROP PROCEDURE IF EXISTS SP_Bitacora_ListarUltimos30;
DROP PROCEDURE IF EXISTS SP_Bitacora_ObtenerExcedente;
DROP PROCEDURE IF EXISTS SP_Bitacora_EliminarPorId;
GO

-- Eventos de la tabla (los últimos 30 días), incluidos los eliminados: la
-- BLL decide si se muestran.
CREATE OR ALTER PROCEDURE SP_Bitacora_ListarRecientes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT B.Id, B.Accion, B.Fecha, B.Detalle, B.UsuarioId, U.NombreUsuario,
           B.Eliminado, B.FechaEliminacion, B.EliminadoPor, E.NombreUsuario AS NombreEliminadoPor
    FROM Bitacora B
    LEFT JOIN Usuario U ON U.Id = B.UsuarioId
    LEFT JOIN Usuario E ON E.Id = B.EliminadoPor
    ORDER BY B.Fecha DESC;
END
GO

-- Eventos con más de 30 días, para pasarlos al archivo histórico.
CREATE OR ALTER PROCEDURE SP_Bitacora_ObtenerVencidos
    @Dias INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    SELECT B.Id, B.Accion, B.Fecha, B.Detalle, B.UsuarioId, U.NombreUsuario,
           B.Eliminado, B.FechaEliminacion, B.EliminadoPor, E.NombreUsuario AS NombreEliminadoPor
    FROM Bitacora B
    LEFT JOIN Usuario U ON U.Id = B.UsuarioId
    LEFT JOIN Usuario E ON E.Id = B.EliminadoPor
    WHERE B.Fecha < DATEADD(DAY, -@Dias, GETDATE())
    ORDER BY B.Fecha;
END
GO

-- Saca de la tabla un evento que ya quedó en el archivo histórico. Es el
-- único borrado físico, y solo para eventos vencidos.
CREATE OR ALTER PROCEDURE SP_Bitacora_QuitarArchivado
    @Id INT,
    @Dias INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Bitacora WHERE Id = @Id AND Fecha < DATEADD(DAY, -@Dias, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE SP_Bitacora_EliminarLogico
    @Id INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Bitacora SET Eliminado = 1, FechaEliminacion = GETDATE(), EliminadoPor = @UsuarioId
    WHERE Id = @Id AND Eliminado = 0;
END
GO

CREATE OR ALTER PROCEDURE SP_Bitacora_Restaurar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Bitacora SET Eliminado = 0, FechaEliminacion = NULL, EliminadoPor = NULL
    WHERE Id = @Id AND Eliminado = 1;
END
GO

-- ----------------------------------------
-- Permiso para eliminar y restaurar registros (Familia AuditoriaSistema,
-- la misma de VerBitacora).
-- ----------------------------------------
DECLARE @Id INT = (SELECT Id FROM Permiso WHERE Nombre = 'EliminarRegistroBitacora');
IF @Id IS NULL
BEGIN
    SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
    INSERT INTO Permiso (Id, Nombre, Descripcion)
    VALUES (@Id, 'EliminarRegistroBitacora', 'Eliminar (baja lógica) y restaurar registros de la Bitácora.');
END
DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'AuditoriaSistema');
IF @FamiliaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @Id)
    INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @Id, NULL);
GO
