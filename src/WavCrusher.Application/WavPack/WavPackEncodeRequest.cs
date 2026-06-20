using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public sealed record WavPackEncodeRequest(
    NormalizedAbsolutePath EncoderPath,
    NormalizedAbsolutePath SourceWavePath,
    NormalizedAbsolutePath TemporaryArchivePath,
    ArchiveProfile Profile,
    ToolIdentity ExpectedTool);
