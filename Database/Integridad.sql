-- ========================================
-- Integridad de permisos, perfiles y usuarios.
-- Antes, borrar una Familia o un Permiso en uso dejaba filas huérfanas en
-- FamiliaComponente y el armado del árbol de permisos (que corre en cada
-- login) fallaba para todos los usuarios. Este script:
--   1. limpia huérfanos, si los hubiera;
--   2. agrega las claves foráneas y los nombres únicos que faltaban;
--   3. agrega los SPs que usa la BLL para validar antes de borrar o repetir
--      nombres (ERROR-022, 025, 027, 045/047, 048, 005);
--   4. hace que SP_Familia_Eliminar borre también los componentes propios.
-- El script se puede ejecutar más de una vez sin duplicar nada.
-- ========================================

-- El índice único filtrado de Usuario.Gmail lo exige (sqlcmd arranca con
-- QUOTED_IDENTIFIER OFF; SSMS ya lo trae en ON).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ----------------------------------------
-- 1. Huérfanos (hoy no hay; se limpian por si acaso, porque sin esto no se
--    pueden crear las claves foráneas)
-- ----------------------------------------
DELETE FC FROM FamiliaComponente FC
WHERE NOT EXISTS (SELECT 1 FROM Familia F WHERE F.Id = FC.FamiliaId)
   OR (FC.PermisoId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Permiso P WHERE P.Id = FC.PermisoId))
   OR (FC.SubFamiliaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Familia F WHERE F.Id = FC.SubFamiliaId));
DELETE PF FROM PerfilFamilia PF
WHERE NOT EXISTS (SELECT 1 FROM Familia F WHERE F.Id = PF.FamiliaId)
   OR NOT EXISTS (SELECT 1 FROM Perfil P WHERE P.Id = PF.PerfilId);
GO

-- ----------------------------------------
-- 2. Claves foráneas
-- ----------------------------------------
IF OBJECT_ID('FK_FamiliaComponente_Familia', 'F') IS NULL
    ALTER TABLE FamiliaComponente ADD CONSTRAINT FK_FamiliaComponente_Familia FOREIGN KEY (FamiliaId) REFERENCES Familia(Id);
IF OBJECT_ID('FK_FamiliaComponente_Permiso', 'F') IS NULL
    ALTER TABLE FamiliaComponente ADD CONSTRAINT FK_FamiliaComponente_Permiso FOREIGN KEY (PermisoId) REFERENCES Permiso(Id);
IF OBJECT_ID('FK_FamiliaComponente_SubFamilia', 'F') IS NULL
    ALTER TABLE FamiliaComponente ADD CONSTRAINT FK_FamiliaComponente_SubFamilia FOREIGN KEY (SubFamiliaId) REFERENCES Familia(Id);
IF OBJECT_ID('FK_PerfilFamilia_Perfil', 'F') IS NULL
    ALTER TABLE PerfilFamilia ADD CONSTRAINT FK_PerfilFamilia_Perfil FOREIGN KEY (PerfilId) REFERENCES Perfil(Id);
IF OBJECT_ID('FK_PerfilFamilia_Familia', 'F') IS NULL
    ALTER TABLE PerfilFamilia ADD CONSTRAINT FK_PerfilFamilia_Familia FOREIGN KEY (FamiliaId) REFERENCES Familia(Id);
IF OBJECT_ID('FK_Usuario_Perfil', 'F') IS NULL
    ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Perfil FOREIGN KEY (PerfilId) REFERENCES Perfil(Id);
IF OBJECT_ID('FK_Usuario_Empresa', 'F') IS NULL
    ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Empresa FOREIGN KEY (EmpresaId) REFERENCES Usuario(Id);
GO

-- Nombres únicos: la BLL ya valida, pero la base también lo garantiza.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Permiso_Nombre')
    CREATE UNIQUE INDEX UQ_Permiso_Nombre ON Permiso(Nombre);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Familia_Nombre')
    CREATE UNIQUE INDEX UQ_Familia_Nombre ON Familia(Nombre);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Perfil_NombrePerfil')
    CREATE UNIQUE INDEX UQ_Perfil_NombrePerfil ON Perfil(NombrePerfil);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Usuario_Gmail')
    CREATE UNIQUE INDEX UQ_Usuario_Gmail ON Usuario(Gmail) WHERE Gmail IS NOT NULL;
GO

-- ----------------------------------------
-- 3. SPs de validación
-- ----------------------------------------

-- CU-002-017 Baja Familia [ERROR-048]: Perfiles que la tienen asignada +
-- Familias que la contienen.
CREATE OR ALTER PROCEDURE SP_Familia_ContarUsos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT (SELECT COUNT(*) FROM PerfilFamilia WHERE FamiliaId = @Id)
         + (SELECT COUNT(*) FROM FamiliaComponente WHERE SubFamiliaId = @Id);
END
GO

-- CU-002-015/016 [ERROR-045/047]: nombre repetido (excluyendo la propia
-- Familia al modificar; @IdExcluir = 0 en el alta).
CREATE OR ALTER PROCEDURE SP_Familia_ExistePorNombre
    @Nombre NVARCHAR(50),
    @IdExcluir INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Familia WHERE Nombre = @Nombre AND Id <> @IdExcluir;
END
GO

-- CU-002-005 Baja Permiso [ERROR-023]: Familias que lo contienen.
CREATE OR ALTER PROCEDURE SP_Permiso_ContarUsos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM FamiliaComponente WHERE PermisoId = @Id;
END
GO

-- CU-002-004/006 [ERROR-022]
CREATE OR ALTER PROCEDURE SP_Permiso_ExistePorNombreExcluyendo
    @Nombre NVARCHAR(50),
    @IdExcluir INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Permiso WHERE Nombre = @Nombre AND Id <> @IdExcluir;
END
GO

-- CU-002-007/009 [ERROR-025]
CREATE OR ALTER PROCEDURE SP_Perfil_ExistePorNombre
    @Nombre NVARCHAR(50),
    @IdExcluir INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Perfil WHERE NombrePerfil = @Nombre AND Id <> @IdExcluir;
END
GO

-- CU-002-008 Baja Perfil [ERROR-027]
CREATE OR ALTER PROCEDURE SP_Perfil_ContarUsuariosActivos
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Usuario WHERE PerfilId = @Id AND Suspendido = 0;
END
GO

-- ----------------------------------------
-- 4. Baja de Familia: primero sus propios componentes, después la Familia.
-- La BLL ya verificó que nadie la use (ERROR-048).
-- ----------------------------------------
CREATE OR ALTER PROCEDURE SP_Familia_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
        DELETE FROM FamiliaComponente WHERE FamiliaId = @Id;
        DELETE FROM Familia WHERE Id = @Id;
    COMMIT TRANSACTION;
END
GO

-- ----------------------------------------
-- CU-001-016: "Eliminar Usuario Empresa" pasa a ser una revocación
-- permanente (SP_Usuario_RevocarAcceso). Las bajas hechas antes por la
-- Empresa usaban otro motivo; se unifican.
-- ----------------------------------------
UPDATE Usuario SET Motivo = 'Acceso revocado por la empresa'
WHERE Suspendido = 1 AND Motivo = 'Dado de baja por la Empresa';
GO
