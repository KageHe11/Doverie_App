-- Los SPs que modifican tablas con índices filtrados (UQ_Usuario_GmailIndice)
-- tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Cifrado de los datos personales (checklist: "todos los datos sensibles
-- de las tablas cifrados").
-- Usuario.Gmail, Dni y Telefono, DocumentoCompartido.EmailDestinatario,
-- Dispositivo.Descripcion (lleva la IP) y NotificacionRecibida.Detalle
-- (lleva la IP o el email del destinatario) se guardan cifrados con AES-256
-- desde la DAL (DAL/CifradoDatos.cs). La clave está en AppSettings
-- (CifradoClave), no en la base.
-- Un valor cifrado ocupa más que el original: se agrandan las columnas.
-- La unicidad del email pasa a GmailIndice (HMAC del email normalizado),
-- porque el mismo email cifrado dos veces da textos distintos.
--
-- Orden: 1) este script; 2) Database/CifrarDatos.ps1 (cifra los datos que
-- ya estaban); 3) Integridad -> Recalcular todos los dígitos.
-- Se puede ejecutar más de una vez.
-- ========================================

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Usuario_Gmail' AND object_id = OBJECT_ID('Usuario'))
    DROP INDEX UQ_Usuario_Gmail ON Usuario;
GO
ALTER TABLE Usuario ALTER COLUMN Gmail NVARCHAR(200) NULL;
ALTER TABLE Usuario ALTER COLUMN Dni NVARCHAR(200) NULL;
ALTER TABLE Usuario ALTER COLUMN Telefono NVARCHAR(200) NULL;
IF COL_LENGTH('Usuario', 'GmailIndice') IS NULL
    ALTER TABLE Usuario ADD GmailIndice CHAR(64) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Usuario_GmailIndice' AND object_id = OBJECT_ID('Usuario'))
    CREATE UNIQUE INDEX UQ_Usuario_GmailIndice ON Usuario (GmailIndice) WHERE GmailIndice IS NOT NULL;
GO
ALTER TABLE DocumentoCompartido ALTER COLUMN EmailDestinatario NVARCHAR(400) NOT NULL;
ALTER TABLE Dispositivo ALTER COLUMN Descripcion NVARCHAR(600) NULL;
ALTER TABLE NotificacionRecibida ALTER COLUMN Detalle NVARCHAR(MAX) NULL;
GO

-- ----------------------------------------
-- Usuario: los datos llegan cifrados. @Dni y @Telefono eran INT: un
-- teléfono con espacios o "+" fallaba al registrarse.
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Usuario_Registrar
    @Id INT,
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @NombreUsuario NVARCHAR(50),
    @Contrasena NVARCHAR(MAX),
    @Gmail NVARCHAR(200),
    @GmailIndice CHAR(64),
    @Dni NVARCHAR(200),
    @Telefono NVARCHAR(200) = NULL,
    @Empleado BIT,
    @Empresa BIT,
    @Independiente BIT,
    @Local BIT,
    @PerfilId INT,
    @EmpresaId INT = NULL,
    @IdiomaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Usuario
        (Id, Nombre, Apellido, NombreUsuario, Contrasena, Gmail, GmailIndice, Dni, Telefono,
         Empleado, Empresa, Independiente, Local, Suspendido, Motivo, Intentos,
         PerfilId, EmpresaId, IdiomaId)
    VALUES
        (@Id, @Nombre, @Apellido, @NombreUsuario, @Contrasena, @Gmail, @GmailIndice, @Dni, @Telefono,
         @Empleado, @Empresa, @Independiente, @Local, 0, NULL, 0,
         @PerfilId, @EmpresaId, @IdiomaId);
END
GO

CREATE OR ALTER PROCEDURE SP_Usuario_ActualizarDatos
    @Id INT,
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @Contrasena NVARCHAR(MAX),
    @Gmail NVARCHAR(200),
    @GmailIndice CHAR(64),
    @Dni NVARCHAR(200),
    @Telefono NVARCHAR(200) = NULL,
    @Empleado BIT,
    @Empresa BIT,
    @Independiente BIT,
    @Local BIT,
    @Suspendido BIT,
    @Motivo NVARCHAR(50) = NULL,
    @Intentos INT,
    @PerfilId INT,
    @EmpresaId INT = NULL,
    @IdiomaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuario
    SET Nombre = @Nombre,
        Apellido = @Apellido,
        Contrasena = @Contrasena,
        Gmail = @Gmail,
        GmailIndice = @GmailIndice,
        Dni = @Dni,
        Telefono = @Telefono,
        Empleado = @Empleado,
        Empresa = @Empresa,
        Independiente = @Independiente,
        Local = @Local,
        Suspendido = @Suspendido,
        Motivo = @Motivo,
        Intentos = @Intentos,
        PerfilId = @PerfilId,
        EmpresaId = @EmpresaId,
        IdiomaId = @IdiomaId
    WHERE Id = @Id;
END
GO

-- [ERROR-005 / ERROR-021] email ya registrado: se compara el índice.
CREATE OR ALTER PROCEDURE SP_Usuario_ExistePorGmail
    @GmailIndice CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Usuario WHERE GmailIndice = @GmailIndice;
END
GO

CREATE OR ALTER PROCEDURE SP_Usuario_ExistePorGmailExcluyendo
    @GmailIndice CHAR(64),
    @IdExcluir INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Usuario WHERE GmailIndice = @GmailIndice AND Id != @IdExcluir;
END
GO

-- ----------------------------------------
-- Destinatario de un documento compartido y descripción del dispositivo.
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_DocumentoCompartido_Registrar
    @Id INT,
    @DocumentoId INT,
    @UsuarioId INT,
    @EmailDestinatario NVARCHAR(400),
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

CREATE OR ALTER PROCEDURE SP_NotificacionRecibida_Registrar
    @UsuarioId INT,
    @Evento NVARCHAR(100),
    @Detalle NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NotificacionRecibida (UsuarioId, Evento, Detalle) VALUES (@UsuarioId, @Evento, @Detalle);
END
GO

CREATE OR ALTER PROCEDURE SP_Dispositivo_Registrar
    @UsuarioId INT,
    @Identificador NVARCHAR(64),
    @Descripcion NVARCHAR(600) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Dispositivo WHERE UsuarioId = @UsuarioId AND Identificador = @Identificador)
        INSERT INTO Dispositivo (UsuarioId, Identificador, Descripcion) VALUES (@UsuarioId, @Identificador, @Descripcion);
END
GO
