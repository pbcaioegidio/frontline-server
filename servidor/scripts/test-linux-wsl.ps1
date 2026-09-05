$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$wslPath = (wsl.exe -d Ubuntu-22.04 -e wslpath -a $repoPath).Trim()
$command = "cd '$wslPath' && sudo -n docker run --rm -v `"`$PWD:/src`" -w /src mcr.microsoft.com/dotnet/sdk:8.0 dotnet run --project Server.PortabilityTests/Server.PortabilityTests.csproj -c Release"
wsl.exe -d Ubuntu-22.04 -e bash -lc $command
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
