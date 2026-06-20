namespace WavCrusher.Application.Archiving;

public sealed record ArchiveRequest(
    Guid ItemId,
    string SourcePath,
    string DestinationPath);
