# Completar PT no PBConfig (dialog + ASCII combos + titulo)
$ErrorActionPreference = 'Stop'
$client = 'C:\Users\pbcai\Downloads\source\client'
$path = Join-Path $client 'PBConfig.exe'
$bak = Join-Path $client 'PBConfig.exe.bak-pt'

Get-Process -Name 'PBConfig' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 400
Copy-Item $bak $path -Force
$b = [IO.File]::ReadAllBytes($path)

function U([int[]]$c) { -join ($c | ForEach-Object { [char]$_ }) }
function Pad([string]$s, [int]$n) {
  if ($s.Length -gt $n) { throw "longo '$s'>$n" }
  $s.PadRight($n, ' ')
}

function Patch-U16([int]$off, [string]$old, [string]$newRaw) {
  $got = [Text.Encoding]::Unicode.GetString($b, $off, $old.Length * 2)
  if ($got -ne $old) { throw "U16 0x$($off.ToString('X')) '$got'" }
  $nb = [Text.Encoding]::Unicode.GetBytes((Pad $newRaw $old.Length))
  [Array]::Copy($nb, 0, $b, $off, $nb.Length)
  Write-Output ("U16 {0} -> {1}" -f $old, $newRaw)
}

function Patch-U16Grow([int]$off, [string]$old, [string]$neu, [int]$cap) {
  # cap = max chars including content, excluding final null (null at index cap)
  if ($neu.Length -gt $cap) { throw 'grow' }
  $got = [Text.Encoding]::Unicode.GetString($b, $off, $old.Length * 2)
  if ($got -ne $old) { throw "growU '$got'" }
  for ($i = $old.Length + 1; $i -le $cap; $i++) {
    if ([BitConverter]::ToUInt16($b, $off + $i*2) -ne 0) { throw "no pad U at $i" }
  }
  $bytes = [Text.Encoding]::Unicode.GetBytes($neu + [char]0)
  [Array]::Copy($bytes, 0, $b, $off, $bytes.Length)
  for ($i = $neu.Length + 1; $i -le $cap; $i++) {
    $b[$off+$i*2]=0; $b[$off+$i*2+1]=0
  }
  Write-Output ("U16grow {0} -> {1}" -f $old, $neu)
}

function Patch-Asc([int]$off, [string]$old, [string]$newRaw) {
  $got = [Text.Encoding]::ASCII.GetString($b, $off, $old.Length)
  if ($got -ne $old) { throw "ASC 0x$($off.ToString('X')) '$got'" }
  if ($b[$off+$old.Length] -ne 0) { throw 'asc term' }
  $nb = [Text.Encoding]::ASCII.GetBytes((Pad $newRaw $old.Length))
  [Array]::Copy($nb, 0, $b, $off, $nb.Length)
  Write-Output ("ASC {0} -> {1}" -f $old, $newRaw)
}

function Patch-AscGrow([int]$off, [string]$old, [string]$neu, [int]$cap) {
  # cap = max content length; need cap+1 bytes total with null; verify zeros after old null up to cap
  if ($neu.Length -gt $cap) { throw 'ascgrow len' }
  $got = [Text.Encoding]::ASCII.GetString($b, $off, $old.Length)
  if ($got -ne $old) { throw "ascgrow '$got'" }
  if ($b[$off+$old.Length] -ne 0) { throw 'ascgrow term' }
  for ($i = $old.Length + 1; $i -le $cap; $i++) {
    if ($b[$off+$i] -ne 0) { throw "ascgrow pad at $i" }
  }
  $nb = [Text.Encoding]::ASCII.GetBytes($neu + "`0")
  [Array]::Copy($nb, 0, $b, $off, $nb.Length)
  for ($i = $neu.Length + 1; $i -le $cap; $i++) { $b[$off+$i] = 0 }
  Write-Output ("ASCgrow {0} -> {1}" -f $old, $neu)
}

$Opcoes = U @(0x4F,0x70,0xE7,0xF5,0x65,0x73,0x20,0x41,0x76,0x61,0x6E,0xE7,0x61,0x64,0x61,0x73)
$Preset = U @(0x50,0x72,0x65,0x73,0x65,0x74,0x20,0x64,0x65,0x20,0x56,0xED,0x64,0x65,0x6F)
$LuzDin = U @(0x4C,0x75,0x7A,0x20,0x44,0x69,0x6E,0xE2,0x6D,0x69,0x63,0x61)
$Graf = U @(0x47,0x52,0xC1,0x46,0x49,0x43,0x4F)
$Res = U @(0x52,0x65,0x73,0x6F,0x6C,0x75,0xE7,0xE3,0x6F)
$Min = U @(0x4D,0xED,0x6E,0x69,0x6D,0x6F)
$Max = U @(0x4D,0xE1,0x78,0x69,0x6D,0x6F)
$Baixo = U @(0x42,0x61,0x69,0x78,0x6F)
$Medio = U @(0x4D,0xE9,0x64,0x69,0x6F)

