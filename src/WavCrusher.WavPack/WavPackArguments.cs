using WavCrusher.Application.WavPack;
using WavCrusher.Domain;

namespace WavCrusher.WavPack;

public static class WavPackArguments
{
    public static IReadOnlyList<string> BuildEncodeArguments(WavPackEncodeRequest request)
    {
        if (request.Profile != ArchiveProfile.PureLosslessHighCompressionV1)
        {
            throw new ArgumentException("Only the version-1 pure-lossless profile is supported.", nameof(request));
        }

        return request.Profile
            .EncoderArguments
            .Concat(new[] { request.SourceWavePath.Value, request.TemporaryArchivePath.Value })
            .ToArray();
    }

    public static IReadOnlyList<string> BuildDecodeArguments(WavPackDecodeRequest request) =>
        new[] { request.ArchivePath.Value, "-o", request.RestoredWavePath.Value };
}
