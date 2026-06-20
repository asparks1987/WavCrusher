namespace WavCrusher.Domain;

/// <summary>
/// The single version-1 archival WavPack profile. Dangerous codec options are not configurable.
/// </summary>
public sealed record ArchiveProfile(string Id, string Codec, IReadOnlyList<string> EncoderArguments)
{
    public static ArchiveProfile PureLosslessHighCompressionV1 { get; } = new(
        "wavpack-pure-lossless-hh-x6-v1",
        "WavPack",
        new[] { "-hh", "-x6", "-m", "-v", "-t", "-z0", "--no-overwrite" });
}
