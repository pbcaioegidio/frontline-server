# Empacota instalador Windows (.exe) do jogador via Inno Setup.
# Uso:
#   .\scripts\pack-player-setup.ps1 -Mode Slim
#   .\scripts\pack-player-setup.ps1 -Mode Full -Upload
#       → ZIP local + sobe pro Cloudflare R2 (caminho oficial do site)
#   .\scripts\pack-player-setup.ps1 -Mode Slim -GitHubRelease
#       → LEGADO (GitHub Releases ~2 GB; Full nao serve)
#
# Requisitos: Inno Setup 6 (ISCC). Se faltar: winget install JRSoftware.InnoSetup
# -Upload: docs/r2-secrets.local.env + AWS CLI (local → R2). NAO usa VPS.
# -GitHubRelease: legado Slim; preferir -Upload (R2)

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
$redistDir = Join-Path $PSScriptRoot "redist"

function Ensure-VcRedist {
    param([string] $Dir)
    New-Item -ItemType Directory -Force -Path $Dir | Out-Null
    $files = @(
        @{ Name = "vc_redist.x86.exe"; Url = "https://aka.ms/vs/17/release/vc_redist.x86.exe"; MinBytes = 5MB },
        @{ Name = "vc_redist.x64.exe"; Url = "https://aka.ms/vs/17/release/vc_redist.x64.exe"; MinBytes = 5MB },
        @{ Name = "vcredist2013_x86.exe"; Url = "https://aka.ms/highdpimfc2013x86enu"; MinBytes = 3MB },
        @{ Name = "vcredist2010_x86.exe"; Url = "https://download.microsoft.com/download/1/6/5/165255E7-1014-4D0A-B094-B6A430A6BFFC/vcredist_x86.exe"; MinBytes = 3MB }
    )
    foreach ($f in $files) {
        $dest = Join-Path $Dir $f.Name
        if ((Test-Path $dest) -and (Get-Item $dest).Length -ge $f.MinBytes) {
            Write-Host "    Redist OK: $($f.Name) ($([math]::Round((Get-Item $dest).Length/1MB,1)) MB)"
            continue
        }
        Write-Host "    Baixando $($f.Name) ..."
        try {
            Invoke-WebRequest -Uri $f.Url -OutFile $dest -UseBasicParsing -TimeoutSec 180
        } catch {
            throw "Falha ao baixar $($f.Name): $($_.Exception.Message)"
        }
        if (-not (Test-Path $dest) -or (Get-Item $dest).Length -lt $f.MinBytes) {
            throw "Download incompleto: $dest"
        }
        Write-Host "    Redist OK: $($f.Name) ($([math]::Round((Get-Item $dest).Length/1MB,1)) MB)"
    }
}

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

$excludeDirs = @(
    "CEF\Cache",
    "CEF\UserData",
    "CEF\GPUCache",
    "Shader\Cache",
    "_fl_backup",
    "_fl_publish_tmp",
    "_icon_bak",
    "_tmp_extract_shop",
    "Gui\Loading\_preview",
    "CHEAT_BLOCKER\Temp",
    "tools",
    ".git"
)
if ($Mode -eq "Slim") {
    Write-Warning "Modo Slim: SEM pasta Pack (so teste). Jogadores devem usar -Mode Full."
    $excludeDirs += "Pack"
} else {
    Write-Host "==> Modo Full: inclui Pack (instalador grande; use -Upload para R2)."
}

# Dados de jogador / maquina / lixo de build — nao podem ir no instalador publico.
# launcher.svl e obrigatorio (versao do launcher); sem ele o FLLauncher quebra na abertura.
$excludeFiles = @(
    "LocalConfig.json",
    "UserFileList.sig.bak",
    "UserFileList.dat.bak_pre50",
    "UserFileList.sig.bak_pre50",
    "UserFileList.dat.bad_sig",
    "UserFileList.sig.bad_sig",
    "FrontLine.exe.bak-admin",
    "FrontLine.exe.bak",
    "FLLauncher.exe.bak",
    "BC.log",
    "CrashTrace.log",
    "FLLauncher.log",
    "Cef.log",
    "Thumbs.db",
    "desktop.ini",
    "_preview_eventportal.png",
    "_preview_eventportal_OLD.png",
    "_preview_logo_text01.png",
    "_preview_vertical.png",
    "frontline_gnb.png",
    "vertical.jpg"
)

