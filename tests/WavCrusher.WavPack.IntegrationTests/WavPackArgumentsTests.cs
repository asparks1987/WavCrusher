using WavCrusher.Application.WavPack;
using WavCrusher.Domain;

namespace WavCrusher.WavPack.IntegrationTests;

public sealed class WavPackArgumentsTests
{
    [Fact]
    public void BuildEncodeArguments_UsesExactPureLosslessProfile()
    {
        var request = new WavPackEncodeRequest(
            NormalizedAbsolutePath.Create(@"C:\Tools\wavpack.exe"),
            NormalizedAbsolutePath.Create(@"C:\Audio Source\Album & Mix\Track 01.wav"),
            NormalizedAbsolutePath.Create(@"D:\Archive Dest\Album & Mix\Track 01.op-123.partial.wv"),
            ArchiveProfile.PureLosslessHighCompressionV1,
            Tool());

        Assert.Equal(
            new[]
            {
                "-hh",
                "-x6",
                "-m",
                "-v",
                "-t",
                "-z0",
                "--no-overwrite",
                @"C:\Audio Source\Album & Mix\Track 01.wav",
                @"D:\Archive Dest\Album & Mix\Track 01.op-123.partial.wv"
            },
            WavPackArguments.BuildEncodeArguments(request));
    }

    [Fact]
    public void BuildDecodeArguments_DoesNotForceWrapperOrRawOutput()
    {
        var request = new WavPackDecodeRequest(
            NormalizedAbsolutePath.Create(@"C:\Tools\wvunpack.exe"),
            NormalizedAbsolutePath.Create(@"D:\Archive\Track.wv.partial"),
            NormalizedAbsolutePath.Create(@"E:\Temp\Track.restored.wav"),
            Tool());

        var arguments = WavPackArguments.BuildDecodeArguments(request);

        Assert.Equal(new[] { @"D:\Archive\Track.wv.partial", "-o", @"E:\Temp\Track.restored.wav" }, arguments);
        Assert.DoesNotContain("--wav", arguments);
        Assert.DoesNotContain("--raw", arguments);
    }

    private static ToolIdentity Tool() => new(
        "wavpack.exe",
        "5.9.0",
        HashDigest.Sha256("e26ad2ed3c8e417bd62e0b8eb4ee9b9e2f261a859c9cc18c99026e2b7f8fc661"),
        "official-win64",
        "third_party/wavpack/dependency.json");
}
