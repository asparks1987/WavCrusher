namespace WavCrusher.EndToEndTests;

public sealed class RepositoryBootstrapTests
{
    [Fact]
    public void SolutionAndPinnedDependencyMetadataExist()
    {
        var root = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(root, "WavCrusher.sln")));
        Assert.True(File.Exists(Path.Combine(root, "global.json")));
        Assert.True(File.Exists(Path.Combine(root, "third_party", "wavpack", "dependency.json")));
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
