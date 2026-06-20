Param(
    [string]$Configuration = "Release",
    [string]$ProjectPath = (Join-Path $PSScriptRoot "src\WavCrusher.WinForms\WavCrusher.WinForms.csproj"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "artifacts\WavCrusher.WinForms"),
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$NoRestore,
    [switch]$RunAfterBuild
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "WinForms project not found at '$ProjectPath'."
}

$outDir = New-Object System.IO.DirectoryInfo $OutputPath
if (-not $outDir.Exists) {
    $null = New-Item -Path $OutputPath -ItemType Directory -Force
}

$dotnetArgs = @(
    "publish",
    $ProjectPath,
    "-c", $Configuration,
    "-r", $RuntimeIdentifier,
    "--output", $OutputPath,
    "-p:UseAppHost=true",
    "-p:PublishSingleFile=false"
)

if ($NoRestore) {
    $dotnetArgs += "--no-restore"
}

Write-Host "Publishing WavCrusher WinForms project from source..."
Write-Host ("  Project: {0}" -f $ProjectPath)
Write-Host ("  Configuration: {0}" -f $Configuration)
Write-Host ("  Output: {0}" -f $OutputPath)

& dotnet @dotnetArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$expectedExe = Join-Path $OutputPath "WavCrusher.WinForms.exe"
if (-not (Test-Path -LiteralPath $expectedExe)) {
    throw "Build completed, but '$expectedExe' was not found."
}

Write-Host "Publish complete."
Write-Host "Exe available at: $expectedExe"

if ($RunAfterBuild.IsPresent) {
    Write-Host "Launching WinForms executable..."
    Start-Process -FilePath $expectedExe
}
