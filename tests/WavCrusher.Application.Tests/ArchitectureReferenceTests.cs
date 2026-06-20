using System.Xml.Linq;

namespace WavCrusher.Application.Tests;

public sealed class ArchitectureReferenceTests
{
    [Fact]
    public void ProjectReferences_FollowDocumentedLayerDirection()
    {
        var root = FindRepositoryRoot();
        var references = Directory
            .GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path)!, ReadProjectReferences, StringComparer.OrdinalIgnoreCase);

        Assert.Empty(references["WavCrusher.Domain"]);
        Assert.Equal(new[] { "WavCrusher.Domain" }, references["WavCrusher.Application"]);
        Assert.Equal(new[] { "WavCrusher.Application" }, references["WavCrusher.Infrastructure"]);
        Assert.Equal(new[] { "WavCrusher.Application" }, references["WavCrusher.WavPack"]);
        Assert.Equal(
            new[] { "WavCrusher.Application", "WavCrusher.Infrastructure", "WavCrusher.WavPack" },
            references["WavCrusher.WinForms"]);
    }

    private static string[] ReadProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var document = XDocument.Load(projectPath);

        return document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFileNameWithoutExtension(Path.GetFullPath(Path.Combine(projectDirectory, value!))))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
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
