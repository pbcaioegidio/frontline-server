# Try bit-rotate decrypt of lwsi_En.sif (common PB config)
$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\Config\lwsi_En.sif')
for ($shift = 1; $shift -le 7; $shift++) {
  $dec = New-Object byte[] $b.Length
  for ($i = 0; $i -lt $b.Length; $i++) {
    $dec[$i] = [byte]((($b[$i] -shr $shift) -bor ($b[$i] -shl (8-$shift))) -band 0xFF)
  }
  $t = [Text.Encoding]::ASCII.GetString($dec, 0, 80) -replace '[^\x20-\x7E]', '.'
  Write-Output ("ror$shift : $t")
  $dec2 = New-Object byte[] $b.Length
  for ($i = 0; $i -lt $b.Length; $i++) {
    $dec2[$i] = [byte]((($b[$i] -shl $shift) -bor ($b[$i] -shr (8-$shift))) -band 0xFF)
  }
  $t2 = [Text.Encoding]::ASCII.GetString($dec2, 0, 80) -replace '[^\x20-\x7E]', '.'
  Write-Output ("rol$shift : $t2")
}

# XOR with position
$dec3 = New-Object byte[] 64
for ($i=0; $i -lt 64; $i++) { $dec3[$i] = $b[$i] -bxor [byte]($i -band 0xFF) }
Write-Output ("xor i: " + ([Text.Encoding]::ASCII.GetString($dec3) -replace '[^\x20-\x7E]','.'))

# Check if PointBlank.exe references env_settings
$pb = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PointBlank.exe')
$pa = [Text.Encoding]::ASCII.GetString($pb)
foreach ($k in @('env_settings','EnvSet','ScreenMode','lwsi_En','lwsi')) {
  $p = $pa.IndexOf($k)
  Write-Output ("PB {0}={1}" -f $k, $(if($p -ge 0){'0x{0:X}' -f $p}else{'MISS'}))
}
