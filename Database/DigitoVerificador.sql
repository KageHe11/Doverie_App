-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- ========================================
-- Dígitos verificadores: integridad de los datos a nivel registro (DVH),
-- columna y tabla. Los dígitos los calcula la aplicación (DAL) con
-- HMAC-SHA256 y una clave que no está en la base: quien edite la base a
-- mano no puede recalcularlos. Estos SPs solo leen y guardan.
-- El script se puede ejecutar más de una vez. Después de ejecutarlo, el
-- Administrador tiene que calcular los dígitos desde la pantalla Integridad.
-- ========================================

-- ----------------------------------------
-- Columna DVH en cada tabla de la aplicación.
-- ----------------------------------------
DECLARE @Tabla SYSNAME, @Sql NVARCHAR(MAX);
DECLARE CursorTablas CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.name FROM sys.tables t
    WHERE t.is_ms_shipped = 0 AND t.name <> 'DigitoVerificador'
      AND NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'DVH');
OPEN CursorTablas;
FETCH NEXT FROM CursorTablas INTO @Tabla;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'ALTER TABLE ' + QUOTENAME(@Tabla) + N' ADD DVH CHAR(64) NULL;';
    EXEC (@Sql);
    FETCH NEXT FROM CursorTablas INTO @Tabla;
END
CLOSE CursorTablas;
DEALLOCATE CursorTablas;
GO

-- ----------------------------------------
-- Dígitos verticales: uno por columna de cada tabla y uno de la tabla
-- entera (Columna = '').
-- ----------------------------------------
IF OBJECT_ID('DigitoVerificador', 'U') IS NULL
BEGIN
    CREATE TABLE DigitoVerificador (
        Tabla SYSNAME NOT NULL,
        Columna SYSNAME NOT NULL,
        DV CHAR(64) NOT NULL,
        CONSTRAINT PK_DigitoVerificador PRIMARY KEY (Tabla, Columna)
    );
END
GO

-- ----------------------------------------
-- Stored Procedures
-- ----------------------------------------

-- Tablas protegidas: las que tienen columna DVH.
CREATE OR ALTER PROCEDURE SP_DV_ListarTablas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.name AS Tabla FROM sys.tables t
    WHERE EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'DVH')
    ORDER BY t.name;
END
GO

-- Tablas protegidas que modifica un SP (INSERT, UPDATE, DELETE o MERGE),
-- según las dependencias que registra SQL Server. El DAO lo usa para saber
-- qué dígitos recalcular después de cada escritura.
CREATE OR ALTER PROCEDURE SP_DV_TablasModificadas
    @Procedimiento SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Objeto NVARCHAR(300) = N'dbo.' + PARSENAME(@Procedimiento, 1);
    IF OBJECT_ID(@Objeto, 'P') IS NULL RETURN;
    SELECT DISTINCT e.referenced_entity_name AS Tabla
    FROM sys.dm_sql_referenced_entities(@Objeto, 'OBJECT') e
    WHERE e.referenced_minor_id = 0 AND e.is_updated = 1
      AND EXISTS (SELECT 1 FROM sys.columns c
                  WHERE c.object_id = OBJECT_ID(QUOTENAME(e.referenced_entity_name)) AND c.name = 'DVH');
END
GO

-- Filas de una tabla protegida ordenadas por su clave primaria. Primero
-- devuelve las columnas de la clave (en orden) y después las filas.
CREATE OR ALTER PROCEDURE SP_DV_ObtenerFilas
    @Tabla SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT = OBJECT_ID(QUOTENAME(@Tabla), 'U');
    IF @Id IS NULL OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @Id AND name = 'DVH')
        THROW 50010, 'La tabla no está protegida con dígitos verificadores.', 1;
    SELECT c.name AS Columna
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = @Id AND i.is_primary_key = 1
    ORDER BY ic.key_ordinal;
    DECLARE @Orden NVARCHAR(MAX) = (
        SELECT STRING_AGG(QUOTENAME(c.name), N', ') WITHIN GROUP (ORDER BY ic.key_ordinal)
        FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = @Id AND i.is_primary_key = 1);
    DECLARE @Sql NVARCHAR(MAX) = N'SELECT * FROM ' + QUOTENAME(@Tabla) + N' ORDER BY ' + @Orden + N';';
    EXEC (@Sql);
END
GO

-- Guarda el DVH de una fila. @Clave es un JSON con los valores de la clave
-- primaria, por ejemplo {"Id":"5"} o {"FamiliaId":"1","PermisoId":"3"}.
CREATE OR ALTER PROCEDURE SP_DV_GuardarDVH
    @Tabla SYSNAME,
    @Clave NVARCHAR(MAX),
    @DVH CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT = OBJECT_ID(QUOTENAME(@Tabla), 'U');
    IF @Id IS NULL OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @Id AND name = 'DVH')
        THROW 50010, 'La tabla no está protegida con dígitos verificadores.', 1;
    DECLARE @Condicion NVARCHAR(MAX) = (
        SELECT STRING_AGG(QUOTENAME(c.name) + N' = JSON_VALUE(@Clave, N''$."' + REPLACE(c.name, '''', '''''') + N'"'')', N' AND ')
        FROM sys.indexes i
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE i.object_id = @Id AND i.is_primary_key = 1);
    DECLARE @Sql NVARCHAR(MAX) = N'UPDATE ' + QUOTENAME(@Tabla) + N' SET DVH = @DVH WHERE ' + @Condicion + N';';
    EXEC sp_executesql @Sql, N'@Clave NVARCHAR(MAX), @DVH CHAR(64)', @Clave = @Clave, @DVH = @DVH;
END
GO

CREATE OR ALTER PROCEDURE SP_DV_ObtenerDigitos
    @Tabla SYSNAME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Columna, DV FROM DigitoVerificador WHERE Tabla = @Tabla;
END
GO

-- Reemplaza los dígitos verticales de una tabla. @Json:
-- [{"Columna":"Nombre","DV":"..."}, ..., {"Columna":"","DV":"..."}]
CREATE OR ALTER PROCEDURE SP_DV_GuardarDigitos
    @Tabla SYSNAME,
    @Json NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    DELETE FROM DigitoVerificador WHERE Tabla = @Tabla;
    INSERT INTO DigitoVerificador (Tabla, Columna, DV)
    SELECT @Tabla, J.Columna, J.DV FROM OPENJSON(@Json) WITH (Columna SYSNAME '$.Columna', DV CHAR(64) '$.DV') J;
    COMMIT TRANSACTION;
END
GO

-- ----------------------------------------
-- Permiso GestionarIntegridad en la Familia RespaldoSistema (Administrador):
-- con la base inconsistente, solo quien lo tiene puede iniciar sesión.
-- ----------------------------------------
DECLARE @PermisoId INT = (SELECT Id FROM Permiso WHERE Nombre = 'GestionarIntegridad');
IF @PermisoId IS NULL
BEGIN
    SELECT @PermisoId = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
    INSERT INTO Permiso (Id, Nombre, Descripcion)
    VALUES (@PermisoId, 'GestionarIntegridad', 'Permite ver el estado de los dígitos verificadores, recalcularlos e ingresar con la base inconsistente');
END
DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'RespaldoSistema');
IF @FamiliaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @PermisoId)
    INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @PermisoId, NULL);
GO
