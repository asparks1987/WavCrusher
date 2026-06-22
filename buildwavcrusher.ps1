Param(
    [string]$Configuration = "Release",
    [string]$ProjectPath = (Join-Path $PSScriptRoot "src\WavCrusher.WinForms\WavCrusher.WinForms.csproj"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "artifacts\WavCrusher.WinForms"),
    [string]$InstallerOutputPath = (Join-Path $PSScriptRoot "artifacts\installer"),
    [string]$RuntimeIdentifier = "win-x64",
    [string]$ProductVersion = "",
    [string]$Manufacturer = "Aryn Mikel Sparks",
    [switch]$NoRestore,
    [switch]$RunAfterBuild,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$productName = "WavCrusher"
$installFolderName = "WavCrusher"
$upgradeCode = "8b1d3e23-604b-4b1d-b779-74dbe4d23d67"
$mainExeName = "WavCrusher.WinForms.exe"
$readmeFileName = "README.md"
$wixToolsetVersion = "5.0.2"

function Read-ReleaseVersion([string]$ProvidedVersion) {
    $version = $ProvidedVersion.Trim()
    while ([string]::IsNullOrWhiteSpace($version)) {
        $version = (Read-Host "Enter WavCrusher release version, for example 1.0.0a or 1.0.1").Trim()
    }

    return $version
}

function ConvertTo-MsiProductVersion([string]$ReleaseVersion) {
    $match = [regex]::Match($ReleaseVersion, '^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)')
    if (-not $match.Success) {
        throw "Version '$ReleaseVersion' must start with a numeric MSI-compatible version like 1.0.0 or 1.0.0a."
    }

    $major = [int]$match.Groups["major"].Value
    $minor = [int]$match.Groups["minor"].Value
    $build = [int]$match.Groups["build"].Value

    if ($major -gt 255 -or $minor -gt 255 -or $build -gt 65535) {
        throw "Version '$ReleaseVersion' is outside Windows Installer limits. Use major/minor <= 255 and build <= 65535."
    }

    return "$major.$minor.$build"
}

function ConvertTo-ArtifactVersion([string]$ReleaseVersion) {
    $artifactVersion = $ReleaseVersion -replace '[^A-Za-z0-9._-]', '-'
    if ([string]::IsNullOrWhiteSpace($artifactVersion)) {
        throw "Version '$ReleaseVersion' cannot be used in an artifact filename."
    }

    return $artifactVersion
}

function ConvertTo-WixXml([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

function ConvertTo-WixId([string]$Value) {
    $id = ($Value -replace '[^A-Za-z0-9_]', '_')
    if ($id.Length -eq 0 -or $id[0] -notmatch '[A-Za-z_]') {
        $id = "id_$id"
    }

    if ($id.Length -gt 72) {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($id)
        $hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).Substring(0, 12).ToLowerInvariant()
        $id = $id.Substring(0, 58) + "_" + $hash
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

function New-WixUiBitmaps([string]$AssetsRoot) {
    Add-Type -AssemblyName System.Drawing

    $null = New-Item -Path $AssetsRoot -ItemType Directory -Force
    $dialogPath = Join-Path $AssetsRoot "wavcrusher-wix-dialog.png"
    $bannerPath = Join-Path $AssetsRoot "wavcrusher-wix-banner.png"

    $dialog = New-Object System.Drawing.Bitmap 493, 312
    $g = [System.Drawing.Graphics]::FromImage($dialog)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $paperBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(246, 244, 238))
    $g.FillRectangle($paperBrush, 0, 0, 493, 312)

    $sidebarBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Rectangle 0, 0, 164, 312),
        [System.Drawing.Color]::FromArgb(18, 24, 22),
        [System.Drawing.Color]::FromArgb(8, 50, 36),
        90)
    $g.FillRectangle($sidebarBrush, 0, 0, 164, 312)

    $dividerBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(238, 183, 93))
    $g.FillRectangle($dividerBrush, 164, 0, 4, 312)

    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(130, 94, 224, 167)), 7
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $points = @(
        (New-Object System.Drawing.Point 18, 234),
        (New-Object System.Drawing.Point 50, 202),
        (New-Object System.Drawing.Point 82, 278),
        (New-Object System.Drawing.Point 114, 204),
        (New-Object System.Drawing.Point 146, 238)
    )
    $g.DrawCurve($pen, $points, 0.55)
    $titleFont = New-Object System.Drawing.Font "Segoe UI Black", 28, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $smallFont = New-Object System.Drawing.Font "Segoe UI", 13, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    $white = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(245, 243, 236))
    $green = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(94, 224, 167))
    $g.DrawString("Wav", $titleFont, $white, 18, 36)
    $g.DrawString("Crusher", $titleFont, $white, 18, 68)
    $g.DrawString("Verified", $smallFont, $green, 20, 124)
    $g.DrawString("lossless WAV", $smallFont, $green, 20, 144)
    $g.DrawString("archiving", $smallFont, $green, 20, 164)
    $dialog.Save($dialogPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $dialog.Dispose()

    $banner = New-Object System.Drawing.Bitmap 493, 58
    $g = [System.Drawing.Graphics]::FromImage($banner)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.FillRectangle($paperBrush, 0, 0, 493, 58)
    $bannerBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Rectangle 0, 0, 164, 58),
        [System.Drawing.Color]::FromArgb(8, 50, 36),
        [System.Drawing.Color]::FromArgb(18, 24, 22),
        0)
    $g.FillRectangle($bannerBrush, 0, 0, 164, 58)
    $g.FillRectangle($dividerBrush, 164, 0, 4, 58)
    $font = New-Object System.Drawing.Font "Segoe UI Black", 18, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $g.DrawString("WavCrusher", $font, $white, 20, 15)
    $g.FillEllipse($green, 132, 18, 20, 20)
    $banner.Save($bannerPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $banner.Dispose()

    return @{
        Dialog = $dialogPath
        Banner = $bannerPath
    }
}

