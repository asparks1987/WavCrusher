using WavCrusher.Infrastructure.Scanning;

namespace WavCrusher.Infrastructure.Tests;

public sealed class FileSystemWaveFileScannerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "WavCrusherTests", Guid.NewGuid().ToString("N"));

    public FileSystemWaveFileScannerTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task ScanAsyncFindsWavFilesCaseInsensitively()
    {
        File.WriteAllBytes(Path.Combine(_root, "a.wav"), []);
        File.WriteAllBytes(Path.Combine(_root, "b.WAV"), []);
        File.WriteAllBytes(Path.Combine(_root, "c.txt"), []);

        var scanner = new FileSystemWaveFileScanner();
        var results = await scanner.ScanAsync(_root, recursive: false, CancellationToken.None);

        Assert.Equal(new[] { "a.wav", "b.WAV" }, results.Select(item => item.RelativePath).ToArray());
    }

    [Fact]
    public async Task ScanAsyncHonorsRecursiveOption()
    {
        var child = Path.Combine(_root, "child");
        Directory.CreateDirectory(child);
        File.WriteAllBytes(Path.Combine(_root, "root.wav"), []);
        File.WriteAllBytes(Path.Combine(child, "nested.wav"), []);

        var scanner = new FileSystemWaveFileScanner();

        var shallow = await scanner.ScanAsync(_root, recursive: false, CancellationToken.None);
        var recursive = await scanner.ScanAsync(_root, recursive: true, CancellationToken.None);

        Assert.Equal("root.wav", Assert.Single(shallow).RelativePath);
        Assert.Equal(new[] { "child/nested.wav", "root.wav" }, recursive.Select(item => item.RelativePath).ToArray());
    }

    [Fact]
    public async Task ScanAsyncRecursesIntoAllDirectories()
    {
        var child = Path.Combine(_root, "child");
        var packed = Path.Combine(child, "compressed");
        Directory.CreateDirectory(packed);
        File.WriteAllBytes(Path.Combine(child, "original.wav"), []);
        File.WriteAllBytes(Path.Combine(packed, "restored.wav"), []);

        var scanner = new FileSystemWaveFileScanner();
        var results = await scanner.ScanAsync(_root, recursive: true, CancellationToken.None);

        Assert.Equal(new[] { "child/compressed/restored.wav", "child/original.wav" }, results.Select(item => item.RelativePath).OrderBy(x => x).ToArray());
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
