using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class ArchiveRootsTests
{
    [Fact]
    public void Create_RejectsEqualRootsCaseInsensitively()
    {
        var source = NormalizedAbsolutePath.Create(@"C:\Audio");
        var destination = NormalizedAbsolutePath.Create(@"c:\audio\");

        Assert.Throws<ArgumentException>(() => ArchiveRoots.Create(source, destination));
    }

    [Fact]
    public void Create_RejectsDestinationInsideSource()
    {
        var source = NormalizedAbsolutePath.Create(@"C:\Audio");
        var destination = NormalizedAbsolutePath.Create(@"C:\Audio\Archive");

        Assert.Throws<ArgumentException>(() => ArchiveRoots.Create(source, destination));
    }

    [Fact]
    public void Create_RejectsSourceInsideDestination()
    {
        var source = NormalizedAbsolutePath.Create(@"C:\Audio\Source");
        var destination = NormalizedAbsolutePath.Create(@"C:\Audio");

        Assert.Throws<ArgumentException>(() => ArchiveRoots.Create(source, destination));
    }

    [Fact]
    public void Contains_DoesNotTreatPrefixSiblingAsNested()
    {
        var source = NormalizedAbsolutePath.Create(@"C:\Audio");
        var sibling = NormalizedAbsolutePath.Create(@"C:\Audio2");

        Assert.False(source.Contains(sibling));
    }
}
