$path = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($path)
$u = [Text.Encoding]::Unicode.GetString($b)

function Find-AllExact([string]$text) {
  $results = @()
  $start = 0
  while (($i = $u.IndexOf($text, $start)) -ge 0) {
    $prevOk = ($i -eq 0) -or (-not [char]::IsLetterOrDigit($u[$i-1]))
    $nextNull = ($i + $text.Length -lt $u.Length) -and ($u[$i + $text.Length] -eq [char]0)
    if ($prevOk -and $nextNull) {
      $results += ($i * 2)
    }
    $start = $i + 1
  }
  return $results
}

$keys = @('Lowest','Highest','Middle','High','Low','None','AUTO','Auto','Save','DX11(BETA)','DX11','DX9','Custom','Med','Mid')
$out = @()
foreach ($k in $keys) {
  $offs = Find-AllExact $k
  foreach ($o in $offs) {
    $out += ("{0} @ 0x{1:X8}" -f $k, $o)
  }
}

# Search title strings
foreach ($k in @('Point Blank Configuration','PointBlank Configuration','PB Configuration','Configuration')) {
  $offs = Find-AllExact $k
  foreach ($o in $offs) { $out += ("TITLE {0} @ 0x{1:X8}" -f $k, $o) }
  # also non-null-term search
  $i = $u.IndexOf($k)
  if ($i -ge 0) { $out += ("loose TITLE {0} @ 0x{1:X8}" -f $k, ($i*2)) }
}

# Find clustered quality strings - look for Lowest near Middle
$i = 0
while (($i = $u.IndexOf('Lowest', $i)) -ge 0) {
  $snip = $u.Substring([Math]::Max(0,$i-5), [Math]::Min(80, $u.Length - [Math]::Max(0,$i-5))) -replace '[^\x20-\x7E]', '.'
  $out += ("cluster Lowest@0x{0:X}: {1}" -f ($i*2), $snip)
  $i++
  if ($out.Count -gt 40) { break }
}

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_combo_en.txt', $out)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_combo_en.txt'
