namespace WavCrusher.Application.WavPack;

public sealed record ToolProgress(long? BytesProcessed, double? Percent, string? Stage);
