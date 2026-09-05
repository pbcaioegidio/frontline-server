# Restore PBConfig and re-apply only verified long captions (no short/ambiguous)
$ErrorActionPreference = 'Stop'
$client = 'C:\Users\pbcai\Downloads\source\client'
$path = Join-Path $client 'PBConfig.exe'
$bak = Join-Path $client 'PBConfig.exe.bak-pt'
Copy-Item $bak $path -Force
Write-Output 'Restaurado bak-pt'

$b = [IO.File]::ReadAllBytes($path)

function Patch-At([byte[]]$bytes, [int]$off, [string]$old, [string]$new) {
  $got = [Text.Encoding]::Unicode.GetString($bytes, $off, $old.Length * 2)
  if ($got -ne $old) { throw "mismatch 0x$($off.ToString('X')) got='$got' expected='$old'" }
  if ($new.Length -gt $old.Length) { throw "too long '$new'" }
  $nb = [Text.Encoding]::Unicode.GetBytes($new)
  [Array]::Copy($nb, 0, $bytes, $off, $nb.Length)
  for ($c = $new.Length; $c -lt $old.Length; $c++) {
    $bytes[$off + $c*2] = 0
    $bytes[$off + $c*2 + 1] = 0
  }
  # keep original terminating null at old.Length (already 00 00)
  Write-Output ("OK 0x{0:X} '{1}' -> '{2}'" -f $off, $old, $new)
}

function U([int[]]$codes) { -join ($codes | ForEach-Object { [char]$_ }) }

# Exact offsets from hex verification (dialog resource)
$patches = @(
  @{ Off = 0x54C68A; Old = 'PB Configuration'; New = 'Config FrontLine' },
  @{ Off = 0x54CC8C; Old = 'Advanced Settings'; New = (U @(0x4F,0x70,0xE7,0xF5,0x65,0x73,0x20,0x41,0x76,0x61,0x6E,0xE7,0x61,0x64,0x61,0x73)) }, # Opcoes Avancadas
  @{ Off = 0x54C8E4; Old = 'Vertical Sync.'; New = 'Sinc. Vertical' },
  @{ Off = 0x54CF94; Old = 'Windowed Fullscreen'; New = 'Tela Cheia Janela' },
  @{ Off = 0x54CA28; Old = 'Video Quick Setting'; New = (U @(0x50,0x72,0x65,0x73,0x65,0x74,0x20,0x64,0x65,0x20,0x56,0xED,0x64,0x65,0x6F)) },
  @{ Off = 0x54C7C4; Old = 'Normal Mapping'; New = 'Mapeam. Normal' },
  @{ Off = 0x54C784; Old = 'Dynamic Lighting'; New = (U @(0x4C,0x75,0x7A,0x20,0x44,0x69,0x6E,0xE2,0x6D,0x69,0x63,0x61)) },
  @{ Off = 0x54CB70; Old = 'Bullet Trace'; New = 'Rastro Bala' },
  @{ Off = 0x54C89C; Old = 'Tri-Linear Filtering'; New = 'Filtro Tri-Linear' },
  @{ Off = 0x54CBA8; Old = 'Terrain Effect'; New = 'Efeito Terreno' },
  @{ Off = 0x54C9E4; Old = 'ANTI ALIAS OPTION'; New = 'ANTI-ALIASING' },
  @{ Off = 0x54CCD0; Old = 'GRAPHIC'; New = (U @(0x47,0x52,0xC1,0x46,0x49,0x43,0x4F)) },
  @{ Off = 0x54CE4C; Old = 'Rim Light'; New = 'Luz Borda' },
  @{ Off = 0x54CE80; Old = 'ImageBasedLight'; New = 'Luz Base Imagem' },
  @{ Off = 0x54CEE8; Old = 'ScreenSpaceReflection'; New = 'Reflexo de Tela' },
  @{ Off = 0x54CC30; Old = 'Specular'; New = 'Brilho' },
  @{ Off = 0x54C704; Old = 'Cancel'; New = 'Voltar' },
  @{ Off = 0x54C730; Old = 'Resolution'; New = (U @(0x52,0x65,0x73,0x6F,0x6C,0x75,0xE7,0xE3,0x6F)) },
  @{ Off = 0x54CF60; Old = 'Fullscreen'; New = 'Tela Cheia' },
  @{ Off = 0x54CFDC; Old = 'Windowed'; New = 'Janela' },
  @{ Off = 0x54CA70; Old = 'Lowest'; New = (U @(0x4D,0xED,0x6E,0x69,0x6D,0x6F)) },
  @{ Off = 0x54CB14; Old = 'Highest'; New = (U @(0x4D,0xE1,0x78,0x69,0x6D,0x6F)) },
  @{ Off = 0x54C86C; Old = 'Texture'; New = 'Textura' },
  @{ Off = 0x54C820; Old = 'Shadow'; New = 'Sombra' },
  @{ Off = 0x54CF34; Old = 'Screen'; New = 'Tela' },
  @{ Off = 0x54CE14; Old = 'HDR && Bloom'; New = 'HDR e Bloom' }
  # Nao patchar: Save, High, Mid, Low, Custom — quebram o dialog
)

foreach ($p in $patches) {
  Patch-At $b $p.Off $p.Old $p.New
}

[IO.File]::WriteAllBytes($path, $b)

# Launch test
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $path
$psi.WorkingDirectory = $client
$psi.UseShellExecute = $false
$proc = New-Object System.Diagnostics.Process
$proc.StartInfo = $psi
[void]$proc.Start()
Start-Sleep -Milliseconds 1500
if ($proc.HasExited) {
  Write-Output ("FAIL exited $($proc.ExitCode)")
} elseif ($proc.MainWindowHandle -eq 0) {
  Write-Output 'FAIL sem janela (hwnd=0) — restaurando ingles'
  Stop-Process -Id $proc.Id -Force
  Copy-Item $bak $path -Force
} else {
  Write-Output ("OK janela title='$($proc.MainWindowTitle)' hwnd=$($proc.MainWindowHandle)")
  Stop-Process -Id $proc.Id -Force
}

# cleanup test copy
Remove-Item (Join-Path $client 'PBConfig_orig_test.exe') -Force -ErrorAction SilentlyContinue
