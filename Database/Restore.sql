-- ========================================
-- Restore de un backup de Doverie (requisito "Restore" de la
-- documentación, complemento de CU-002-013).
-- Restaurar una base no se puede hacer conectado a esa misma base: estos
-- SPs viven en master y la app los llama con una conexión a master. Hace
-- falta un login con permiso de restore (dbcreator o sysadmin).
-- El script se puede ejecutar más de una vez.
-- ========================================
USE master;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Encabezado del .bak: de qué base es (DatabaseName) y con qué nombre se
-- generó (BackupName, que en los backups de Doverie trae la fecha).
CREATE OR ALTER PROCEDURE dbo.SP_Doverie_LeerBackup
    @Ruta NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    RESTORE HEADERONLY FROM DISK = @Ruta;
END
GO

-- Verifica que el .bak esté completo y se pueda leer (y sus checksums, si
-- los tiene). Si no, lanza el error.
CREATE OR ALTER PROCEDURE dbo.SP_Doverie_VerificarBackup
    @Ruta NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    RESTORE VERIFYONLY FROM DISK = @Ruta;
END
GO

-- Reemplaza la base @Base con el contenido del .bak. La base se pone
-- OFFLINE (cortando las conexiones abiertas) para que nadie la tome entre
-- medio; los archivos se restauran sobre los actuales de la base (MOVE).
-- Si el restore falla, la base vuelve a ONLINE como estaba.
CREATE OR ALTER PROCEDURE dbo.SP_Doverie_RestaurarBaseDatos
    @Base SYSNAME,
    @Ruta NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    IF DB_ID(@Base) IS NULL THROW 50001, 'La base a restaurar no existe.', 1;
    DECLARE @Mover NVARCHAR(MAX) = N'';
    SELECT @Mover += N', MOVE N''' + REPLACE(name, '''', '''''') + N''' TO N''' + REPLACE(physical_name, '''', '''''') + N''''
    FROM sys.master_files WHERE database_id = DB_ID(@Base);

    DECLARE @Offline NVARCHAR(400) = N'ALTER DATABASE ' + QUOTENAME(@Base) + N' SET OFFLINE WITH ROLLBACK IMMEDIATE;';
    DECLARE @Online NVARCHAR(400) = N'ALTER DATABASE ' + QUOTENAME(@Base) + N' SET ONLINE;';
    DECLARE @Restore NVARCHAR(MAX) = N'RESTORE DATABASE ' + QUOTENAME(@Base) + N' FROM DISK = @Ruta WITH REPLACE' + @Mover + N';';

    EXEC (@Offline);
    BEGIN TRY
        EXEC sp_executesql @Restore, N'@Ruta NVARCHAR(500)', @Ruta = @Ruta;
    END TRY
    BEGIN CATCH
        EXEC (@Online);
        THROW;
    END CATCH
END
GO
