-- ========================================
-- Idiomas: CU-002-018 Alta / CU-002-019 Modificar / CU-002-020 Baja
-- y RC-004 Traducir (patrón Strategy).
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- ========================================

-- ----------------------------------------
-- Tabla Idioma
-- ----------------------------------------
IF OBJECT_ID('Idioma', 'U') IS NULL
BEGIN
    CREATE TABLE Idioma (
        Id INT NOT NULL CONSTRAINT PK_Idioma PRIMARY KEY,
        Codigo VARCHAR(10) NOT NULL CONSTRAINT UQ_Idioma_Codigo UNIQUE,
        Nombre VARCHAR(50) NOT NULL,
        ArchivoJson NVARCHAR(MAX) NOT NULL,
        EsPredeterminado BIT NOT NULL CONSTRAINT DF_Idioma_EsPredeterminado DEFAULT 0
    );
END
GO

-- Usuario.IdiomaId -> Idioma.Id (idioma preferido; NULL = predeterminado)
IF OBJECT_ID('FK_Usuario_Idioma', 'F') IS NULL
BEGIN
    UPDATE Usuario SET IdiomaId = NULL WHERE IdiomaId IS NOT NULL AND IdiomaId NOT IN (SELECT Id FROM Idioma);
    ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Idioma FOREIGN KEY (IdiomaId) REFERENCES Idioma(Id);
END
GO

-- ----------------------------------------
-- Stored Procedures
-- ----------------------------------------

-- Usado por IdiomaDAL.ProximoId()
CREATE OR ALTER PROCEDURE SP_Idioma_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM Idioma;
END
GO

-- Usado por Listar(): no trae el JSON, que puede ser grande y no hace
-- falta para la grilla ni para el selector de idioma.
CREATE OR ALTER PROCEDURE SP_Idioma_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, EsPredeterminado
    FROM Idioma
    ORDER BY Nombre;
END
GO

-- Usado por ObtenerDatos(int Pid)
CREATE OR ALTER PROCEDURE SP_Idioma_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Codigo, Nombre, ArchivoJson, EsPredeterminado
    FROM Idioma
    WHERE Id = @Id;
END
GO

-- Usado por ObtenerPredeterminado() -> RC-004 Traducir
CREATE OR ALTER PROCEDURE SP_Idioma_ObtenerPredeterminado
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 Id, Codigo, Nombre, ArchivoJson, EsPredeterminado
    FROM Idioma
    WHERE EsPredeterminado = 1;
END
GO

-- Usado por ExistePorCodigo(string) -> CU-002-018 [ERROR-051]
CREATE OR ALTER PROCEDURE SP_Idioma_ExistePorCodigo
    @Codigo VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Idioma WHERE Codigo = @Codigo;
END
GO

-- Usado por Registrar(IdiomaBE) -> CU-002-018
CREATE OR ALTER PROCEDURE SP_Idioma_Registrar
    @Id INT,
    @Codigo VARCHAR(10),
    @Nombre VARCHAR(50),
    @ArchivoJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Idioma (Id, Codigo, Nombre, ArchivoJson, EsPredeterminado)
    VALUES (@Id, @Codigo, @Nombre, @ArchivoJson, 0);
END
GO

-- Usado por ActualizarDatos(IdiomaBE) -> CU-002-019
-- El Código no se modifica (identifica al idioma).
CREATE OR ALTER PROCEDURE SP_Idioma_ActualizarDatos
    @Id INT,
    @Nombre VARCHAR(50),
    @ArchivoJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Idioma
    SET Nombre = @Nombre,
        ArchivoJson = @ArchivoJson
    WHERE Id = @Id;
END
GO

