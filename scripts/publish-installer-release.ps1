# Sobe um instalador ja gerado em dist\ para o repo PUBLICO de downloads.
# Uso (apos gh auth login):
#   .\scripts\publish-installer-release.ps1
#   .\scripts\publish-installer-release.ps1 -Exe .\dist\Instalador-FrontLine-Slim-20260905.exe

param(
    [string] $Exe = "",
    [string] $GitHubRepo = "pbcaioegidio/frontline-downloads",
    [string] $ReleaseTag = "",
    [string] $Mode = "Slim",
    [string] $Version = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$gh = @(
    (Get-Command gh -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source),
    "$env:ProgramFiles\GitHub CLI\gh.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $gh) { throw "Instale GitHub CLI: winget install GitHub.cli" }

if (-not $Exe) {
    $Exe = Get-ChildItem (Join-Path $root "dist\Instalador-FrontLine-*.exe") |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $Exe -or -not (Test-Path $Exe)) { throw "EXE nao encontrado em dist\" }

$stamp = Get-Date -Format "yyyyMMdd"
if (-not $ReleaseTag) { $ReleaseTag = "installer-v$stamp" }
if (-not $Version) { $Version = (Get-Date -Format "yyyy.M.d") }

$title = "Instalador FrontLine $Version ($Mode)"
$notes = @"
<p align="center">
  <img src="https://github.com/$GitHubRepo/raw/main/media/banner.jpg" alt="FrontLine — Instalador Windows" width="100%">
</p>

## Instalador FrontLine ($Mode)

Baixe **apenas** o arquivo ``Instalador-FrontLine-*.exe`` abaixo  
(ignore "Source code" — nao e o jogo).

### Passo a passo

1. **Baixar** o ``.exe`` desta pagina
2. **Instalar** (UAC / administrador — so nesta vez)
3. **Abrir** o FrontLine pelo atalho — updates pelo launcher

Depois de instalado, nao precisa baixar o instalador de novo.

Requisitos: Windows 10/11 64 bits + internet.
"@

Write-Host "EXE: $Exe ($([math]::Round((Get-Item $Exe).Length/1MB,1)) MB)"
Write-Host "Repo: $GitHubRepo  Tag: $ReleaseTag"

& $gh auth status
if ($LASTEXITCODE -ne 0) { throw "Rode: gh auth login" }

$ErrorActionPreference = "Continue"
& $gh repo view $GitHubRepo 2>$null | Out-Null
$repoOk = ($LASTEXITCODE -eq 0)
$ErrorActionPreference = "Stop"
if (-not $repoOk) {
    Write-Host "Criando repo publico $GitHubRepo ..."
    & $gh repo create $GitHubRepo --public --description "Downloads do instalador FrontLine (sem codigo do servidor)"
    if ($LASTEXITCODE -ne 0) { throw "Falha ao criar repo" }
}

# GitHub exige pelo menos 1 commit antes de criar Release
$ErrorActionPreference = "Continue"
$defaultBranch = (& $gh api "repos/$GitHubRepo" --jq .default_branch 2>$null)
$ErrorActionPreference = "Stop"
if (-not $defaultBranch) {
    Write-Host "Repo vazio: criando README inicial..."
    $seed = Join-Path $env:TEMP ("fl-dl-seed-" + [guid]::NewGuid().ToString("n"))
    New-Item -ItemType Directory -Force -Path $seed | Out-Null
    @"
# FrontLine — downloads

Instalador Windows. Codigo do servidor e privado.

**[Ultima versao](https://github.com/$GitHubRepo/releases/latest)**
"@ | Set-Content (Join-Path $seed "README.md") -Encoding UTF8
    Push-Location $seed
    git init -b main | Out-Null
    git add README.md
    git -c user.email="noreply@github.com" -c user.name="FrontLine" commit -m "README"
    git remote add origin "https://github.com/$GitHubRepo.git"
    git push -u origin main
    if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Falha ao fazer seed do repo" }
    Pop-Location
    Remove-Item $seed -Recurse -Force -ErrorAction SilentlyContinue
}

$ErrorActionPreference = "Continue"
& $gh release view $ReleaseTag -R $GitHubRepo 2>$null | Out-Null
$relOk = ($LASTEXITCODE -eq 0)
$ErrorActionPreference = "Stop"
if ($relOk) {
    Write-Host "Upload (release ja existe)..."
    & $gh release upload $ReleaseTag $Exe -R $GitHubRepo --clobber
} else {
    Write-Host "Criando release + upload (arquivo grande, pode demorar varios minutos)..."
    & $gh release create $ReleaseTag $Exe -R $GitHubRepo --title $title --notes $notes
}
if ($LASTEXITCODE -ne 0) { throw "Falha no release" }

Write-Host "OK https://github.com/$GitHubRepo/releases/latest"