function ConvertTo-RtfText([string]$Value) {
    $escaped = $Value.Replace('\', '\\').Replace('{', '\{').Replace('}', '\}')
    $escaped = $escaped -replace "(`r`n|`n|`r)", "\par`r`n"
    return $escaped
}

function New-InstallerTextAssets([string]$InstallerRoot, [string]$PayloadRoot) {
    $null = New-Item -Path $InstallerRoot -ItemType Directory -Force

    $licenseText = @"
MIT License

Copyright (c) 2026 WavCrusher contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"@

    $licensePath = Join-Path $InstallerRoot "WavCrusher-MIT-License.rtf"
    $rtf = "{\rtf1\ansi\deff0{\fonttbl{\f0 Segoe UI;}}\fs18 " + (ConvertTo-RtfText $licenseText) + "}"
    Set-Content -LiteralPath $licensePath -Value $rtf -Encoding ASCII

    $sourceReadme = Join-Path $PSScriptRoot "README.md"
    $payloadReadme = Join-Path $PayloadRoot $readmeFileName
    if (Test-Path -LiteralPath $sourceReadme) {
        Copy-Item -LiteralPath $sourceReadme -Destination $payloadReadme -Force
    }
    else {
        $readme = @"
# WavCrusher

WavCrusher is a local Windows desktop application for archiving large WAV collections into standard pure-lossless WavPack files.

Author: Aryn Mikel Sparks
Email: Aryn.sparks1987@gmail.com
License: MIT

Verified items have been restored and compared byte-for-byte before being reported as successful.
"@
        Set-Content -LiteralPath $payloadReadme -Value $readme -Encoding UTF8
    }

    $localizationPath = Join-Path $InstallerRoot "WavCrusher.en-us.wxl"
    $welcomeDescription = "Archive large WAV collections into verified pure-lossless WavPack files. Author: Aryn Mikel Sparks. Click Next to continue."
    $wxl = @"
<WixLocalization Culture="en-US" xmlns="http://wixtoolset.org/schemas/v4/wxl">
  <String Id="WelcomeDlgTitle" Value="Welcome to [ProductName] Setup" />
  <String Id="WelcomeDlgDescription" Value="$(ConvertTo-WixXml $welcomeDescription)" />
  <String Id="VerifyReadyDlgInstallTitle" Value="Ready to install [ProductName]" />
  <String Id="VerifyReadyDlgInstallText" Value="Setup is ready to install [ProductName]. Click Install to begin copying files." />
</WixLocalization>
"@
    Set-Content -LiteralPath $localizationPath -Value $wxl -Encoding UTF8

    return @{
        License = $licensePath
        Localization = $localizationPath
        Readme = $payloadReadme
    }
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
    Write-Host "Installing WiX Toolset $wixToolsetVersion locally under artifacts\tools..."
    & dotnet tool install wix --version $wixToolsetVersion --tool-path $ToolsRoot | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install WiX Toolset with dotnet tool."
    }

    return $localWix
}

