using WavCrusher.Application.Scanning;

namespace WavCrusher.Infrastructure.Scanning;

public sealed class FileSystemWaveFileScanner : IWaveFileScanner
{
    public Task<IReadOnlyList<WaveFileCandidate>> ScanAsync(
        string sourceRoot,
        bool recursive,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceRoot))
        {
            throw new ArgumentException("A source folder is required.", nameof(sourceRoot));
        }

        var root = Path.GetFullPath(sourceRoot);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"The source folder does not exist: {root}");
        }

        var results = new List<WaveFileCandidate>();
        ScanDirectory(root, root, recursive, results, cancellationToken);

        return Task.FromResult<IReadOnlyList<WaveFileCandidate>>(
            results.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static void ScanDirectory(
        string root,
        string directory,
        bool recursive,
        List<WaveFileCandidate> results,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attributes = File.GetAttributes(directory);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(directory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.Equals(Path.GetExtension(filePath), ".wav", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var info = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(root, filePath).Replace(Path.DirectorySeparatorChar, '/');
            results.Add(new WaveFileCandidate(Guid.NewGuid(), filePath, relativePath, info.Length));
        }

        if (!recursive)
        {
            return;
        }

        foreach (var childDirectory in Directory.EnumerateDirectories(directory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ScanDirectory(root, childDirectory, recursive, results, cancellationToken);
        }
    }
}
