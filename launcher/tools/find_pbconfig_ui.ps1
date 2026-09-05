$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)

function Find-Utf16([byte[]]$bytes, [string]$text) {
  $pat = [Text.Encoding]::Unicode.GetBytes($text)
  for ($i = 0; $i -le $bytes.Length - $pat.Length; $i++) {
    $ok = $true
    for ($j = 0; $j -lt $pat.Length; $j++) {
      if ($bytes[$i+$j] -ne $pat[$j]) { $ok = $false; break }
    }
    if ($ok) { return $i }
  }
  return -1
}

$keys = @(
  'Point Blank Configuration',
  'PointBlank Configuration',
  'GRAPHIC',
  'Advanced Settings',
  'Vertical Sync.',
  'Vertical Sync',
  'Fullscreen',
  'Windowed Fullscreen',
  'Windowed',
  'Video Quick Setting',
  'Lowest',
  'Low',
  'Mid',
  'High',
  'Highest',
  'Custom',
  'Normal Mapping',
  'Dynamic Lighting',
  'Bullet Trace',
  'Tri-Linear Filtering',
  'Terrain Effect',
  'PhysX',
  'DirectX',
  'HDR & Bloom',
  'Rim Light',
  'ImageBasedLight',
  'SSAO',
  'ScreenSpaceReflection',
  'Texture',
  'Shadow',
  'Specular',
  'ANTI ALIAS OPTION',
  'None',
  'Save',
  'Cancel',
  'Resolution',
  'Screen',
  'AUTO',
  'FPS'
)

foreach ($k in $keys) {
  $pos = Find-Utf16 $b $k
  if ($pos -ge 0) {
    # measure full null-terminated string length in chars
    $chars = 0
    $p = $pos
    while ($p+1 -lt $b.Length -and -not ($b[$p] -eq 0 -and $b[$p+1] -eq 0)) {
      $chars++
      $p += 2
      if ($chars -gt 80) { break }
    }
    $full = [Text.Encoding]::Unicode.GetString($b, $pos, $chars*2)
    Write-Output ("0x{0:X8} len={1} '{2}'" -f $pos, $chars, $full)
  } else {
    Write-Output ("MISSING '{0}'" -f $k)
  }
}

# List string table / resources with dumpbin if available
Write-Output "==== PE RESOURCES ===="
$dumpbin = @(
  "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe",
  "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\VC\Tools\MSVC\*\bin\Hostx64\x64\dumpbin.exe"
) | ForEach-Object { Get-Item $_ -ErrorAction SilentlyContinue } | Select-Object -First 1
if ($dumpbin) {
  & $dumpbin.FullName /HEADERS $f | Select-String -Pattern 'resource|subsystem|machine' -CaseSensitive:$false
} else {
  Write-Output 'dumpbin not found'
}
