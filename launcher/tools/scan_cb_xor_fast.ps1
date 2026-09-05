# Fast ASCII-only XOR scan + PE import parse
$ErrorActionPreference = 'Stop'
$path = 'C:\Users\pbcai\Downloads\source\client\PointBlank.exe'
$data = [IO.File]::ReadAllBytes($path)
$enc = [Text.Encoding]::GetEncoding(28591)
$hay = $enc.GetString($data)
$needles = @('CheatBlocker','CHEAT_BLOCKER','CB.exe','Initialize Load Failed','CB.cbm','\\CB.exe')
$out = New-Object System.Collections.Generic.List[string]
foreach ($n in $needles) {
  $plain = [Text.Encoding]::ASCII.GetBytes($n)
  for ($xor = 0; $xor -le 255; $xor++) {
    $chars = New-Object char[] $plain.Length
    for ($j = 0; $j -lt $plain.Length; $j++) { $chars[$j] = [char]($plain[$j] -bxor [byte]$xor) }
    $pat = -join $chars
    $idx = $hay.IndexOf($pat)
    if ($idx -ge 0) { $out.Add(("ASCII '{0}' xor=0x{1:X2} @ 0x{2:X}" -f $n, $xor, $idx)) }
  }
}
Write-Output ("xor hits={0}" -f $out.Count)
$out | ForEach-Object { Write-Output $_ }

# PE imports via kernel32 names present as ASCII
foreach ($api in @('CreateProcessW','CreateProcessA','LoadLibraryW','LoadLibraryA','LoadLibraryExW','ShellExecuteW','ShellExecuteA','WinExec','CreateProcessAsUserW')) {
  $i = $hay.IndexOf($api + [char]0)
  if ($i -lt 0) { $i = $hay.IndexOf($api) }
  Write-Output ("import-ish {0}={1}" -f $api, $(if($i -ge 0){'0x{0:X}' -f $i}else{'MISS'}))
}

# Also search fragment 'Cheat' xor
$plain = [Text.Encoding]::ASCII.GetBytes('Cheat')
$cheatHits = 0
for ($xor = 1; $xor -le 255; $xor++) {
  $chars = New-Object char[] $plain.Length
  for ($j = 0; $j -lt $plain.Length; $j++) { $chars[$j] = [char]($plain[$j] -bxor [byte]$xor) }
  $pat = -join $chars
  $start = 0
  while (($idx = $hay.IndexOf($pat, $start)) -ge 0) {
    $cheatHits++
    if ($cheatHits -le 5) {
      $ctx = $hay.Substring($idx, [Math]::Min(40, $hay.Length-$idx)) -replace '[^\x20-\x7E]','.'
      $out.Add(("frag Cheat xor=0x{0:X2} @ 0x{1:X} ctx={2}" -f $xor, $idx, $ctx))
    }
    $start = $idx + 1
    if ($cheatHits -gt 20) { break }
  }
}
Write-Output ("Cheat frag hits (capped)={0}" -f $cheatHits)
$out | Where-Object { $_ -like 'frag*' } | ForEach-Object { Write-Output $_ }

[IO.File]::WriteAllLines('C:\Users\pbcai\Downloads\source\launcher\tools\cb_xor_hits.txt', $out)
