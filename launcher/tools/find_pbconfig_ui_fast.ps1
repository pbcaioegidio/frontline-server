$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)
$u = [Text.Encoding]::Unicode.GetString($b)

$keys = @(
  'Advanced Settings',
  'Vertical Sync.',
  'Windowed Fullscreen',
  'Video Quick Setting',
  'Normal Mapping',
  'Dynamic Lighting',
  'Bullet Trace',
  'Tri-Linear Filtering',
  'Terrain Effect',
  'ANTI ALIAS OPTION',
  'GRAPHIC',
  'HDR & Bloom',
  'Rim Light',
  'ImageBasedLight',
  'ScreenSpaceReflection',
  'SSAO',
  'PhysX',
  'DirectX',
  'Texture',
  'Shadow',
  'Specular',
  'Resolution',
  'Fullscreen',
  'Windowed',
  'Lowest',
  'Highest',
  'Custom',
  'None',
  'Save',
  'Cancel',
  'Screen',
  'AUTO',
  'FPS',
  'Mid',
  'Low',
  'High',
  'Point Blank',
  'Configuration'
)

foreach ($k in $keys) {
  $i = $u.IndexOf($k)
  if ($i -ge 0) {
    $end = $u.IndexOf([char]0, $i)
    if ($end -lt 0 -or ($end - $i) -gt 80) { $end = $i + [Math]::Min(80, $k.Length + 20) }
    $full = $u.Substring($i, $end - $i)
    # find next null for capacity
    $cap = 0
    for ($p = $i; $p -lt $u.Length -and $u[$p] -ne [char]0; $p++) { $cap++ }
    Write-Output ("0x{0:X8} cap={1} '{2}'" -f ($i * 2), $cap, $full)
  } else {
    Write-Output ("MISS '{0}'" -f $k)
  }
}

# Window title might be in ANSI resources
$a = [Text.Encoding]::ASCII.GetString($b)
foreach ($k in @('Point Blank Configuration','PBConfig','Configuration')) {
  $i = $a.IndexOf($k)
  Write-Output ("ASCII '{0}' -> {1}" -f $k, $(if ($i -ge 0) { "0x{0:X}" -f $i } else { 'MISS' }))
}
