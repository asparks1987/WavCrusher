namespace WavCrusher.Application.Archiving;

public sealed record ArchiveResult(
    Guid ItemId,
    string SourcePath,
    string DestinationPath,
    bool Succeeded,
    string Status,
    string Message,
    string? SourceSha256,
    string? RestoredSha256,
    string? ArchiveSha256);
