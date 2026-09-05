$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$o = 0x54CC8C
$hex = (0..39 | ForEach-Object { '{0:X2}' -f $b[$o + $_] }) -join ' '
$lines = @()
$lines += "Advanced hex: $hex"
$lines += "As unicode codepoints: " + ((0..16 | ForEach-Object { '{0:X4}' -f [BitConverter]::ToUInt16($b, $o + $_*2) }) -join ' ')
$lines += "Bloom@CE22: " + ((-10..20 | ForEach-Object { '{0:X4}' -f [BitConverter]::ToUInt16($b, 0x54CE22 + $_*2) }) -join ' ')
$lines += "Low@CA9C: " + ((0..10 | ForEach-Object { '{0:X4}' -f [BitConverter]::ToUInt16($b, 0x54CA9C + $_*2) }) -join ' ')
$lines += "PhysX: " + ([Text.Encoding]::Unicode.GetString($b, 0x54CBE4, 12))
# decode expected Opções
$expect = @(0x004F,0x0070,0x00E7,0x00F5,0x0065,0x0073,0x0020,0x0041,0x0076,0x0061,0x006E,0x00E7,0x0061,0x0064,0x0061,0x0073,0x0000)
$ok = $true
for ($i=0; $i -lt $expect.Length; $i++) {
  $got = [BitConverter]::ToUInt16($b, $o + $i*2)
  if ($got -ne $expect[$i]) { $ok = $false; $lines += "DIFF at $i expect=$($expect[$i].ToString('X4')) got=$($got.ToString('X4'))" }
}
$lines += "OpcoesAvancadas bytes OK=$ok"
[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_hexcheck.txt', $lines)
Get-Content 'C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_hexcheck.txt'
