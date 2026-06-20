using System.Text.RegularExpressions;

namespace WavCrusher.Domain;

public sealed partial record ValidatedRelativePath
{
    private ValidatedRelativePath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ValidatedRelativePath Create(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path is required.", nameof(relativePath));
        }

        var normalized = relativePath.Replace('\\', '/');
        if (normalized.StartsWith('/') ||
            normalized.StartsWith("//", StringComparison.Ordinal) ||
            DriveQualifiedRegex().IsMatch(normalized) ||
            normalized.Contains('\0'))
        {
            throw new ArgumentException("Path must be relative and unrooted.", nameof(relativePath));
        }

        var segments = normalized.Split('/');
        if (segments.Length == 0)
        {
            throw new ArgumentException("Relative path is required.", nameof(relativePath));
        }

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (string.IsNullOrEmpty(segment) || segment is "." or "..")
            {
                throw new ArgumentException("Path must not contain empty, current, or parent segments.", nameof(relativePath));
            }

            if (segment.Contains(':') || UnsafeSegmentRegex().IsMatch(segment))
            {
                throw new ArgumentException("Path contains unsafe characters.", nameof(relativePath));
            }

            if (i == 0 && string.Equals(segment, ProductConventions.ReservedMetadataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Path collides with the reserved metadata namespace.", nameof(relativePath));
            }
        }

        return new ValidatedRelativePath(normalized);
    }

    public ValidatedRelativePath ChangeExtension(string newExtension)
    {
        if (string.IsNullOrWhiteSpace(newExtension) || newExtension[0] != '.')
        {
            throw new ArgumentException("Extension must start with a dot.", nameof(newExtension));
        }

        var slashIndex = Value.LastIndexOf('/');
        var fileNameStart = slashIndex + 1;
        var dotIndex = Value.LastIndexOf('.');
        var basePath = dotIndex >= fileNameStart ? Value[..dotIndex] : Value;
        return Create(basePath + newExtension);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Za-z]:", RegexOptions.CultureInvariant)]
    private static partial Regex DriveQualifiedRegex();

    [GeneratedRegex("[<>\"|?*\\x00-\\x1F]", RegexOptions.CultureInvariant)]
    private static partial Regex UnsafeSegmentRegex();
}
