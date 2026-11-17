#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiSrc = Join-Path $root "publish\api"
$webSrc = Join-Path $root "mobile\build\web"

$apiDest = "C:\inetpub\wwwroot\catproductos-api"
$webDest = "C:\inetpub\wwwroot\catproductos-web"

if (-not (Test-Path "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) {
    Write-Host "FALTA EL ASP.NET CORE HOSTING BUNDLE (.NET 10)." -ForegroundColor Yellow
    Write-Host "Descárgalo e instálalo desde: https://dotnet.microsoft.com/download/dotnet/10.0 (Hosting Bundle)" -ForegroundColor Yellow
    Write-Host "Después vuelve a ejecutar este script." -ForegroundColor Yellow
    exit 1
}

Import-Module WebAdministration

# Detener sitios existentes antes de copiar para evitar DLLs bloqueados
if (Test-Path "IIS:\Sites\CatProductosApi") {
    Stop-Website -Name "CatProductosApi" -ErrorAction SilentlyContinue
}
if (Test-Path "IIS:\Sites\CatProductosWeb") {
    Stop-Website -Name "CatProductosWeb" -ErrorAction SilentlyContinue
}

# 1. Copiar archivos publicados
New-Item -ItemType Directory -Force -Path $apiDest | Out-Null
New-Item -ItemType Directory -Force -Path $webDest | Out-Null
Copy-Item -Path "$apiSrc\*" -Destination $apiDest -Recurse -Force
Copy-Item -Path "$webSrc\*" -Destination $webDest -Recurse -Force

# Permisos de escritura para SQLite (productos.db)
icacls $apiDest /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null

# 2. Configurar Application Pool (No Managed Code para ASP.NET Core)
if (-not (Test-Path "IIS:\AppPools\CatProductosApiPool")) {
    New-WebAppPool -Name "CatProductosApiPool" | Out-Null
}
Set-ItemProperty "IIS:\AppPools\CatProductosApiPool" -Name "managedRuntimeVersion" -Value ""

# 3. Sitio IIS para la API en puerto 5000
if (Test-Path "IIS:\Sites\CatProductosApi") {
    Remove-Website -Name "CatProductosApi"
}
New-Website -Name "CatProductosApi" -Port 5000 -PhysicalPath $apiDest -ApplicationPool "CatProductosApiPool" | Out-Null

# 4. Sitio IIS para Flutter Web en puerto 8080
if (Test-Path "IIS:\Sites\CatProductosWeb") {
    Remove-Website -Name "CatProductosWeb"
}
New-Website -Name "CatProductosWeb" -Port 8080 -PhysicalPath $webDest | Out-Null

Start-Website -Name "CatProductosApi"
Start-Website -Name "CatProductosWeb"

Write-Host "Despliegue completado:" -ForegroundColor Green
Write-Host "  API: http://localhost:5000/api/productos"
Write-Host "  Web: http://localhost:8080"
