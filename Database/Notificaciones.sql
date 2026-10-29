-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Notificaciones: CU-001-015 Configurar Notificaciones y patrón Observer.
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- ========================================

-- ----------------------------------------
-- Tablas del diccionario de datos
-- Notificacion: cabecera de la configuración de cada Usuario. Que exista
--   indica que el Usuario ya guardó sus preferencias (si no, se usan las
--   preferencias por defecto).
-- NotificacionEvento: una fila por evento habilitado y canal (Email/Push).
-- ----------------------------------------
IF OBJECT_ID('Notificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Notificacion (
        Id INT NOT NULL CONSTRAINT PK_Notificacion PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_Notificacion_Usuario REFERENCES Usuario(Id)
            CONSTRAINT UQ_Notificacion_Usuario UNIQUE
    );
END
GO
IF OBJECT_ID('NotificacionEvento', 'U') IS NULL
BEGIN
    CREATE TABLE NotificacionEvento (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificacionEvento PRIMARY KEY,
        NotificacionId INT NOT NULL CONSTRAINT FK_NotificacionEvento_Notificacion REFERENCES Notificacion(Id),
        Evento NVARCHAR(100) NOT NULL,
        Canal VARCHAR(50) NOT NULL CONSTRAINT CK_NotificacionEvento_Canal CHECK (Canal IN ('Email', 'Push'))
    );
END
GO

-- ----------------------------------------
-- Tablas nuevas (no están en el diccionario de datos)
-- NotificacionRecibida: bandeja de las notificaciones "dentro de la
--   plataforma" (canal Push). Guarda el evento y su dato, no el texto: el
--   mensaje se arma al mostrarlo, en el idioma de quien lo lee.
-- Dispositivo: dispositivos conocidos de cada Usuario, para el evento
--   "inicio de sesión desde un nuevo dispositivo". Identificador es el
--   SHA-256 del token guardado en una cookie del navegador.
-- ----------------------------------------
IF OBJECT_ID('NotificacionRecibida', 'U') IS NULL
BEGIN
    CREATE TABLE NotificacionRecibida (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificacionRecibida PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_NotificacionRecibida_Usuario REFERENCES Usuario(Id),
        Evento NVARCHAR(100) NOT NULL,
        Detalle NVARCHAR(500) NULL,
        Fecha DATETIME NOT NULL CONSTRAINT DF_NotificacionRecibida_Fecha DEFAULT GETDATE(),
        Leida BIT NOT NULL CONSTRAINT DF_NotificacionRecibida_Leida DEFAULT 0
    );
    CREATE INDEX IX_NotificacionRecibida_Usuario ON NotificacionRecibida(UsuarioId, Leida);
END
GO
IF OBJECT_ID('Dispositivo', 'U') IS NULL
BEGIN
    CREATE TABLE Dispositivo (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dispositivo PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_Dispositivo_Usuario REFERENCES Usuario(Id),
        Identificador NVARCHAR(64) NOT NULL,
        Descripcion NVARCHAR(200) NULL,
        FechaAlta DATETIME NOT NULL CONSTRAINT DF_Dispositivo_FechaAlta DEFAULT GETDATE(),
        CONSTRAINT UQ_Dispositivo UNIQUE (UsuarioId, Identificador)
    );
END
GO

-- ----------------------------------------
-- Stored Procedures: preferencias
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Notificacion_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM Notificacion;
END
GO

-- Devuelve el Id de la configuración del Usuario, o nada si nunca la guardó.
CREATE OR ALTER PROCEDURE SP_Notificacion_ObtenerPorUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id FROM Notificacion WHERE UsuarioId = @UsuarioId;
END
GO

