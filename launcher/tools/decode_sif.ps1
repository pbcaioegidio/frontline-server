# Try decrypt sif with launcher crypto AND simple XOR; also dump strings from PBConfig related to EnvSet/lwsi
$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\Config\lwsi_En.sif')

function RotateRight([byte]$v, [int]$bits) {
  [byte]((($v -shr $bits) -bor ($v -shl (8-$bits))) -band 0xFF)
}

# Try ConfigFileEncrypter decrypt (inverse of Encrypt)
$key = [Text.Encoding]::ASCII.GetBytes('PointBlank.Config.Security')
$feedback = [byte]0xA7
$plain = New-Object byte[] $b.Length
for ($i = 0; $i -lt $b.Length; $i++) {
  $cipher = $b[$i]
  $k = $key[$i % $key.Length]
  $positionMask = [byte]((($i * 37) + 0x5A) -band 0xFF)
  $mask = [byte](($k + $positionMask + $feedback) -band 0xFF)
  $rotated = RotateRight $cipher 3
  $plain[$i] = [byte]($rotated -bxor $mask)
  $feedback = $cipher
}
$t = [Text.Encoding]::ASCII.GetString($plain)
$preview = ($t.Substring(0, [Math]::Min(200,$t.Length)) -replace '[^\x20-\x7E]', '.')
Write-Output "launcher-crypto: $preview"

# Try common PB sif XOR keys
foreach ($keyStr in @('i3', 'PB', 'lwsi', 'zepetto', 'PointBlank', 'i3Game')) {
  $kb = [Text.Encoding]::ASCII.GetBytes($keyStr)
  $dec = New-Object byte[] ([Math]::Min(64,$b.Length))
  for ($i=0; $i -lt $dec.Length; $i++) { $dec[$i] = $b[$i] -bxor $kb[$i % $kb.Length] }
  $s = [Text.Encoding]::ASCII.GetString($dec) -replace '[^\x20-\x7E]','.'
  Write-Output ("xor $keyStr : $s")
}

# Search PBConfig for EnvSet / env_settings / lwsi
$exe = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$a = [Text.Encoding]::ASCII.GetString($exe)
$u = [Text.Encoding]::Unicode.GetString($exe)
foreach ($k in @('env_settings','EnvSet','lwsi','DXVersion','VideoResolution','ScreenMode','NormalMapping','BulletTrace')) {
  $ia = $a.IndexOf($k); $iu = $u.IndexOf($k)
  Write-Output ("{0} A={1} U={2}" -f $k, $(if($ia -ge 0){"0x{0:X}" -f $ia}else{'MISS'}), $(if($iu -ge 0){"0x{0:X}" -f ($iu*2)}else{'MISS'}))
}
