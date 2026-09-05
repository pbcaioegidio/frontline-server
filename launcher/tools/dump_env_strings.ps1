$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$a = [Text.Encoding]::ASCII.GetString($b)
$start = 0x3B8C00
$end = 0x3B9300
$cur = New-Object System.Text.StringBuilder
$off0 = -1
$lines = New-Object System.Collections.Generic.List[string]
for ($i = $start; $i -lt $end; $i++) {
  $c = $b[$i]
  if ($c -ge 32 -and $c -lt 127) {
    if ($cur.Length -eq 0) { $off0 = $i }
    [void]$cur.Append([char]$c)
  } else {
    if ($cur.Length -ge 3) { $lines.Add(("{0:X} {1}" -f $off0, $cur.ToString())) }
    $cur.Clear() | Out-Null
  }
}
[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_env_strings.txt', $lines)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_env_strings.txt'
