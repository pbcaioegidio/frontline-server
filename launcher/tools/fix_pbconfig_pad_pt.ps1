# Restore + PT com MESMO comprimento (pad espaco) — dialog Win32 para no primeiro \0
$ErrorActionPreference = 'Stop'
$client = 'C:\Users\pbcai\Downloads\source\client'
$path = Join-Path $client 'PBConfig.exe'
$bak = Join-Path $client 'PBConfig.exe.bak-pt'

Get-Process -Name 'PBConfig','PBConfig_orig_test' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 400
Copy-Item $bak $path -Force
Write-Output 'Restaurado ingles'

$b = [IO.File]::ReadAllBytes($path)

function U([int[]]$codes) { -join ($codes | ForEach-Object { [char]$_ }) }

function Pad-Exact([string]$new, [int]$len) {
  if ($new.Length -gt $len) { throw "PT maior: '$new' ($($new.Length)) > $len" }
  return $new.PadRight($len, ' ')
}

function Patch-At([byte[]]$bytes, [int]$off, [string]$old, [string]$newRaw) {
  $got = [Text.Encoding]::Unicode.GetString($bytes, $off, $old.Length * 2)
  if ($got -ne $old) { throw "mismatch 0x$($off.ToString('X')) '$got'" }
  $new = Pad-Exact $newRaw $old.Length
  $nb = [Text.Encoding]::Unicode.GetBytes($new)
  if ($nb.Length -ne $old.Length * 2) { throw 'byte len' }
  [Array]::Copy($nb, 0, $bytes, $off, $nb.Length)
  Write-Output ("OK '{0}' -> '{1}'" -f $old, $new.TrimEnd())
}

$Opcoes = U @(0x4F,0x70,0xE7,0xF5,0x65,0x73,0x20,0x41,0x76,0x61,0x6E,0xE7,0x61,0x64,0x61,0x73) #16
$Preset = U @(0x50,0x72,0x65,0x73,0x65,0x74,0x20,0x64,0x65,0x20,0x56,0xED,0x64,0x65,0x6F) #15
$LuzDin = U @(0x4C,0x75,0x7A,0x20,0x44,0x69,0x6E,0xE2,0x6D,0x69,0x63,0x61) #12
$Graf   = U @(0x47,0x52,0xC1,0x46,0x49,0x43,0x4F) #7
$Res    = U @(0x52,0x65,0x73,0x6F,0x6C,0x75,0xE7,0xE3,0x6F) #9
$Min    = U @(0x4D,0xED,0x6E,0x69,0x6D,0x6F) #6
$Max    = U @(0x4D,0xE1,0x78,0x69,0x6D,0x6F) #6

$patches = @(
  @{ Off = 0x54C68A; Old = 'PB Configuration'; New = 'Config FrontLine' },
  @{ Off = 0x54CC8C; Old = 'Advanced Settings'; New = $Opcoes },
  @{ Off = 0x54C8E4; Old = 'Vertical Sync.'; New = 'Sinc. Vertical' },
  @{ Off = 0x54CF94; Old = 'Windowed Fullscreen'; New = 'Tela Cheia Janela' },
  @{ Off = 0x54CA28; Old = 'Video Quick Setting'; New = $Preset },
  @{ Off = 0x54C7C4; Old = 'Normal Mapping'; New = 'Mapeam. Normal' },
  @{ Off = 0x54C784; Old = 'Dynamic Lighting'; New = $LuzDin },
  @{ Off = 0x54CB70; Old = 'Bullet Trace'; New = 'Rastro Bala' },
  @{ Off = 0x54C89C; Old = 'Tri-Linear Filtering'; New = 'Filtro Tri-Linear' },
  @{ Off = 0x54CBA8; Old = 'Terrain Effect'; New = 'Efeito Terreno' },
  @{ Off = 0x54C9E4; Old = 'ANTI ALIAS OPTION'; New = 'ANTI-ALIASING' },
  @{ Off = 0x54CCD0; Old = 'GRAPHIC'; New = $Graf },
  @{ Off = 0x54CE4C; Old = 'Rim Light'; New = 'Luz Borda' },
  @{ Off = 0x54CE80; Old = 'ImageBasedLight'; New = 'Luz Base Imagem' },
  @{ Off = 0x54CEE8; Old = 'ScreenSpaceReflection'; New = 'Reflexo de Tela' },
  @{ Off = 0x54CC30; Old = 'Specular'; New = 'Brilho' },
  @{ Off = 0x54C704; Old = 'Cancel'; New = 'Voltar' },
  @{ Off = 0x54C730; Old = 'Resolution'; New = $Res },
  @{ Off = 0x54CF60; Old = 'Fullscreen'; New = 'Tela Cheia' },
  @{ Off = 0x54CFDC; Old = 'Windowed'; New = 'Janela' },
  @{ Off = 0x54CA70; Old = 'Lowest'; New = $Min },
  @{ Off = 0x54CB14; Old = 'Highest'; New = $Max },
  @{ Off = 0x54C86C; Old = 'Texture'; New = 'Textura' },
  @{ Off = 0x54C820; Old = 'Shadow'; New = 'Sombra' },
  @{ Off = 0x54CF34; Old = 'Screen'; New = 'Tela' },
  @{ Off = 0x54CE14; Old = 'HDR && Bloom'; New = 'HDR e Bloom' },
  @{ Off = 0x54C6DC; Old = 'Save'; New = 'OK' },
  @{ Off = 0x54CAEC; Old = 'High'; New = 'Alto' },
  @{ Off = 0x54CAC4; Old = 'Mid'; New = 'Med' },
  @{ Off = 0x54CB44; Old = 'Custom'; New = 'Manual' }
)

foreach ($p in $patches) { Patch-At $b $p.Off $p.Old $p.New }
[IO.File]::WriteAllBytes($path, $b)

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $path
$psi.WorkingDirectory = $client
$psi.UseShellExecute = $false
$proc = New-Object System.Diagnostics.Process
$proc.StartInfo = $psi
[void]$proc.Start()
Start-Sleep -Milliseconds 1800
if (-not $proc.HasExited -and $proc.MainWindowHandle -ne [IntPtr]::Zero) {
  Write-Output ("SUCCESS title='$($proc.MainWindowTitle)'")
  Stop-Process -Id $proc.Id -Force
} else {
  Write-Output 'FAIL — voltando ingles puro'
  if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force }
  Start-Sleep -Milliseconds 300
  Copy-Item $bak $path -Force
  # test english works
  $proc2 = New-Object System.Diagnostics.Process
  $proc2.StartInfo = $psi
  [void]$proc2.Start()
  Start-Sleep -Milliseconds 1200
  Write-Output ("english hwnd=$($proc2.MainWindowHandle) title='$($proc2.MainWindowTitle)'")
  if (-not $proc2.HasExited) { Stop-Process -Id $proc2.Id -Force }
}
