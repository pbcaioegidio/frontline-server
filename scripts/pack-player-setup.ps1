# Empacota setup do jogador (launcher + FL Guard + client).
# Uso:
#   .\scripts\pack-player-setup.ps1 -Mode Slim
#   .\scripts\pack-player-setup.ps1 -Mode Full
#   .\scripts\pack-player-setup.ps1 -Mode Slim -Upload
#
# Upload: defina $env:FL_VPS_SSH (nunca IP no codigo).

param(
    [ValidateSet("Slim", "Full")]
    [string] $Mode = "Slim",
    [switch] $Upload,
    [string] $ClientRoot = "",
    [string] $OutDir = "",
    [string] $VpsHost = "",
    [string] $VpsDir = "/var/frontline/downloads",
    [string] $PublicDownloadBase = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path (Join-Path $root "client"))) {
    $root = "c:\Users\pbcai\Downloads\source"
}
if (-not $ClientRoot) { $ClientRoot = Join-Path $root "client" }
if (-not $OutDir) { $OutDir = Join-Path $root "dist" }

if (-not (Test-Path $ClientRoot)) {
    throw "Client nao encontrado: $ClientRoot"
}

$stamp = Get-Date -Format "yyyyMMdd"
$name = "FrontLine-Setup-$Mode-$stamp"
$stage = Join-Path $env:TEMP $name

Write-Host "==> Modo $Mode"
Write-Host "    Client: $ClientRoot"
Write-Host "    Stage:  $stage"
Write-Host "    Out:    $OutDir"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage, $OutDir | Out-Null

$excludeDirs = @("CEF\Cache", "_fl_backup", "_fl_publish_tmp", "tools", ".git")
if ($Mode -eq "Slim") {
    $excludeDirs += "Pack"
}

$xd = @()
foreach ($d in $excludeDirs) {
    $xd += "/XD"
    $xd += $d
}
$rcArgs = @($ClientRoot, $stage, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/nc", "/ns", "/np") + $xd
& robocopy @rcArgs | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw "robocopy falhou: $LASTEXITCODE"
}

if (-not (Test-Path (Join-Path $stage "config.zpt"))) {
    Write-Warning "config.zpt ausente no stage"
}
if (-not (Test-Path (Join-Path $stage "FLLauncher.exe"))) {
    throw "FLLauncher.exe ausente"
}
if (-not (Test-Path (Join-Path $stage "FrontLine.exe"))) {
    throw "FrontLine.exe ausente"
}
if (-not (Test-Path (Join-Path $stage "UserFileList.dat"))) {
    Write-Warning "UserFileList.dat ausente - rode FileListBuilder"
}

$readme = @"
FrontLine - instalacao

1. Extraia esta pasta inteira.
2. Abra FLLauncher.exe
3. Login e Start.

Nao feche o FLLauncher enquanto joga (FL Guard / heartbeat).
Atualizacoes: o launcher baixa so o que mudou (Socket).

Modo deste pacote: $Mode
"@
Set-Content -Path (Join-Path $stage "LEIA-ME.txt") -Value $readme -Encoding UTF8

Write-Host "==> Compactando tar.gz (pode demorar muito no Full)..."
$tarGz = Join-Path $OutDir "$name.tar.gz"
if (Test-Path $tarGz) { Remove-Item $tarGz -Force }

Push-Location (Split-Path $stage -Parent)
try {
    tar -czf $tarGz (Split-Path $stage -Leaf)
}
finally {
    Pop-Location
}

if (-not (Test-Path $tarGz)) {
    throw "falha ao criar $tarGz"
}

$sizeGb = [math]::Round((Get-Item $tarGz).Length / 1GB, 2)
Write-Host "==> OK: $tarGz ($sizeGb GB)"

if ($Upload) {
    if (-not $VpsHost) { $VpsHost = $env:FL_VPS_SSH }
    if (-not $VpsHost) {
        throw "Defina -VpsHost ou env FL_VPS_SSH. Nao use IP no codigo."
    }
    if (-not $PublicDownloadBase) { $PublicDownloadBase = $env:FL_DOWNLOAD_BASE }

    Write-Host ("==> Upload para {0} ..." -f $VpsHost)
    ssh -o BatchMode=yes $VpsHost "sudo mkdir -p $VpsDir && sudo chown ubuntu:ubuntu $VpsDir"
    scp -o BatchMode=yes $tarGz "${VpsHost}:$VpsDir/"
    $base = Split-Path $tarGz -Leaf
    ssh -o BatchMode=yes $VpsHost "cd $VpsDir && ln -sfn $base FrontLine-Setup-latest.tar.gz && ls -lh"
    if ($PublicDownloadBase) {
        $baseUrl = $PublicDownloadBase.TrimEnd('/')
        Write-Host ("==> Download: {0}/downloads/{1}" -f $baseUrl, $base)
    } else {
        Write-Host "==> Upload OK (sem URL publica)."
    }
}

Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Concluido. Arquivo local em dist\ (nao sobe sozinho para a VPS)."
