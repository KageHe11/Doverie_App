-- ========================================
-- Documentos: CU-001-002 Subir / 003 Descargar / 004 Eliminar /
-- 006 Verificar Integridad / 012 Ver Listado.
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- ========================================

-- ----------------------------------------
-- Tabla Documento
-- Tamaños ajustados respecto del diccionario de datos:
--   Nombre 255 (nombres de archivo reales), UrlDocumento 500 (claves de S3),
--   Hash 64 (SHA-256 en hexadecimal), TransaccionBlockchain 100 (hash de
--   transacción de Ethereum: 66 caracteres).
-- ----------------------------------------
IF OBJECT_ID('Documento', 'U') IS NULL
BEGIN
    CREATE TABLE Documento (
        Id INT NOT NULL CONSTRAINT PK_Documento PRIMARY KEY,
        UsuarioId INT NOT NULL CONSTRAINT FK_Documento_Usuario REFERENCES Usuario(Id),
        Nombre NVARCHAR(255) NOT NULL,
        UrlDocumento NVARCHAR(500) NOT NULL,
        Hash NVARCHAR(64) NOT NULL,
        TransaccionBlockchain NVARCHAR(100) NOT NULL,
        NFTId NVARCHAR(100) NULL,
        Estado VARCHAR(50) NOT NULL CONSTRAINT DF_Documento_Estado DEFAULT 'Activo',
        FechaSubida DATETIME NOT NULL CONSTRAINT DF_Documento_FechaSubida DEFAULT GETDATE()
    );
    CREATE INDEX IX_Documento_UsuarioId ON Documento(UsuarioId);
END
GO

-- ----------------------------------------
-- Stored Procedures
-- "Cuenta" = la organización dueña del documento: la Empresa (si quien lo
-- subió es la Empresa o uno de sus empleados) o el propio Usuario
-- independiente. Un Usuario solo accede a documentos de su cuenta.
-- ----------------------------------------

-- Usado por DocumentoDAL.ProximoId()
CREATE OR ALTER PROCEDURE SP_Documento_ObtenerUltimoId
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(Id), 0) AS UltimoId FROM Documento;
END
GO

-- Usado por Registrar(DocumentoBE) -> CU-001-002
CREATE OR ALTER PROCEDURE SP_Documento_Registrar
    @Id INT,
    @UsuarioId INT,
    @Nombre NVARCHAR(255),
    @UrlDocumento NVARCHAR(500),
    @Hash NVARCHAR(64),
    @TransaccionBlockchain NVARCHAR(100),
    @FechaSubida DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Documento (Id, UsuarioId, Nombre, UrlDocumento, Hash, TransaccionBlockchain, NFTId, Estado, FechaSubida)
    VALUES (@Id, @UsuarioId, @Nombre, @UrlDocumento, @Hash, @TransaccionBlockchain, NULL, 'Activo', @FechaSubida);
END
GO

-- Usado por ListarPorCuenta(int) -> CU-001-012 (solo documentos activos)
CREATE OR ALTER PROCEDURE SP_Documento_ListarPorCuenta
    @CuentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT D.Id, D.UsuarioId, D.Nombre, D.UrlDocumento, D.Hash, D.TransaccionBlockchain,
           D.NFTId, D.Estado, D.FechaSubida, U.NombreUsuario,
           ISNULL(U.EmpresaId, U.Id) AS CuentaId
    FROM Documento D
    JOIN Usuario U ON U.Id = D.UsuarioId
    WHERE D.Estado = 'Activo'
      AND ISNULL(U.EmpresaId, U.Id) = @CuentaId
    ORDER BY D.FechaSubida DESC;
END
GO

-- Usado por ObtenerDatos(int): trae también los eliminados, para poder
-- informar [ERROR-016].
CREATE OR ALTER PROCEDURE SP_Documento_ObtenerDatos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT D.Id, D.UsuarioId, D.Nombre, D.UrlDocumento, D.Hash, D.TransaccionBlockchain,
           D.NFTId, D.Estado, D.FechaSubida, U.NombreUsuario,
           ISNULL(U.EmpresaId, U.Id) AS CuentaId
    FROM Documento D
    JOIN Usuario U ON U.Id = D.UsuarioId
    WHERE D.Id = @Id;
END
GO

-- Usado por ActualizarEstado(int, string) -> CU-001-004
-- El registro nunca se borra: el hash sigue en la blockchain.
CREATE OR ALTER PROCEDURE SP_Documento_ActualizarEstado
    @Id INT,
    @Estado VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Documento SET Estado = @Estado WHERE Id = @Id;
