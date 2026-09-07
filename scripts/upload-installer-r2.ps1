# Sobe FrontLine-Setup-latest.zip da VPS (ou local) para Cloudflare R2.
# Requisitos: aws CLI v2 (S3-compatible) + docs/r2-secrets.local.env preenchido
#
# Uso (na maquina com o ZIP, ou via SSH na VPS apos scp do script):
#   .\scripts\upload-installer-r2.ps1
#   .\scripts\upload-installer-r2.ps1 -Source "C:\path\FrontLine-Setup-latest.zip"

param(
    [string] $Source = "",
    [string] $SecretsFile = "",
    [string] $ObjectKey = "FrontLine-Setup-latest.zip",
    [switch] $FromVps
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not $SecretsFile) { $SecretsFile = Join-Path $root "docs\r2-secrets.local.env" }
if (-not (Test-Path $SecretsFile)) { throw "Crie $SecretsFile com R2_ACCESS_KEY_ID e R2_SECRET_ACCESS_KEY" }

Get-Content $SecretsFile | ForEach-Object {
    if ($_ -match '^\s*#' -or $_ -notmatch '=') { return }
    $k, $v = $_.Split('=', 2)
    Set-Item -Path "Env:$($k.Trim())" -Value $v.Trim()
}
foreach ($req in @('R2_ACCOUNT_ID', 'R2_BUCKET', 'R2_ACCESS_KEY_ID', 'R2_SECRET_ACCESS_KEY')) {
    if (-not [string]::IsNullOrWhiteSpace((Get-Item "Env:$req").Value)) { continue }
    throw "Falta $req em $SecretsFile"
}
if ($env:R2_ACCESS_KEY_ID -notmatch '\S' -or $env:R2_SECRET_ACCESS_KEY -notmatch '\S') {
    throw "Preencha R2_ACCESS_KEY_ID e R2_SECRET_ACCESS_KEY em $SecretsFile"
}

$endpoint = "https://$($env:R2_ACCOUNT_ID).r2.cloudflarestorage.com"
$env:AWS_ACCESS_KEY_ID = $env:R2_ACCESS_KEY_ID
$env:AWS_SECRET_ACCESS_KEY = $env:R2_SECRET_ACCESS_KEY
$env:AWS_DEFAULT_REGION = "auto"

if ($FromVps) {
    if (-not $env:FL_VPS_SSH) { $env:FL_VPS_SSH = "ubuntu@132.226.74.48" }
    Write-Host "==> Upload direto na VPS (arquivo ja esta em /var/frontline/downloads) ..."
    $remote = @"
set -euo pipefail
    if ! command -v aws >/dev/null 2>&1; then
      ARCH=`$(uname -m)
      case "`$ARCH" in
        aarch64|arm64) AWSZIP=awscli-exe-linux-aarch64.zip ;;
        *) AWSZIP=awscli-exe-linux-x86_64.zip ;;
      esac
      echo "==> instalando AWS CLI v2 (`$ARCH)..."
      cd /tmp
      curl -fsSL "https://awscli.amazonaws.com/`$AWSZIP" -o awscliv2.zip
      sudo apt-get update -qq
      sudo apt-get install -y -qq unzip
      unzip -qo awscliv2.zip
      sudo ./aws/install --update
    fi
    command -v aws >/dev/null || { echo "aws CLI ausente"; exit 1; }
export AWS_ACCESS_KEY_ID='$($env:R2_ACCESS_KEY_ID)'
export AWS_SECRET_ACCESS_KEY='$($env:R2_SECRET_ACCESS_KEY)'
export AWS_DEFAULT_REGION=auto
ENDPOINT='$endpoint'
SRC=/var/frontline/downloads/FrontLine-Setup-latest.zip
test -f "`$SRC"
ls -lh "`$SRC"
aws s3 cp "`$SRC" "s3://$($env:R2_BUCKET)/$ObjectKey" --endpoint-url "`$ENDPOINT"
echo OK
aws s3 ls "s3://$($env:R2_BUCKET)/" --endpoint-url "`$ENDPOINT"
"@
    $remote = $remote -replace "`r", ""
    $remote | ssh -o BatchMode=yes $env:FL_VPS_SSH bash
} else {
    if (-not $Source) {
        $cand = @(
            (Join-Path $root "dist\FrontLine-Setup-latest.zip"),
            "$env:USERPROFILE\Downloads\FrontLine-Setup-latest.zip"
        )
        foreach ($c in $cand) { if (Test-Path $c) { $Source = $c; break } }
    }
    if (-not $Source -or -not (Test-Path $Source)) {
        throw "ZIP nao encontrado. Passe -Source ou use -FromVps"
    }
    $aws = Get-Command aws -ErrorAction SilentlyContinue
    if (-not $aws) { throw "Instale AWS CLI: winget install Amazon.AWSCLI" }
    Write-Host "==> Upload local: $Source -> s3://$($env:R2_BUCKET)/$ObjectKey"
    & aws s3 cp $Source "s3://$($env:R2_BUCKET)/$ObjectKey" --endpoint-url $endpoint
}

$public = ($env:R2_PUBLIC_BASE).TrimEnd('/')
Write-Host "==> Teste: $public/$ObjectKey"