$xd = @()
foreach ($d in $excludeDirs) {
    $xd += "/XD"
    $xd += $d
}
$xf = @()
foreach ($f in $excludeFiles) {
    $xf += "/XF"
    $xf += $f
}
$rcArgs = @($ClientRoot, $stage, "/E", "/NFL", "/NDL", "/NJH", "/NJS", "/nc", "/ns", "/np") + $xd + $xf
Write-Host "==> Copiando arquivos para stage..."
& robocopy @rcArgs | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw "robocopy falhou: $LASTEXITCODE"
}
if ($Mode -eq "Full" -and -not (Test-Path (Join-Path $stage "Pack"))) {
    throw "Modo Full exige pasta Pack no client: $(Join-Path $ClientRoot 'Pack')"
}

# NAO alterar FrontLine.exe (manifest/UAC). Hex-patch ja quebrou SxS antes.
# Client fonte e stage devem permanecer byte-a-byte iguais no EXE do jogo.

# Cinto de seguranca: remove rastros de conta/sessao se escaparem do robocopy
foreach ($f in $excludeFiles) {
    Get-ChildItem -LiteralPath $stage -Filter $f -Recurse -Force -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
}
# Remove qualquer *.bak* que tenha escapado
Get-ChildItem -LiteralPath $stage -Filter "*.bak*" -Recurse -Force -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -LiteralPath $stage -Filter "*.bad_sig" -Recurse -Force -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -LiteralPath $stage -Filter "*.log" -Recurse -Force -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue
# Pastas CEF de usuário (conta/cache) — nunca no instalador público
@(
    "CEF\UserData",
    "CEF\Cache",
    "CEF\GPUCache",
    "Shader\Cache"
) | ForEach-Object {
    $p = Join-Path $stage $_
    if (Test-Path -LiteralPath $p) {
        Remove-Item -LiteralPath $p -Recurse -Force
        Write-Host "==> Removido do stage: $_"
    }
}
# Sujeira de pintura de logo: Text_3 so existe em Locale\Brazil\UI_V11 na lista oficial.
# Se vazar em Locale\_Common\UI_V12, o FL Guard marca EXTRA e confunde o jogador.
$junkText3 = Join-Path $stage "Locale\_Common\UI_V12\VTexList\Text_3.i3VTexImage"
if (Test-Path -LiteralPath $junkText3) {
    Remove-Item -LiteralPath $junkText3 -Force
    Write-Host "==> Removido extra do stage: Locale\_Common\UI_V12\VTexList\Text_3.i3VTexImage"
}
# Pastas/arquivos de preview/bak que nao podem ir no Full
@(
    "_icon_bak",
    "_tmp_extract_shop",
    "Gui\Loading\_preview"
) | ForEach-Object {
    $p = Join-Path $stage $_
    if (Test-Path -LiteralPath $p) {
        Remove-Item -LiteralPath $p -Recurse -Force
        Write-Host "==> Removido do stage: $_"
    }
}
Get-ChildItem -LiteralPath $stage -Force -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^(?i)_preview_|\.bak$|\.log$|frontline_gnb\.png$|^vertical\.jpg$' } |
    ForEach-Object {
        Remove-Item -LiteralPath $_.FullName -Force
        Write-Host "==> Removido do stage: $($_.Name)"
    }
if (Test-Path (Join-Path $stage "LocalConfig.json")) {
    throw "LocalConfig.json ainda no stage - nao publicar instalador com conta de teste"
}
if (Test-Path (Join-Path $stage "FrontLine.exe.bak-admin")) {
    throw "Backup .bak-admin vazou para o stage - nao publicar"
}

