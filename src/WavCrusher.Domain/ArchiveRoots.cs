namespace WavCrusher.Domain;

public sealed record ArchiveRoots(NormalizedAbsolutePath SourceRoot, NormalizedAbsolutePath DestinationRoot)
{
    public static ArchiveRoots Create(NormalizedAbsolutePath sourceRoot, NormalizedAbsolutePath destinationRoot)
    {
        if (sourceRoot.Equals(destinationRoot))
        {
            throw new ArgumentException("Source and destination roots must be different.", nameof(destinationRoot));
        }

        if (sourceRoot.Contains(destinationRoot))
        {
            throw new ArgumentException("Destination root must not be inside the source root.", nameof(destinationRoot));
        }

        if (destinationRoot.Contains(sourceRoot))
        {
            throw new ArgumentException("Source root must not be inside the destination root.", nameof(sourceRoot));
        }

        return new ArchiveRoots(sourceRoot, destinationRoot);
    }
}
