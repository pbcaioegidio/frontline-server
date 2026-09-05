# Empacota setup do jogador (launcher + FL Guard + client).
# Uso:
#   .\scripts\pack-player-setup.ps1 -Mode Slim    # sem Pack (~3 GB) — Pack baixa pelo Socket
#   .\scripts\pack-player-setup.ps1 -Mode Full    # client completo (~16 GB)
#   .\scripts\pack-player-setup.ps1 -Mode Slim -Upload
#
# Upload: scp para VPS /var/frontline/downloads/

param(
    [ValidateSet("Slim", "Full")]
    [string] $Mode = "Slim",
    [switch] $Upload,
    [string] $ClientRoot = "",
    [string] $OutDir = "",
    # Nunca versionar IP. Use: $env:FL_VPS_SSH = "ubuntu@SEU_HOST"
    [string] $VpsHost = "",
    [string] $VpsDir = "/var/frontline/downloads",
    [string] $PublicDownloadBase = ""  # ex.: https://download.seudominio.com — opcional
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path (Join-Path $root "client"))) {
    $root = "c:\Users\pbcai\Downloads\source"
}
if (-not $ClientRoot) { $ClientRoot = Join-Path $root "client" }
if (-not $OutDir) { $OutDir = Join-Path $root "dist" }

if (-not (Test-Path $ClientRoot)) { throw "Client nao encontrado: $ClientRoot" }

$stamp = Get-Date -Format "yyyyMMdd"
$name = "FrontLine-Setup-$Mode-$stamp"
$stage = Join-Path $env:TEMP $name
$zip = Join-Path $OutDir "$name.zip"

Write-Host "==> Modo $Mode"
Write-Host "    Client: $ClientRoot"
Write-Host "    Stage:  $stage"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage, $OutDir | Out-Null

$excludeDirs = @("CEF\Cache", "_fl_backup", "_fl_publish_tmp", "tools", ".git")
if ($Mode -eq "Slim") {
    $excludeDirs += "Pack"
}

# robocopy espelha o client
$xd = ($excludeDirs | ForEach-Object { "/XD"; $_ })
$args = @($ClientRoot, $stage, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/nc", "/ns", "/np") + $xd
& robocopy @args | Out-Null
# robocopy exit 0-7 = ok
if ($LASTEXITCODE -ge 8) { throw "robocopy falhou: $LASTEXITCODE" }

# Garante config apontando para VPS (se existir SetIp tool skip — assume ja atualizado)
$cfg = Join-Path $stage "config.zpt"
if (-not (Test-Path $cfg)) { Write-Warning "config.zpt ausente no stage" }
if (-not (Test-Path (Join-Path $stage "FLLauncher.exe"))) { throw "FLLauncher.exe ausente" }
if (-not (Test-Path (Join-Path $stage "FrontLine.exe"))) { throw "FrontLine.exe ausente" }
if (-not (Test-Path (Join-Path $stage "UserFileList.dat"))) { Write-Warning "UserFileList.dat ausente — rode FileListBuilder" }

# README do jogador
@"
FrontLine — instalacao

1. Extraia esta pasta inteira (nao rode so o zip).
2. Abra FLLauncher.exe
3. Login e Start.

Nao apague FLLauncher enquanto joga (FL Guard / heartbeat).
Atualizacoes futuras: o launcher baixa so o que mudou (Socket).

Modo deste pacote: $Mode
"@ | Set-Content (Join-Path $stage "LEIA-ME.txt") -Encoding UTF8

Write-Host "==> Compactando zip (pode demorar)..."
if (Test-Path $zip) { Remove-Item $zip -Force }
# Compress-Archive e lento/limitado; preferir tar.gz se disponivel
$tarGz = Join-Path $OutDir "$name.tar.gz"
Push-Location (Split-Path $stage -Parent)
tar -czf $tarGz (Split-Path $stage -Leaf)
Pop-Location
if (-not (Test-Path $tarGz)) { throw "falha ao criar $tarGz" }

$sizeGb = [math]::Round((Get-Item $tarGz).Length / 1GB, 2)
Write-Host "==> OK: $tarGz ($sizeGb GB)"

# zip opcional menor so para Slim pequeno — tar.gz e o artefato principal
$artifact = $tarGz

if ($Upload) {
    if (-not $VpsHost) { $VpsHost = $env:FL_VPS_SSH }
    if (-not $VpsHost) { throw "Defina -VpsHost ou env FL_VPS_SSH (ex.: ubuntu@host). Nao use IP no codigo." }
    if (-not $PublicDownloadBase) { $PublicDownloadBase = $env:FL_DOWNLOAD_BASE }

    Write-Host "==> Upload para $VpsHost:$VpsDir ..."
    ssh -o BatchMode=yes $VpsHost "sudo mkdir -p $VpsDir && sudo chown ubuntu:ubuntu $VpsDir"
    scp -o BatchMode=yes $artifact "${VpsHost}:$VpsDir/"
    $base = Split-Path $artifact -Leaf
    ssh -o BatchMode=yes $VpsHost "cd $VpsDir && ln -sfn $base FrontLine-Setup-latest.tar.gz && ls -lh"
    if ($PublicDownloadBase) {
        $baseUrl = $PublicDownloadBase.TrimEnd('/')
        Write-Host "==> Download: $baseUrl/downloads/$base"
        Write-Host "             $baseUrl/downloads/FrontLine-Setup-latest.tar.gz"
    } else {
        Write-Host "==> Upload OK. Publique o link via dominio/CDN (nao no Git)."
    }
}

Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Concluido."
