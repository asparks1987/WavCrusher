using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class VerificationEvidenceTests
{
    private static readonly HashDigest SourceHash = HashDigest.Sha256(new string('a', 64));
    private static readonly HashDigest ArchiveHash = HashDigest.Sha256(new string('b', 64));

    [Fact]
    public void IsVerified_RequiresEveryMandatoryEvidenceField()
    {
        var complete = new VerificationEvidence(
            SourceHash,
            42,
            SourceHash,
            42,
            ArchiveHash,
            21,
            EncoderExitedSuccessfully: true,
            EncoderVerifyRequested: true,
            EncoderVerifySucceeded: true,
            DecoderExitedSuccessfully: true);

        Assert.True(complete.IsVerified);

        Assert.False(complete with { ArchiveHash = null } is { IsVerified: true });
        Assert.False(complete with { RestoredHash = ArchiveHash } is { IsVerified: true });
        Assert.False(complete with { RestoredLength = 41 } is { IsVerified: true });
        Assert.False(complete with { EncoderVerifyRequested = false } is { IsVerified: true });
        Assert.False(complete with { EncoderVerifySucceeded = false } is { IsVerified: true });
        Assert.False(complete with { DecoderExitedSuccessfully = false } is { IsVerified: true });
    }
}
