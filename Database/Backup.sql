-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ========================================
-- CU-002-013 Generar Backup Manual: copia de la base de datos y de los
-- archivos de los documentos (AWS S3 o disco).
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- ========================================

-- ----------------------------------------
-- Tabla BackUp (diccionario de datos), entre corchetes porque BACKUP es
-- palabra reservada. Detalle pasa de nvarchar(50) a
-- nvarchar(500): guarda el archivo .bak, su tamaño y dónde quedó la copia
-- de los documentos, que no entran en 50 caracteres.
-- ----------------------------------------
IF OBJECT_ID('BackUp', 'U') IS NULL
BEGIN
    CREATE TABLE [BackUp] (
        Id INT NOT NULL CONSTRAINT PK_BackUp PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_BackUp_Usuario REFERENCES Usuario(Id),
        Fecha DATETIME NOT NULL,
        Estado VARCHAR(50) NOT NULL CONSTRAINT CK_BackUp_Estado CHECK (Estado IN ('Exitoso', 'Fallido')),
        Detalle NVARCHAR(500) NULL
    );
END
GO

-- ----------------------------------------
-- Stored Procedures
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_BackUp_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM [BackUp];
END
GO

CREATE OR ALTER PROCEDURE SP_BackUp_Registrar
    @Id INT,
    @UsuarioId INT,
    @Fecha DATETIME,
    @Estado VARCHAR(50),
    @Detalle NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [BackUp] (Id, UsuarioId, Fecha, Estado, Detalle)
    VALUES (@Id, @UsuarioId, @Fecha, @Estado, @Detalle);
END
GO

-- Historial de backups (los más recientes primero). Completo: el restore
-- lo usa para no perder backups que siguen existiendo.
CREATE OR ALTER PROCEDURE SP_BackUp_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT B.Id, B.UsuarioId, U.NombreUsuario, B.Fecha, B.Estado, B.Detalle
    FROM [BackUp] B
    JOIN Usuario U ON U.Id = B.UsuarioId
    ORDER BY B.Fecha DESC, B.Id DESC;
END
GO

-- CU-002-013 paso 4: copia completa de la base. COPY_ONLY para no cortar
-- la cadena de los backups periódicos automáticos; CHECKSUM verifica las
-- páginas al leerlas. Comprime salvo en Express, que no lo soporta.
-- Sin @Carpeta usa la carpeta de backups de la instancia, donde el
-- servicio de SQL Server ya tiene permiso de escritura.
CREATE OR ALTER PROCEDURE SP_BackUp_GenerarBaseDatos
    @Archivo NVARCHAR(200),
    @Carpeta NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Base SYSNAME = DB_NAME();
    IF @Carpeta IS NULL SET @Carpeta = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(300));
    DECLARE @Ruta NVARCHAR(500) = @Carpeta + CASE WHEN RIGHT(@Carpeta, 1) = '\' THEN '' ELSE '\' END + @Archivo;
    -- El nombre del backup es el del archivo (trae la fecha): el restore lo
    -- usa para encontrar la copia de los documentos de ese mismo backup.
    DECLARE @Nombre NVARCHAR(128) = LEFT(@Archivo, 128);

    IF CAST(SERVERPROPERTY('EngineEdition') AS INT) = 4
        BACKUP DATABASE @Base TO DISK = @Ruta WITH COPY_ONLY, INIT, FORMAT, CHECKSUM, NAME = @Nombre;
    ELSE
        BACKUP DATABASE @Base TO DISK = @Ruta WITH COPY_ONLY, INIT, FORMAT, CHECKSUM, COMPRESSION, NAME = @Nombre;

    SELECT TOP 1 @Ruta AS Ruta, ISNULL(S.compressed_backup_size, S.backup_size) AS Tamano
    FROM msdb.dbo.backupset S
    JOIN msdb.dbo.backupmediafamily M ON M.media_set_id = S.media_set_id
    WHERE S.database_name = @Base AND M.physical_device_name = @Ruta
    ORDER BY S.backup_set_id DESC;
END
GO

-- Restore: un backup del historial por Id.
CREATE OR ALTER PROCEDURE SP_BackUp_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT B.Id, B.UsuarioId, U.NombreUsuario, B.Fecha, B.Estado, B.Detalle
    FROM [BackUp] B
    JOIN Usuario U ON U.Id = B.UsuarioId
    WHERE B.Id = @Id;
END
GO

-- Carpeta de backups de la instancia, donde SP_BackUp_GenerarBaseDatos deja
-- los .bak cuando no se configura otra.
CREATE OR ALTER PROCEDURE SP_BackUp_ObtenerCarpeta
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(300)) AS Carpeta;
END
GO

-- ----------------------------------------
-- Permisos GenerarBackup y RestaurarBackup en la Familia RespaldoSistema,
-- asignada al Perfil Administrador.
-- ----------------------------------------
DECLARE @Permisos TABLE (Nombre NVARCHAR(50), Descripcion NVARCHAR(MAX));
INSERT INTO @Permisos VALUES
    ('GenerarBackup', 'Permite generar un backup manual de la base de datos y de los archivos en AWS S3 (CU-002-013)'),
    ('RestaurarBackup', 'Permite restaurar la base de datos y los archivos en AWS S3 desde un backup');

DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'RespaldoSistema');
IF @FamiliaId IS NULL
BEGIN
    SELECT @FamiliaId = ISNULL(MAX(Id), 0) + 1 FROM Familia;
    INSERT INTO Familia (Id, Nombre) VALUES (@FamiliaId, 'RespaldoSistema');
END

DECLARE @Nombre NVARCHAR(50), @Descripcion NVARCHAR(MAX), @PermisoId INT;
DECLARE CursorPermisos CURSOR LOCAL FAST_FORWARD FOR SELECT Nombre, Descripcion FROM @Permisos;
OPEN CursorPermisos;
FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @PermisoId = (SELECT Id FROM Permiso WHERE Nombre = @Nombre);
    IF @PermisoId IS NULL
    BEGIN
        SELECT @PermisoId = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
        INSERT INTO Permiso (Id, Nombre, Descripcion) VALUES (@PermisoId, @Nombre, @Descripcion);
    END
    IF NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @PermisoId)
        INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @PermisoId, NULL);
    FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion;
END
CLOSE CursorPermisos;
DEALLOCATE CursorPermisos;

DECLARE @PerfilAdminId INT = (SELECT Id FROM Perfil WHERE NombrePerfil = 'Administrador');
IF @PerfilAdminId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerfilFamilia WHERE PerfilId = @PerfilAdminId AND FamiliaId = @FamiliaId)
    INSERT INTO PerfilFamilia (PerfilId, FamiliaId) VALUES (@PerfilAdminId, @FamiliaId);
GO
