$ErrorActionPreference = 'Stop'
$path = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($path)

function Patch-At([byte[]]$bytes, [int]$off, [string]$old, [string]$new) {
  $got = [Text.Encoding]::Unicode.GetString($bytes, $off, $old.Length * 2)
  if ($got -ne $old) { throw "mismatch at 0x$($off.ToString('X')): '$got'" }
  if ($new.Length -gt $old.Length) { throw "too long" }
  $nb = [Text.Encoding]::Unicode.GetBytes($new)
  [Array]::Copy($nb, 0, $bytes, $off, $nb.Length)
  for ($c = $new.Length; $c -lt $old.Length; $c++) {
    $bytes[$off + $c*2] = 0
    $bytes[$off + $c*2 + 1] = 0
  }
  Write-Output ("OK '{0}' -> '{1}'" -f $old, $new)
}

# Dialog shows & as &&
Patch-At $b 0x54CE14 'HDR && Bloom' 'HDR e Bloom'

# Low=3: use 'Bai' no; keep or 'Lo '
# Actually pad allows? Low\0 then nulls - capacity is still 3 for display string length in resource
# Leave Low / Mid already Med / High Alto

# Try patch Low -> use fullwidth? no.

[IO.File]::WriteAllBytes($path, $b)
Write-Output done
