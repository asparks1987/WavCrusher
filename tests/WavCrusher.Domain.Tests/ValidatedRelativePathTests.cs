using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class ValidatedRelativePathTests
{
    [Fact]
    public void Create_NormalizesBackslashesToManifestSeparators()
    {
        var path = ValidatedRelativePath.Create(@"Album\Track 01.wav");

        Assert.Equal("Album/Track 01.wav", path.Value);
    }

    [Theory]
    [InlineData("../Track.wav")]
    [InlineData("Album/../Track.wav")]
    [InlineData("/Album/Track.wav")]
    [InlineData(@"C:\Album\Track.wav")]
    [InlineData(@"C:Album\Track.wav")]
    [InlineData("Album/Track:wav")]
    [InlineData("Album//Track.wav")]
    [InlineData(".wavcrusher/journals/file.jsonl")]
    public void Create_RejectsUnsafeRelativePaths(string value)
    {
        Assert.Throws<ArgumentException>(() => ValidatedRelativePath.Create(value));
    }

    [Fact]
    public void ChangeExtension_ChangesOnlyFinalExtension()
    {
        var source = ValidatedRelativePath.Create("Album/mix.final.WAV");

        Assert.Equal("Album/mix.final.wv", source.ChangeExtension(".wv").Value);
    }
}
