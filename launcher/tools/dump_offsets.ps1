$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)

function Dump-At([int]$off, [int]$bytes = 80) {
  $chunk = New-Object byte[] $bytes
  [Array]::Copy($b, $off, $chunk, 0, $bytes)
  $hex = ($chunk | ForEach-Object { '{0:X2}' -f $_ }) -join ' '
  $uni = [Text.Encoding]::Unicode.GetString($chunk)
  Write-Output ("--- 0x{0:X} ---" -f $off)
  Write-Output $hex
  Write-Output ("UNI: " + ($uni -replace '[^\x20-\x7E]', '.'))
}

Dump-At 0x54C68A
Dump-At 0x54CC8C
Dump-At 0x54C8E4
Dump-At 0x54CA28
Dump-At 0x54C704

# Search all occurrences of "Advanced Settings" in unicode
$pat = [Text.Encoding]::Unicode.GetBytes('Advanced Settings')
$count = 0
for ($i = 0; $i -le $b.Length - $pat.Length; $i++) {
  $ok = $true
  for ($j = 0; $j -lt $pat.Length; $j++) {
    if ($b[$i+$j] -ne $pat[$j]) { $ok = $false; break }
  }
  if ($ok) {
    Write-Output ("HIT Advanced Settings @ 0x{0:X}" -f $i)
    $count++
    if ($count -ge 5) { break }
  }
}
Write-Output ("total shown hits=$count")

# Also check dialog title via version / STRINGTABLE - use strings.exe if present
Write-Output '==== window title candidates ===='
$u = [Text.Encoding]::Unicode.GetString($b)
$idx = 0
while (($idx = $u.IndexOf('Configuration', $idx)) -ge 0) {
  $from = [Math]::Max(0, $idx - 20)
  $snip = $u.Substring($from, [Math]::Min(60, $u.Length - $from)) -replace '[^\x20-\x7E]', '.'
  Write-Output ("0x{0:X}: {1}" -f ($idx*2), $snip)
  $idx++
  if ($idx -gt 20) { break }
}
