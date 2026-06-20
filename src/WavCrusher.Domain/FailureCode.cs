namespace WavCrusher.Domain;

public enum FailureCode
{
    None = 0,
    InvalidRoot,
    RootOverlap,
    ReparsePointSkipped,
    AccessDenied,
    UnsupportedWave,
    SourceChanged,
    DestinationConflict,
    InsufficientSpace,
    ToolMissing,
    ToolHashMismatch,
    ToolVersionMismatch,
    EncoderFailed,
    DecoderFailed,
    RoundTripHashMismatch,
    ArchiveHashFailed,
    PublishFailed,
    JournalWriteFailed,
    Cancelled,
    UnknownFailure
}
