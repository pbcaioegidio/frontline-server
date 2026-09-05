$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)

# Walk UTF-16LE strings in range
$start = 2777900
$len = 4000
$end = $start + $len
$cur = New-Object System.Text.StringBuilder
$i = $start
# align to even
if (($i % 2) -ne 0) { $i++ }
while ($i -lt $end - 1) {
  $lo = $b[$i]
  $hi = $b[$i+1]
  if ($hi -eq 0 -and $lo -ge 32 -and $lo -lt 127) {
    [void]$cur.Append([char]$lo)
  } else {
    if ($cur.Length -ge 2) {
      Write-Output ("0x{0:X} {1}" -f ($i - 2*$cur.Length), $cur.ToString())
    }
    $cur.Clear() | Out-Null
  }
  $i += 2
}

Write-Output "==== SIF HEAD ===="
$sif = 'C:\Users\pbcai\Downloads\source\client\Config\lwsi_En.sif'
$sb = [IO.File]::ReadAllBytes($sif)
Write-Output ("sif size=" + $sb.Length)
# try ascii dump printable
$a = -join ($sb | ForEach-Object { if ($_ -ge 32 -and $_ -lt 127) { [char]$_ } else { '.' } })
Write-Output $a.Substring(0, [Math]::Min(500, $a.Length))

Write-Output "==== config.zpt ===="
Get-Content 'C:\Users\pbcai\Downloads\source\client\config.zpt' -Raw -ErrorAction SilentlyContinue

# Also search Locale folder
Write-Output "==== Locale ===="
Get-ChildItem 'C:\Users\pbcai\Downloads\source\client\Locale' -Recurse -File | Select-Object -First 40 FullName, Length | ForEach-Object { Write-Output ($_.FullName + ' ' + $_.Length) }
