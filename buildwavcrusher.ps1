Param(
    [string]$Configuration = "Release",
    [string]$ProjectPath = (Join-Path $PSScriptRoot "src\WavCrusher.WinForms\WavCrusher.WinForms.csproj"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "artifacts\WavCrusher.WinForms"),
    [string]$InstallerOutputPath = (Join-Path $PSScriptRoot "artifacts\installer"),
    [string]$RuntimeIdentifier = "win-x64",
    [string]$ProductVersion = "1.0.0",
    [string]$Manufacturer = "Aryn Mikel Sparks",
    [switch]$NoRestore,
    [switch]$RunAfterBuild,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$productName = "WavCrusher"
$installFolderName = "WaveCrusher"
$upgradeCode = "8b1d3e23-604b-4b1d-b779-74dbe4d23d67"
$mainExeName = "WavCrusher.WinForms.exe"

function ConvertTo-WixXml([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

function ConvertTo-WixId([string]$Value) {
    $id = ($Value -replace '[^A-Za-z0-9_]', '_')
    if ($id.Length -eq 0 -or $id[0] -notmatch '[A-Za-z_]') {
        $id = "id_$id"
    }

    return $id
}

function Get-RelativePath([string]$BasePath, [string]$Path) {
    $resolvedBaseRaw = (Resolve-Path -LiteralPath $BasePath).Path.TrimEnd('\')
    $resolvedPath = (Resolve-Path -LiteralPath $Path).Path.TrimEnd('\')
    if ($resolvedPath -ieq $resolvedBaseRaw) {
        return "."
    }

    $resolvedBase = $resolvedBaseRaw + '\'
    $baseUri = New-Object System.Uri $resolvedBase
    $pathUri = New-Object System.Uri $resolvedPath
    $relativeUri = $baseUri.MakeRelativeUri($pathUri).ToString()
    $relativePath = [System.Uri]::UnescapeDataString($relativeUri).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
    if ([string]::IsNullOrWhiteSpace($relativePath) -or $relativePath -eq ".") {
        return "."
    }

    return $relativePath.TrimEnd('\')
}

function New-WavCrusherIcon([string]$IconPath) {
    Add-Type -AssemblyName System.Drawing

    $directory = Split-Path -Parent $IconPath
    if (-not (Test-Path -LiteralPath $directory)) {
        $null = New-Item -Path $directory -ItemType Directory -Force
    }

    $bitmap = New-Object System.Drawing.Bitmap 256, 256
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::FromArgb(18, 24, 22))

    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Rectangle 0, 0, 256, 256),
        [System.Drawing.Color]::FromArgb(15, 53, 39),
        [System.Drawing.Color]::FromArgb(7, 13, 12),
        45)
    $graphics.FillRectangle($brush, 0, 0, 256, 256)

    $greenPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(94, 224, 167)), 12
    $greenPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $greenPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $amberPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(238, 183, 93)), 7
    $whitePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(245, 243, 236)), 4

    $wave = @(
        (New-Object System.Drawing.Point 24, 132),
        (New-Object System.Drawing.Point 55, 93),
        (New-Object System.Drawing.Point 86, 163),
        (New-Object System.Drawing.Point 118, 76),
        (New-Object System.Drawing.Point 150, 178),
        (New-Object System.Drawing.Point 186, 100),
        (New-Object System.Drawing.Point 232, 132)
    )
    $graphics.DrawCurve($greenPen, $wave, 0.55)

    $blockBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(232, 238, 234))
    $shadowBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(90, 0, 0, 0))
    foreach ($x in @(56, 96, 136, 176)) {
        $graphics.FillRectangle($shadowBrush, $x + 4, 180, 25, 28)
        $graphics.FillRectangle($blockBrush, $x, 176, 25, 28)
    }

    $graphics.DrawLine($amberPen, 165, 55, 191, 81)
    $graphics.DrawLine($amberPen, 191, 81, 232, 37)
    $graphics.DrawEllipse($whitePen, 18, 18, 220, 220)

    $font = New-Object System.Drawing.Font "Segoe UI Black", 68, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(245, 243, 236))
    $graphics.DrawString("W", $font, $textBrush, 78, 62)

    $pngStream = New-Object System.IO.MemoryStream
    $bitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $pngStream.ToArray()

    $fileStream = [System.IO.File]::Create($IconPath)
    $writer = New-Object System.IO.BinaryWriter $fileStream
    $writer.Write([UInt16]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]1)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$pngBytes.Length)
    $writer.Write([UInt32]22)
    $writer.Write($pngBytes)
    $writer.Dispose()
    $fileStream.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

