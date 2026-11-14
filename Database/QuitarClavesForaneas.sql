-- ========================================
-- Quita todas las claves foráneas de la base.
-- Para la evaluación, la cátedra puede borrar cualquier tabla (salvo
-- Usuario, Familia y Perfil) y comprobar que el sistema detecta la tabla
-- eliminada y se recupera con un backup. Con claves foráneas, SQL Server
-- no deja borrar una tabla a la que otra hace referencia.
-- La integridad referencial queda a cargo de la aplicación: la BLL valida
-- antes de borrar (permiso o familia en uso, perfil con usuarios activos,
-- etc.). Para volver a crearlas: Database/ClavesForaneas.sql.
-- Ojo: algunos scripts anteriores (por ejemplo Integridad.sql) vuelven a
-- crear claves si se ejecutan de nuevo, y restaurar un backup generado
-- antes de este script también las trae de vuelta.
-- No cambia datos, así que no hace falta recalcular los dígitos
-- verificadores. El script se puede ejecutar más de una vez.
-- ========================================
SET NOCOUNT ON;
DECLARE @Sql NVARCHAR(MAX) = N'';
SELECT @Sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.'
             + QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';' + CHAR(10)
FROM sys.foreign_keys;
PRINT @Sql;
EXEC sp_executesql @Sql;
DECLARE @Quedan INT = (SELECT COUNT(*) FROM sys.foreign_keys);
PRINT CONCAT(N'Claves foráneas que quedan: ', @Quedan);
GO
