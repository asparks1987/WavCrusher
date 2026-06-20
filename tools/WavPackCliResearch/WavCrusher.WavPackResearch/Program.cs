using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var repositoryRoot = args.Length > 0 ? Path.GetFullPath(args[0]) : FindRepositoryRoot();
var workRoot = Path.Combine(repositoryRoot, "artifacts", "wavpack-research");
Directory.CreateDirectory(workRoot);

var wavpack = Path.Combine(repositoryRoot, "third_party", "wavpack", "win-x64", "wavpack.exe");
var wvunpack = Path.Combine(repositoryRoot, "third_party", "wavpack", "win-x64", "wvunpack.exe");
if (!File.Exists(wavpack) || !File.Exists(wvunpack))
{
    throw new FileNotFoundException("WavPack sidecars are missing under third_party/wavpack/win-x64.");
}

var source = Path.Combine(workRoot, "sine 440 & metachar test.wav");
var archive = Path.Combine(workRoot, "sine 440 & metachar test.research.partial.wv");
var restored = Path.Combine(workRoot, "sine 440 & metachar test.restored.wav");
DeleteIfExists(source);
DeleteIfExists(archive);
DeleteIfExists(restored);

CreateResearchWave(source);
var sourceHashBefore = Sha256(source);

var encode = await RunProcessAsync(
    wavpack,
    ["-hh", "-x6", "-m", "-v", "-t", "-z0", "--no-overwrite", source, archive],
    workRoot);

var decode = await RunProcessAsync(
    wvunpack,
    [archive, "-o", restored],
    workRoot);

var sourceHashAfter = Sha256(source);
var restoredHash = File.Exists(restored) ? Sha256(restored) : null;

var result = new
{
    Source = source,
    Archive = archive,
    Restored = restored,
    SourceHashBefore = sourceHashBefore,
    SourceHashAfter = sourceHashAfter,
    RestoredHash = restoredHash,
    SourceUnchanged = sourceHashBefore == sourceHashAfter,
    RoundTripMatched = sourceHashBefore == restoredHash,
    Encode = encode,
    Decode = decode
};

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

static async Task<object> RunProcessAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
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
    var stopwatch = Stopwatch.StartNew();
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    stopwatch.Stop();

    return new
    {
        FileName = fileName,
        Arguments = arguments,
        process.ExitCode,
        DurationMilliseconds = stopwatch.ElapsedMilliseconds,
        Stdout = await stdout,
        Stderr = await stderr
    };
}

static void CreateResearchWave(string path)
{
    const int sampleRate = 8000;
    const int seconds = 1;
    const short channels = 1;
    const short bitsPerSample = 16;
    var sampleCount = sampleRate * seconds;
    var dataBytes = sampleCount * channels * (bitsPerSample / 8);

    using var stream = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: false);
    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
    writer.Write(36 + dataBytes);
    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
    writer.Write(Encoding.ASCII.GetBytes("fmt "));
    writer.Write(16);
    writer.Write((short)1);
    writer.Write(channels);
    writer.Write(sampleRate);
    writer.Write(sampleRate * channels * (bitsPerSample / 8));
    writer.Write((short)(channels * (bitsPerSample / 8)));
    writer.Write(bitsPerSample);
    writer.Write(Encoding.ASCII.GetBytes("data"));
    writer.Write(dataBytes);

    for (var i = 0; i < sampleCount; i++)
    {
        var sample = (short)(12000 * Math.Sin(2 * Math.PI * 440 * i / sampleRate));
        writer.Write(sample);
    }
}

static string Sha256(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static void DeleteIfExists(string path)
{
    if (File.Exists(path))
    {
        File.Delete(path);
    }
}

static string FindRepositoryRoot()
{
    var current = AppContext.BaseDirectory;
    while (!string.IsNullOrEmpty(current))
    {
        if (File.Exists(Path.Combine(current, "WavCrusher.sln")))
        {
            return current;
        }

        current = Directory.GetParent(current)?.FullName ?? string.Empty;
    }

    throw new InvalidOperationException("Repository root could not be found.");
}
