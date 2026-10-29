# Completa datos históricos a partir de la Bitácora (tabla Bitacora + archivo
# App_Data/BitacoraArchivo.xml), para las estadísticas:
#   - Usuario.FechaAlta de los usuarios que no la tienen: la fecha de su
#     primer evento registrado (aproximación: es la mejor fecha disponible).
#   - VerificacionDocumento: las verificaciones de integridad hechas antes de
#     que existiera la tabla (eventos "Verificar Integridad").
# Genera Database/CompletarHistoricos.generado.sql para revisarlo y
# ejecutarlo. No se versiona (contiene datos de la Bitácora).
#   powershell -ExecutionPolicy Bypass -File Database\CompletarHistoricos.ps1
$Carpeta = $PSScriptRoot
$Xml = Join-Path $Carpeta "..\Doverie_App\App_Data\BitacoraArchivo.xml"
$Salida = Join-Path $Carpeta "CompletarHistoricos.generado.sql"

function Texto([string]$valor) {
    if ($null -eq $valor -or $valor -eq "") { return "NULL" }
    return "N'" + $valor.Replace("'", "''") + "'"
}

$Sql = New-Object System.Text.StringBuilder
[void]$Sql.AppendLine("-- GENERADO por Database/CompletarHistoricos.ps1 el " + (Get-Date -Format "yyyy-MM-dd HH:mm") + ". No editar a mano.")
[void]$Sql.AppendLine("-- Se puede ejecutar más de una vez: solo completa lo que falta.")
[void]$Sql.AppendLine("SET NOCOUNT ON;")
[void]$Sql.AppendLine("CREATE TABLE #Archivo (Accion NVARCHAR(100), Fecha DATETIME, Detalle NVARCHAR(MAX), UsuarioId INT NULL);")
$Cantidad = 0
if (Test-Path $Xml) {
    [xml]$Documento = [IO.File]::ReadAllText((Resolve-Path $Xml), [Text.Encoding]::UTF8)
    foreach ($E in $Documento.Bitacora.Evento) {
        $Usuario = if ($E.UsuarioId -and $E.UsuarioId -ne "") { [string]$E.UsuarioId } else { "NULL" }
        [void]$Sql.AppendLine("INSERT INTO #Archivo VALUES (" + (Texto $E.Accion) + ", '" + $E.Fecha + "', " + (Texto $E.Detalle) + ", " + $Usuario + ");")
        $Cantidad++
    }
}
[void]$Sql.AppendLine(@"

-- Todos los eventos: los de la base + los archivados en XML.
SELECT Accion, Fecha, Detalle, UsuarioId INTO #Eventos FROM Bitacora
UNION ALL SELECT Accion, Fecha, Detalle, UsuarioId FROM #Archivo;

-- 1. Fecha de alta = primer evento del usuario (solo donde falta).
UPDATE U SET FechaAlta = E.Primera
FROM Usuario U
JOIN (SELECT UsuarioId, MIN(Fecha) AS Primera FROM #Eventos WHERE UsuarioId IS NOT NULL GROUP BY UsuarioId) E ON E.UsuarioId = U.Id
WHERE U.FechaAlta IS NULL;
PRINT 'Usuarios con fecha de alta completada: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- 2. Verificaciones históricas. El detalle es "Documento #N: nombre (íntegro|MODIFICADO)".
--    Se omiten las que ya están en la tabla (misma fecha, con 5 s de margen).
INSERT INTO VerificacionDocumento (DocumentoId, UsuarioId, Fecha, Integro)
SELECT V.DocumentoId, V.UsuarioId, V.Fecha, CASE WHEN V.Detalle LIKE '%(MODIFICADO)' THEN 0 ELSE 1 END
FROM (
    SELECT TRY_CAST(SUBSTRING(Detalle, 12, CHARINDEX(':', Detalle) - 12) AS INT) AS DocumentoId, UsuarioId, Fecha, Detalle
    FROM #Eventos
    WHERE Accion = 'Verificar Integridad' AND Detalle LIKE 'Documento #%:%'
) V
WHERE V.DocumentoId IS NOT NULL AND V.UsuarioId IS NOT NULL
  AND EXISTS (SELECT 1 FROM Documento D WHERE D.Id = V.DocumentoId)
  AND EXISTS (SELECT 1 FROM Usuario U WHERE U.Id = V.UsuarioId)
  AND NOT EXISTS (SELECT 1 FROM VerificacionDocumento X WHERE X.DocumentoId = V.DocumentoId AND ABS(DATEDIFF(SECOND, X.Fecha, V.Fecha)) <= 5);
PRINT 'Verificaciones históricas agregadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

DROP TABLE #Eventos;
DROP TABLE #Archivo;
"@)
[IO.File]::WriteAllText($Salida, $Sql.ToString(), (New-Object System.Text.UTF8Encoding($true)))
Write-Output "Generado: $Salida ($Cantidad eventos del archivo XML)"
