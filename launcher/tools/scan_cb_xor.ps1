# XOR single-byte search for key needles in main binaries only
$ErrorActionPreference = 'Stop'
$files = @(
  'C:\Users\pbcai\Downloads\source\client\PointBlank.exe',
  'C:\Users\pbcai\Downloads\source\client\PointBlank.i3Exec',
  'C:\Users\pbcai\Downloads\source\client\EditGameDlg.dll',
  'C:\Users\pbcai\Downloads\source\client\i3TDK.dll',
  'C:\Users\pbcai\Downloads\source\client\CrashTrace.dll'
)
$needles = @('CheatBlocker','CHEAT_BLOCKER','CB.exe','Initialize Load Failed','Load Failed')

function Find-Xor([byte[]]$data, [byte[]]$plain) {
  $hits = @()
  for ($xor = 0; $xor -le 255; $xor++) {
    $pat = New-Object byte[] $plain.Length
    for ($j = 0; $j -lt $plain.Length; $j++) { $pat[$j] = $plain[$j] -bxor [byte]$xor }
    # latin1 string indexof
    $enc = [Text.Encoding]::GetEncoding(28591)
    $idx = $enc.GetString($data).IndexOf($enc.GetString($pat))
    if ($idx -ge 0) { $hits += @{ Xor = $xor; Off = $idx } }
  }
  return $hits
}

$out = New-Object System.Collections.Generic.List[string]
foreach ($path in $files) {
  if (-not (Test-Path $path)) { continue }
  $data = [IO.File]::ReadAllBytes($path)
  $name = Split-Path $path -Leaf
  Write-Output ("scanning {0} ({1} MB)..." -f $name, [math]::Round($data.Length/1MB,2))
  foreach ($n in $needles) {
    $plain = [Text.Encoding]::ASCII.GetBytes($n)
    $hits = Find-Xor $data $plain
    foreach ($h in $hits) {
      $out.Add(("{0} '{1}' xor=0x{2:X2} @ 0x{3:X}" -f $name, $n, $h.Xor, $h.Off))
    }
    # UTF16LE xor same byte on each
    $uplain = [Text.Encoding]::Unicode.GetBytes($n)
    for ($xor = 0; $xor -le 255; $xor++) {
      $pat = New-Object byte[] $uplain.Length
      for ($j = 0; $j -lt $uplain.Length; $j++) { $pat[$j] = $uplain[$j] -bxor [byte]$xor }
      $enc = [Text.Encoding]::GetEncoding(28591)
      $idx = $enc.GetString($data).IndexOf($enc.GetString($pat))
      if ($idx -ge 0) {
        $out.Add(("{0} UNI '{1}' xor=0x{2:X2} @ 0x{3:X}" -f $name, $n, $xor, $idx))
      }
    }
  }
}
[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\cb_xor_hits.txt', $out)
Write-Output "----"
Write-Output ("total hits={0}" -f $out.Count)
$out | ForEach-Object { Write-Output $_ }
