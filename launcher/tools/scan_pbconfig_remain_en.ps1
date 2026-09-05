# Dump remaining English UI strings in PBConfig (dialog + nearby/runtime)
$ErrorActionPreference = 'Stop'
$path = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($path)
$u = [Text.Encoding]::Unicode.GetString($b)

$needles = @(
  'Point Blank Configuration','Point Blank','PB Configuration','Configuration',
  'Low','Middle','High','Highest','Lowest','None','AUTO','Save','Cancel',
  'Custom','Mid','Full','Window','Graphic','Texture','Shadow','Specular',
  'DX11','DX9','BETA','PhysX','SSAO','Bloom','DirectX',
  'Normal Mapping','Vertical Sync','Fullscreen','Windowed',
  'ANTI','Alias','Resolution','Screen','FPS',
  'Team Band','New'
)

$lines = New-Object System.Collections.Generic.List[string]
foreach ($n in $needles) {
  $start = 0
  $count = 0
  while (($i = $u.IndexOf($n, $start)) -ge 0 -and $count -lt 8) {
    $off = $i * 2
    # measure null-terminated length
    $len = 0
    while (($i + $len) -lt $u.Length -and $u[$i + $len] -ne [char]0 -and $len -lt 64) { $len++ }
    $full = $u.Substring($i, $len)
    # prev char
    $prev = if ($i -gt 0) { $u[$i-1] } else { [char]0 }
    $bound = -not [char]::IsLetterOrDigit($prev)
    if ($bound -and $full.Length -ge $n.Length) {
      $lines.Add(("0x{0:X8} len={1} '{2}'" -f $off, $full.Length, $full))
      $count++
    }
    $start = $i + 1
  }
}

# Also scan dialog region for any remaining ASCII words
$ds = [int](0x54C600/2); $de = [int](0x54D500/2)
$i = $ds
while ($i -lt $de) {
  if ($u[$i] -eq [char]0) { $i++; continue }
  $j = $i
  while ($j -lt $de -and $u[$j] -ne [char]0) { $j++ }
  $s = $u.Substring($i, $j - $i)
  if ($s -match '^[A-Za-z].{1,40}$' -and $s -notmatch 'msctls_|Tahoma|Button|Static|Combo|Edit') {
    $lines.Add(("DLG 0x{0:X8} [{1}] {2}" -f ($i*2), $s.Length, $s))
  }
  $i = $j + 1
}

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_remain_en.txt', $lines, [Text.UTF8Encoding]::new($false))
Write-Output ("wrote {0} lines" -f $lines.Count)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_remain_en.txt' -Encoding UTF8 | Select-Object -First 80
