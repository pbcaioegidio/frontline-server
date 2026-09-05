$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe.bak-pt')
foreach ($o in @(0x54F348,0x54F355,0x54F361,0x54F370,0x54F37D,0x54F38A,0x54F399,0x54F3A5,0x54F3B4,0x54F3C0,0x54F3CF)) {
  $hex = (($o)..($o+15) | ForEach-Object { '{0:X2}' -f $b[$_] }) -join ' '
  Write-Output ("{0:X8} {1}" -f $o, $hex)
}
