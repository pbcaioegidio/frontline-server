$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$wslPath = (wsl.exe -d Ubuntu-22.04 -e wslpath -a $repoPath).Trim()
$command = "cd '$wslPath' && sudo -n docker compose --env-file .env down"
wsl.exe -d Ubuntu-22.04 -e bash -lc $command
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
