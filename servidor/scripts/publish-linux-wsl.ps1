$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$wslPath = (wsl.exe -d Ubuntu-22.04 -e wslpath -a $repoPath).Trim()
$command = "cd '$wslPath' && sudo -n docker run --rm -v `"`$PWD:/src`" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet publish Server.Host/Server.Host.csproj -c Release -r linux-x64 --self-contained true -o /src/artifacts/linux-x64"
wsl.exe -d Ubuntu-22.04 -e bash -lc $command
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