function Copy-WavPackSidecars([string]$DestinationRoot) {
    $sourceRoot = Join-Path $PSScriptRoot "third_party\wavpack"
    $toolRoot = Join-Path $sourceRoot "win-x64"
    foreach ($toolName in @("wavpack.exe", "wvunpack.exe")) {
        $source = Join-Path $toolRoot $toolName
        if (-not (Test-Path -LiteralPath $source)) {
            throw "Required WavPack sidecar is missing: $source"
        }

        Copy-Item -LiteralPath $source -Destination (Join-Path $DestinationRoot $toolName) -Force
    }

    $metadataRoot = Join-Path $DestinationRoot "third_party\wavpack"
    $metadataToolRoot = Join-Path $metadataRoot "win-x64"
    $null = New-Item -Path $metadataToolRoot -ItemType Directory -Force
    foreach ($fileName in @("LICENSE", "VERSION", "dependency.json")) {
        Copy-Item -LiteralPath (Join-Path $sourceRoot $fileName) -Destination (Join-Path $metadataRoot $fileName) -Force
    }

    Copy-Item -LiteralPath (Join-Path $toolRoot "wavpack.exe") -Destination (Join-Path $metadataToolRoot "wavpack.exe") -Force
    Copy-Item -LiteralPath (Join-Path $toolRoot "wvunpack.exe") -Destination (Join-Path $metadataToolRoot "wvunpack.exe") -Force
}

function Get-WixCommand([string]$ToolsRoot) {
    $pathCommand = Get-Command "wix" -ErrorAction SilentlyContinue
    if ($null -ne $pathCommand) {
        $versionText = & $pathCommand.Source --version
        if ($versionText -notmatch '^7\.') {
            return $pathCommand.Source
        }
    }

    $localWix = Join-Path $ToolsRoot "wix.exe"
    if (Test-Path -LiteralPath $localWix) {
        $versionText = & $localWix --version
        if ($versionText -notmatch '^7\.') {
            return $localWix
        }
    }

    $null = New-Item -Path $ToolsRoot -ItemType Directory -Force
    Write-Host "Installing WiX Toolset 5.0.2 locally under artifacts\tools..."
    & dotnet tool install wix --version 5.0.2 --tool-path $ToolsRoot | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install WiX Toolset with dotnet tool."
    }

    return $localWix
}

