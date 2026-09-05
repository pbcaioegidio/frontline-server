$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$wslPath = (wsl.exe -d Ubuntu-22.04 -e wslpath -a $repoPath).Trim()
wsl.exe -d Ubuntu-22.04 -e bash -lc "cd '$wslPath' && sudo -n docker build -t prismbleed-server:net8 ."
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
