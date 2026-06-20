using WavCrusher.Domain;

namespace WavCrusher.Application.Archiving;

public static class TarballPackagePaths
{
    public const string ManifestFileName = "wavcrusher-manifest.v1.json";
    public const string ManifestFormat = "wavcrusher-tarball";
    public const string PackageFileExtension = ".tar.gz";

    private const string PackageNamePrefix = "wavcrusher";
    private const string ArchiveDirectory = "archives";

    public static string GetArchiveEntryPath(string sourceRelativePath)
    {
        var normalizedRelative = ValidatedRelativePath.Create(sourceRelativePath.Replace('\\', '/'));

        var lastSeparator = normalizedRelative.Value.LastIndexOf('/');
        var directory = lastSeparator <= 0
            ? string.Empty
            : normalizedRelative.Value[..lastSeparator];

        var fileName = lastSeparator < 0
            ? normalizedRelative.Value
            : normalizedRelative.Value[(lastSeparator + 1)..];

        var archiveFileName = Path.GetFileNameWithoutExtension(fileName) + ".wv";
        return string.IsNullOrEmpty(directory)
            ? $"{ArchiveDirectory}/{archiveFileName}"
            : $"{ArchiveDirectory}/{directory}/{archiveFileName}";
    }

    public static string BuildPackagePath(
        string sourceRoot,
        string outputRoot,
        DateTime utcNow,
        Func<string, bool>? fileExists = null)
    {
        var rootForName = Path.GetFileName(sourceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var safeBase = string.IsNullOrWhiteSpace(rootForName)
            ? PackageNamePrefix
            : string.Join("_", rootForName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        fileExists ??= File.Exists;
        var timestamp = utcNow.ToString("yyyyMMdd-HHmmss'Z'", System.Globalization.CultureInfo.InvariantCulture);

        var candidate = Path.Combine(outputRoot, $"{safeBase}.{PackageNamePrefix}.{timestamp}{PackageFileExtension}");
        if (!fileExists(candidate))
        {
            return candidate;
        }

        for (var attempt = 2;; attempt++)
        {
            var withSuffix = Path.Combine(
                outputRoot,
                $"{safeBase}.{PackageNamePrefix}.{timestamp}-{attempt}{PackageFileExtension}");

            if (!fileExists(withSuffix))
            {
                return withSuffix;
            }
        }
    }

    public static string BuildIntermediateTarPath(string packageStagingRoot, string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageStagingRoot))
        {
            throw new ArgumentException("Package staging root is required.", nameof(packageStagingRoot));
        }

        if (string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException("Package id is required.", nameof(packageId));
        }

        var stagingRoot = Path.GetFullPath(packageStagingRoot);
        var parent = Directory.GetParent(stagingRoot)?.FullName
            ?? throw new ArgumentException("Package staging root must have a parent directory.", nameof(packageStagingRoot));

        var safePackageId = string.Join("_", packageId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return Path.Combine(parent, $"{safePackageId}.package.tar");
    }

    public static bool LooksLikeSafeArchiveRelativePath(string archiveRelativePath)
    {
        if (string.IsNullOrWhiteSpace(archiveRelativePath))
        {
            return false;
        }

        if (archiveRelativePath.Contains('\\'))
        {
            return false;
        }

        const string prefix = ArchiveDirectory + "/";
        if (!archiveRelativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var nestedPath = archiveRelativePath[prefix.Length..];
        try
        {
            _ = ValidatedRelativePath.Create(nestedPath);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
