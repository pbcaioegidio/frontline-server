# Empacota instalador Windows (.exe) do jogador via Inno Setup.
# Uso:
#   .\scripts\pack-player-setup.ps1 -Mode Slim
#   .\scripts\pack-player-setup.ps1 -Mode Slim -GitHubRelease
#       → sobe no repo PUBLICO de download (padrao: pbcaioegidio/frontline-downloads)
#   .\scripts\pack-player-setup.ps1 -Mode Slim -Upload
#
# Requisitos: Inno Setup 6 (ISCC). Se faltar: winget install JRSoftware.InnoSetup
# -GitHubRelease: winget install GitHub.cli + gh auth login
# -Upload: defina $env:FL_VPS_SSH (nunca IP no codigo).

param(
    [ValidateSet("Slim", "Full")]
    [string] $Mode = "Full",
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
    # Repo PUBLICO so de instalador (codigo do server fica no privado)
    [string] $GitHubRepo = "pbcaioegidio/frontline-downloads",
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
    Write-Warning "Modo Slim: SEM pasta Pack (so teste). Jogadores devem usar -Mode Full."
    $excludeDirs += "Pack"
} else {
    Write-Host "==> Modo Full: inclui Pack (instalador grande; GitHub Releases max ~2 GB — use -Upload na VPS)."
}

# FrontLine.exe sem UAC a cada abertura (manifest asInvoker)
$flExe = Join-Path $ClientRoot "FrontLine.exe"
if (Test-Path $flExe) {
    $raw = [IO.File]::ReadAllBytes($flExe)
    $ascii = [Text.Encoding]::ASCII.GetString($raw)
    if ($ascii.Contains("requireAdministrator")) {
        Write-Host "==> Removendo requireAdministrator de FrontLine.exe (asInvoker)..."
        $needle = [Text.Encoding]::ASCII.GetBytes("requireAdministrator")
        $repl = [Text.Encoding]::ASCII.GetBytes("asInvoker            ")
        for ($i = 0; $i -le $raw.Length - $needle.Length; $i++) {
            $ok = $true
            for ($j = 0; $j -lt $needle.Length; $j++) {
                if ($raw[$i + $j] -ne $needle[$j]) { $ok = $false; break }
            }
            if ($ok) {
                for ($j = 0; $j -lt $repl.Length; $j++) { $raw[$i + $j] = $repl[$j] }
                $i += $needle.Length - 1
            }
        }
        [IO.File]::WriteAllBytes($flExe, $raw)
    }
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
if ($Mode -eq "Full" -and -not (Test-Path (Join-Path $stage "Pack"))) {
    throw "Modo Full exige pasta Pack no client: $(Join-Path $ClientRoot 'Pack')"
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
# Slim (e Full se a lista do client estiver velha): UserFileList deve bater com o stage.
# Sem isso o Slim leva lista Full com Pack\ → FL Guard marca milhares de arquivos ausentes.
$pem = Join-Path $root "launcher\security\filelist-private.pem"
$flProj = Join-Path $root "launcher\tools\FileListBuilder\FileListBuilder.csproj"
if (-not (Test-Path $pem)) {
    throw "Chave FileList ausente: $pem"
}
if (-not (Test-Path $flProj)) {
    throw "FileListBuilder ausente: $flProj"
}
Write-Host "==> Regenerando UserFileList.dat/.sig a partir do stage ($Mode)..."
dotnet run --project $flProj -c Release --no-launch-profile -- $stage $pem
if ($LASTEXITCODE -ne 0) {
    throw "FileListBuilder falhou: exit $LASTEXITCODE"
}
if (-not (Test-Path (Join-Path $stage "UserFileList.dat")) -or -not (Test-Path (Join-Path $stage "UserFileList.sig"))) {
    throw "UserFileList.dat/.sig nao gerados no stage"
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
    if ((Get-Item $exeOut).Length -gt 1900MB) {
        Write-Warning "Arquivo > 1.9 GB — GitHub Releases nao aceita. Use -Upload (VPS) ou hospede o Full fora do GitHub."
        Write-Host "EXE local: $exeOut"
    } else {
    $ghCmd = $null
    $ghFound = Get-Command gh -ErrorAction SilentlyContinue
    if ($ghFound) { $ghCmd = $ghFound.Source }
    else {
        foreach ($p in @(
            "$env:ProgramFiles\GitHub CLI\gh.exe",
            "$env:LOCALAPPDATA\Programs\GitHub CLI\gh.exe"
        )) {
            if (Test-Path $p) { $ghCmd = $p; break }
        }
    }
    if (-not $ghCmd) {
        throw "gh CLI nao encontrado. Instale: winget install GitHub.cli  e rode gh auth login"
    }
    if (-not $GitHubRepo) { $GitHubRepo = "pbcaioegidio/frontline-downloads" }
    if (-not $ReleaseTag) {
        $ReleaseTag = "installer-v$stamp"
    }
    $title = "Instalador FrontLine $Version ($Mode)"
    $notes = @"
<p align="center">
  <img src="https://github.com/$GitHubRepo/raw/main/media/banner.jpg" alt="FrontLine - Instalador Windows" width="100%">
</p>

## Instalador FrontLine ($Mode)

Baixe **apenas** o arquivo ``Instalador-FrontLine-*.exe`` abaixo  
(ignore "Source code" - nao e o jogo).

Versao do setup: **$Version**

1. **Baixar** o ``.exe`` desta pagina
2. **Instalar** (UAC / administrador - so nesta instalacao)
3. **Abrir** o FrontLine pelo atalho - login - se pedir, use **Update**

Depois de instalado, patches saem pelo **FLLauncher** (nao precisa baixar o instalador de novo).

Slim = sem pasta Pack. Full = client completo.
"@
    Write-Host "==> GitHub Release $ReleaseTag em $GitHubRepo ..."
    # Cria repo publico se ainda nao existir (ignora erro se ja existe)
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    & $ghCmd repo view $GitHubRepo 2>$null | Out-Null
    $repoOk = ($LASTEXITCODE -eq 0)
    if (-not $repoOk) {
        Write-Host "    Criando repo publico $GitHubRepo ..."
        & $ghCmd repo create $GitHubRepo --public --description "Downloads do instalador FrontLine (sem codigo do servidor)" --confirm 2>$null
        if ($LASTEXITCODE -ne 0) {
            $ErrorActionPreference = $prevEap
            throw "Nao consegui criar $GitHubRepo. Rode: gh auth login  e tente de novo."
        }
    }
    & $ghCmd release view $ReleaseTag -R $GitHubRepo 2>$null | Out-Null
    $relOk = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = $prevEap
    if ($relOk) {
        & $ghCmd release upload $ReleaseTag $exeOut -R $GitHubRepo --clobber
        if ($LASTEXITCODE -ne 0) { throw "gh release upload falhou" }
        $notesFile = Join-Path $env:TEMP "fl-release-notes.md"
        Set-Content -Path $notesFile -Value $notes -Encoding UTF8
        & $ghCmd release edit $ReleaseTag -R $GitHubRepo --title $title --notes-file $notesFile
    } else {
        & $ghCmd release create $ReleaseTag $exeOut -R $GitHubRepo --title $title --notes $notes
        if ($LASTEXITCODE -ne 0) { throw "gh release create falhou" }
    }
    Write-Host "==> Download: https://github.com/$GitHubRepo/releases/tag/$ReleaseTag"
    Write-Host "==> Latest:   https://github.com/$GitHubRepo/releases/latest"
    } # else tamanho ok pro GitHub
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
