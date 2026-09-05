$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)

# Extract readable UTF-16LE strings near the UI cluster (~2778000)
$start = 2777800
$end = [Math]::Min($b.Length - 2, 2780500)
$sb = New-Object System.Text.StringBuilder
$strings = New-Object System.Collections.Generic.List[string]
$cur = New-Object System.Text.StringBuilder
$i = $start
while ($i -lt $end) {
  $c = [BitConverter]::ToUInt16($b, $i)
  if ($c -ge 32 -and $c -lt 127 -or $c -in @(0x00E9,0x00E1,0x00E7,0x00F5,0x00E3)) {
    [void]$cur.Append([char]$c)
  } elseif ($c -eq 0) {
    if ($cur.Length -ge 3) { $strings.Add($cur.ToString()) }
    $cur.Clear() | Out-Null
  } else {
    if ($cur.Length -ge 3) { $strings.Add($cur.ToString()) }
    $cur.Clear() | Out-Null
  }
  $i += 2
}
$strings | Select-Object -Unique | ForEach-Object { Write-Output $_ }

Write-Output "==== CONFIG FILES ===="
Get-ChildItem 'C:\Users\pbcai\Downloads\source\client' -File | Where-Object { $_.Name -match 'config|Config|ini|xml|cfg' } | ForEach-Object { Write-Output ($_.Name + ' ' + $_.Length) }
Get-ChildItem 'C:\Users\pbcai\Downloads\source\client' -Directory | ForEach-Object { Write-Output ('DIR ' + $_.Name) }