-- Usado por EstablecerPredeterminado(int) -> CU-002-019
-- Siempre queda exactamente un idioma predeterminado.
CREATE OR ALTER PROCEDURE SP_Idioma_EstablecerPredeterminado
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Idioma WHERE Id = @Id) RETURN;
    BEGIN TRANSACTION;
        UPDATE Idioma SET EsPredeterminado = 0 WHERE EsPredeterminado = 1 AND Id <> @Id;
        UPDATE Idioma SET EsPredeterminado = 1 WHERE Id = @Id;
    COMMIT TRANSACTION;
END
GO

-- Usado por Eliminar(int) -> CU-002-020
-- Los usuarios que tenían este idioma pasan a usar el predeterminado.
CREATE OR ALTER PROCEDURE SP_Idioma_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
        UPDATE Usuario SET IdiomaId = NULL WHERE IdiomaId = @Id;
        DELETE FROM Idioma WHERE Id = @Id AND EsPredeterminado = 0;
    COMMIT TRANSACTION;
END
GO

-- Usado por UsuarioDAL.ActualizarIdioma(int, int?) -> SelectorIdioma
CREATE OR ALTER PROCEDURE SP_Usuario_ActualizarIdioma
    @Id INT,
    @IdiomaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuario SET IdiomaId = @IdiomaId WHERE Id = @Id;
END
GO

-- ----------------------------------------
-- Permisos (Composite): Familia "GestionIdiomas" con un permiso por
-- operación, asignada al Perfil Administrador.
-- ----------------------------------------
DECLARE @Permisos TABLE (Nombre NVARCHAR(50), Descripcion NVARCHAR(MAX));
INSERT INTO @Permisos VALUES
    ('VerIdiomas', 'Permite acceder a la pantalla Gestionar Idiomas y ver el listado'),
    ('AltaIdioma', 'Permite dar de alta un idioma nuevo (CU-002-018)'),
    ('ModificarIdioma', 'Permite modificar un idioma existente (CU-002-019)'),
    ('BajaIdioma', 'Permite eliminar un idioma (CU-002-020)');

DECLARE @Nombre NVARCHAR(50), @Descripcion NVARCHAR(MAX), @Id INT;
DECLARE CursorPermisos CURSOR LOCAL FAST_FORWARD FOR SELECT Nombre, Descripcion FROM @Permisos;
OPEN CursorPermisos;
FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Permiso WHERE Nombre = @Nombre)
    BEGIN
        SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
        INSERT INTO Permiso (Id, Nombre, Descripcion) VALUES (@Id, @Nombre, @Descripcion);
    END
    FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion;
END
CLOSE CursorPermisos;
DEALLOCATE CursorPermisos;

DECLARE @FamiliaId INT = (SELECT Id FROM Familia WHERE Nombre = 'GestionIdiomas');
IF @FamiliaId IS NULL
BEGIN
    SELECT @FamiliaId = ISNULL(MAX(Id), 0) + 1 FROM Familia;
    INSERT INTO Familia (Id, Nombre) VALUES (@FamiliaId, 'GestionIdiomas');
END

INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId)
SELECT @FamiliaId, P.Id, NULL
FROM Permiso P
JOIN @Permisos X ON X.Nombre = P.Nombre
WHERE NOT EXISTS (SELECT 1 FROM FamiliaComponente FC WHERE FC.FamiliaId = @FamiliaId AND FC.PermisoId = P.Id);

DECLARE @PerfilAdminId INT = (SELECT Id FROM Perfil WHERE NombrePerfil = 'Administrador');
IF @PerfilAdminId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerfilFamilia WHERE PerfilId = @PerfilAdminId AND FamiliaId = @FamiliaId)
    INSERT INTO PerfilFamilia (PerfilId, FamiliaId) VALUES (@PerfilAdminId, @FamiliaId);
GO

