using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public sealed record WavPackEncodeResult(
    bool Succeeded,
    bool EncoderVerificationSucceeded,
    int? ExitCode,
    ToolIdentity ToolIdentity,
    TimeSpan Duration,
    string DiagnosticSummary,
    FailureCode FailureCode);
