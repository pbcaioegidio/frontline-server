$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$a = [Text.Encoding]::ASCII.GetString($b)
$u = [Text.Encoding]::Unicode.GetString($b)
$keys = @('Lowest','Highest','Middle','None','AUTO','DX11(BETA)','Point Blank Configuration','Point Blank','High','Low','Save')
$out = @()
foreach ($k in $keys) {
  $ia = $a.IndexOf($k)
  $iu = $u.IndexOf($k)
  $out += ("A {0}={1} U={2}" -f $k, $(if ($ia -ge 0) { '0x{0:X}' -f $ia } else { 'MISS' }), $(if ($iu -ge 0) { '0x{0:X}' -f ($iu * 2) } else { 'MISS' }))
}
$out += '--- region ---'
$out += ([Text.Encoding]::Unicode.GetString($b, 0x54CA60, 0x200) -replace '[^\u0020-\u007E\u00C0-\u00FF]', '|')

# Find all Uni occurrences of Middle by IndexOf loop
$start = 0
while (($i = $u.IndexOf('Middle', $start)) -ge 0) {
  $out += ('Middle@0x{0:X} prev={1}' -f ($i*2), [int]$u[[Math]::Max(0,$i-1)])
  $start = $i + 1
  if ($start -gt 20) { break }
}
$start = 0
$c = 0
while (($i = $u.IndexOf('None', $start)) -ge 0 -and $c -lt 15) {
  $prev = if ($i -gt 0) { $u[$i-1] } else { [char]0 }
  $next = if ($i+4 -lt $u.Length) { $u[$i+4] } else { [char]0 }
  if (-not [char]::IsLetterOrDigit($prev) -and ($next -eq [char]0 -or -not [char]::IsLetterOrDigit($next))) {
    $out += ('None@0x{0:X}' -f ($i*2))
    $c++
  }
  $start = $i + 1
}

# Window title - search ProductName etc in version resource
foreach ($k in @('ProductName','FileDescription','InternalName','OriginalFilename','PointBlank','PBConfig')) {
  $iu = $u.IndexOf($k)
  $out += ('{0} U={1}' -f $k, $(if ($iu -ge 0) { '0x{0:X}' -f ($iu*2) } else { 'MISS' }))
  if ($iu -ge 0) {
    $out += ([Text.Encoding]::Unicode.GetString($b, $iu*2, 120) -replace '[^\u0020-\u007E]', '.')
  }
}

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_ascii_en.txt', $out)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_ascii_en.txt'
