# Empacota instalador Windows (.exe) do jogador via Inno Setup.
# Uso:
#   .\scripts\pack-player-setup.ps1 -Mode Slim
#   .\scripts\pack-player-setup.ps1 -Mode Full
#   .\scripts\pack-player-setup.ps1 -Mode Slim -GitHubRelease   # download facil: Releases do repo
#   .\scripts\pack-player-setup.ps1 -Mode Slim -Upload          # espelho VPS (opcional)
#
# Requisitos: Inno Setup 6 (ISCC). Se faltar: winget install JRSoftware.InnoSetup
# -GitHubRelease: gh CLI autenticado (gh auth login) no repo frontline-server
# -Upload: defina $env:FL_VPS_SSH (nunca IP no codigo).

param(
    [ValidateSet("Slim", "Full")]
    [string] $Mode = "Slim",
    [switch] $Upload,
    [switch] $GitHubRelease,
    [switch] $KeepStage,
    [string] $ClientRoot = "",
    [string] $OutDir = "",
    [string] $VpsHost = "",
    [string] $VpsDir = "/var/frontline/downloads",
    [string] $PublicDownloadBase = "",
    [string] $Version = "",
    [string] $ReleaseTag = "",
    [switch] $Sign,
    [string] $SignThumbprint = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path (Join-Path $root "client"))) {
    $root = "c:\Users\pbcai\Downloads\source"
}
if (-not $ClientRoot) { $ClientRoot = Join-Path $root "client" }
if (-not $OutDir) { $OutDir = Join-Path $root "dist" }
if (-not $Version) { $Version = Get-Date -Format "yyyy.M.d" }

if (-not (Test-Path $ClientRoot)) {
    throw "Client nao encontrado: $ClientRoot"
}

function Find-Iscc {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 7\ISCC.exe")
    )
    foreach ($c in $candidates) {
        if ($c -and (Test-Path $c)) { return $c }
    }
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    return $null
}

$iscc = Find-Iscc
if (-not $iscc) {
    throw @"
Inno Setup (ISCC.exe) nao encontrado.
Instale: winget install --id JRSoftware.InnoSetup -e
Depois rode este script de novo.
"@
}

$stamp = Get-Date -Format "yyyyMMdd"
$outName = "Instalador-FrontLine-$Mode-$stamp"
$stage = Join-Path $env:TEMP "FrontLine-Setup-stage-$Mode"
$iss = Join-Path $PSScriptRoot "FrontLine-Setup.iss"
$icon = Join-Path $root "docs\frontline-setup.ico"

if (-not (Test-Path $iss)) {
    throw "Script Inno ausente: $iss"
}

Write-Host "==> Modo $Mode (instalador .exe)"
Write-Host "    Client: $ClientRoot"
Write-Host "    Stage:  $stage"
Write-Host "    Out:    $OutDir"
Write-Host "    ISCC:   $iscc"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage, $OutDir | Out-Null

# Icone multi-tamanho (ICO minimo ~10 KB; se for stub velho, regenera)
$needIcon = (-not (Test-Path $icon)) -or ((Get-Item $icon).Length -lt 4096)
if ($needIcon) {
    $launcherIcoSrc = Join-Path $ClientRoot "FLLauncher.exe"
    if (-not (Test-Path $launcherIcoSrc)) {
        $launcherIcoSrc = Join-Path $ClientRoot "FrontLine.exe"
    }
    if (-not (Test-Path $launcherIcoSrc)) {
        throw "Icone ausente e sem EXE fonte: $icon"
    }
    $docs = Join-Path $root "docs"
    New-Item -ItemType Directory -Force -Path $docs | Out-Null
    $iconTool = Join-Path $PSScriptRoot "_make-setup-ico.ps1"
    & $iconTool -SourceExe $launcherIcoSrc -OutIco $icon
    Write-Host "    Icone gerado: $icon ($((Get-Item $icon).Length) bytes)"
}
if (-not (Test-Path $icon) -or (Get-Item $icon).Length -lt 4096) {
    throw "Icone invalido: $icon"
}

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
Write-Host "==> Copiando arquivos para stage..."
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
FrontLine

Instalado pelo setup Windows (FLSetup / FrontLine-Setup).
Abra FLLauncher.exe (atalho no menu Iniciar / Area de trabalho).

Nao feche o FLLauncher enquanto joga (FL Guard / heartbeat).
Atualizacoes: o launcher baixa so o que mudou (Socket).

Modo deste pacote: $Mode
Versao: $Version
"@
Set-Content -Path (Join-Path $stage "LEIA-ME.txt") -Value $readme -Encoding UTF8

# Liberar EXE antigo se estiver travado
$exeOut = Join-Path $OutDir "$outName.exe"
Get-Process ISCC, Compil32 -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400
if (Test-Path $exeOut) {
    try { Remove-Item $exeOut -Force -ErrorAction Stop }
    catch {
        throw "Nao consegui apagar $exeOut (arquivo em uso). Feche o Explorer/propriedades e tente de novo."
    }
}