-- ----------------------------------------
-- Idioma predeterminado inicial: Español (contenido de Idiomas/es.json).
-- Precondición de RC-004: debe existir al menos un idioma cargado.
-- ----------------------------------------
IF NOT EXISTS (SELECT 1 FROM Idioma)
BEGIN
    INSERT INTO Idioma (Id, Codigo, Nombre, ArchivoJson, EsPredeterminado)
    VALUES (1, 'es', 'Español', N'{
  "nav.usuarios": "Usuarios",
  "nav.credenciales": "Credenciales",
  "nav.perfiles": "Perfiles",
  "nav.familias": "Familias",
  "nav.permisos": "Permisos",
  "nav.idiomas": "Idiomas",
  "nav.bitacora": "Bitácora",
  "nav.documentos": "Documentos",
  "nav.proximamente": "Próximamente",
  "nav.iniciarSesion": "Iniciar sesión",
  "nav.miPerfil": "Mi Perfil",
  "nav.cerrarSesion": "Cerrar sesión",
  "nav.idioma": "Idioma",
  "saludo.empresa": "Bienvenido Empresa {0}",
  "saludo.empleado": "Bienvenido Empleado Empresa {0}",
  "saludo.freelance": "Bienvenido Freelance {0}",
  "saludo.general": "Hola, {0}",
  "login.titulo": "Iniciar sesión",
  "login.usuario": "Usuario",
  "login.contrasena": "Contraseña",
  "login.ingresar": "Ingresar",
  "login.olvideContrasena": "¿Olvidaste tu contraseña?",
  "login.registrarse": "¿No tenés cuenta? Registrate",
  "login.cuentaBloqueada": "Tu cuenta se encuentra bloqueada. Recuperá tu contraseña para desbloquearla.",
  "login.cuentaSuspendida": "Tu cuenta se encuentra suspendida. Contactá al administrador.",
  "login.credencialesInvalidas": "Usuario o contraseña incorrectos.",
  "acceso.titulo": "Acceso denegado",
  "acceso.mensaje": "No tenés los permisos necesarios para ver esta página.",
  "acceso.volver": "Volver al inicio",
  "comun.acciones": "Acciones",
  "comun.editar": "Editar",
  "comun.eliminar": "Eliminar",
  "comun.cancelar": "Cancelar",
  "comun.guardar": "Guardar",
  "idiomas.titulo": "Gestionar Idiomas",
  "idiomas.nuevo": "+ Nuevo Idioma",
  "idiomas.tituloNuevo": "Nuevo Idioma",
  "idiomas.tituloEditar": "Modificar Idioma",
  "idiomas.codigo": "Código",
  "idiomas.codigoAyuda": "Código ISO del idioma, por ejemplo: es, en, pt-BR.",
  "idiomas.nombre": "Nombre",
  "idiomas.predeterminado": "Predeterminado",
  "idiomas.marcarPredeterminado": "Usar como idioma predeterminado del sistema",
  "idiomas.archivo": "Archivo JSON de traducciones",
  "idiomas.archivoAyuda": "Debe contener todas las claves del idioma predeterminado.",
  "idiomas.archivoAyudaEditar": "Dejalo vacío para conservar las traducciones actuales.",
  "idiomas.jsonActual": "Traducciones actuales",
  "idiomas.confirmarEliminar": "¿Eliminar este idioma? Los usuarios que lo tengan elegido pasarán al idioma predeterminado.",
  "idiomas.altaExitosa": "El idioma se creó correctamente.",
  "idiomas.modificacionExitosa": "El idioma se modificó correctamente.",
  "idiomas.bajaExitosa": "El idioma se eliminó correctamente.",
  "idiomas.clavesFaltantes": "Claves faltantes: {0}",
  "error.004": "Los datos ingresados son incorrectos o incompletos. Por favor verifique la información.",
  "error.050": "El archivo JSON de traducciones tiene formato inválido o le faltan claves.",
  "error.051": "El idioma ya existe.",
  "error.052": "No se puede crear el idioma.",
  "error.053": "No se puede actualizar el idioma.",
  "error.054": "El idioma es el predeterminado o el único disponible, no puede eliminarse.",
  "error.055": "No se puede eliminar el idioma.",
  "error.056": "No tiene permisos para realizar esta operación."
}', 1);
END
GO