END
GO

-- ----------------------------------------
-- Permisos (Composite)
--   AccesoDocumentos (ya existe, con VerDocumentos): se le suman
--     DescargarDocumento y VerificarDocumento -> operaciones de lectura.
--   GestionDocumentos (nueva): SubirDocumento y EliminarDocumento.
-- Asignación: Freelance y Empresa reciben las dos. EmpleadoEmpresa queda
-- de solo lectura. EmpleadoAvanzado recibe las dos, para que la Empresa
-- elija qué empleados pueden subir (CU-001-011).
-- ----------------------------------------
DECLARE @Permisos TABLE (Nombre NVARCHAR(50), Descripcion NVARCHAR(MAX), Familia NVARCHAR(50));
INSERT INTO @Permisos VALUES
    ('VerDocumentos', 'Permite acceder a la sección de Documentos', 'AccesoDocumentos'),
    ('DescargarDocumento', 'Permite descargar documentos de la cuenta (CU-001-003)', 'AccesoDocumentos'),
    ('VerificarDocumento', 'Permite verificar la integridad de un documento (CU-001-006)', 'AccesoDocumentos'),
    ('SubirDocumento', 'Permite subir documentos a la cuenta (CU-001-002)', 'GestionDocumentos'),
    ('EliminarDocumento', 'Permite eliminar documentos de la cuenta (CU-001-004)', 'GestionDocumentos');

DECLARE @Nombre NVARCHAR(50), @Descripcion NVARCHAR(MAX), @Familia NVARCHAR(50), @Id INT, @FamiliaId INT;

-- Familias
DECLARE CursorFamilias CURSOR LOCAL FAST_FORWARD FOR SELECT DISTINCT Familia FROM @Permisos;
OPEN CursorFamilias;
FETCH NEXT FROM CursorFamilias INTO @Familia;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Familia WHERE Nombre = @Familia)
    BEGIN
        SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Familia;
        INSERT INTO Familia (Id, Nombre) VALUES (@Id, @Familia);
    END
    FETCH NEXT FROM CursorFamilias INTO @Familia;
END
CLOSE CursorFamilias;
DEALLOCATE CursorFamilias;

-- Permisos y su Familia
DECLARE CursorPermisos CURSOR LOCAL FAST_FORWARD FOR SELECT Nombre, Descripcion, Familia FROM @Permisos;
OPEN CursorPermisos;
FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion, @Familia;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Permiso WHERE Nombre = @Nombre)
    BEGIN
        SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Permiso;
        INSERT INTO Permiso (Id, Nombre, Descripcion) VALUES (@Id, @Nombre, @Descripcion);
    END
    SELECT @Id = Id FROM Permiso WHERE Nombre = @Nombre;
    SELECT @FamiliaId = Id FROM Familia WHERE Nombre = @Familia;
    IF NOT EXISTS (SELECT 1 FROM FamiliaComponente WHERE FamiliaId = @FamiliaId AND PermisoId = @Id)
        INSERT INTO FamiliaComponente (FamiliaId, PermisoId, SubFamiliaId) VALUES (@FamiliaId, @Id, NULL);
    FETCH NEXT FROM CursorPermisos INTO @Nombre, @Descripcion, @Familia;
END
CLOSE CursorPermisos;
DEALLOCATE CursorPermisos;

-- Familias por Perfil
DECLARE @Asignaciones TABLE (Perfil NVARCHAR(50), Familia NVARCHAR(50));
INSERT INTO @Asignaciones VALUES
    ('Freelance', 'AccesoDocumentos'), ('Freelance', 'GestionDocumentos'),
    ('Empresa', 'AccesoDocumentos'), ('Empresa', 'GestionDocumentos'),
    ('EmpleadoEmpresa', 'AccesoDocumentos'),
    ('EmpleadoAvanzado', 'AccesoDocumentos'), ('EmpleadoAvanzado', 'GestionDocumentos');

INSERT INTO PerfilFamilia (PerfilId, FamiliaId)
SELECT P.Id, F.Id
FROM @Asignaciones A
JOIN Perfil P ON P.NombrePerfil = A.Perfil
JOIN Familia F ON F.Nombre = A.Familia
WHERE NOT EXISTS (SELECT 1 FROM PerfilFamilia PF WHERE PF.PerfilId = P.Id AND PF.FamiliaId = F.Id);
GO
