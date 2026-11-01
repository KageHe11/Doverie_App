-- Igual que SSMS: los SPs que modifican tablas con índices filtrados (ej.
-- UQ_Usuario_Gmail) tienen que crearse con QUOTED_IDENTIFIER ON.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ========================================
-- Suscripciones: CU-001-017 Suscribirse, CU-001-008 Gestionar Suscripción
-- y CU-001-014 Ver Detalle de Facturación.
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- Requiere haber ejecutado antes Database/Estadisticas.sql.
-- ========================================

-- ----------------------------------------
-- Tabla Plan (diccionario de datos + columnas nuevas)
-- Un Plan es un nivel de servicio en una modalidad de pago: Nombre es el
-- nivel (Plata, Oro, Platino) y Tipo la modalidad (Mensual, Trimestral,
-- Semestral, Anual), como en la lista de precios (tabla 48).
-- Columnas nuevas (no están en el diccionario):
--   PrecioMensual: precio por mes en ARS, ya con el descuento de la modalidad.
--   Meses: duración del período que se abona por adelantado.
--   MaxUsuarios: usuarios de la cuenta, incluido el titular. NULL = sin límite.
-- ----------------------------------------
IF OBJECT_ID('Plan', 'U') IS NULL
BEGIN
    CREATE TABLE [Plan] (
        Id INT NOT NULL CONSTRAINT PK_Plan PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Plan_Activo DEFAULT 1,
        Tipo VARCHAR(50) NOT NULL,
        PrecioMensual DECIMAL(10,2) NOT NULL,
        Meses INT NOT NULL,
        MaxUsuarios INT NULL,
        CONSTRAINT UQ_Plan_NombreTipo UNIQUE (Nombre, Tipo)
    );
END
GO

-- ----------------------------------------
-- Tabla MedioPago (diccionario de datos)
-- ----------------------------------------
IF OBJECT_ID('MedioPago', 'U') IS NULL
BEGIN
    CREATE TABLE MedioPago (
        Id INT NOT NULL CONSTRAINT PK_MedioPago PRIMARY KEY,
        Nombre VARCHAR(50) NOT NULL CONSTRAINT UQ_MedioPago_Nombre UNIQUE,
        Activo BIT NOT NULL CONSTRAINT DF_MedioPago_Activo DEFAULT 1
    );
END
GO

-- ----------------------------------------
-- Tabla Suscripcion (diccionario de datos)
-- UsuarioId es el titular de la cuenta: la Empresa o el Usuario
-- independiente. Los empleados usan la suscripción de su Empresa.
-- Estado (códigos de BE.SuscripcionBE):
--   1 Pendiente de pago, 2 Activa, 3 Cancelada (no se renueva; el acceso
--   sigue hasta FechaRenovacion), 4 Vencida, 5 Rechazada (el pago inicial
--   no se aprobó), 6 Reemplazada (por un cambio de plan).
-- ----------------------------------------
IF OBJECT_ID('Suscripcion', 'U') IS NULL
BEGIN
    CREATE TABLE Suscripcion (
        Id INT NOT NULL CONSTRAINT PK_Suscripcion PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_Suscripcion_Usuario REFERENCES Usuario(Id),
        PlanId INT NOT NULL CONSTRAINT FK_Suscripcion_Plan REFERENCES [Plan](Id),
        Estado INT NOT NULL CONSTRAINT CK_Suscripcion_Estado CHECK (Estado BETWEEN 1 AND 6),
        FechaInicio DATETIME NOT NULL,
        FechaRenovacion DATETIME NOT NULL
    );
    CREATE INDEX IX_Suscripcion_Usuario ON Suscripcion(UsuarioId, Estado);
END
GO

-- ----------------------------------------
-- Tabla Pago (diccionario de datos + Concepto)
-- ComprobanteUrl: identificador de la operación en la pasarela de pago
-- (el comprobante se arma a partir del pago; el PDF se genera al pedirlo).
-- Estado: Pendiente, Aprobado, Rechazado o Anulado (un pago pendiente que
-- se abandonó al iniciar otro).
-- Concepto (nueva): Alta, Renovacion o CambioPlan.
-- ----------------------------------------
IF OBJECT_ID('Pago', 'U') IS NULL
BEGIN
    CREATE TABLE Pago (
        Id INT NOT NULL CONSTRAINT PK_Pago PRIMARY KEY,
        SuscripcionId INT NOT NULL CONSTRAINT FK_Pago_Suscripcion REFERENCES Suscripcion(Id),
        MedioPagoId INT NOT NULL CONSTRAINT FK_Pago_MedioPago REFERENCES MedioPago(Id),
        ComprobanteUrl NVARCHAR(MAX) NULL,
        Estado VARCHAR(50) NOT NULL CONSTRAINT CK_Pago_Estado CHECK (Estado IN ('Pendiente', 'Aprobado', 'Rechazado', 'Anulado')),
        Fecha DATETIME NOT NULL,
        Monto DECIMAL(10,2) NOT NULL,
        Concepto VARCHAR(50) NOT NULL CONSTRAINT CK_Pago_Concepto CHECK (Concepto IN ('Alta', 'Renovacion', 'CambioPlan'))
    );
    CREATE INDEX IX_Pago_Suscripcion ON Pago(SuscripcionId);