Write-Host "==> Compilando instalador Inno (pode demorar no Full)..."
$isccArgs = @(
    "/DFlSource=$stage",
    "/DFlOutDir=$OutDir",
    "/DFlOutName=$outName",
    "/DFlVersion=$Version",
    "/DFlMode=$Mode",
    "/DFlIcon=$icon"
)
if ($Sign) {
    if (-not $SignThumbprint) { $SignThumbprint = $env:FL_CODESIGN_THUMBPRINT }
    if (-not $SignThumbprint) {
        throw "Assinatura pedida: defina -SignThumbprint ou env FL_CODESIGN_THUMBPRINT (certificado Authenticode)."
    }
    $isccArgs += "/DFlSign=1"
    # ISCC usa [SignTools] no ISS ou /Sname=cmd — registramos via extra
    $signCmd = "signtool.exe sign /fd SHA256 /td SHA256 /tr http://timestamp.digicert.com /sha1 $SignThumbprint `$f"
    $isccArgs += "/SFrontLineSign=$signCmd"
}
$isccArgs += $iss
& $iscc @isccArgs
if ($LASTEXITCODE -ne 0) {
    throw "ISCC falhou: exit $LASTEXITCODE"
}

if (-not (Test-Path $exeOut)) {
    throw "falha ao criar $exeOut"
}

$sizeGb = [math]::Round((Get-Item $exeOut).Length / 1GB, 2)
$sizeMb = [math]::Round((Get-Item $exeOut).Length / 1MB, 1)
if ($sizeGb -ge 1) {
    Write-Host "==> OK: $exeOut ($sizeGb GB)"
} else {
    Write-Host "==> OK: $exeOut ($sizeMb MB)"
}

if ($GitHubRelease) {
    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $gh) {
        throw "gh CLI nao encontrado. Instale: winget install GitHub.cli  e rode gh auth login"
    }
    if (-not $ReleaseTag) {
        $ReleaseTag = "installer-v$stamp"
    }
    $title = "Instalador FrontLine $Version ($Mode)"
    $notes = @"
## Instalador FrontLine ($Mode)

Versao do setup: **$Version**

1. Baixe o ``.exe`` abaixo
2. Instale (UAC / administrador — so nesta instalacao)
3. Abra o **FrontLine** pelo atalho
4. Atualizacoes futuras: botao **Update** no FLLauncher (nao precisa baixar o instalador de novo)

Slim = sem pasta Pack. Full = client completo.
"@
    Write-Host "==> GitHub Release $ReleaseTag ..."
    $view = & gh release view $ReleaseTag 2>$null
    if ($LASTEXITCODE -eq 0) {
        & gh release upload $ReleaseTag $exeOut --clobber
        if ($LASTEXITCODE -ne 0) { throw "gh release upload falhou" }
        $notesFile = Join-Path $env:TEMP "fl-release-notes.md"
        Set-Content -Path $notesFile -Value $notes -Encoding UTF8
        & gh release edit $ReleaseTag --title $title --notes-file $notesFile
    } else {
        & gh release create $ReleaseTag $exeOut --title $title --notes $notes
        if ($LASTEXITCODE -ne 0) { throw "gh release create falhou" }
    }
    $repo = (& gh repo view --json nameWithOwner -q .nameWithOwner 2>$null)
    if (-not $repo) { $repo = "pbcaioegidio/frontline-server" }
    Write-Host "==> Download: https://github.com/$repo/releases/tag/$ReleaseTag"
    Write-Host "==> Latest:   https://github.com/$repo/releases/latest"
}

if ($Upload) {
    if (-not $VpsHost) { $VpsHost = $env:FL_VPS_SSH }
    if (-not $VpsHost) {
        throw "Defina -VpsHost ou env FL_VPS_SSH. Nao use IP no codigo."
    }
    if (-not $PublicDownloadBase) { $PublicDownloadBase = $env:FL_DOWNLOAD_BASE }

    Write-Host ("==> Upload para {0} ..." -f $VpsHost)
    ssh -o BatchMode=yes $VpsHost "sudo mkdir -p $VpsDir && sudo chown ubuntu:ubuntu $VpsDir"
    scp -o BatchMode=yes $exeOut "${VpsHost}:$VpsDir/"
    $base = Split-Path $exeOut -Leaf
    ssh -o BatchMode=yes $VpsHost "cd $VpsDir && ln -sfn $base FrontLine-Setup-latest.exe && ls -lh"
    if ($PublicDownloadBase) {
        $baseUrl = $PublicDownloadBase.TrimEnd('/')
        Write-Host ("==> Download: {0}/downloads/{1}" -f $baseUrl, $base)
        Write-Host ("==> Latest:   {0}/downloads/FrontLine-Setup-latest.exe" -f $baseUrl)
    } else {
        Write-Host "==> Upload OK (sem URL publica)."
    }
}

if (-not $KeepStage) {
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "Concluido. Instalador: dist\$outName.exe"
Write-Host "  - Eleva como administrador (uma vez)"
Write-Host "  - Desinstalador no Painel de Controle / menu Iniciar"
Write-Host "  - Atalhos FLLauncher"
Write-Host "  - Pode apagar o .exe antigo em dist\ e gerar de novo quando quiser"
