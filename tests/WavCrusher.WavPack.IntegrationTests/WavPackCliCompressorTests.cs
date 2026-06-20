using System.Security.Cryptography;
using System.Text;
using WavCrusher.Application.Archiving;

namespace WavCrusher.WavPack.IntegrationTests;

public sealed class WavPackCliCompressorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "WavCrusherCompressionTests", Guid.NewGuid().ToString("N"));

    public WavPackCliCompressorTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task CompressAsyncCreatesVerifiedWavPackFileWithoutChangingSource()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoot = Path.Combine(_root, "source");
        var destinationRoot = Path.Combine(_root, "destination");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(destinationRoot);

        var sourcePath = Path.Combine(sourceRoot, "tone.wav");
        var destinationPath = Path.Combine(destinationRoot, "tone.wv");
        CreateWave(sourcePath);
        var sourceHashBefore = Sha256(sourcePath);

        var compressor = new WavPackCliCompressor(
            Path.Combine(repositoryRoot, "third_party", "wavpack", "win-x64", "wavpack.exe"),
            Path.Combine(repositoryRoot, "third_party", "wavpack", "win-x64", "wvunpack.exe"));

        var result = await compressor.CompressAsync(
            new ArchiveRequest(Guid.NewGuid(), sourcePath, destinationPath),
            progress: null,
            CancellationToken.None);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal("Verified", result.Status);
        Assert.True(File.Exists(destinationPath));
        Assert.Equal(sourceHashBefore, Sha256(sourcePath));
        Assert.Equal(result.SourceSha256, result.RestoredSha256);
        Assert.NotNull(result.ArchiveSha256);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static void CreateWave(string path)
    {
        const int sampleRate = 8000;
        const int sampleCount = 8000;
        const short channels = 1;
        const short bitsPerSample = 16;
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
            writer.Write((short)(9000 * Math.Sin(2 * Math.PI * 440 * i / sampleRate)));
        }
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string FindRepositoryRoot()
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
}
