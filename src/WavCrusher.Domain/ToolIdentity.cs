namespace WavCrusher.Domain;

public sealed record ToolIdentity(
    string Name,
    string ReportedVersion,
    HashDigest Sha256,
    string Distribution,
    string DependencyRecord);
