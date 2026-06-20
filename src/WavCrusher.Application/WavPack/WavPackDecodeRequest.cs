using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public sealed record WavPackDecodeRequest(
    NormalizedAbsolutePath DecoderPath,
    NormalizedAbsolutePath ArchivePath,
    NormalizedAbsolutePath RestoredWavePath,
    ToolIdentity ExpectedTool);
