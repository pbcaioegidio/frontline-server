# Monta patch do client para o Socket (Data/Client + Info/manifest.json + FileList).
# Uso local:
#   .\scripts\build-client-patch.ps1 -Version 20260905 -PatchRoot .\client-patch -OutDir .\out\client-patch
#   (opcional) -ClientRoot .\client  → FileListBuilder full
#   (opcional) -ExistingFileList path\UserFileList.dat → merge de hashes
#
# No Actions: chamado pelo client-patch.yml

param(
    [Parameter(Mandatory = $true)]
    [string] $Version,
    [string] $PatchRoot = "",
    [string] $ClientRoot = "",
    [string] $ExistingFileList = "",
    [string] $ExistingSig = "",
    [string] $PrivatePemPath = "",
    [string] $PrivatePemText = "",
    [string] $OutDir = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
if (-not $PatchRoot) { $PatchRoot = Join-Path $root "client-patch" }
if (-not $OutDir) { $OutDir = Join-Path $root "out\client-patch" }
$Version = ($Version -replace '[^\d]', '')
if (-not $Version) { throw "Version precisa ser numerica (ex. 20260905)" }

function Get-Md5Hex([string] $path) {
    $md5 = [Security.Cryptography.MD5]::Create()
    $fs = [IO.File]::OpenRead($path)
    try {
        return ([BitConverter]::ToString($md5.ComputeHash($fs)) -replace '-', '').ToUpperInvariant()
    }
    finally { $fs.Dispose(); $md5.Dispose() }
}

function Normalize-Rel([string] $rel) {
    return ($rel -replace '/', '\').TrimStart('\')
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$stageFiles = Join-Path $OutDir "files"
if (Test-Path $stageFiles) { Remove-Item $stageFiles -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stageFiles | Out-Null

if (-not (Test-Path $PatchRoot)) {
    throw "PatchRoot nao encontrado: $PatchRoot"
}

# Copia delta (ignora README)
$copied = @()
Get-ChildItem $PatchRoot -Recurse -File | Where-Object {
    $_.Name -notin @("README.md", ".gitkeep") -and $_.FullName -notmatch '\\\.git\\'
} | ForEach-Object {
    $rel = Normalize-Rel $_.FullName.Substring((Resolve-Path $PatchRoot).Path.Length)
    if ([string]::IsNullOrWhiteSpace($rel)) { return }
    $dest = Join-Path $stageFiles $rel
    New-Item -ItemType Directory -Force -Path (Split-Path $dest -Parent) | Out-Null
    Copy-Item $_.FullName $dest -Force
    $copied += [pscustomobject]@{
        Path = $rel
        Full = $dest
        Size = $_.Length
        Md5  = Get-Md5Hex $dest
    }
}

if ($copied.Count -eq 0) {
    Write-Warning "Nenhum arquivo em client-patch/ (so README?). Manifest ainda sera gerado/atualizado se houver FileList."
}

$datOut = Join-Path $OutDir "UserFileList.dat"
$sigOut = Join-Path $OutDir "UserFileList.sig"
$uflTxt = Join-Path $OutDir "ufl-md5.txt"
$usedFullBuilder = $false

# PEM
$pemPath = Join-Path $OutDir "_private.pem"
if ($PrivatePemText) {
    Set-Content -Path $pemPath -Value $PrivatePemText.Trim() -Encoding ASCII -NoNewline
    Add-Content -Path $pemPath -Value "" -Encoding ASCII
}
elseif ($PrivatePemPath -and (Test-Path $PrivatePemPath)) {
    Copy-Item $PrivatePemPath $pemPath -Force
}
else {
    $defaultPem = Join-Path $root "launcher\security\filelist-private.pem"
    if (Test-Path $defaultPem) { Copy-Item $defaultPem $pemPath -Force }
}

if (-not (Test-Path $pemPath)) {
    throw "Chave privada ausente. Passe -PrivatePemPath, -PrivatePemText ou coloque launcher/security/filelist-private.pem"
}

# Full client → FileListBuilder
if ($ClientRoot -and (Test-Path $ClientRoot) -and (Test-Path (Join-Path $ClientRoot "FrontLine.exe"))) {
    Write-Host "==> FileListBuilder no client completo: $ClientRoot"
    # Garante que o delta ja esta no client antes de hashear
    foreach ($f in $copied) {
        $dest = Join-Path $ClientRoot $f.Path
        New-Item -ItemType Directory -Force -Path (Split-Path $dest -Parent) | Out-Null
        Copy-Item $f.Full $dest -Force
    }
    $builderProj = Join-Path $root "launcher\tools\FileListBuilder\FileListBuilder.csproj"
    dotnet run --project $builderProj -c Release --no-launch-profile -- $ClientRoot $pemPath
    if ($LASTEXITCODE -ne 0) { throw "FileListBuilder falhou" }
    Copy-Item (Join-Path $ClientRoot "UserFileList.dat") $datOut -Force
    Copy-Item (Join-Path $ClientRoot "UserFileList.sig") $sigOut -Force
    if (Test-Path (Join-Path $ClientRoot "ufl-md5.txt")) {
        Copy-Item (Join-Path $ClientRoot "ufl-md5.txt") $uflTxt -Force
    }
    $usedFullBuilder = $true
}
else {
    # Se o delta ja traz UserFileList.dat limpa/assinada, substitui a da VPS (nao merge).
    # Serve pra tirar lixo (.bad_sig, .bak) que o merge sozinho nao remove.
    $patchDat = Join-Path $PatchRoot "UserFileList.dat"
    $patchSig = Join-Path $PatchRoot "UserFileList.sig"
    if ((Test-Path $patchDat) -and (Test-Path $patchSig)) {
        Write-Host "==> Substituindo FileList pela do patch (UserFileList.dat/.sig)"
        Copy-Item $patchDat $datOut -Force
        Copy-Item $patchSig $sigOut -Force
        $md5 = Get-Md5Hex $datOut
        [IO.File]::WriteAllText($uflTxt, $md5.ToLowerInvariant() + "`r`n", (New-Object Text.UTF8Encoding $false))
        # tira da lista de "copied" generica — entram de novo abaixo como FileList oficial
        $copied = @($copied | Where-Object { $_.Path -notin @("UserFileList.dat", "UserFileList.sig", "ufl-md5.txt") })
    }
    else {
        # Merge na lista existente
        if (-not $ExistingFileList -or -not (Test-Path $ExistingFileList)) {
            throw "Sem client completo e sem -ExistingFileList. Baixe UserFileList.dat da VPS ou rode com -ClientRoot."
        }
        Write-Host "==> Merge FileList: $ExistingFileList"
        $xml = New-Object System.Xml.XmlDocument
        $xml.PreserveWhitespace = $true
        $xml.Load($ExistingFileList)
        $list = $xml.SelectSingleNode("/list")
        if (-not $list) { throw "UserFileList.dat invalido (sem /list)" }

        # Remove lixo / prefs locais (mesma ideia do IntegrityRules.ShouldSkip)
        $removed = 0
        foreach ($n in @($list.SelectNodes("file"))) {
            $rel = Normalize-Rel $n.GetAttribute("local")
            $name = [IO.Path]::GetFileName($rel)
            $isEnvPref = ($rel -match '(?i)^EnvSet\\') -and ($name -ieq 'env_settings.ini' -or $name -ieq 'DXVersion.ini')
            if ($isEnvPref -or $name -match '(?i)\.bak|\.bad_sig$' -or $name -eq 'LEIA-ME.txt') {
                [void]$list.RemoveChild($n)
                $removed++
            }
        }
        if ($removed -gt 0) { Write-Host "  removidos $removed entradas (prefs EnvSet / .bak/.bad_sig)" }

        foreach ($f in $copied) {
            $rel = $f.Path
            if ($rel -in @("UserFileList.dat", "UserFileList.sig", "ufl-md5.txt")) { continue }
            $node = $null
            foreach ($n in $list.SelectNodes("file")) {
                if ((Normalize-Rel $n.GetAttribute("local")) -eq $rel) { $node = $n; break }
            }
            if (-not $node) {
                $node = $xml.CreateElement("file")
                [void]$list.AppendChild($node)
            }
            $node.SetAttribute("local", $rel)
            $node.SetAttribute("hash", $f.Md5)
            Write-Host "  hash $rel = $($f.Md5)"
        }

        # Sempre inclui UserFileList na pasta de patch? O launcher ja tem local — atualizamos apos download.
        # Incluir dat/sig no patch para o Update aplicar a lista nova.
        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Encoding = New-Object System.Text.UTF8Encoding $false
        $settings.Indent = $true
        $settings.IndentChars = "  "
        $w = [System.Xml.XmlWriter]::Create($datOut, $settings)
        try { $xml.Save($w) } finally { $w.Dispose() }

        # Assina com ferramenta .NET (ManifestTrust) — funciona no PS 5 e no PS 7
        $signProj = Join-Path $root "launcher\tools\SignFileList\SignFileList.csproj"
        dotnet run --project $signProj -c Release --no-launch-profile -- $datOut $sigOut $pemPath
        if ($LASTEXITCODE -ne 0) { throw "Assinatura FileList falhou" }

        $md5 = Get-Md5Hex $datOut
        [IO.File]::WriteAllText($uflTxt, $md5.ToLowerInvariant() + "`r`n", (New-Object Text.UTF8Encoding $false))
    }
}

# Copia FileList para files/ (Update aplica no client)
Copy-Item $datOut (Join-Path $stageFiles "UserFileList.dat") -Force
Copy-Item $sigOut (Join-Path $stageFiles "UserFileList.sig") -Force
if (Test-Path $uflTxt) {
    Copy-Item $uflTxt (Join-Path $stageFiles "ufl-md5.txt") -Force
}

# Atualiza lista de copied com FileList
foreach ($name in @("UserFileList.dat", "UserFileList.sig", "ufl-md5.txt")) {
    $p = Join-Path $stageFiles $name
    if (Test-Path $p) {
        $copied = @($copied | Where-Object { $_.Path -ne $name }) + [pscustomobject]@{
            Path = $name; Full = $p; Size = (Get-Item $p).Length; Md5 = Get-Md5Hex $p
        }
    }
}

# manifest.json
$manifest = [ordered]@{
    clientVersion = $Version
    patchUrl      = ""
    files         = @($copied | ForEach-Object {
        [ordered]@{
            path = ($_.Path -replace '\\', '/')
            zip  = ""
            size = [int64]$_.Size
            md5  = $_.Md5
        }
    })
}
$manifestPath = Join-Path $OutDir "manifest.json"
# UTF-8 SEM BOM: o Socket usa Newtonsoft e quebra com BOM ("Unexpected character ... position 0")
$manifestJson = ($manifest | ConvertTo-Json -Depth 6) -replace "`r`n", "`n"
[IO.File]::WriteAllText($manifestPath, $manifestJson + "`n", (New-Object Text.UTF8Encoding $false))

Remove-Item $pemPath -Force -ErrorAction SilentlyContinue

Write-Host "==> Out: $OutDir"
Write-Host "    files: $($copied.Count)  fullBuilder=$usedFullBuilder  version=$Version"
Write-Host "    manifest: $manifestPath"
