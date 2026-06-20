using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class HashDigestTests
{
    [Fact]
    public void Sha256_AcceptsLowercaseHex()
    {
        var digest = HashDigest.Sha256(new string('a', 64));

        Assert.Equal("sha256", digest.Algorithm);
        Assert.Equal(new string('a', 64), digest.Hex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABCDEFabcdefABCDEFabcdefABCDEFabcdefABCDEFabcdefABCDEFabcdefABCD")]
    [InlineData("xyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzz")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Sha256_RejectsInvalidHex(string hex)
    {
        Assert.Throws<ArgumentException>(() => HashDigest.Sha256(hex));
    }
}
