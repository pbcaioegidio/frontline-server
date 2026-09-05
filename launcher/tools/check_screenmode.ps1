$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$a = [Text.Encoding]::ASCII.GetString($b)
# Around ScreenMode and nearby numeric/logic strings
$i = $a.IndexOf('ScreenMode')
Write-Output ("ScreenMode @ 0x{0:X}" -f $i)
Write-Output ($a.Substring($i, 200) -replace '[^\x20-\x7E]', '.')

# Dialog captions order for screen modes (unicode)
$u = [Text.Encoding]::Unicode.GetString($b)
foreach ($k in @('Tela Cheia','Tela Cheia Janela','Janela','Fullscreen','Windowed Fullscreen','Windowed','ScreenMode')) {
  $p = $u.IndexOf($k)
  Write-Output ("U '{0}' @ {1}" -f $k, $(if($p -ge 0){'0x{0:X}' -f ($p*2)}else{'MISS'}))
}

Get-Content 'C:\Users\pbcai\Downloads\source\client\EnvSet\env_settings.ini'
Write-Output '--- DX ---'
Get-Content 'C:\Users\pbcai\Downloads\source\client\EnvSet\DXVersion.ini'
