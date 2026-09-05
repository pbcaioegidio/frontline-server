# Publica o FLMonitor headless para Linux. Nao instala na VPS.
# Uso:  .\linux\publicar.ps1
#       .\linux\publicar.ps1 linux-x64
param(
    [string]$Rid = "linux-arm64"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Out = Join-Path $PSScriptRoot "out\$Rid"
$Data = Join-Path $Root "binary\Debug"

Push-Location $Root
try {
    dotnet publish ".\Server.Host\Server.Host.csproj" -c Release -r $Rid --self-contained true -o $Out
    foreach ($dir in @("Config", "Data")) {
        $src = Join-Path $Data $dir
        if (Test-Path $src) {
            Copy-Item $src (Join-Path $Out $dir) -Recurse -Force
        }
    }
    Write-Host "Pronto: $Out"
    Write-Host "Binario: $Out\FLMonitor  (Ubuntu 24.04 Ampere = linux-arm64)"
}
finally {
    Pop-Location
}
