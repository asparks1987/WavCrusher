namespace WavCrusher.Domain;

public sealed record NormalizedAbsolutePath
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private NormalizedAbsolutePath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static NormalizedAbsolutePath Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is required.", nameof(path));
        }

        var full = Path.GetFullPath(path);
        if (!Path.IsPathFullyQualified(full))
        {
            throw new ArgumentException("Path must be absolute and fully qualified.", nameof(path));
        }

        return new NormalizedAbsolutePath(TrimTrailingSeparators(full));
    }

    public bool Contains(NormalizedAbsolutePath candidate)
    {
        if (Equals(candidate))
        {
            return true;
        }

        var root = Value.EndsWith(Path.DirectorySeparatorChar)
            ? Value
            : Value + Path.DirectorySeparatorChar;

        return candidate.Value.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(NormalizedAbsolutePath? other) =>
        other is not null && Comparer.Equals(Value, other.Value);

    public override int GetHashCode() => Comparer.GetHashCode(Value);

    public override string ToString() => Value;

    private static string TrimTrailingSeparators(string path)
    {
        var root = Path.GetPathRoot(path);
        var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.IsNullOrEmpty(trimmed) || Comparer.Equals(trimmed, root?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            ? path
            : trimmed;
    }
}