END
GO

-- ----------------------------------------
-- Datos: planes de la lista de precios (tabla 48) y medios de pago.
-- Los precios son estimados (sección 6.2.4): se actualizan con este mismo
-- script, que pisa los valores de los planes existentes.
-- ----------------------------------------
DECLARE @Planes TABLE (Id INT, Nombre VARCHAR(50), Tipo VARCHAR(50), PrecioMensual DECIMAL(10,2), Meses INT, MaxUsuarios INT);
INSERT INTO @Planes VALUES
    (1,  'Plata',   'Mensual',    16000, 1,  1),
    (2,  'Plata',   'Trimestral', 14400, 3,  1),
    (3,  'Plata',   'Semestral',  13600, 6,  1),
    (4,  'Plata',   'Anual',      12800, 12, 1),
    (5,  'Oro',     'Mensual',    30000, 1,  5),
    (6,  'Oro',     'Trimestral', 27000, 3,  5),
    (7,  'Oro',     'Semestral',  25500, 6,  5),
    (8,  'Oro',     'Anual',      24000, 12, 5),
    (9,  'Platino', 'Mensual',    52000, 1,  NULL),
    (10, 'Platino', 'Trimestral', 46800, 3,  NULL),
    (11, 'Platino', 'Semestral',  44200, 6,  NULL),
    (12, 'Platino', 'Anual',      41600, 12, NULL);

UPDATE P SET PrecioMensual = N.PrecioMensual, Meses = N.Meses, MaxUsuarios = N.MaxUsuarios
FROM [Plan] P JOIN @Planes N ON N.Nombre = P.Nombre AND N.Tipo = P.Tipo;

INSERT INTO [Plan] (Id, Nombre, Activo, Tipo, PrecioMensual, Meses, MaxUsuarios)
SELECT N.Id, N.Nombre, 1, N.Tipo, N.PrecioMensual, N.Meses, N.MaxUsuarios
FROM @Planes N
WHERE NOT EXISTS (SELECT 1 FROM [Plan] P WHERE P.Nombre = N.Nombre AND P.Tipo = N.Tipo);

-- Mercado Pago (la pasarela prevista) ofrece estos medios.
DECLARE @Medios TABLE (Id INT, Nombre VARCHAR(50));
INSERT INTO @Medios VALUES (1, 'Tarjeta de crédito'), (2, 'Tarjeta de débito'), (3, 'Dinero en cuenta de Mercado Pago');
INSERT INTO MedioPago (Id, Nombre, Activo)
SELECT M.Id, M.Nombre, 1 FROM @Medios M
WHERE NOT EXISTS (SELECT 1 FROM MedioPago X WHERE X.Nombre = M.Nombre);
GO

-- ----------------------------------------
-- Stored Procedures: planes y medios de pago
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Plan_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Activo, Tipo, PrecioMensual, Meses, MaxUsuarios
    FROM [Plan] WHERE Activo = 1
    ORDER BY Id;
END
GO

CREATE OR ALTER PROCEDURE SP_Plan_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Activo, Tipo, PrecioMensual, Meses, MaxUsuarios FROM [Plan] WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE SP_MedioPago_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Activo FROM MedioPago WHERE Activo = 1 ORDER BY Id;
END
GO

-- ----------------------------------------
-- Stored Procedures: suscripciones
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Suscripcion_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM Suscripcion;
END
GO

CREATE OR ALTER PROCEDURE SP_Suscripcion_Registrar
    @Id INT,
    @UsuarioId INT,
    @PlanId INT,
    @Estado INT,
    @FechaInicio DATETIME,
    @FechaRenovacion DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Suscripcion (Id, UsuarioId, PlanId, Estado, FechaInicio, FechaRenovacion)
    VALUES (@Id, @UsuarioId, @PlanId, @Estado, @FechaInicio, @FechaRenovacion);
END
GO

-- Suscripción en curso de la cuenta (Activa o Cancelada), la más reciente.
-- Si ya pasó su FechaRenovacion, la BLL la renueva o la da por vencida.
CREATE OR ALTER PROCEDURE SP_Suscripcion_ObtenerEnCurso
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 S.Id, S.UsuarioId, S.PlanId, S.Estado, S.FechaInicio, S.FechaRenovacion
    FROM Suscripcion S
    WHERE S.UsuarioId = @UsuarioId AND S.Estado IN (2, 3)
    ORDER BY S.FechaInicio DESC, S.Id DESC;
