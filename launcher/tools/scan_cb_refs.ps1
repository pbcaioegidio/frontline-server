# Busca strings CB em todos binarios do client (<25MB)
$ErrorActionPreference = 'Stop'
$client = 'C:\Users\pbcai\Downloads\source\client'
$needles = @(
  'CheatBlocker','CHEAT_BLOCKER','CB.exe','CB.cbm','Initialize Load Failed',
  'Load Failed Cheat','Cheat Blocker','\\CHEAT_BLOCKER','/CHEAT_BLOCKER'
)
$files = Get-ChildItem $client -File | Where-Object {
  $_.Extension -match '\.(exe|dll)$' -and $_.Length -lt 25MB -and $_.Length -gt 1000
}
$out = New-Object System.Collections.Generic.List[string]
foreach ($f in $files) {
  $b = [IO.File]::ReadAllBytes($f.FullName)
  $a = [Text.Encoding]::ASCII.GetString($b)
  $u = [Text.Encoding]::Unicode.GetString($b)
  foreach ($n in $needles) {
    $ia = $a.IndexOf($n)
    $iu = $u.IndexOf($n)
    if ($ia -ge 0) { $out.Add(("{0} ASCII '{1}' @ 0x{2:X}" -f $f.Name, $n, $ia)) }
    if ($iu -ge 0) { $out.Add(("{0} UNI '{1}' @ 0x{2:X}" -f $f.Name, $n, ($iu*2))) }
  }
}
# tambem i3Exec
$i3 = Join-Path $client 'PointBlank.i3Exec'
if (Test-Path $i3) {
  $b = [IO.File]::ReadAllBytes($i3)
  $a = [Text.Encoding]::ASCII.GetString($b)
  $u = [Text.Encoding]::Unicode.GetString($b)
  foreach ($n in $needles) {
    if ($a.IndexOf($n) -ge 0) { $out.Add(("i3Exec ASCII '{0}'" -f $n)) }
    if ($u.IndexOf($n) -ge 0) { $out.Add(("i3Exec UNI '{0}'" -f $n)) }
  }
}
[IO.File]::WriteAllLines("$client\..\launcher\tools\cb_string_hits.txt", $out)
Write-Output ("hits={0}" -f $out.Count)
$out | ForEach-Object { Write-Output $_ }
