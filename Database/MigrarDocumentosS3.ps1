# Copia a S3 los archivos de documentos guardados en disco (App_Data/Documentos,
# o la carpeta de DocumentosRuta), con la misma clave que usa AlmacenamientoS3:
# <S3Prefijo><guid>. Como Documento.UrlDocumento guarda solo el GUID, la base
# no cambia. Lee la configuración de S3 de Doverie_App/AppSettings.config y usa
# las DLL del AWS SDK de Doverie_App/Bin (compilar la solución antes).
# Los archivos que ya están en el bucket se saltean; los locales no se borran.
#   powershell -ExecutionPolicy Bypass -File Database\MigrarDocumentosS3.ps1
$ErrorActionPreference = 'Stop'
$Sitio = Join-Path (Split-Path $PSScriptRoot -Parent) 'Doverie_App'
$Bin = Join-Path $Sitio 'Bin'

# Las DLL del SDK dependen de otras de Bin (System.Text.Json, etc.): se cargan
# de ahí, sin importar la versión exacta pedida.
[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($Origen, $Evento)
    $Ruta = Join-Path $Bin ((New-Object Reflection.AssemblyName($Evento.Name)).Name + '.dll')
    if (Test-Path $Ruta) { return [Reflection.Assembly]::LoadFrom($Ruta) }
    return $null
})
Add-Type -Path (Join-Path $Bin 'AWSSDK.Core.dll')
Add-Type -Path (Join-Path $Bin 'AWSSDK.S3.dll')

$Config = @{}
foreach ($Clave in ([xml](Get-Content (Join-Path $Sitio 'AppSettings.config') -Encoding UTF8)).appSettings.add) {
    $Config[$Clave.key] = $Clave.value
}
foreach ($Requerida in 'S3Bucket', 'S3Region', 'S3AccessKey', 'S3SecretKey') {
    if (-not $Config[$Requerida]) { throw "Falta configurar $Requerida en AppSettings.config." }
}
$Prefijo = if ($Config['S3Prefijo']) { $Config['S3Prefijo'] } else { 'documentos/' }
$Carpeta = if ($Config['DocumentosRuta']) { $Config['DocumentosRuta'] } else { Join-Path $Sitio 'App_Data\Documentos' }

$S3Config = New-Object Amazon.S3.AmazonS3Config
if ($Config['S3ServiceUrl']) {
    $S3Config.ServiceURL = $Config['S3ServiceUrl']
    $S3Config.AuthenticationRegion = $Config['S3Region']
    $S3Config.ForcePathStyle = $true
} else {
    $S3Config.RegionEndpoint = [Amazon.RegionEndpoint]::GetBySystemName($Config['S3Region'])
}
$Credenciales = New-Object Amazon.Runtime.BasicAWSCredentials($Config['S3AccessKey'], $Config['S3SecretKey'])
$Cliente = New-Object Amazon.S3.AmazonS3Client($Credenciales, $S3Config)

$Subidos = 0; $Existentes = 0
foreach ($Archivo in Get-ChildItem $Carpeta -File | Where-Object { $_.Name -match '^[0-9a-f]{32}$' }) {
    $Clave = $Prefijo + $Archivo.Name
    try {
        [void]$Cliente.GetObjectMetadataAsync($Config['S3Bucket'], $Clave).GetAwaiter().GetResult()
        $Existentes++
        continue
    } catch {
        # PowerShell envuelve la excepción del SDK: se busca la de S3 adentro.
        $Excepcion = $_.Exception
        while ($Excepcion -and -not ($Excepcion -is [Amazon.S3.AmazonS3Exception])) { $Excepcion = $Excepcion.InnerException }
        if (-not $Excepcion -or $Excepcion.StatusCode -ne [Net.HttpStatusCode]::NotFound) { throw }
    }
    $Pedido = New-Object Amazon.S3.Model.PutObjectRequest
    $Pedido.BucketName = $Config['S3Bucket']
    $Pedido.Key = $Clave
    $Pedido.FilePath = $Archivo.FullName
    $Pedido.ContentType = 'application/octet-stream'
    $Pedido.ServerSideEncryptionMethod = [Amazon.S3.ServerSideEncryptionMethod]::AES256
    [void]$Cliente.PutObjectAsync($Pedido).GetAwaiter().GetResult()
    Write-Host "Subido: $Clave ($($Archivo.Length) bytes)"
    $Subidos++
}
$Cliente.Dispose()
Write-Host "Listo. Subidos: $Subidos. Ya estaban en el bucket: $Existentes."