CREATE OR ALTER PROCEDURE SP_Notificacion_Registrar
    @Id INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Notificacion (Id, UsuarioId) VALUES (@Id, @UsuarioId);
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionEvento_ListarPorNotificacion
    @NotificacionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Evento, Canal FROM NotificacionEvento WHERE NotificacionId = @NotificacionId;
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionEvento_Eliminar
    @NotificacionId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM NotificacionEvento WHERE NotificacionId = @NotificacionId;
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionEvento_Registrar
    @NotificacionId INT,
    @Evento NVARCHAR(100),
    @Canal VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NotificacionEvento (NotificacionId, Evento, Canal) VALUES (@NotificacionId, @Evento, @Canal);
END
GO

-- ----------------------------------------
-- Stored Procedures: bandeja (canal Push)
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_NotificacionRecibida_Registrar
    @UsuarioId INT,
    @Evento NVARCHAR(100),
    @Detalle NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO NotificacionRecibida (UsuarioId, Evento, Detalle) VALUES (@UsuarioId, @Evento, @Detalle);
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionRecibida_ListarPorUsuario
    @UsuarioId INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Cantidad) Id, UsuarioId, Evento, Detalle, Fecha, Leida
    FROM NotificacionRecibida
    WHERE UsuarioId = @UsuarioId
    ORDER BY Fecha DESC, Id DESC;
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionRecibida_ContarNoLeidas
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM NotificacionRecibida WHERE UsuarioId = @UsuarioId AND Leida = 0;
END
GO

CREATE OR ALTER PROCEDURE SP_NotificacionRecibida_MarcarLeidas
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE NotificacionRecibida SET Leida = 1 WHERE UsuarioId = @UsuarioId AND Leida = 0;
END
GO

-- ----------------------------------------
-- Stored Procedures: dispositivos
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Dispositivo_Existe
    @UsuarioId INT,
    @Identificador NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Dispositivo WHERE UsuarioId = @UsuarioId AND Identificador = @Identificador;
END
GO

CREATE OR ALTER PROCEDURE SP_Dispositivo_Contar
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Dispositivo WHERE UsuarioId = @UsuarioId;
END
GO

CREATE OR ALTER PROCEDURE SP_Dispositivo_Registrar
    @UsuarioId INT,
    @Identificador NVARCHAR(64),
    @Descripcion NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Dispositivo WHERE UsuarioId = @UsuarioId AND Identificador = @Identificador)
        INSERT INTO Dispositivo (UsuarioId, Identificador, Descripcion) VALUES (@UsuarioId, @Identificador, @Descripcion);
END
GO

-- ----------------------------------------
-- Permiso VerNotificaciones en la Familia Notificaciones, asignada a todos
-- los perfiles en uso: cualquier Usuario puede configurar y leer sus
-- propias notificaciones.
-- ----------------------------------------
DECLARE @PermisoId INT = (SELECT Id FROM Permiso WHERE Nombre = 'VerNotificaciones');
IF @PermisoId IS NULL
BEGIN
    SELECT @PermisoId = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
    INSERT INTO Permiso (Id, Nombre, Descripcion)
    VALUES (@PermisoId, 'VerNotificaciones', 'Permite ver la bandeja de notificaciones y configurar las preferencias propias (CU-001-015)');
END

DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'Notificaciones');
IF @FamiliaId IS NULL
BEGIN
    SELECT @FamiliaId = ISNULL(MAX(Id), 0) + 1 FROM Familia;
    INSERT INTO Familia (Id, Nombre) VALUES (@FamiliaId, 'Notificaciones');
END

IF NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @PermisoId)
    INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @PermisoId, NULL);

INSERT INTO PerfilFamilia (PerfilId, FamiliaId)
SELECT P.Id, @FamiliaId
FROM Perfil P
WHERE P.NombrePerfil IN ('Administrador', 'Empresa', 'Freelance', 'EmpleadoEmpresa', 'EmpleadoAvanzado')
  AND NOT EXISTS (SELECT 1 FROM PerfilFamilia PF WHERE PF.PerfilId = P.Id AND PF.FamiliaId = @FamiliaId);
GO
