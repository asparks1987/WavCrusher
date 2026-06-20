namespace WavCrusher.Application.Scanning;

public interface IWaveFileScanner
{
    Task<IReadOnlyList<WaveFileCandidate>> ScanAsync(
        string sourceRoot,
        bool recursive,
        CancellationToken cancellationToken);
}