foreach ($p in @(
  @{O=0x54C68A;A='PB Configuration';B='Config FrontLine'},
  @{O=0x54CC8C;A='Advanced Settings';B=$Opcoes},
  @{O=0x54C8E4;A='Vertical Sync.';B='Sinc. Vertical'},
  @{O=0x54CF94;A='Windowed Fullscreen';B='Tela Cheia Janela'},
  @{O=0x54CA28;A='Video Quick Setting';B=$Preset},
  @{O=0x54C7C4;A='Normal Mapping';B='Mapeam. Normal'},
  @{O=0x54C784;A='Dynamic Lighting';B=$LuzDin},
  @{O=0x54CB70;A='Bullet Trace';B='Rastro Bala'},
  @{O=0x54C89C;A='Tri-Linear Filtering';B='Filtro Tri-Linear'},
  @{O=0x54CBA8;A='Terrain Effect';B='Efeito Terreno'},
  @{O=0x54C9E4;A='ANTI ALIAS OPTION';B='ANTI-ALIASING'},
  @{O=0x54CCD0;A='GRAPHIC';B=$Graf},
  @{O=0x54CE4C;A='Rim Light';B='Luz Borda'},
  @{O=0x54CE80;A='ImageBasedLight';B='Luz Base Imagem'},
  @{O=0x54CEE8;A='ScreenSpaceReflection';B='Reflexo de Tela'},
  @{O=0x54CC30;A='Specular';B='Brilho'},
  @{O=0x54C704;A='Cancel';B='Voltar'},
  @{O=0x54C730;A='Resolution';B=$Res},
  @{O=0x54CF60;A='Fullscreen';B='Tela Cheia'},
  @{O=0x54CFDC;A='Windowed';B='Janela'},
  @{O=0x54CA70;A='Lowest';B=$Min},
  @{O=0x54CB14;A='Highest';B=$Max},
  @{O=0x54C86C;A='Texture';B='Textura'},
  @{O=0x54C820;A='Shadow';B='Sombra'},
  @{O=0x54CF34;A='Screen';B='Tela'},
  @{O=0x54CE14;A='HDR && Bloom';B='HDR e Bloom'},
  @{O=0x54C6DC;A='Save';B='OK'},
  @{O=0x54CAEC;A='High';B='Alto'},
  @{O=0x54CB44;A='Custom';B='Manual'},
  @{O=0x54CAC4;A='Mid';B='Med'}
)) { Patch-U16 $p.O $p.A $p.B }

# NAO expandir Low/Mid — quebra o dialog Win32

Patch-Asc 0x3B8EA0 'Point Blank' 'FrontLine'
Patch-Asc 0x3B8EAC '%s Configuration' '%s Configuracao'

Patch-Asc 0x54F348 'None' 'Nada'
# Low (3) nao cabe Baixo no slot ASCII
Patch-Asc 0x54F361 'Middle' 'Medio'
Patch-Asc 0x54F370 'High' 'Alto'
Patch-Asc 0x54F37D 'High' 'Alto'
Patch-Asc 0x54F38A 'Middle' 'Medio'
Patch-Asc 0x54F3A5 'Lowest' 'Minimo'
Patch-Asc 0x54F3C0 'Middle' 'Medio'
Patch-Asc 0x54F3CF 'High' 'Alto'

Patch-Asc 0x3B9218 'None' 'Nada'
Patch-Asc 0x3B91E8 'CUSTOM' 'MANUAL'

[IO.File]::WriteAllBytes($path, $b)

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $path; $psi.WorkingDirectory = $client; $psi.UseShellExecute = $false
$proc = New-Object System.Diagnostics.Process; $proc.StartInfo = $psi
[void]$proc.Start(); Start-Sleep -Milliseconds 1800
if (-not $proc.HasExited -and $proc.MainWindowHandle -ne [IntPtr]::Zero) {
  Write-Output ("SUCCESS '{0}'" -f $proc.MainWindowTitle)
  Stop-Process -Id $proc.Id -Force
} else {
  Write-Output 'FAIL restore'
  if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
  Start-Sleep -Milliseconds 300
  Copy-Item $bak $path -Force
}
