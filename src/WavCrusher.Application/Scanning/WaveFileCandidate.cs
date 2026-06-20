namespace WavCrusher.Application.Scanning;

public sealed record WaveFileCandidate(
    Guid Id,
    string FullPath,
    string RelativePath,
    long LengthBytes);
