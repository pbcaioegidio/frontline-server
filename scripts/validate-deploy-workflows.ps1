# Valida workflows e scripts de deploy (sem publicar na VPS).
# Uso: .\scripts\validate-deploy-workflows.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$failed = 0

function Ok($msg) { Write-Host "[OK] $msg" -ForegroundColor Green }
function Bad($msg) { Write-Host "[FAIL] $msg" -ForegroundColor Red; $script:failed++ }

$workflows = @(
    ".github/workflows/server-deploy.yml",
    ".github/workflows/launcher-release.yml",
    ".github/workflows/client-patch.yml"
)
foreach ($rel in $workflows) {
    $p = Join-Path $root $rel
    if (-not (Test-Path $p)) { Bad "ausente: $rel"; continue }
    $text = Get-Content $p -Raw
    if ($text -notmatch '(?m)^name:\s') { Bad "$rel sem name:"; continue }
    if ($text -notmatch 'VPS_SSH_KEY') { Bad "$rel sem VPS_SSH_KEY"; continue }
    Ok $rel
}

$scripts = @(
    "scripts/build-client-patch.ps1",
    "scripts/pack-player-setup.ps1",
    "scripts/publish-installer-release.ps1",
    "scripts/validate-deploy-workflows.ps1",
    "scripts/e2e-deploy-smoke.ps1",
    "docs/deploy-github-actions.md",
    "servidor/docker-compose.hostnet.yml",
    "client-patch/README.md",
    "launcher/tools/SignFileList/SignFileList.csproj"
)
foreach ($rel in $scripts) {
    $p = Join-Path $root $rel
    if (Test-Path $p) { Ok $rel } else { Bad "ausente: $rel" }
}

# IntegrityRules deve pular Shop/EventPortal
$rules = Join-Path $root "launcher\Point Blank Launcher\Launcher.PointBlank\Services\IntegrityRules.cs"
$rt = Get-Content $rules -Raw
if ($rt -match 'Shop\.dat' -and $rt -match 'EventPortal\.dat') {
    Ok "IntegrityRules ignora Shop.dat / EventPortal.dat"
} else {
    Bad "IntegrityRules sem skip de Shop/EventPortal"
}

# Checklist E2E (manual)
Write-Host ""
Write-Host "=== Checklist E2E (manual apos secrets) ===" -ForegroundColor Cyan
Write-Host "1. GitHub secrets: VPS_HOST, VPS_SSH_KEY, FILELIST_PRIVATE_PEM"
Write-Host "2. Seed inicial (uma vez): scp client/UserFileList.dat[.sig] → runtime/Socket/Data/Client/"
Write-Host "3. git tag server-vYYYYMMDD && git push origin server-vYYYYMMDD"
Write-Host "4. git tag launcher-vYYYYMMDD && git push origin launcher-vYYYYMMDD"
Write-Host "5. Coloque delta em client-patch/ → tag client-vYYYYMMDD → push"
Write-Host "6. No PC: FLLauncher → Update → Guard OK → Start → lobby"
Write-Host ""

if ($failed -gt 0) {
    Write-Host "Falhas: $failed" -ForegroundColor Red
    exit 1
}
Write-Host "Tudo certo para commit/push dos workflows." -ForegroundColor Green
exit 0
