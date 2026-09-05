$f = 'C:\Users\pbcai\Downloads\source\client\PBConfig.exe'
$b = [IO.File]::ReadAllBytes($f)
Write-Output ("Size=" + $b.Length)

# PE check
$mz = [Text.Encoding]::ASCII.GetString($b, 0, 2)
Write-Output ("MZ=" + $mz)

$ascii = [Text.Encoding]::ASCII.GetString($b)
$unicode = [Text.Encoding]::Unicode.GetString($b)

$patterns = @(
  'Point Blank Configuration',
  'GRAPHIC',
  'Advanced Settings',
  'Vertical Sync',
  'Fullscreen',
  'Windowed',
  'Save',
  'Cancel',
  'Normal Mapping',
  'DX11',
  'Video Quick Setting',
  'ANTI ALIAS',
  'Lowest',
  'PhysX'
)

foreach ($p in $patterns) {
  $ia = $ascii.IndexOf($p)
  $iu = $unicode.IndexOf($p)
  Write-Output ("ASCII[$p]=" + $(if ($ia -ge 0) { "FOUND@$ia" } else { "no" }) + " UNI=" + $(if ($iu -ge 0) { "FOUND@$iu" } else { "no" }))
}

# Detect UI framework markers
foreach ($p in @('.NET', 'mscoree', 'Qt5', 'MFC', 'WTL', 'DialogBox', 'CreateDialog', 'This program cannot be run')) {
  $ia = $ascii.IndexOf($p)
  Write-Output ("MARK[$p]=" + $(if ($ia -ge 0) { "FOUND@$ia" } else { "no" }))
}

# Check if .NET assembly
try {
  Add-Type -AssemblyName System.Reflection
  $asm = [System.Reflection.AssemblyName]::GetAssemblyName($f)
  Write-Output ("NET_ASSEMBLY=" + $asm.FullName)
} catch {
  Write-Output ("NET_ASSEMBLY=no (" + $_.Exception.Message + ")")
}
