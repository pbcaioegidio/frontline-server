$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)
$u = [Text.Encoding]::Unicode.GetString($b)

# Dump all null-terminated strings in UI cluster (char indices = byte/2)
$startChar = [int](0x54C680 / 2)
$endChar = [int](0x54D200 / 2)
$i = $startChar
while ($i -lt $endChar) {
  if ($u[$i] -eq [char]0) { $i++; continue }
  $j = $i
  while ($j -lt $endChar -and $u[$j] -ne [char]0) { $j++ }
  $s = $u.Substring($i, $j - $i)
  if ($s.Length -ge 2 -and $s -match '^[\x20-\x7E\.\&]+$') {
    Write-Output ("0x{0:X8} [{1}] {2}" -f ($i*2), $s.Length, $s)
  }
  $i = $j + 1
}

Write-Output '==== SIF XOR try ===='
$sif = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\Config\lwsi_En.sif')
# try simple XOR key scan for readable 'Resolution' or similar
foreach ($key in 0x20..0x7F) {
  $decoded = New-Object byte[] ([Math]::Min(64, $sif.Length))
  for ($n=0; $n -lt $decoded.Length; $n++) { $decoded[$n] = $sif[$n] -bxor $key }
  $t = [Text.Encoding]::ASCII.GetString($decoded)
  if ($t -match '[A-Za-z]{4,}') {
    Write-Output ("xor {0:X2}: {1}" -f $key, ($t -replace '[^\x20-\x7E]','.'))
  }
}
