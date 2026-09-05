# Patch PBConfig.exe dialog captions to PT-BR (UTF-16 in-place)
$ErrorActionPreference = 'Stop'
$path = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$bak = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe.bak-pt'
Copy-Item $bak $path -Force
Write-Output 'Restaurado do backup'

$b = [IO.File]::ReadAllBytes($path)

function Get-U16At([byte[]]$bytes, [int]$off, [int]$chars) {
  return [Text.Encoding]::Unicode.GetString($bytes, $off, $chars * 2)
}

function Test-NullTerm([byte[]]$bytes, [int]$off, [int]$chars) {
  $z = $off + $chars * 2
  return ($z + 1 -lt $bytes.Length -and $bytes[$z] -eq 0 -and $bytes[$z+1] -eq 0)
}

function Find-ExactU16([byte[]]$bytes, [string]$text, [int]$minOff, [int]$maxOff) {
  $u = [Text.Encoding]::Unicode.GetString($bytes)
  $startChar = [int][Math]::Ceiling($minOff / 2.0)
  $endChar = [int][Math]::Floor($maxOff / 2.0)
  $i = $startChar
  while ($i -lt $endChar) {
    $i = $u.IndexOf($text, $i)
    if ($i -lt 0 -or $i -ge $endChar) { return -1 }
    $off = $i * 2
    if (Test-NullTerm $bytes $off $text.Length) {
      if ($i -gt 0 -and [char]::IsLetterOrDigit($u[$i - 1])) { $i++; continue }
      return $off
    }
    $i++
  }
  return -1
}

function Patch-At([byte[]]$bytes, [int]$off, [string]$old, [string]$new) {
  $got = Get-U16At $bytes $off $old.Length
  if ($got -ne $old) {
    Write-Output ("SKIP mismatch 0x{0:X}: '{1}'" -f $off, $got)
    return
  }
  if ($new.Length -gt $old.Length) {
    Write-Output ("SKIP longo [{0}>{1}] '{2}' -> '{3}'" -f $new.Length, $old.Length, $old, $new)
    return
  }
  $nb = [Text.Encoding]::Unicode.GetBytes($new)
  [Array]::Copy($nb, 0, $bytes, $off, $nb.Length)
  for ($c = $new.Length; $c -lt $old.Length; $c++) {
    $bytes[$off + $c * 2] = 0
    $bytes[$off + $c * 2 + 1] = 0
  }
  Write-Output ("OK 0x{0:X8} '{1}' -> '{2}'" -f $off, $old, $new)
}

function Patch-Str([byte[]]$bytes, [string]$old, [string]$new, [int]$minOff, [int]$maxOff) {
  $off = Find-ExactU16 $bytes $old $minOff $maxOff
  if ($off -lt 0) {
    Write-Output ("SKIP nao achou: '{0}'" -f $old)
    return
  }
  Patch-At $bytes $off $old $new
}

# Unicode via char codes (avoid script encoding issues)
function U([int[]]$codes) {
  return -join ($codes | ForEach-Object { [char]$_ })
}

$OpcoesAvancadas = U @(0x4F,0x70,0xE7,0xF5,0x65,0x73,0x20,0x41,0x76,0x61,0x6E,0xE7,0x61,0x64,0x61,0x73) # Opcoes Avancadas 16
$PresetVideo     = U @(0x50,0x72,0x65,0x73,0x65,0x74,0x20,0x64,0x65,0x20,0x56,0xED,0x64,0x65,0x6F) # Preset de Video 15
$LuzDinamica     = U @(0x4C,0x75,0x7A,0x20,0x44,0x69,0x6E,0xE2,0x6D,0x69,0x63,0x61) # Luz Dinamica 12
$Grafico         = U @(0x47,0x52,0xC1,0x46,0x49,0x43,0x4F) # GRAFICO 7
$Resolucao       = U @(0x52,0x65,0x73,0x6F,0x6C,0x75,0xE7,0xE3,0x6F) # Resolucao 9
$Minimo          = U @(0x4D,0xED,0x6E,0x69,0x6D,0x6F) # Minimo 6
$Maximo          = U @(0x4D,0xE1,0x78,0x69,0x6D,0x6F) # Maximo 6
$Medio           = U @(0x4D,0x65,0x64) # Med 3 (Mid=3)

Write-Output ("len check OpcoesAvancadas=$($OpcoesAvancadas.Length) Grafico=$($Grafico.Length) Minimo=$($Minimo.Length)")

$dlg0 = 0x54C600
$dlg1 = 0x54D400

$pairs = @(
  @{ O = 'PB Configuration'; N = 'Config FrontLine' },
  @{ O = 'Advanced Settings'; N = $OpcoesAvancadas },
  @{ O = 'Vertical Sync.'; N = 'Sinc. Vertical' },
  @{ O = 'Windowed Fullscreen'; N = 'Tela Cheia Janela' },
  @{ O = 'Video Quick Setting'; N = $PresetVideo },
  @{ O = 'Normal Mapping'; N = 'Mapeam. Normal' },
  @{ O = 'Dynamic Lighting'; N = $LuzDinamica },
  @{ O = 'Bullet Trace'; N = 'Rastro Bala' },
  @{ O = 'Tri-Linear Filtering'; N = 'Filtro Tri-Linear' },
  @{ O = 'Terrain Effect'; N = 'Efeito Terreno' },
  @{ O = 'ANTI ALIAS OPTION'; N = 'ANTI-ALIASING' },
  @{ O = 'GRAPHIC'; N = $Grafico },
  @{ O = 'Rim Light'; N = 'Luz Borda' },
  @{ O = 'ImageBasedLight'; N = 'Luz Base Imagem' },
  @{ O = 'ScreenSpaceReflection'; N = 'Reflexo de Tela' },
  @{ O = 'Specular'; N = 'Brilho' },
  @{ O = 'Cancel'; N = 'Voltar' },
  @{ O = 'Resolution'; N = $Resolucao },
  @{ O = 'Fullscreen'; N = 'Tela Cheia' },
  @{ O = 'Windowed'; N = 'Janela' },
  @{ O = 'Lowest'; N = $Minimo },
  @{ O = 'Highest'; N = $Maximo },
  @{ O = 'Texture'; N = 'Textura' },
  @{ O = 'Shadow'; N = 'Sombra' },
  @{ O = 'Screen'; N = 'Tela' },
  @{ O = 'High'; N = 'Alto' },
  @{ O = 'Custom'; N = 'Manual' },
  @{ O = 'Mid'; N = $Medio },
  @{ O = 'HDR & Bloom'; N = 'HDR e Bloom' }
)

foreach ($p in $pairs) {
  Patch-Str $b $p.O $p.N $dlg0 $dlg1
}

# Low=3 -> use 'Low' keep or 'Baix' no; keep Low / use 'Lo' nonsense. Try 'Min' already Lowest.
# None=4 -> 'Nada'=4
Patch-Str $b 'None' 'Nada' $dlg0 $dlg1

# Save=4 -> 'OK'
Patch-Str $b 'Save' 'OK' $dlg0 $dlg1

# Low - leave English (Baixo too long) or patch to 'Bai' no. Use 'Lo ' 
# Actually radio might show "Low" - use "Low" unchanged; optional 'Baixo' needs expand.

[IO.File]::WriteAllBytes($path, $b)
Write-Output 'PBConfig.exe patch PT aplicado.'
