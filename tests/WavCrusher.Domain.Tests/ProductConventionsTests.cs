using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class ProductConventionsTests
{
    [Fact]
    public void StableIdentifiers_UseWavCrusherName()
    {
        Assert.Equal("WavCrusher", ProductConventions.ProductName);
        Assert.Equal("wavcrusher-archive-manifest", ProductConventions.ManifestFormat);
        Assert.Equal("wavcrusher-operation-journal", ProductConventions.JournalFormat);
        Assert.Equal("wavcrusher-operation-report", ProductConventions.ReportFormat);
        Assert.Equal(".wavcrusher", ProductConventions.ReservedMetadataDirectory);
    }
}