END
GO

CREATE OR ALTER PROCEDURE SP_Suscripcion_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UsuarioId, PlanId, Estado, FechaInicio, FechaRenovacion FROM Suscripcion WHERE Id = @Id;
END
GO

-- CU-001-008 4.1 Cancelar (Estado 3) y vencimiento (Estado 4).
CREATE OR ALTER PROCEDURE SP_Suscripcion_ActualizarEstado
    @Id INT,
    @Estado INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Suscripcion SET Estado = @Estado WHERE Id = @Id;
END
GO

-- Renovación automática: el período abonado se extiende.
CREATE OR ALTER PROCEDURE SP_Suscripcion_ActualizarRenovacion
    @Id INT,
    @FechaRenovacion DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Suscripcion SET FechaRenovacion = @FechaRenovacion WHERE Id = @Id;
END
GO

-- ----------------------------------------
-- Stored Procedures: pagos
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Pago_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM Pago;
END
GO

CREATE OR ALTER PROCEDURE SP_Pago_Registrar
    @Id INT,
    @SuscripcionId INT,
    @MedioPagoId INT,
    @ComprobanteUrl NVARCHAR(MAX) = NULL,
    @Estado VARCHAR(50),
    @Fecha DATETIME,
    @Monto DECIMAL(10,2),
    @Concepto VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Pago (Id, SuscripcionId, MedioPagoId, ComprobanteUrl, Estado, Fecha, Monto, Concepto)
    VALUES (@Id, @SuscripcionId, @MedioPagoId, @ComprobanteUrl, @Estado, @Fecha, @Monto, @Concepto);
END
GO

-- Columnas comunes de las consultas de pagos (con plan, medio y titular).
CREATE OR ALTER PROCEDURE SP_Pago_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PG.Id, PG.SuscripcionId, PG.MedioPagoId, PG.ComprobanteUrl, PG.Estado, PG.Fecha, PG.Monto, PG.Concepto,
           M.Nombre AS MedioPago, S.UsuarioId, S.PlanId, P.Nombre AS PlanNombre, P.Tipo AS PlanTipo
    FROM Pago PG
    JOIN Suscripcion S ON S.Id = PG.SuscripcionId
    JOIN [Plan] P ON P.Id = S.PlanId
    JOIN MedioPago M ON M.Id = PG.MedioPagoId
    WHERE PG.Id = @Id;
END
GO

-- CU-001-014 paso 2: historial de pagos de la cuenta (los anulados no
-- son operaciones reales y no se muestran).
CREATE OR ALTER PROCEDURE SP_Pago_ListarPorUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PG.Id, PG.SuscripcionId, PG.MedioPagoId, PG.ComprobanteUrl, PG.Estado, PG.Fecha, PG.Monto, PG.Concepto,
           M.Nombre AS MedioPago, S.UsuarioId, S.PlanId, P.Nombre AS PlanNombre, P.Tipo AS PlanTipo
    FROM Pago PG
    JOIN Suscripcion S ON S.Id = PG.SuscripcionId
    JOIN [Plan] P ON P.Id = S.PlanId
    JOIN MedioPago M ON M.Id = PG.MedioPagoId
    WHERE S.UsuarioId = @UsuarioId AND PG.Estado <> 'Anulado'
    ORDER BY PG.Fecha DESC, PG.Id DESC;
END
GO

-- Medio del último pago aprobado: el "medio de pago registrado", que se
-- usa para el débito automático de las renovaciones.
CREATE OR ALTER PROCEDURE SP_Pago_ObtenerUltimoMedioAprobado
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 PG.MedioPagoId
    FROM Pago PG JOIN Suscripcion S ON S.Id = PG.SuscripcionId
    WHERE S.UsuarioId = @UsuarioId AND PG.Estado = 'Aprobado'
    ORDER BY PG.Fecha DESC, PG.Id DESC;
END
GO

-- Al iniciar un pago nuevo, los pendientes anteriores de la cuenta (pagos
-- abandonados en la pasarela) se anulan, así no se pueden confirmar después.
CREATE OR ALTER PROCEDURE SP_Pago_AnularPendientes
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    UPDATE PG SET Estado = 'Anulado'
    FROM Pago PG JOIN Suscripcion S ON S.Id = PG.SuscripcionId
    WHERE S.UsuarioId = @UsuarioId AND PG.Estado = 'Pendiente';
    UPDATE Suscripcion SET Estado = 5 WHERE UsuarioId = @UsuarioId AND Estado = 1;
    COMMIT TRANSACTION;
END
GO

