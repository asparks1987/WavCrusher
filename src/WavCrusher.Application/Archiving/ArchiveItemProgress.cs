namespace WavCrusher.Application.Archiving;

public sealed record ArchiveItemProgress(Guid ItemId, string Stage, string Message);