# Integridade: FrontLine.exe do stage == client (nenhum patch)
$srcFl = Join-Path $ClientRoot "FrontLine.exe"
$stgFl = Join-Path $stage "FrontLine.exe"
if ((Test-Path $srcFl) -and (Test-Path $stgFl)) {
    $h1 = (Get-FileHash -Algorithm SHA256 -LiteralPath $srcFl).Hash
    $h2 = (Get-FileHash -Algorithm SHA256 -LiteralPath $stgFl).Hash
    if ($h1 -ne $h2) {
        throw "FrontLine.exe no stage difere do client - abortando"
    }
    Write-Host "==> FrontLine.exe intacto (SHA256=$h1)"
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

Write-Host "==> Garantindo Visual C++ Redistributable (embutido no setup)..."
Ensure-VcRedist -Dir $redistDir

Write-Host "==> Compilando instalador Inno (pode demorar no Full)..."
$isccArgs = @(
    "/DFlSource=$stage",
    "/DFlOutDir=$OutDir",
    "/DFlOutName=$outName",
    "/DFlVersion=$Version",
    "/DFlMode=$Mode",
    "/DFlIcon=$icon",
    "/DFlRedist=$redistDir"
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
        Write-Warning "Arquivo > 1.9 GB - GitHub Releases nao aceita. Use -Upload (R2)."
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

Baixe apenas o arquivo Instalador-FrontLine-*.exe abaixo
(ignore Source code - nao e o jogo).

Versao do setup: $Version

1. Baixar o .exe desta pagina
2. Instalar (UAC / administrador - so nesta instalacao)
3. Abrir o FrontLine pelo atalho - login - se pedir, use Update

Depois de instalado, patches saem pelo FLLauncher (nao precisa baixar o instalador de novo).

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
        $notesFile = Join-Path $env:TEMP "fl-release-notes.md"
        Set-Content -Path $notesFile -Value $notes -Encoding UTF8
        & $ghCmd release create $ReleaseTag $exeOut -R $GitHubRepo --title $title --notes-file $notesFile
        if ($LASTEXITCODE -ne 0) { throw "gh release create falhou" }
    }
    Write-Host "==> Download: https://github.com/$GitHubRepo/releases/tag/$ReleaseTag"
    Write-Host "==> Latest:   https://github.com/$GitHubRepo/releases/latest"
    }
}

if ($Upload) {
    # Oficial: ZIP local → Cloudflare R2 (site BAIXAR). NAO sobe instalador pra VPS.
    if (-not $PublicDownloadBase) {
        $PublicDownloadBase = $env:FL_DOWNLOAD_BASE
        if (-not $PublicDownloadBase) { $PublicDownloadBase = "https://downloads.frontlinebattle.com.br" }
    }

    $zipOut = Join-Path $OutDir "FrontLine-Setup-latest.zip"
    $zipStage = Join-Path $env:TEMP ("FrontLine-Setup-zip-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
    New-Item -ItemType Directory -Force -Path $zipStage | Out-Null
    try {
        Copy-Item $exeOut (Join-Path $zipStage "FrontLine-Setup-latest.exe") -Force
        Get-ChildItem $OutDir -File | Where-Object { $_.Name -like "$outName-*.bin" } | ForEach-Object {
            if ($_.Name -match '-(\d+)\.bin$') {
                Copy-Item $_.FullName (Join-Path $zipStage "FrontLine-Setup-latest-$($Matches[1]).bin") -Force
            }
        }
        if (Test-Path $zipOut) { Remove-Item $zipOut -Force }
        Write-Host "==> ZIP local (store) ..."
        Push-Location $zipStage
        try {
            & tar -a -cf $zipOut --format=zip *
            if ($LASTEXITCODE -ne 0) { throw "tar zip falhou" }
        } finally { Pop-Location }
        Write-Host ("==> ZIP OK: {0} ({1:N1} MB)" -f $zipOut, ((Get-Item $zipOut).Length / 1MB))

        $r2Script = Join-Path $PSScriptRoot "upload-installer-r2.ps1"
        if (-not (Test-Path $r2Script)) { throw "Falta $r2Script" }
        Write-Host "==> Upload Cloudflare R2 (local) ..."
        & powershell -NoProfile -ExecutionPolicy Bypass -File $r2Script -Source $zipOut
        if ($LASTEXITCODE -ne 0) { throw "upload-installer-r2.ps1 falhou" }
    } finally {
        Remove-Item $zipStage -Recurse -Force -ErrorAction SilentlyContinue
    }

    $baseUrl = $PublicDownloadBase.TrimEnd('/')
    Write-Host ("==> ZIP R2:   {0}/FrontLine-Setup-latest.zip" -f $baseUrl)
    Write-Host "==> Site:     https://www.frontlinebattle.com.br/#download"
    Write-Host "    (jogador baixa 1 ZIP no R2, extrai e roda o .exe)"
}

if (-not $KeepStage) {
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "Concluido. Instalador: dist\$outName.exe"
Write-Host "  - Eleva como administrador (uma vez)"
Write-Host "  - Desinstalador no Painel de Controle / menu Iniciar"
Write-Host "  - Atalhos FLLauncher"
Write-Host "  - Pode apagar o .exe antigo em dist\ e gerar de novo quando quiser"
