$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)
$u = [Text.Encoding]::Unicode.GetString($b)

# Scan dialog resource region for UI captions
$start = [int](0x54C600 / 2)
$end = [int](0x54D400 / 2)
$i = $start
$items = @()
while ($i -lt $end) {
  if ($u[$i] -eq [char]0) { $i++; continue }
  $j = $i
  while ($j -lt $end -and $u[$j] -ne [char]0) { $j++ }
  $s = $u.Substring($i, $j - $i)
  if ($s.Length -ge 2 -and $s -match '^[\x20-\x7E\&\.\-]+$' -and $s -notmatch 'msctls_|Button|Static|ComboBox|Edit|SysList') {
    # capacity = chars until next non-null after padding? measure contiguous reserved: from start until next printable string past nulls that look like template
    $cap = $j - $i
    # look ahead at padding nulls count
    $pad = 0
    $k = $j
    while ($k -lt $end -and $u[$k] -eq [char]0) { $pad++; $k++ }
    $items += [pscustomobject]@{ Off = $i*2; Len = $cap; Pad = $pad; Text = $s }
  }
  $i = $j + 1
}
$items | ForEach-Object { Write-Output ("0x{0:X8} len={1} pad={2} | {3}" -f $_.Off, $_.Len, $_.Pad, $_.Text) }