-- CU-001-017 pasos 7-8 y CU-001-008 paso 7: pago aprobado. En una sola
-- transacción: el pago queda aprobado, su suscripción se activa y la que
-- estuviera en curso (si es un cambio de plan) queda reemplazada.
CREATE OR ALTER PROCEDURE SP_Pago_Aprobar
    @Id INT,
    @ComprobanteUrl NVARCHAR(MAX),
    @FechaInicio DATETIME,
    @FechaRenovacion DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @SuscripcionId INT, @UsuarioId INT;
    SELECT @SuscripcionId = PG.SuscripcionId, @UsuarioId = S.UsuarioId
    FROM Pago PG JOIN Suscripcion S ON S.Id = PG.SuscripcionId
    WHERE PG.Id = @Id AND PG.Estado = 'Pendiente';
    IF @SuscripcionId IS NULL RETURN;
    BEGIN TRANSACTION;
    UPDATE Pago SET Estado = 'Aprobado', ComprobanteUrl = @ComprobanteUrl, Fecha = @FechaInicio WHERE Id = @Id;
    UPDATE Suscripcion SET Estado = 6 WHERE UsuarioId = @UsuarioId AND Estado IN (2, 3) AND Id <> @SuscripcionId;
    UPDATE Suscripcion SET Estado = 2, FechaInicio = @FechaInicio, FechaRenovacion = @FechaRenovacion WHERE Id = @SuscripcionId;
    COMMIT TRANSACTION;
END
GO

-- Resultado del débito automático de una renovación.
CREATE OR ALTER PROCEDURE SP_Pago_ActualizarEstado
    @Id INT,
    @Estado VARCHAR(50),
    @ComprobanteUrl NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Pago SET Estado = @Estado, ComprobanteUrl = @ComprobanteUrl WHERE Id = @Id;
END
GO

-- 7.1 Pago rechazado [ERROR-014]: la suscripción pendiente no se activa.
CREATE OR ALTER PROCEDURE SP_Pago_Rechazar
    @Id INT,
    @ComprobanteUrl NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
    UPDATE Pago SET Estado = 'Rechazado', ComprobanteUrl = @ComprobanteUrl WHERE Id = @Id AND Estado = 'Pendiente';
    UPDATE S SET Estado = 5
    FROM Suscripcion S JOIN Pago PG ON PG.SuscripcionId = S.Id
    WHERE PG.Id = @Id AND S.Estado = 1;
    COMMIT TRANSACTION;
END
GO

-- ----------------------------------------
-- WS_Usuarios.ObtenerPlanMasContratado: el plan con más suscripciones
-- contratadas (pagadas) en el período. Empate: el de mayor Id.
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Estadistica_ObtenerPlanMasContratado
    @Desde DATETIME,
    @Hasta DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 P.Id, P.Nombre, P.Activo, P.Tipo, P.PrecioMensual, P.Meses, P.MaxUsuarios
    FROM Suscripcion S
    JOIN [Plan] P ON P.Id = S.PlanId
    WHERE S.Estado IN (2, 3, 4, 6) AND S.FechaInicio >= @Desde AND S.FechaInicio < @Hasta
    GROUP BY P.Id, P.Nombre, P.Activo, P.Tipo, P.PrecioMensual, P.Meses, P.MaxUsuarios
    ORDER BY COUNT(*) DESC, P.Id DESC;
END
GO

-- ----------------------------------------
-- Permisos (Composite): Familia Suscripcion con Suscribirse,
-- GestionarSuscripcion y VerFacturacion, para los titulares de cuenta
-- (Freelance y Empresa). Los empleados usan el plan de su Empresa.
-- ----------------------------------------
DECLARE @Permisos TABLE (Nombre NVARCHAR(50), Descripcion NVARCHAR(MAX));
INSERT INTO @Permisos VALUES
    ('Suscribirse', 'Permite ver los planes y contratar una suscripción (CU-001-017)'),
    ('GestionarSuscripcion', 'Permite cambiar de plan o cancelar la suscripción (CU-001-008)'),
    ('VerFacturacion', 'Permite ver el historial de pagos y descargar comprobantes (CU-001-014)');

DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'Suscripcion');
IF @FamiliaId IS NULL
BEGIN
    SELECT @FamiliaId = ISNULL(MAX(Id), 0) + 1 FROM Familia;
    INSERT INTO Familia (Id, Nombre) VALUES (@FamiliaId, 'Suscripcion');
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

INSERT INTO PerfilFamilia (PerfilId, FamiliaId)
SELECT P.Id, @FamiliaId FROM Perfil P
WHERE P.NombrePerfil IN ('Freelance', 'Empresa')
  AND NOT EXISTS (SELECT 1 FROM PerfilFamilia PF WHERE PF.PerfilId = P.Id AND PF.FamiliaId = @FamiliaId);
GO
