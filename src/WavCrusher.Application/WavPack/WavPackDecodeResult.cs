using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public sealed record WavPackDecodeResult(
    bool Succeeded,
    int? ExitCode,
    ToolIdentity ToolIdentity,
    TimeSpan Duration,
    string DiagnosticSummary,
    FailureCode FailureCode);