function New-DirectoryXml([string]$DirectoryPath, [hashtable]$DirectoryIds, [string]$PayloadRoot, [int]$Depth) {
    $relative = Get-RelativePath $PayloadRoot $DirectoryPath
    $directoryId = $DirectoryIds[$relative]
    $name = Split-Path -Leaf $DirectoryPath
    $indent = " " * $Depth
    $builder = New-Object System.Text.StringBuilder
    [void]$builder.AppendLine("$indent<Directory Id=`"$directoryId`" Name=`"$(ConvertTo-WixXml $name)`">")

    foreach ($child in Get-ChildItem -LiteralPath $DirectoryPath -Directory | Sort-Object Name) {
        [void]$builder.Append((New-DirectoryXml $child.FullName $DirectoryIds $PayloadRoot ($Depth + 2)))
    }

    [void]$builder.AppendLine("$indent</Directory>")
    return $builder.ToString()
}

function New-MsiPackage(
    [string]$PayloadRoot,
    [string]$InstallerRoot,
    [string]$IconPath,
    [string]$Version,
    [string]$WixCommand
) {
    $null = New-Item -Path $InstallerRoot -ItemType Directory -Force
    $wxsPath = Join-Path $InstallerRoot "WavCrusher.Installer.wxs"
    $msiPath = Join-Path $InstallerRoot "WavCrusher.Setup.$Version.msi"

    $directories = @(Get-ChildItem -LiteralPath $PayloadRoot -Directory -Recurse | Sort-Object FullName)
    $directoryIds = @{}
    foreach ($directory in $directories) {
        $relative = Get-RelativePath $PayloadRoot $directory.FullName
        $directoryIds[$relative] = ConvertTo-WixId "dir_$relative"
    }

    $directoryXml = New-Object System.Text.StringBuilder
    foreach ($directory in Get-ChildItem -LiteralPath $PayloadRoot -Directory | Sort-Object Name) {
        [void]$directoryXml.Append((New-DirectoryXml $directory.FullName $directoryIds $PayloadRoot 8))
    }

    $componentXml = New-Object System.Text.StringBuilder
    $componentRefs = New-Object System.Text.StringBuilder
    $mainExePath = Join-Path $PayloadRoot $mainExeName
    $mainExeComponentId = $null
    $fileIndex = 0

    foreach ($file in Get-ChildItem -LiteralPath $PayloadRoot -File -Recurse | Sort-Object FullName) {
        $fileIndex++
        $relativeDirectory = Get-RelativePath $PayloadRoot $file.DirectoryName
        $directoryId = if ($relativeDirectory -eq ".") { "INSTALLFOLDER" } else { $directoryIds[$relativeDirectory] }
        $componentId = ConvertTo-WixId "cmp_$fileIndex`_$($file.BaseName)"
        $fileId = ConvertTo-WixId "fil_$fileIndex`_$($file.BaseName)"
        if ($file.FullName -ieq $mainExePath) {
            $mainExeComponentId = $componentId
        }

        [void]$componentXml.AppendLine("    <DirectoryRef Id=`"$directoryId`">")
        [void]$componentXml.AppendLine("      <Component Id=`"$componentId`" Bitness=`"always64`" Guid=`"*`">")
        [void]$componentXml.AppendLine("        <File Id=`"$fileId`" Source=`"$(ConvertTo-WixXml $file.FullName)`" KeyPath=`"yes`" />")
        [void]$componentXml.AppendLine("      </Component>")
        [void]$componentXml.AppendLine("    </DirectoryRef>")
        [void]$componentRefs.AppendLine("      <ComponentRef Id=`"$componentId`" />")
    }

    if ($null -eq $mainExeComponentId) {
        throw "Published executable was not found for installer packaging: $mainExePath"
    }

    $wxs = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="$productName" Manufacturer="$(ConvertTo-WixXml $Manufacturer)" Version="$Version" UpgradeCode="$upgradeCode" Scope="perMachine">
    <MajorUpgrade DowngradeErrorMessage="A newer version of $productName is already installed." />
    <MediaTemplate EmbedCab="yes" />
    <Icon Id="WavCrusherIcon" SourceFile="$(ConvertTo-WixXml $IconPath)" />
    <Property Id="ARPPRODUCTICON" Value="WavCrusherIcon" />

    <StandardDirectory Id="ProgramFiles64Folder">
      <Directory Id="INSTALLFOLDER" Name="$(ConvertTo-WixXml $installFolderName)">
$($directoryXml.ToString().TrimEnd())
      </Directory>
    </StandardDirectory>
    <StandardDirectory Id="DesktopFolder" />

$($componentXml.ToString().TrimEnd())
    <DirectoryRef Id="INSTALLFOLDER">
      <Component Id="cmp_DesktopShortcut" Bitness="always64" Guid="*">
        <Shortcut Id="DesktopShortcut" Directory="DesktopFolder" Name="$productName" Description="Verified lossless WAV archiving" Target="[INSTALLFOLDER]$mainExeName" WorkingDirectory="INSTALLFOLDER" Icon="WavCrusherIcon" />
        <RegistryValue Root="HKLM" Key="Software\$productName" Name="DesktopShortcut" Type="integer" Value="1" KeyPath="yes" />
      </Component>
    </DirectoryRef>

    <Feature Id="MainFeature" Title="$productName" Level="1">
$($componentRefs.ToString().TrimEnd())
      <ComponentRef Id="cmp_DesktopShortcut" />
    </Feature>
  </Package>
</Wix>
"@

    Set-Content -LiteralPath $wxsPath -Value $wxs -Encoding UTF8
    & $WixCommand build $wxsPath -o $msiPath
    if ($LASTEXITCODE -ne 0) {
        throw "WiX MSI build failed with exit code $LASTEXITCODE."
    }

    return $msiPath
}

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "WinForms project not found at '$ProjectPath'."
}

$null = New-Item -Path $OutputPath -ItemType Directory -Force
$null = New-Item -Path $InstallerOutputPath -ItemType Directory -Force
$assetPath = Join-Path $PSScriptRoot "artifacts\assets\wavcrusher.ico"
New-WavCrusherIcon $assetPath

$dotnetArgs = @(
    "publish",
    $ProjectPath,
    "-c", $Configuration,
    "-r", $RuntimeIdentifier,
    "--self-contained", "true",
    "--output", $OutputPath,
    "-p:UseAppHost=true",
    "-p:PublishSingleFile=false",
    "-p:ApplicationIcon=$assetPath"
)

if ($NoRestore) {
    $dotnetArgs += "--no-restore"
}

Write-Host "Publishing WavCrusher WinForms project from source..."
Write-Host ("  Project: {0}" -f $ProjectPath)
Write-Host ("  Configuration: {0}" -f $Configuration)
Write-Host ("  Runtime: {0}" -f $RuntimeIdentifier)
Write-Host ("  Output: {0}" -f $OutputPath)

& dotnet @dotnetArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Copy-WavPackSidecars $OutputPath

$expectedExe = Join-Path $OutputPath $mainExeName
if (-not (Test-Path -LiteralPath $expectedExe)) {
    throw "Build completed, but '$expectedExe' was not found."
}

Write-Host "Publish complete."
Write-Host "Exe available at: $expectedExe"

if (-not $SkipInstaller.IsPresent) {
    $wixToolsRoot = Join-Path $PSScriptRoot "artifacts\tools\wix5"
    $wixCommand = Get-WixCommand $wixToolsRoot
    $msiPath = New-MsiPackage $OutputPath $InstallerOutputPath $assetPath $ProductVersion $wixCommand
    Write-Host "MSI installer available at: $msiPath"
    Write-Host "Installer target folder: C:\Program Files\$installFolderName"
}

if ($RunAfterBuild.IsPresent) {
    Write-Host "Launching WinForms executable..."
    Start-Process -FilePath $expectedExe
}
