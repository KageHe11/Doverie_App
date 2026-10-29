-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Estadísticas: CU-002-012 Ver Estadísticas de la Plataforma y los
-- webservices WS_Documentos, WS_Usuarios y WS_Blockchain (anexo 10.12.1).
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- Requiere haber ejecutado antes Database/Documentos.sql.
-- ========================================

-- ----------------------------------------
-- Usuario.FechaAlta (no está en el diccionario de datos): necesaria para
-- ObtenerUsuariosPorPeriodo. Los usuarios existentes quedan en NULL
-- (no se sabe cuándo se registraron); los nuevos la toman del DEFAULT, sin
-- tocar SP_Usuario_Registrar.
-- ----------------------------------------
IF COL_LENGTH('Usuario', 'FechaAlta') IS NULL
BEGIN
    ALTER TABLE Usuario ADD FechaAlta DATETIME NULL CONSTRAINT DF_Usuario_FechaAlta DEFAULT GETDATE();
END
GO

-- ----------------------------------------
-- VerificacionDocumento (no está en el diccionario de datos): registro de
-- cada verificación de integridad (CU-001-006), para
-- ObtenerVerificacionesPorPeriodo. La Bitácora no sirve para contarlas
-- porque archiva en XML todo lo que pasa de 30 filas.
-- ----------------------------------------
IF OBJECT_ID('VerificacionDocumento', 'U') IS NULL
BEGIN
    CREATE TABLE VerificacionDocumento (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VerificacionDocumento PRIMARY KEY,
        DocumentoId INT NOT NULL CONSTRAINT FK_VerificacionDocumento_Documento REFERENCES Documento(Id),
        UsuarioId INT NOT NULL CONSTRAINT FK_VerificacionDocumento_Usuario REFERENCES Usuario(Id),
        Fecha DATETIME NOT NULL CONSTRAINT DF_VerificacionDocumento_Fecha DEFAULT GETDATE(),
        Integro BIT NOT NULL
    );
    CREATE INDEX IX_VerificacionDocumento_Fecha ON VerificacionDocumento(Fecha);
END
GO

CREATE OR ALTER PROCEDURE SP_VerificacionDocumento_Registrar
    @DocumentoId INT,
    @UsuarioId INT,
    @Integro BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO VerificacionDocumento (DocumentoId, UsuarioId, Integro) VALUES (@DocumentoId, @UsuarioId, @Integro);
END
GO

-- ----------------------------------------
-- Stored Procedures de estadísticas. Los períodos son [@Desde, @Hasta):
-- la BLL pasa @Hasta como el día siguiente al elegido.
-- ----------------------------------------

-- CU-002-012 pasos 2, 3 y 4: panel general.
-- Cada documento registrado es una operación en la blockchain (el hash
-- queda aunque el documento se elimine); los almacenados son los activos.
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerResumen
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM Usuario WHERE Suspendido = 0) AS UsuariosActivos,
        (SELECT COUNT(*) FROM Documento WHERE Estado = 'Activo') AS DocumentosAlmacenados,
        (SELECT COUNT(*) FROM Documento) AS OperacionesBlockchain;
END
GO

-- WS_Documentos.ObtenerDocumentosPorPeriodo
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerDocumentosPorPeriodo
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Documento WHERE FechaSubida >= @Desde AND FechaSubida < @Hasta;
END
GO

-- WS_Documentos.ObtenerVerificacionesPorPeriodo
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerVerificacionesPorPeriodo
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM VerificacionDocumento WHERE Fecha >= @Desde AND Fecha < @Hasta;
END
GO

-- WS_Usuarios.ObtenerUsuariosPorPeriodo
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerUsuariosPorPeriodo
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Usuario WHERE FechaAlta >= @Desde AND FechaAlta < @Hasta;
END
GO

-- WS_Blockchain.ObtenerOperacionesPorPeriodo
-- Hoy cada operación en la blockchain es el registro del hash de un
-- documento subido. Cuando existan los NFT (CU-001-007), se suman acá.
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerOperacionesPorPeriodo
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Documento WHERE FechaSubida >= @Desde AND FechaSubida < @Hasta;
END
GO

-- WS_Blockchain.ObtenerTransaccionesPorUsuario
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerTransaccionesPorUsuario
    @UsuarioId INT,
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Documento WHERE UsuarioId = @UsuarioId AND FechaSubida >= @Desde AND FechaSubida < @Hasta;
END
GO

-- ----------------------------------------
-- Permiso VerEstadisticas en la Familia EstadisticasPlataforma, asignada
-- al Administrador (RF-002).
-- ----------------------------------------
DECLARE @PermisoId INT = (SELECT Id FROM Permiso WHERE Nombre = 'VerEstadisticas');
IF @PermisoId IS NULL
BEGIN
    SELECT @PermisoId = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
    INSERT INTO Permiso (Id, Nombre, Descripcion)
    VALUES (@PermisoId, 'VerEstadisticas', 'Permite ver las estadísticas de la plataforma y consultar sus webservices (CU-002-012)');
END
DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'EstadisticasPlataforma');
IF @FamiliaId IS NULL
BEGIN
    SELECT @FamiliaId = ISNULL(MAX(Id), 0) + 1 FROM Familia;
    INSERT INTO Familia (Id, Nombre) VALUES (@FamiliaId, 'EstadisticasPlataforma');
END
IF NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @PermisoId)
    INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @PermisoId, NULL);
INSERT INTO PerfilFamilia (PerfilId, FamiliaId)
SELECT P.Id, @FamiliaId FROM Perfil P
WHERE P.NombrePerfil = 'Administrador'
  AND NOT EXISTS (SELECT 1 FROM PerfilFamilia PF WHERE PF.PerfilId = P.Id AND PF.FamiliaId = @FamiliaId);
GO
