using System.Security.Cryptography;
using System.Text.Json;

namespace WavCrusher.WavPack.IntegrationTests;

public sealed class WavPackDependencyMetadataTests
{
    [Fact]
    public void DependencyMetadataMatchesBundledSidecars()
    {
        var root = FindRepositoryRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "third_party", "wavpack", "dependency.json")));
        var tools = document.RootElement.GetProperty("tools").EnumerateArray();

        foreach (var tool in tools)
        {
            var relativePath = tool.GetProperty("relativePath").GetString()!;
            var expectedHash = tool.GetProperty("sha256").GetProperty("hex").GetString();
            var fullPath = Path.Combine(root, "third_party", "wavpack", relativePath.Replace('/', Path.DirectorySeparatorChar));

            Assert.True(File.Exists(fullPath), $"Missing WavPack sidecar: {relativePath}");
            Assert.Equal(expectedHash, Sha256(fullPath));
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