function Ensure-WixExtension([string]$WixCommand, [string]$ExtensionName) {
    $installedExtensions = & $WixCommand extension list 2>$null
    if ($LASTEXITCODE -eq 0 -and ($installedExtensions -match [regex]::Escape($ExtensionName))) {
        return
    }

    Write-Host "Installing WiX extension $ExtensionName $wixToolsetVersion..."
    & $WixCommand extension add "$ExtensionName/$wixToolsetVersion" | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install WiX extension $ExtensionName."
    }
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
    [hashtable]$TextAssets,
    [hashtable]$UiBitmaps,
    [string]$ReleaseVersion,
    [string]$MsiVersion,
    [string]$WixCommand
) {
    $null = New-Item -Path $InstallerRoot -ItemType Directory -Force
    $wxsPath = Join-Path $InstallerRoot "WavCrusher.Installer.wxs"
    $artifactVersion = ConvertTo-ArtifactVersion $ReleaseVersion
    $msiPath = Join-Path $InstallerRoot "WavCrusher.Setup.$artifactVersion.msi"

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
    $readmePath = Join-Path $PayloadRoot $readmeFileName
    $mainExeFileId = $null
    $readmeFileId = $null
    $fileIndex = 0

    foreach ($file in Get-ChildItem -LiteralPath $PayloadRoot -File -Recurse | Sort-Object FullName) {
        $fileIndex++
        $relativeDirectory = Get-RelativePath $PayloadRoot $file.DirectoryName
        $directoryId = if ($relativeDirectory -eq ".") { "INSTALLFOLDER" } else { $directoryIds[$relativeDirectory] }
        $componentId = ConvertTo-WixId "cmp_$fileIndex`_$($file.BaseName)"
        $fileId = ConvertTo-WixId "fil_$fileIndex`_$($file.BaseName)"

        if ($file.FullName -ieq $mainExePath) {
            $mainExeFileId = $fileId
        }

        if ($file.FullName -ieq $readmePath) {
            $readmeFileId = $fileId
        }

        [void]$componentXml.AppendLine("    <DirectoryRef Id=`"$directoryId`">")
        [void]$componentXml.AppendLine("      <Component Id=`"$componentId`" Bitness=`"always64`" Guid=`"*`">")
        [void]$componentXml.AppendLine("        <File Id=`"$fileId`" Source=`"$(ConvertTo-WixXml $file.FullName)`" KeyPath=`"yes`" />")
        [void]$componentXml.AppendLine("      </Component>")
        [void]$componentXml.AppendLine("    </DirectoryRef>")
        [void]$componentRefs.AppendLine("      <ComponentRef Id=`"$componentId`" />")
    }

    if ($null -eq $mainExeFileId) {
        throw "Published executable was not found for installer packaging: $mainExePath"
    }

    if ($null -eq $readmeFileId) {
        throw "Installed README was not found for installer packaging: $readmePath"
    }

    $wxs = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs"
     xmlns:ui="http://wixtoolset.org/schemas/v4/wxs/ui">
  <Package Name="$productName" Manufacturer="$(ConvertTo-WixXml $Manufacturer)" Version="$MsiVersion" UpgradeCode="$upgradeCode" Scope="perMachine">
    <MajorUpgrade AllowDowngrades="yes" />
    <MediaTemplate EmbedCab="yes" />
    <Icon Id="WavCrusherIcon" SourceFile="$(ConvertTo-WixXml $IconPath)" />
    <Property Id="ARPPRODUCTICON" Value="WavCrusherIcon" />
    <Property Id="WIXUI_EXITDIALOGOPTIONALTEXT" Value="Setup has finished installing WavCrusher. You can launch the app and open the README after closing this installer." />
    <Property Id="WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT" Value="Launch WavCrusher and open README" />
    <Property Id="WixShellExecTarget" Value="[#${readmeFileId}]" />
    <WixVariable Id="WixUILicenseRtf" Value="$(ConvertTo-WixXml $TextAssets.License)" />
    <WixVariable Id="WixUIDialogBmp" Value="$(ConvertTo-WixXml $UiBitmaps.Dialog)" />
    <WixVariable Id="WixUIBannerBmp" Value="$(ConvertTo-WixXml $UiBitmaps.Banner)" />

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

    <CustomAction Id="LaunchApplication" FileRef="$mainExeFileId" ExeCommand="" Execute="immediate" Return="asyncNoWait" Impersonate="yes" />
    <CustomAction Id="OpenReadme" BinaryRef="Wix4UtilCA_`$(sys.BUILDARCHSHORT)" DllEntry="WixShellExec" Execute="immediate" Return="ignore" />

    <Feature Id="MainFeature" Title="$productName" Level="1">
$($componentRefs.ToString().TrimEnd())
      <ComponentRef Id="cmp_DesktopShortcut" />
    </Feature>

    <ui:WixUI Id="WixUI_InstallDir" InstallDirectory="INSTALLFOLDER" />
    <UIRef Id="WixUI_ErrorProgressText" />
    <UI>
      <Publish Dialog="ExitDialog" Control="Finish" Event="DoAction" Value="LaunchApplication" Order="1" Condition="WIXUI_EXITDIALOGOPTIONALCHECKBOX = 1 AND NOT Installed" />
      <Publish Dialog="ExitDialog" Control="Finish" Event="DoAction" Value="OpenReadme" Order="2" Condition="WIXUI_EXITDIALOGOPTIONALCHECKBOX = 1 AND NOT Installed" />
    </UI>
  </Package>
</Wix>
"@

    Set-Content -LiteralPath $wxsPath -Value $wxs -Encoding UTF8

    Ensure-WixExtension $WixCommand "WixToolset.UI.wixext"
    Ensure-WixExtension $WixCommand "WixToolset.Util.wixext"

    & $WixCommand build $wxsPath `
        -arch x64 `
        -ext WixToolset.UI.wixext `
        -ext WixToolset.Util.wixext `
        -culture en-US `
        -loc $TextAssets.Localization `
        -o $msiPath

    if ($LASTEXITCODE -ne 0) {
        throw "WiX MSI build failed with exit code $LASTEXITCODE."
    }

    return $msiPath
}

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "WinForms project not found at '$ProjectPath'."
}

