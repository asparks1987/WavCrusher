using System.Diagnostics;
using System.Security.Cryptography;
using WavCrusher.Application.Archiving;
using WavCrusher.Application.WavPack;
using WavCrusher.Domain;

namespace WavCrusher.WavPack;

public sealed class WavPackCliCompressor : IArchiveCompressor
{
    private const int MaxDiagnosticChars = 12_000;

    private readonly string _wavpackPath;
    private readonly string _wvunpackPath;

    public WavPackCliCompressor(string wavpackPath, string wvunpackPath)
    {
        _wavpackPath = Path.GetFullPath(wavpackPath);
        _wvunpackPath = Path.GetFullPath(wvunpackPath);
    }

    public async Task<ArchiveResult> CompressAsync(
        ArchiveRequest request,
        IProgress<ArchiveItemProgress>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(_wavpackPath))
            {
                return Failure(request, "ToolMissing", $"Missing wavpack.exe: {_wavpackPath}");
            }

            if (!File.Exists(_wvunpackPath))
            {
                return Failure(request, "ToolMissing", $"Missing wvunpack.exe: {_wvunpackPath}");
            }

            var sourcePath = Path.GetFullPath(request.SourcePath);
            var destinationPath = Path.GetFullPath(request.DestinationPath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath)
                ?? throw new InvalidOperationException("Destination path has no parent directory.");

            if (File.Exists(destinationPath))
            {
                return Failure(request, "DestinationConflict", "The destination .wv file already exists.");
            }

            Directory.CreateDirectory(destinationDirectory);
            var operationId = Guid.NewGuid().ToString("N");
            var finalNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationPath);
            var tempArchivePath = Path.Combine(destinationDirectory, $"{finalNameWithoutExtension}.{operationId}.partial.wv");
            var restoreDirectory = Path.Combine(Path.GetTempPath(), "WavCrusher", operationId);
            var restoredPath = Path.Combine(restoreDirectory, Path.GetFileName(sourcePath));

            Directory.CreateDirectory(restoreDirectory);

            try
            {
                progress?.Report(new ArchiveItemProgress(request.ItemId, "Hashing source", sourcePath));
                var sourceHash = await Sha256Async(sourcePath, cancellationToken).ConfigureAwait(false);
                var sourceLength = new FileInfo(sourcePath).Length;

                var encodeRequest = new WavPackEncodeRequest(
                    NormalizedAbsolutePath.Create(_wavpackPath),
                    NormalizedAbsolutePath.Create(sourcePath),
                    NormalizedAbsolutePath.Create(tempArchivePath),
                    ArchiveProfile.PureLosslessHighCompressionV1,
                    PinnedToolIdentity("wavpack.exe"));

                progress?.Report(new ArchiveItemProgress(request.ItemId, "Encoding", Path.GetFileName(sourcePath)));
                var encodeResult = await RunProcessAsync(
                    _wavpackPath,
                    WavPackArguments.BuildEncodeArguments(encodeRequest),
                    destinationDirectory,
                    cancellationToken).ConfigureAwait(false);

                if (encodeResult.ExitCode != 0 || !File.Exists(tempArchivePath))
                {
                    return Failure(request, "EncoderFailed", encodeResult.Diagnostics);
                }

                var decodeRequest = new WavPackDecodeRequest(
                    NormalizedAbsolutePath.Create(_wvunpackPath),
                    NormalizedAbsolutePath.Create(tempArchivePath),
                    NormalizedAbsolutePath.Create(restoredPath),
                    PinnedToolIdentity("wvunpack.exe"));

                progress?.Report(new ArchiveItemProgress(request.ItemId, "Verifying", Path.GetFileName(sourcePath)));
                var decodeResult = await RunProcessAsync(
                    _wvunpackPath,
                    WavPackArguments.BuildDecodeArguments(decodeRequest),
                    restoreDirectory,
                    cancellationToken).ConfigureAwait(false);

                if (decodeResult.ExitCode != 0 || !File.Exists(restoredPath))
                {
                    return Failure(request, "DecoderFailed", decodeResult.Diagnostics);
                }

                var restoredLength = new FileInfo(restoredPath).Length;
                var restoredHash = await Sha256Async(restoredPath, cancellationToken).ConfigureAwait(false);
                if (sourceLength != restoredLength || !string.Equals(sourceHash, restoredHash, StringComparison.Ordinal))
                {
                    return Failure(request, "RoundTripHashMismatch", "Decoded WAV did not match the original file byte-for-byte.");
                }

                progress?.Report(new ArchiveItemProgress(request.ItemId, "Publishing", Path.GetFileName(destinationPath)));
                var archiveHash = await Sha256Async(tempArchivePath, cancellationToken).ConfigureAwait(false);
                File.Move(tempArchivePath, destinationPath, overwrite: false);

                return new ArchiveResult(
                    request.ItemId,
                    sourcePath,
                    destinationPath,
                    Succeeded: true,
                    Status: "Verified",
                    Message: "Compressed and verified byte-for-byte restoration.",
                    sourceHash,
                    restoredHash,
                    archiveHash);
            }
            finally
            {
                DeleteIfOwnedTemp(tempArchivePath);
                if (Directory.Exists(restoreDirectory))
                {
                    Directory.Delete(restoreDirectory, recursive: true);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return Failure(request, "Cancelled", "Compression was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            return Failure(request, "Failed", ex.Message);
        }
    }

    private static ArchiveResult Failure(ArchiveRequest request, string status, string message) =>
        new(
            request.ItemId,
            request.SourcePath,
            request.DestinationPath,
            Succeeded: false,
            status,
            message,
            SourceSha256: null,
            RestoredSha256: null,
            ArchiveSha256: null);

    private static async Task<ProcessRunResult> RunProcessAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKillProcessTree(process);
            throw;
        }

        var diagnostics = string.Concat(await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false));
        if (diagnostics.Length > MaxDiagnosticChars)
        {
            diagnostics = diagnostics[^MaxDiagnosticChars..];
        }

        return new ProcessRunResult(process.ExitCode, diagnostics);
    }

    private static async Task<string> Sha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 128,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static ToolIdentity PinnedToolIdentity(string name)
    {
        var hash = name.Equals("wavpack.exe", StringComparison.OrdinalIgnoreCase)
            ? "e26ad2ed3c8e417bd62e0b8eb4ee9b9e2f261a859c9cc18c99026e2b7f8fc661"
            : "9ed083b1c80392e1035125d1576c39151079ff160fb6bf7635e782186c11aae7";

        return new ToolIdentity(
            name,
            "5.9.0",
            HashDigest.Sha256(hash),
            "official-win64",
            "third_party/wavpack/dependency.json");
    }

    private static void DeleteIfOwnedTemp(string path)
    {
        if (File.Exists(path) && path.EndsWith(".partial.wv", StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(path);
        }
    }

    private static void TryKillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed record ProcessRunResult(int ExitCode, string Diagnostics);
}
