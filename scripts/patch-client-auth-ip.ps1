# Atualiza o IP do Auth no client (lwsi_En.sif) — bitstream ROL1.
# Uso: .\scripts\patch-client-auth-ip.ps1 -Ip 132.226.74.48
param(
    [Parameter(Mandatory = $true)][string] $Ip,
    [string] $SifPath = "",
    [string] $OldIp = ""
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not $SifPath) { $SifPath = Join-Path $root "client\Config\lwsi_En.sif" }
if (-not (Test-Path $SifPath)) { throw "sif nao encontrado: $SifPath" }

function Get-Bits([byte[]]$data) {
    $bits = New-Object bool[] ($data.Length * 8)
    for ($i = 0; $i -lt $data.Length; $i++) {
        for ($b = 0; $b -lt 8; $b++) {
            $bits[$i * 8 + $b] = (($data[$i] -shr (7 - $b)) -band 1) -eq 1
        }
    }
    return $bits
}
function Set-Bits([bool[]]$bits) {
    $len = [int]($bits.Length / 8)
    $out = New-Object byte[] $len
    for ($i = 0; $i -lt $len; $i++) {
        $v = 0
        for ($b = 0; $b -lt 8; $b++) {
            if ($bits[$i * 8 + $b]) { $v = $v -bor (1 -shl (7 - $b)) }
        }
        $out[$i] = [byte]$v
    }
    return $out
}
function Rotate-Bits([bool[]]$bits, [int]$n, [switch]$Left) {
    $L = $bits.Length
    $n = $n % $L
    $out = New-Object bool[] $L
    for ($i = 0; $i -lt $L; $i++) {
        if ($Left) { $out[$i] = $bits[($i + $n) % $L] }
        else { $out[$i] = $bits[($i - $n + $L) % $L] }
    }
    return $out
}

$raw = [IO.File]::ReadAllBytes($SifPath)
$txt = [Text.Encoding]::ASCII.GetString((Set-Bits (Rotate-Bits (Get-Bits $raw) 1 -Left)))
if ($txt -notmatch 'ServerIp00') { throw "decrypt falhou (ServerIp00 nao encontrado)" }

if (-not $OldIp) {
    $m = [regex]::Match($txt, 'ServerIp00\s*=\s*"([^"]+)"')
    if (-not $m.Success) { throw "nao achei ServerIp00" }
    $OldIp = $m.Groups[1].Value
}
if ($OldIp.Length -ne $Ip.Length) {
    throw "IP novo deve ter o mesmo tamanho ($($OldIp.Length) chars). Velho='$OldIp' Novo='$Ip'"
}

Copy-Item $SifPath "$SifPath.bak" -Force
$n = ([regex]::Matches($txt, [regex]::Escape($OldIp))).Count
$txt2 = $txt.Replace($OldIp, $Ip)
$plain = [Text.Encoding]::ASCII.GetBytes($txt2)
if ($plain.Length -ne $raw.Length) {
    $fixed = New-Object byte[] $raw.Length
    [Array]::Copy($plain, $fixed, [Math]::Min($plain.Length, $raw.Length))
    $plain = $fixed
}
$out = Set-Bits (Rotate-Bits (Get-Bits $plain) 1) # ROR1 encrypt
[IO.File]::WriteAllBytes($SifPath, $out)
Write-Host "OK: $n ocorrencias $OldIp -> $Ip em $SifPath"
