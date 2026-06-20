namespace WavCrusher.Application.Archiving;

public interface IArchiveCompressor
{
    Task<ArchiveResult> CompressAsync(
        ArchiveRequest request,
        IProgress<ArchiveItemProgress>? progress,
        CancellationToken cancellationToken);
}
