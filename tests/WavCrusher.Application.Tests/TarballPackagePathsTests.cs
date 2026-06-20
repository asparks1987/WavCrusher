using WavCrusher.Application.Archiving;

using System.Text.Json;

namespace WavCrusher.Application.Tests;

public sealed class TarballPackagePathsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void GetArchiveEntryPath_UsesArchiveDirectoryPrefix_AndWavExtension()
    {
        var path = TarballPackagePaths.GetArchiveEntryPath("Album\\Live\\Track 01.wav");

        Assert.Equal("archives/Album/Live/Track 01.wv", path);
        Assert.DoesNotContain('\\', path);
    }

    [Fact]
    public void BuildPackagePath_SkipsExistingPackage()
    {
        var now = new DateTime(2026, 6, 19, 12, 30, 45, DateTimeKind.Utc);
        var sourceRoot = Path.Combine(Path.GetTempPath(), "source-root");
        var outputRoot = Path.Combine(Path.GetTempPath(), "package-output");
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string Build()
            => TarballPackagePaths.BuildPackagePath(sourceRoot, outputRoot, now, p => existing.Contains(p));

        var first = Build();
        existing.Add(first);

        var second = Build();
        Assert.Equal(Path.Combine(outputRoot, "source-root.wavcrusher.20260619-123045Z.tar.gz"), first);
        Assert.NotEqual(first, second);
        Assert.Contains("-2", second);
    }

    [Fact]
    public void BuildIntermediateTarPath_PlacesTarOutsidePackageStagingRoot()
    {
        var stagingRoot = Path.Combine(Path.GetTempPath(), "WavCrusher", "TarballBuild", "package-id");

        var tarPath = TarballPackagePaths.BuildIntermediateTarPath(stagingRoot, "package-id");
        var relativeToStaging = Path.GetRelativePath(Path.GetFullPath(stagingRoot), Path.GetFullPath(tarPath));

        Assert.Equal(Path.Combine(Path.GetDirectoryName(stagingRoot)!, "package-id.package.tar"), tarPath);
        Assert.StartsWith("..", relativeToStaging, StringComparison.Ordinal);
    }

    [Fact]
    public void ManifestSerializesExpectedFormat_AndPreservesManifestPaths()
    {
        var manifest = new TarballArchiveManifest(
            "wavcrusher-tarball",
            "1.0",
            new DateTime(2026, 6, 19, 12, 31, 0, DateTimeKind.Utc).ToString("O"),
            "package-id",
            "C:\\Music\\Session",
            new TarballManifestSummary(1, 1, 0, 0),
            new[]
            {
                new TarballManifestItem(
                    "Album\\Track 01.wav",
                    "C:\\Music\\Album\\Track 01.wav",
                    123_456,
                    new HashReference("sha256", "a1"),
                    "archives/Album/Track 01.wv",
                    33,
                    new HashReference("sha256", "b1"),
                    new HashReference("sha256", "c1"),
                    "wavpack-pure-lossless-hh-x6-v1",
                    "Verified",
                    null,
                    "OK",
                    new DateTime(2026, 6, 19, 12, 30, 0, DateTimeKind.Utc).ToString("O"),
                    new DateTime(2026, 6, 19, 12, 31, 0, DateTimeKind.Utc).ToString("O"))
            });

        var json = JsonSerializer.Serialize(
            manifest,
            SerializerOptions);
        var parsed = JsonDocument.Parse(json).RootElement;
        var item = parsed.GetProperty("items")[0];
        var archiveRelativePath = item.GetProperty("archiveRelativePath").GetString();
        var sourceRelativePath = item.GetProperty("sourceRelativePath").GetString();

        Assert.Equal("wavcrusher-tarball", parsed.GetProperty("format").GetString());
        Assert.Equal("archives/Album/Track 01.wv", archiveRelativePath);
        Assert.Equal("Album\\Track 01.wav", sourceRelativePath);
        Assert.NotNull(archiveRelativePath);
        Assert.DoesNotContain('\\', archiveRelativePath);
        Assert.Equal("1.0", parsed.GetProperty("version").GetString());
    }

    [Fact]
    public void LooksLikeSafeArchiveRelativePath_RejectsUnsafePaths()
    {
        Assert.True(TarballPackagePaths.LooksLikeSafeArchiveRelativePath("archives/Album/Track.wv"));
        Assert.False(TarballPackagePaths.LooksLikeSafeArchiveRelativePath("archives\\Album\\Track.wv"));
        Assert.False(TarballPackagePaths.LooksLikeSafeArchiveRelativePath("archive/Album/Track.wv"));
        Assert.False(TarballPackagePaths.LooksLikeSafeArchiveRelativePath("archives/../Track.wv"));
    }
}
