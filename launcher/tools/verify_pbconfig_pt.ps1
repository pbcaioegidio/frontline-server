$b = [IO.File]::ReadAllBytes('C:\Users\pbcai\Downloads\source\client\PBConfig.exe')
$checks = @(
  @{ Off = 0x54C68A; N = 16 },
  @{ Off = 0x54CC8C; N = 17 },
  @{ Off = 0x54CCD0; N = 7 },
  @{ Off = 0x54C730; N = 10 },
  @{ Off = 0x54CA70; N = 6 },
  @{ Off = 0x54CB14; N = 7 },
  @{ Off = 0x54C6DC; N = 4 },
  @{ Off = 0x54C8E4; N = 14 },
  @{ Off = 0x54CF60; N = 10 }
)
# Write verification to UTF-8 file
$out = New-Object System.Collections.Generic.List[string]
foreach ($c in $checks) {
  $s = [Text.Encoding]::Unicode.GetString($b, $c.Off, $c.N * 2).TrimEnd([char]0)
  $out.Add(("0x{0:X8} = [{1}]" -f $c.Off, $s))
}

# Find remaining English in dialog region
$u = [Text.Encoding]::Unicode.GetString($b)
$start = [int](0x54C600/2); $end = [int](0x54D400/2)
$i = $start
while ($i -lt $end) {
  if ($u[$i] -eq [char]0) { $i++; continue }
  $j = $i
  while ($j -lt $end -and $u[$j] -ne [char]0) { $j++ }
  $s = $u.Substring($i, $j-$i)
  if ($s.Length -ge 3 -and $s -match '^[A-Za-z]') {
    $out.Add(("remain 0x{0:X} {1}" -f ($i*2), $s))
  }
  $i = $j + 1
}

# Search HDR Bloom variants
foreach ($t in @('HDR & Bloom','HDR&Bloom','HDR and Bloom','Bloom','None','Low','AUTO','PhysX','DX11','DX9','DX11(BETA)')) {
  $idx = $u.IndexOf($t, $start)
  if ($idx -ge 0 -and $idx -lt $end) {
    $out.Add(("found '{0}' @ 0x{1:X}" -f $t, ($idx*2)))
  } else {
    # global
    $g = $u.IndexOf($t)
    if ($g -ge 0) { $out.Add(("global '{0}' @ 0x{1:X}" -f $t, ($g*2))) }
  }
}

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\pbconfig_pt_verify.txt', $out, [Text.UTF8Encoding]::new($false))
Write-Output 'wrote verify txt'
