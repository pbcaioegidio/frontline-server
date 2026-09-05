@echo off
setlocal
where dotnet >nul 2>nul || (
  echo .NET 8 SDK is required. Install it from https://dotnet.microsoft.com/download/dotnet/8.0
  exit /b 1
)
dotnet --list-sdks | findstr /b "8." >nul || (
  echo .NET 8 SDK is required; only a runtime is currently installed.
  exit /b 1
)
if /i "%~1"=="linux" (
  dotnet publish Server.Host\Server.Host.csproj -c Debug -o binary\Debug\console
  exit /b %errorlevel%
)
dotnet publish Unico.Execute\Executable.csproj -c Debug -o binary\Debug
exit /b %errorlevel%
