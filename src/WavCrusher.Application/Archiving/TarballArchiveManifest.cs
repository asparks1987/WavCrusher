using System.Text.Json.Serialization;

namespace WavCrusher.Application.Archiving;

public sealed record HashReference(
    [property: JsonPropertyName("algorithm")] string Algorithm,
    [property: JsonPropertyName("hex")] string Hex);

public sealed record TarballManifestItem(
    [property: JsonPropertyName("sourceRelativePath")] string SourceRelativePath,
    [property: JsonPropertyName("sourceAbsolutePathHint")] string SourceAbsolutePathHint,
    [property: JsonPropertyName("sourceLengthBytes")] long SourceLengthBytes,
    [property: JsonPropertyName("sourceSha256")] HashReference SourceSha256,
    [property: JsonPropertyName("archiveRelativePath")] string ArchiveRelativePath,
    [property: JsonPropertyName("archiveLengthBytes")] long ArchiveLengthBytes,
    [property: JsonPropertyName("archiveSha256")] HashReference? ArchiveSha256,
    [property: JsonPropertyName("restoredSha256")] HashReference? RestoredSha256,
    [property: JsonPropertyName("profileId")] string ProfileId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("failureCode")] string? FailureCode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("startedUtc")] string StartedUtc,
    [property: JsonPropertyName("completedUtc")] string CompletedUtc);

public sealed record TarballManifestSummary(
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("verifiedCount")] int VerifiedCount,
    [property: JsonPropertyName("failedCount")] int FailedCount,
    [property: JsonPropertyName("conflictCount")] int ConflictCount);

public sealed record TarballArchiveManifest(
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("createdUtc")] string CreatedUtc,
    [property: JsonPropertyName("packageId")] string PackageId,
    [property: JsonPropertyName("sourceRootHint")] string? SourceRootHint,
    [property: JsonPropertyName("summary")] TarballManifestSummary Summary,
    [property: JsonPropertyName("items")] IReadOnlyList<TarballManifestItem> Items);
