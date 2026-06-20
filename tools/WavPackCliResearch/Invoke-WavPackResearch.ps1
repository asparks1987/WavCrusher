param(
    [string]$RepositoryRoot = (Resolve-Path "$PSScriptRoot\..\..").Path,
    [string]$WorkRoot = (Join-Path $RepositoryRoot "artifacts\wavpack-research")
)

$ErrorActionPreference = "Stop"

function Invoke-Process {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.WorkingDirectory = $WorkingDirectory
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    [pscustomobject]@{
        FileName = $FileName
        Arguments = $Arguments
        ExitCode = $process.ExitCode
        Stdout = $stdoutTask.GetAwaiter().GetResult()
        Stderr = $stderrTask.GetAwaiter().GetResult()
    }
}

function New-ResearchWave {
    param([Parameter(Mandatory = $true)][string]$Path)

    $sampleRate = 8000
    $seconds = 1
    $channels = 1
    $bitsPerSample = 16
    $sampleCount = $sampleRate * $seconds
    $dataBytes = $sampleCount * $channels * ($bitsPerSample / 8)

    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try {
        $writer = [System.IO.BinaryWriter]::new($stream, [System.Text.Encoding]::ASCII, $true)
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("RIFF"))
        $writer.Write([int](36 + $dataBytes))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("WAVE"))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("fmt "))
        $writer.Write([int]16)
        $writer.Write([int16]1)
        $writer.Write([int16]$channels)
        $writer.Write([int]$sampleRate)
        $writer.Write([int]($sampleRate * $channels * ($bitsPerSample / 8)))
        $writer.Write([int16]($channels * ($bitsPerSample / 8)))
        $writer.Write([int16]$bitsPerSample)
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("data"))
        $writer.Write([int]$dataBytes)

        for ($i = 0; $i -lt $sampleCount; $i++) {
            $sample = [int16](12000 * [Math]::Sin(2 * [Math]::PI * 440 * $i / $sampleRate))
            $writer.Write($sample)
        }
    }
    finally {
        $stream.Dispose()
    }
}

$wavpack = Join-Path $RepositoryRoot "third_party\wavpack\win-x64\wavpack.exe"
$wvunpack = Join-Path $RepositoryRoot "third_party\wavpack\win-x64\wvunpack.exe"
if (-not (Test-Path $wavpack) -or -not (Test-Path $wvunpack)) {
    throw "WavPack sidecars are missing. Expected tools under third_party\wavpack\win-x64."
}

New-Item -ItemType Directory -Force -Path $WorkRoot | Out-Null
$source = Join-Path $WorkRoot "sine 440 & metachar test.wav"
$archive = Join-Path $WorkRoot "sine 440 & metachar test.wv.research.partial"
$restored = Join-Path $WorkRoot "sine 440 & metachar test.restored.wav"
Remove-Item -LiteralPath $source, $archive, $restored -Force -ErrorAction SilentlyContinue

New-ResearchWave -Path $source
$sourceHashBefore = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()

$encode = Invoke-Process `
    -FileName $wavpack `
    -Arguments @("-hh", "-x6", "-m", "-v", "-t", "-z0", "--no-overwrite", $source, $archive) `
    -WorkingDirectory $WorkRoot

$decode = Invoke-Process `
    -FileName $wvunpack `
    -Arguments @($archive, "-o", $restored) `
    -WorkingDirectory $WorkRoot

$sourceHashAfter = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant()
$restoredHash = (Get-FileHash -LiteralPath $restored -Algorithm SHA256).Hash.ToLowerInvariant()

[pscustomobject]@{
    Source = $source
    Archive = $archive
    Restored = $restored
    SourceHashBefore = $sourceHashBefore
    SourceHashAfter = $sourceHashAfter
    RestoredHash = $restoredHash
    SourceUnchanged = $sourceHashBefore -eq $sourceHashAfter
    RoundTripMatched = $sourceHashBefore -eq $restoredHash
    EncodeExitCode = $encode.ExitCode
    DecodeExitCode = $decode.ExitCode
    EncodeArguments = $encode.Arguments
    DecodeArguments = $decode.Arguments
    EncodeStdout = $encode.Stdout
    EncodeStderr = $encode.Stderr
    DecodeStdout = $decode.Stdout
    DecodeStderr = $decode.Stderr
} | ConvertTo-Json -Depth 5
