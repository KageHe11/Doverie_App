# Genera Database/Idiomas_Traducciones.sql a partir de Database/Idiomas/*.json.
# Correrlo cada vez que se modifique algún JSON de traducciones:
#   powershell -ExecutionPolicy Bypass -File Database\GenerarTraducciones.ps1
$Carpeta = $PSScriptRoot
$Idiomas = @(
    @{ Codigo = 'es'; Nombre = 'Español' },
    @{ Codigo = 'en'; Nombre = 'English' },
    @{ Codigo = 'pt'; Nombre = 'Português' }
)
$Sql = New-Object System.Text.StringBuilder
[void]$Sql.AppendLine("-- ========================================")
[void]$Sql.AppendLine("-- Carga / actualización de las traducciones de Español, English y Português.")
[void]$Sql.AppendLine("-- GENERADO por Database/GenerarTraducciones.ps1 a partir de Database/Idiomas/*.json.")
[void]$Sql.AppendLine("-- No editar a mano. Requiere haber ejecutado antes Database/Idiomas.sql.")
[void]$Sql.AppendLine("-- Se puede ejecutar más de una vez.")
[void]$Sql.AppendLine("-- ========================================")
[void]$Sql.AppendLine("SET NOCOUNT ON;")
[void]$Sql.AppendLine("DECLARE @Id INT;")
foreach ($Idioma in $Idiomas) {
    $Json = [IO.File]::ReadAllText((Join-Path $Carpeta "Idiomas\$($Idioma.Codigo).json"), [Text.Encoding]::UTF8).Trim().Replace("'", "''")
    [void]$Sql.AppendLine("")
    [void]$Sql.AppendLine("-- $($Idioma.Nombre) ($($Idioma.Codigo))")
    [void]$Sql.AppendLine("SET @Id = (SELECT Id FROM Idioma WHERE Codigo = '$($Idioma.Codigo)');")
    [void]$Sql.AppendLine("IF @Id IS NULL")
    [void]$Sql.AppendLine("BEGIN")
    [void]$Sql.AppendLine("    SELECT @Id = ISNULL(MAX(Id), 0) + 1 FROM Idioma;")
    [void]$Sql.AppendLine("    EXEC SP_Idioma_Registrar @Id = @Id, @Codigo = '$($Idioma.Codigo)', @Nombre = '$($Idioma.Nombre)', @ArchivoJson = N'$Json';")
    [void]$Sql.AppendLine("END")
    [void]$Sql.AppendLine("ELSE")
    [void]$Sql.AppendLine("    EXEC SP_Idioma_ActualizarDatos @Id = @Id, @Nombre = '$($Idioma.Nombre)', @ArchivoJson = N'$Json';")
}
[void]$Sql.AppendLine("GO")
[IO.File]::WriteAllText((Join-Path $Carpeta "Idiomas_Traducciones.sql"), $Sql.ToString(), (New-Object System.Text.UTF8Encoding($true)))
Write-Output "Generado: $(Join-Path $Carpeta 'Idiomas_Traducciones.sql')"
