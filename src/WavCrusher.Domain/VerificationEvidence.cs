namespace WavCrusher.Domain;

public sealed record VerificationEvidence(
    HashDigest? SourceHash,
    long? SourceLength,
    HashDigest? RestoredHash,
    long? RestoredLength,
    HashDigest? ArchiveHash,
    long? ArchiveLength,
    bool EncoderExitedSuccessfully,
    bool EncoderVerifyRequested,
    bool EncoderVerifySucceeded,
    bool DecoderExitedSuccessfully)
{
    public bool IsVerified =>
        SourceHash is not null &&
        RestoredHash is not null &&
        ArchiveHash is not null &&
        SourceLength is > 0 &&
        RestoredLength == SourceLength &&
        ArchiveLength is > 0 &&
        SourceHash == RestoredHash &&
        EncoderExitedSuccessfully &&
        EncoderVerifyRequested &&
        EncoderVerifySucceeded &&
        DecoderExitedSuccessfully;
}
