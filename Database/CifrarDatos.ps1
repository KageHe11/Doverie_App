# Cifra los datos personales que ya estaban en la base antes de
# Database/Cifrado.sql: Usuario (Gmail, Dni, Telefono y su GmailIndice),
# DocumentoCompartido.EmailDestinatario, Dispositivo.Descripcion y
# NotificacionRecibida.Detalle.
# Usa la misma clase que la aplicación (DAL.CifradoDatos, desde Bin\DAL.dll)
# y la clave CifradoClave de Doverie_App\AppSettings.config.
# Se puede ejecutar más de una vez: lo que ya está cifrado no se toca.
# Después hay que recalcular los dígitos verificadores (Integridad).
#   powershell -ExecutionPolicy Bypass -File Database\CifrarDatos.ps1
param([string]$Servidor = '.', [string]$Base = 'DB_Doverie')
$ErrorActionPreference = 'Stop'
$Raiz = Split-Path $PSScriptRoot -Parent
Add-Type -Path (Join-Path $Raiz 'Doverie_App\Bin\DAL.dll')
[xml]$Config = [IO.File]::ReadAllText((Join-Path $Raiz 'Doverie_App\AppSettings.config'))
$Clave = ($Config.appSettings.add | Where-Object { $_.key -eq 'CifradoClave' }).value
if ([string]::IsNullOrWhiteSpace($Clave)) { throw 'Falta CifradoClave en AppSettings.config.' }

function Cifrar([object]$Valor) {
    if ($Valor -is [DBNull] -or $null -eq $Valor) { return [DBNull]::Value }
    if (([string]$Valor).StartsWith('v1:')) { return $Valor }
    return [DAL.CifradoDatos]::Cifrar([string]$Valor, $Clave)
}
function Parametro($Comando, [string]$Nombre, [object]$Valor) {
    [void]$Comando.Parameters.AddWithValue($Nombre, $Valor)
}

$Conexion = New-Object System.Data.SqlClient.SqlConnection "Data Source=$Servidor;Initial Catalog=$Base;Integrated Security=True"
$Conexion.Open()
$Transaccion = $Conexion.BeginTransaction()
try {
    $Tablas = @{}
    foreach ($Consulta in @(
        @{ Tabla = 'Usuario'; Sql = 'SELECT Id, Gmail, Dni, Telefono FROM Usuario' },
        @{ Tabla = 'DocumentoCompartido'; Sql = 'SELECT Id, EmailDestinatario FROM DocumentoCompartido' },
        @{ Tabla = 'Dispositivo'; Sql = 'SELECT Id, Descripcion FROM Dispositivo' },
        @{ Tabla = 'NotificacionRecibida'; Sql = 'SELECT Id, Detalle FROM NotificacionRecibida' })) {
        $Lector = New-Object System.Data.SqlClient.SqlCommand $Consulta.Sql, $Conexion, $Transaccion
        $Filas = New-Object System.Data.DataTable
        $Filas.Load($Lector.ExecuteReader())
        $Tablas[$Consulta.Tabla] = $Filas
    }
    foreach ($Fila in $Tablas['Usuario'].Rows) {
        $Comando = New-Object System.Data.SqlClient.SqlCommand 'UPDATE Usuario SET Gmail = @Gmail, GmailIndice = @Indice, Dni = @Dni, Telefono = @Telefono WHERE Id = @Id', $Conexion, $Transaccion
        $Gmail = if ($Fila.Gmail -is [DBNull]) { $null } else { [DAL.CifradoDatos]::Descifrar([string]$Fila.Gmail, $Clave) }
        Parametro $Comando '@Gmail' (Cifrar $Fila.Gmail)
        Parametro $Comando '@Indice' $(if ($null -eq $Gmail) { [DBNull]::Value } else { [DAL.CifradoDatos]::Indice($Gmail, $Clave) })
        Parametro $Comando '@Dni' (Cifrar $Fila.Dni)
        Parametro $Comando '@Telefono' (Cifrar $Fila.Telefono)
        Parametro $Comando '@Id' $Fila.Id
        [void]$Comando.ExecuteNonQuery()
    }
    foreach ($Fila in $Tablas['DocumentoCompartido'].Rows) {
        $Comando = New-Object System.Data.SqlClient.SqlCommand 'UPDATE DocumentoCompartido SET EmailDestinatario = @Email WHERE Id = @Id', $Conexion, $Transaccion
        Parametro $Comando '@Email' (Cifrar $Fila.EmailDestinatario)
        Parametro $Comando '@Id' $Fila.Id
        [void]$Comando.ExecuteNonQuery()
    }
    foreach ($Fila in $Tablas['Dispositivo'].Rows) {
        $Comando = New-Object System.Data.SqlClient.SqlCommand 'UPDATE Dispositivo SET Descripcion = @Descripcion WHERE Id = @Id', $Conexion, $Transaccion
        Parametro $Comando '@Descripcion' (Cifrar $Fila.Descripcion)
        Parametro $Comando '@Id' $Fila.Id
        [void]$Comando.ExecuteNonQuery()
    }
    foreach ($Fila in $Tablas['NotificacionRecibida'].Rows) {
        $Comando = New-Object System.Data.SqlClient.SqlCommand 'UPDATE NotificacionRecibida SET Detalle = @Detalle WHERE Id = @Id', $Conexion, $Transaccion
        Parametro $Comando '@Detalle' (Cifrar $Fila.Detalle)
        Parametro $Comando '@Id' $Fila.Id
        [void]$Comando.ExecuteNonQuery()
    }
    $Transaccion.Commit()
    Write-Host ("Cifrado: {0} usuarios, {1} documentos compartidos, {2} dispositivos, {3} notificaciones." -f $Tablas['Usuario'].Rows.Count, $Tablas['DocumentoCompartido'].Rows.Count, $Tablas['Dispositivo'].Rows.Count, $Tablas['NotificacionRecibida'].Rows.Count)
    Write-Host 'Ahora: Integridad -> Recalcular todos los dígitos.'
}
catch {
    $Transaccion.Rollback()
    throw
}
finally {
    $Conexion.Close()
}
