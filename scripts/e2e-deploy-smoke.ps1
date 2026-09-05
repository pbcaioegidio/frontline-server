# Smoke E2E do pipeline de deploy (VPS + artefatos locais).
# Nao dispara tags no GitHub — so confere se a VPS esta pronta para receber Actions.
# Uso: .\scripts\e2e-deploy-smoke.ps1
# Env: $env:FL_VPS_HOST (default 132.226.74.48), $env:FL_VPS_USER (default ubuntu)

$ErrorActionPreference = "Stop"
$hostName = if ($env:FL_VPS_HOST) { $env:FL_VPS_HOST } else { "132.226.74.48" }
$user = if ($env:FL_VPS_USER) { $env:FL_VPS_USER } else { "ubuntu" }
$root = Split-Path $PSScriptRoot -Parent

Write-Host "==> Validate workflows"
& (Join-Path $PSScriptRoot "validate-deploy-workflows.ps1")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "==> VPS smoke ($user@$hostName)"
$remote = @"
set -euo pipefail
ROOT=/opt/frontline/servidor
test -f "`$ROOT/docker-compose.vps.yml"
test -f "`$ROOT/docker-compose.hostnet.yml"
test -f "`$ROOT/runtime/Socket/Config/config.ini"
test -f "`$ROOT/runtime/Socket/Info/manifest.json"
test -f "`$ROOT/runtime/Socket/Data/Client/UserFileList.dat"
test -f "`$ROOT/runtime/Socket/Data/Client/UserFileList.sig"
grep -E '^(Launcher|Client)Version=' "`$ROOT/runtime/Socket/Config/config.ini"
cd "`$ROOT"
docker compose -f docker-compose.vps.yml ps
ss -ltn | grep -E ':9000|:39190' || true
echo E2E_VPS_OK
"@
# Bash na VPS nao tolera CRLF do Windows
$remoteLf = ($remote -replace "`r`n", "`n" -replace "`r", "`n")
$tmpSh = Join-Path $env:TEMP "fl-e2e-vps.sh"
[IO.File]::WriteAllText($tmpSh, $remoteLf, [Text.UTF8Encoding]::new($false))
scp -o BatchMode=yes $tmpSh "${user}@${hostName}:/tmp/fl-e2e-vps.sh"
ssh -o BatchMode=yes "${user}@${hostName}" "bash /tmp/fl-e2e-vps.sh"
if ($LASTEXITCODE -ne 0) { throw "VPS smoke falhou" }

$pem = Join-Path $root "launcher\security\filelist-private.pem"
$ufl = Join-Path $root "client\UserFileList.dat"
if ((Test-Path $pem) -and (Test-Path $ufl)) {
    Write-Host "==> Local SignFileList / merge smoke"
    $patch = Join-Path $root "client-patch"
    $smoke = Join-Path $patch "_e2e_smoke.txt"
    Set-Content $smoke "e2e"
    try {
        & (Join-Path $PSScriptRoot "build-client-patch.ps1") `
            -Version 20990101 `
            -PatchRoot $patch `
            -ExistingFileList $ufl `
            -PrivatePemPath $pem `
            -OutDir (Join-Path $root "out\e2e-smoke")
        if ($LASTEXITCODE -ne 0) { throw "build-client-patch falhou" }
        Write-Host "PATCH_BUILD_OK"
    }
    finally {
        Remove-Item $smoke -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path $root "out\e2e-smoke") -Recurse -Force -ErrorAction SilentlyContinue
    }
}
else {
    Write-Host "Skip local patch smoke (sem pem ou UserFileList local)"
}

Write-Host ""
Write-Host "E2E smoke OK. Proximo passo manual: push das tags com secrets no GitHub." -ForegroundColor Green
exit 0
