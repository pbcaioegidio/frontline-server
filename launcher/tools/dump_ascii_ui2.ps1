$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$sb = New-Object System.Text.StringBuilder
function Dump([int]$start, [int]$len) {
  [void]$sb.AppendLine(("--- 0x{0:X} ---" -f $start))
  $cur = New-Object System.Text.StringBuilder
  $off0 = -1
  for ($i = $start; $i -lt ($start + $len); $i++) {
    $c = $b[$i]
    if ($c -ge 32 -and $c -lt 127) {
      if ($cur.Length -eq 0) { $off0 = $i }
      [void]$cur.Append([char]$c)
    } else {
      if ($cur.Length -ge 2) {
        [void]$sb.AppendLine(("0x{0:X8} [{1}] {2}" -f $off0, $cur.Length, $cur.ToString()))
      }
      $cur.Clear() | Out-Null
    }
  }
}
Dump 0x54F300 0x180
Dump 0x3B91C0 0x100
Dump 0x3B8E80 0xA0
Dump 0x40B600 0xA0
# also search Highest High as ascii C strings near Middle
$hay = [Text.Encoding]::ASCII.GetString($b)
foreach ($k in @('Highest','High','Middle','Low','Lowest','None','AUTO','DX11(BETA)','Point Blank','Configuration','Specular','Texture','Shadow','Save')) {
  $i = 0
  $n = 0
  while (($i = $hay.IndexOf($k, $i)) -ge 0 -and $n -lt 6) {
    if ($b[$i + $k.Length] -eq 0) {
      $prev = if ($i -gt 0) { $b[$i-1] } else { 0 }
      $isWord = ($prev -ge 65 -and $prev -le 90) -or ($prev -ge 97 -and $prev -le 122) -or ($prev -ge 48 -and $prev -le 57)
      if (-not $isWord) {
        [void]$sb.AppendLine(("HIT '{0}' @ 0x{1:X8}" -f $k, $i))
        $n++
      }
    }
    $i++
  }
}
[IO.File]::WriteAllText('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_ascii_dump.txt', $sb.ToString())
Write-Output 'ok'
