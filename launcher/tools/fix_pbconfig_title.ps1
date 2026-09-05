# Ajuste: ASCII com null-pad (nao espaco) + Low restantes se possivel
$ErrorActionPreference = 'Stop'
$client = 'C:\Users\pbcai\Downloads\source\client'
$path = Join-Path $client 'PBConfig.exe'
Get-Process -Name 'PBConfig' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 300

$b = [IO.File]::ReadAllBytes($path)

function Patch-AscNull([int]$off, [string]$old, [string]$neu) {
  $got = [Text.Encoding]::ASCII.GetString($b, $off, $old.Length)
  if ($got -ne $old -and $got.TrimEnd([char]0) -ne $old.TrimEnd()) {
    # allow already patched
    $cur = ''
    for ($i = 0; $i -lt $old.Length; $i++) {
      if ($b[$off+$i] -eq 0) { break }
      $cur += [char]$b[$off+$i]
    }
    if ($cur -eq $neu) { Write-Output ("skip already {0}" -f $neu); return }
    # if current is space-padded version of neu or old target
  }
  if ($neu.Length -gt $old.Length) { throw "longo $neu" }
  if ($b[$off+$old.Length] -ne 0) { throw 'no term' }
  $bytes = New-Object byte[] $old.Length
  $nb = [Text.Encoding]::ASCII.GetBytes($neu)
  [Array]::Copy($nb, 0, $bytes, 0, $nb.Length)
  # rest already 0
  [Array]::Copy($bytes, 0, $b, $off, $bytes.Length)
  Write-Output ("ASC '{0}' -> '{1}'" -f $old, $neu)
}

# Fix title (currently space-padded FrontLine)
# Read current
$t = -join (0..10 | ForEach-Object { if ($b[0x3B8EA0+$_] -eq 0) { '' } else { [char]$b[0x3B8EA0+$_] } })
Write-Output ("title now=[$t]")

# Force write FrontLine + nulls
$nb = [Text.Encoding]::ASCII.GetBytes('FrontLine')
for ($i = 0; $i -lt 11; $i++) {
  $b[0x3B8EA0+$i] = if ($i -lt $nb.Length) { $nb[$i] } else { 0 }
}
$cfg = [Text.Encoding]::ASCII.GetBytes('%s Configuracao')
for ($i = 0; $i -lt 16; $i++) {
  $b[0x3B8EAC+$i] = if ($i -lt $cfg.Length) { $cfg[$i] } else { 0 }
}
Write-Output 'title fixed null-pad'

# Fix space-padded ASCII Middle/High/etc to null-pad
function Fix-Slot([int]$off, [int]$slotLen, [string]$neu) {
  if ($neu.Length -gt $slotLen) { throw 'slot' }
  $nb = [Text.Encoding]::ASCII.GetBytes($neu)
  for ($i = 0; $i -lt $slotLen; $i++) {
    $b[$off+$i] = if ($i -lt $nb.Length) { $nb[$i] } else { 0 }
  }
  # keep original null at slotLen position - should already be 0
  Write-Output ("fix 0x{0:X} -> {1}" -f $off, $neu)
}

Fix-Slot 0x54F348 4 'Nada'
Fix-Slot 0x54F361 6 'Medio'
Fix-Slot 0x54F370 4 'Alto'
Fix-Slot 0x54F37D 4 'Alto'
Fix-Slot 0x54F38A 6 'Medio'
Fix-Slot 0x54F3A5 6 'Minimo'
Fix-Slot 0x54F3C0 6 'Medio'
Fix-Slot 0x54F3CF 4 'Alto'
Fix-Slot 0x3B9218 4 'Nada'
Fix-Slot 0x3B91E8 6 'MANUAL'

# Low slots 3 chars - leave Low OR try nothing

[IO.File]::WriteAllBytes($path, $b)

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $path; $psi.WorkingDirectory = $client; $psi.UseShellExecute = $false
$p = New-Object System.Diagnostics.Process; $p.StartInfo = $psi
[void]$p.Start(); Start-Sleep -Milliseconds 1500
Write-Output ("title='{0}' hwnd={1}" -f $p.MainWindowTitle, $p.MainWindowHandle)
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
