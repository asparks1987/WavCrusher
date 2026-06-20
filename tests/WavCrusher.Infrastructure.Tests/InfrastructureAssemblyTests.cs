using WavCrusher.Infrastructure;

namespace WavCrusher.Infrastructure.Tests;

public sealed class InfrastructureAssemblyTests
{
    [Fact]
    public void AssemblyMarkerLoadsInfrastructureAssembly()
    {
        Assert.Equal("WavCrusher.Infrastructure", typeof(AssemblyMarker).Assembly.GetName().Name);
    }
}
