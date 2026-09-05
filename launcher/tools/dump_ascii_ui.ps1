$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$out = @()

function Dump-AsciiRegion([int]$start, [int]$len, [string]$label) {
  $chars = New-Object System.Collections.Generic.List[string]
  $cur = New-Object System.Text.StringBuilder
  $off0 = -1
  for ($i = $start; $i -lt $start + $len -and $i -lt $b.Length; $i++) {
    $c = $b[$i]
    if ($c -ge 32 -and $c -lt 127) {
      if ($cur.Length -eq 0) { $off0 = $i }
      [void]$cur.Append([char]$c)
    } else {
      if ($cur.Length -ge 2) {
        $chars.Add(("0x{0:X8} [{1}] {2}" -f $off0, $cur.Length, $cur.ToString()))
      }
      $cur.Clear() | Out-Null
    }
  }
  $out.Add("--- $label ---")
  $chars | ForEach-Object { $out.Add($_) }
}

Dump-AsciiRegion 0x54F300 0x200 'combo quality ~54F3'
Dump-AsciiRegion 0x3B91C0 0x120 'AUTO/DX/None ~3B91'
Dump-AsciiRegion 0x3B8E80 0x80 'Point Blank ~3B8E'
Dump-AsciiRegion 0x40B600 0x80 'High/Low ~40B6'
Dump-AsciiRegion 0x54EE00 0x200 'version info'

# Also find High Middle Low Lowest as consecutive combo items
foreach ($k in @('Highest','High','Middle','Low','Lowest','None','AUTO','Custom')) {
  $a = [Text.Encoding]::ASCII.GetString($b)
  $i = 0
  $n = 0
  while (($i = $a.IndexOf($k + [char]0, $i)) -ge 0 -and $n -lt 5) {
    # IndexOf with null might not work in .NET string easily
    $i++
  }
}

# manual null-terminated ASCII search
function Find-Asc([string]$text) {
  $pat = [Text.Encoding]::ASCII.GetBytes($text + "`0")
  $hits = @()
  $limit = $b.Length - $pat.Length
  # use IndexOf on ascii string with sentinel
  $hay = [Text.Encoding]::ASCII.GetString($b)
  $start = 0
  while (($i = $hay.IndexOf($text, $start)) -ge 0) {
    if (($i + $text.Length) -lt $b.Length -and $b[$i + $text.Length] -eq 0) {
      if ($i -eq 0 -or -not (($b[$i-1] -ge 48 -and $b[$i-1] -le 57) -or ($b[$i-1] -ge 65 -and $b[$i-1] -le 90) -or ($b[$i-1] -ge 97 -and $b[$i-1] -le 122))) {
        $hits += $i
      }
    }
    $start = $i + 1
    if ($hits.Count -ge 10) { break }
  }
  return $hits
}

$out.Add('--- exact ASCII null-term ---')
foreach ($k in @('Lowest','Highest','Middle','High','Low','None','AUTO','DX11(BETA)','DX9','Custom','Point Blank Configuration','Point Blank','Save','Cancel','Specular','Texture','Shadow')) {
  $hits = Find-Asc $k
  foreach ($h in $hits) { $out.Add("'{0}' @ 0x{1:X8}" -f $k, $h) }
  if ($hits.Count -eq 0) { $out.Add("MISS '{0}'" -f $k) }
}

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_ascii_dump.txt', $out)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_ascii_dump.txt'