$releaseVersion = Read-ReleaseVersion $ProductVersion
$msiProductVersion = ConvertTo-MsiProductVersion $releaseVersion

$null = New-Item -Path $OutputPath -ItemType Directory -Force
$null = New-Item -Path $InstallerOutputPath -ItemType Directory -Force
$assetRoot = Join-Path $PSScriptRoot "artifacts\assets"
$assetPath = Join-Path $assetRoot "wavcrusher.ico"
New-WavCrusherIcon $assetPath
$uiBitmaps = New-WixUiBitmaps $assetRoot

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
Write-Host ("  Release version: {0}" -f $releaseVersion)
Write-Host ("  MSI product version: {0}" -f $msiProductVersion)

& dotnet @dotnetArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Copy-WavPackSidecars $OutputPath
$textAssets = New-InstallerTextAssets $InstallerOutputPath $OutputPath

$expectedExe = Join-Path $OutputPath $mainExeName
if (-not (Test-Path -LiteralPath $expectedExe)) {
    throw "Build completed, but '$expectedExe' was not found."
}

Write-Host "Publish complete."
Write-Host "Exe available at: $expectedExe"

if (-not $SkipInstaller.IsPresent) {
    $wixToolsRoot = Join-Path $PSScriptRoot "artifacts\tools\wix5"
    $wixCommand = Get-WixCommand $wixToolsRoot
    $msiPath = New-MsiPackage $OutputPath $InstallerOutputPath $assetPath $textAssets $uiBitmaps $releaseVersion $msiProductVersion $wixCommand
    Write-Host "MSI installer available at: $msiPath"
    Write-Host "Installer target folder: C:\Program Files\$installFolderName"
}

if ($RunAfterBuild.IsPresent) {
    Write-Host "Launching WinForms executable..."
    Start-Process -FilePath $expectedExe
}
