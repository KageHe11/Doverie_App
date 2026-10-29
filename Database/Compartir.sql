-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Compartir documentos: CU-001-005 Compartir Documento y
-- CU-001-018 Acceder a Documento Compartido.
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- Requiere haber ejecutado antes Database/Documentos.sql.
-- ========================================

-- ----------------------------------------
-- Tabla DocumentoCompartido (no está en el diccionario de datos)
-- Un enlace de solo lectura a un documento, para un destinatario externo.
-- TokenHash es el SHA-256 del token que viaja en el enlace: el token en
-- sí nunca se guarda, así que leer esta tabla no permite armar enlaces.
-- ----------------------------------------
IF OBJECT_ID('DocumentoCompartido', 'U') IS NULL
BEGIN
    CREATE TABLE DocumentoCompartido (
        Id INT NOT NULL CONSTRAINT PK_DocumentoCompartido PRIMARY KEY,
        DocumentoId INT NOT NULL CONSTRAINT FK_DocumentoCompartido_Documento REFERENCES Documento(Id),
        UsuarioId INT NOT NULL CONSTRAINT FK_DocumentoCompartido_Usuario REFERENCES Usuario(Id),
        EmailDestinatario NVARCHAR(100) NOT NULL,
        TokenHash NVARCHAR(64) NOT NULL CONSTRAINT UQ_DocumentoCompartido_Token UNIQUE,
        FechaCreacion DATETIME NOT NULL,
        FechaVencimiento DATETIME NOT NULL,
        Revocado BIT NOT NULL CONSTRAINT DF_DocumentoCompartido_Revocado DEFAULT 0,
        CantidadAccesos INT NOT NULL CONSTRAINT DF_DocumentoCompartido_Accesos DEFAULT 0,
        UltimoAcceso DATETIME NULL
    );
    CREATE INDEX IX_DocumentoCompartido_Documento ON DocumentoCompartido(DocumentoId);
END
GO

-- ----------------------------------------
-- Stored Procedures
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM DocumentoCompartido;
END
GO

-- Usado por Registrar -> CU-001-005
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_Registrar
    @Id INT,
    @DocumentoId INT,
    @UsuarioId INT,
    @EmailDestinatario NVARCHAR(100),
    @TokenHash NVARCHAR(64),
    @FechaCreacion DATETIME,
    @FechaVencimiento DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DocumentoCompartido (Id, DocumentoId, UsuarioId, EmailDestinatario, TokenHash, FechaCreacion, FechaVencimiento)
    VALUES (@Id, @DocumentoId, @UsuarioId, @EmailDestinatario, @TokenHash, @FechaCreacion, @FechaVencimiento);
END
GO

-- Usado por ObtenerPorToken -> CU-001-018 (incluye quién compartió)
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_ObtenerPorTokenHash
    @TokenHash NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT C.Id, C.DocumentoId, C.UsuarioId, C.EmailDestinatario, C.FechaCreacion, C.FechaVencimiento,
           C.Revocado, C.CantidadAccesos, C.UltimoAcceso, U.NombreUsuario
    FROM DocumentoCompartido C
    JOIN Usuario U ON U.Id = C.UsuarioId
    WHERE C.TokenHash = @TokenHash;
END
GO

CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT C.Id, C.DocumentoId, C.UsuarioId, C.EmailDestinatario, C.FechaCreacion, C.FechaVencimiento,
           C.Revocado, C.CantidadAccesos, C.UltimoAcceso, U.NombreUsuario
    FROM DocumentoCompartido C
    JOIN Usuario U ON U.Id = C.UsuarioId
    WHERE C.Id = @Id;
END
GO

-- Usado por ListarPorDocumento -> enlaces existentes de un documento
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_ListarPorDocumento
    @DocumentoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT C.Id, C.DocumentoId, C.UsuarioId, C.EmailDestinatario, C.FechaCreacion, C.FechaVencimiento,
           C.Revocado, C.CantidadAccesos, C.UltimoAcceso, U.NombreUsuario
    FROM DocumentoCompartido C
    JOIN Usuario U ON U.Id = C.UsuarioId
    WHERE C.DocumentoId = @DocumentoId
    ORDER BY C.FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_Revocar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DocumentoCompartido SET Revocado = 1 WHERE Id = @Id;
END
GO

-- Usado por RegistrarAcceso -> CU-001-018 paso 7
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_RegistrarAcceso
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DocumentoCompartido
    SET CantidadAccesos = CantidadAccesos + 1,
        UltimoAcceso = GETDATE()
    WHERE Id = @Id;
END
GO

-- ----------------------------------------
-- Permiso CompartirDocumento en la Familia GestionDocumentos (Freelance,
-- Empresa y EmpleadoAvanzado; EmpleadoEmpresa sigue de solo lectura).
-- ----------------------------------------
DECLARE @PermisoId INT = (SELECT Id FROM Permiso WHERE Nombre = 'CompartirDocumento');
IF @PermisoId IS NULL
BEGIN
    SELECT @PermisoId = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
    INSERT INTO Permiso (Id, Nombre, Descripcion)
    VALUES (@PermisoId, 'CompartirDocumento', 'Permite compartir documentos de la cuenta con terceros mediante un enlace de solo lectura (CU-001-005)');
END
DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'GestionDocumentos');
IF @FamiliaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @PermisoId)
    INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @PermisoId, NULL);
GO
