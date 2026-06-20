# Manifest and Journal Specification

**Format family:** WavCrusher Archive Manifest  
**Initial major version:** 1  
**Encoding:** UTF-8 without a required BOM  
**Path separator in JSON:** `/`

This document defines portable operation evidence. Examples are illustrative; implementation must add JSON Schema files and compatibility tests before release.

## 1. Files in an archive set

Recommended layout:

```text
<destination-root>/
  Artist/Track.wv
  .wavcrusher/
    manifests/archive-manifest.v1.json
    journals/<operation-id>.jsonl
    reports/<operation-id>.json
    reports/<operation-id>.html
    operations/<operation-id>/operation.json
```

`.wavcrusher` is reserved under the destination root. A source relative path that would map into this reserved namespace must be rejected or escaped by a future documented mapping rule; version 1 should reject it clearly.

## 2. Design principles

- Relative paths make the archive set movable.
- SHA-256 identifies complete files.
- WavPack/raw-audio MD5, when present, is distinctly labeled.
- Archive-time evidence is immutable.
- Audit and restore events append new evidence.
- Machine status codes are stable English tokens; display text is localized separately.
- Unknown compatible fields may be ignored, but unknown major versions are rejected.
- A success record contains all mandatory proof fields.

## 3. Canonical hash object

```json
{
  "algorithm": "sha256",
  "hex": "0123456789abcdef..."
}
```

Rules:

- `algorithm` is lowercase.
- SHA-256 hex is exactly 64 lowercase hexadecimal characters.
- Hashes are over raw file bytes unless `scope` explicitly says otherwise.

Audio MD5 example:

```json
{
  "algorithm": "md5",
  "scope": "wavpack-raw-audio",
  "hex": "0123456789abcdef0123456789abcdef"
}
```

## 4. Manifest top-level object

```json
{
  "format": "wavcrusher-archive-manifest",
  "version": "1.0",
  "createdUtc": "2026-06-19T18:30:00.0000000Z",
  "updatedUtc": "2026-06-19T20:01:02.0000000Z",
  "archiveSetId": "7f20368a-97dc-49ad-982e-232a3a2995a0",
  "generator": {
    "name": "WavCrusher",
    "version": "0.1.0-prealpha",
    "runtime": ".NET 10",
    "platform": "win-x64"
  },
  "profile": {
    "id": "wavpack-pure-lossless-hh-x6-v1",
    "codec": "WavPack",
    "codecVersion": "5.9.0"
  },
  "roots": {
    "sourceLabel": "Studio Masters",
    "sourceRootHint": "D:/Masters",
    "destinationRoot": "."
  },
  "summary": {
    "itemCount": 1,
    "verifiedCount": 1,
    "failedCount": 0,
    "conflictCount": 0,
    "skippedCount": 0,
    "sourceBytes": 105840044,
    "archiveBytes": 59301688
  },
  "items": [],
  "operations": [],
  "extensions": {}
}
```

### Required top-level fields

| Field | Type | Meaning |
|---|---|---|
| `format` | string | Exact identifier `wavcrusher-archive-manifest`. |
| `version` | string | `major.minor`; major controls compatibility. |
| `createdUtc` | string | UTC ISO-8601 creation timestamp. |
| `updatedUtc` | string | UTC ISO-8601 snapshot timestamp. |
| `archiveSetId` | UUID string | Stable identity for the archive set. |
| `generator` | object | App/runtime provenance. |
| `profile` | object | Archive policy identity. |
| `roots` | object | Portable root context; absolute source may be redacted. |
| `summary` | object | Derived counts/bytes; never the sole evidence. |
| `items` | array | One current item record per logical source relative path. |
| `operations` | array | Operation summaries/references. |

## 5. Tool identity

```json
{
  "name": "wavpack.exe",
  "reportedVersion": "5.9.0",
  "sha256": {
    "algorithm": "sha256",
    "hex": "REAL_RELEASE_HASH_REQUIRED"
  },
  "distribution": "official-win64",
  "dependencyRecord": "third_party/wavpack/dependency.json"
}
```

`REAL_RELEASE_HASH_REQUIRED` is a documentation placeholder and MUST never appear in a built release or successful operation record. Release validation must reject placeholder-like values.

## 6. Item record

```json
{
  "itemId": "019a0ce7-6ceb-7f26-8ffd-c16eea8b1668",
  "source": {
    "relativePath": "Album/Track 01.wav",
    "length": 105840044,
    "completeFileHash": {
      "algorithm": "sha256",
      "hex": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
    },
    "observed": {
      "creationTimeUtc": "2020-01-02T03:04:05Z",
      "lastWriteTimeUtc": "2020-01-02T03:04:05Z",
      "attributes": ["Archive"]
    }
  },
  "archive": {
    "relativePath": "Album/Track 01.wv",
    "length": 59301688,
    "completeFileHash": {
      "algorithm": "sha256",
      "hex": "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
    },
    "wavpackAudioMd5": {
      "algorithm": "md5",
      "scope": "wavpack-raw-audio",
      "hex": "cccccccccccccccccccccccccccccccc"
    }
  },
  "roundTrip": {
    "restoredLength": 105840044,
    "restoredCompleteFileHash": {
      "algorithm": "sha256",
      "hex": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
    },
    "lengthMatched": true,
    "hashMatched": true
  },
  "verification": {
    "encoderExitedSuccessfully": true,
    "encoderVerifyRequested": true,
    "encoderVerifySucceeded": true,
    "decoderExitedSuccessfully": true,
    "wholeFileRoundTripSucceeded": true
  },
  "profileId": "wavpack-pure-lossless-hh-x6-v1",
  "toolchain": {
    "encoder": {},
    "decoder": {}
  },
  "result": {
    "status": "verified",
    "failureCode": null,
    "message": "Verified byte-for-byte restoration."
  },
  "timing": {
    "startedUtc": "2026-06-19T18:31:00Z",
    "completedUtc": "2026-06-19T18:35:12Z",
    "durationMilliseconds": 252000
  },
  "operationId": "80636c48-668b-4eac-9864-e5ac52c3f22c"
}
```

### Successful item requirements

When `result.status` is `verified`, all of these are mandatory:

- Source relative path, length, and complete-file SHA-256.
- Archive relative path, length, and complete-file SHA-256.
- Restored length and complete-file SHA-256.
- Source and restored values equal.
- All five verification booleans true.
- Profile ID.
- Encoder and decoder identity with real SHA-256.
- Operation ID and timestamps.

A serializer/validator must reject a purported verified item lacking any field above.

### Non-success result

```json
{
  "result": {
    "status": "failed",
    "failureCode": "SourceChanged",
    "message": "The source changed while it was being archived.",
    "stage": "encoding",
    "retryable": true,
    "diagnosticId": "ERR-01J..."
  }
}
```

Do not store unbounded console output in the manifest. Refer to a bounded/redactable operation diagnostic record.

## 7. Item statuses

Stable persisted values:

```text
verified
failed
conflict
skipped
cancelled
```

In-progress stages belong in journals/operation state and generally should not appear as current manifest success.

## 8. Operation record

```json
{
  "operationId": "80636c48-668b-4eac-9864-e5ac52c3f22c",
  "type": "archive",
  "status": "completed",
  "startedUtc": "2026-06-19T18:30:00Z",
  "completedUtc": "2026-06-19T20:01:02Z",
  "journalRelativePath": ".wavcrusher/journals/80636c48-668b-4eac-9864-e5ac52c3f22c.jsonl",
  "reportRelativePath": ".wavcrusher/reports/80636c48-668b-4eac-9864-e5ac52c3f22c.json",
  "summary": {
    "verified": 1,
    "failed": 0,
    "conflict": 0,
    "skipped": 0,
    "cancelled": 0
  }
}
```

Operation types:

```text
archive
audit
restore
reconcile
```

Operation statuses:

```text
running
paused
completed
completed-with-issues
cancelled
failed
interrupted
```

## 9. JSONL journal

Each line is an independent JSON object terminated by LF. Implementations should accept CRLF. A crash may leave the final line truncated; readers ignore only the malformed final line and warn. Malformed non-final lines are corruption.

### Common envelope

```json
{
  "journalFormat": "wavcrusher-operation-journal",
  "version": "1.0",
  "sequence": 42,
  "eventId": "f2c1f625-286c-4476-b850-bb23f4fa87d7",
  "operationId": "80636c48-668b-4eac-9864-e5ac52c3f22c",
  "timestampUtc": "2026-06-19T18:35:12.1234567Z",
  "eventType": "item-terminal",
  "payload": {}
}
```

Rules:

- `sequence` starts at 1 and increases by one.
- Event IDs are unique.
- Records are immutable.
- Readers detect gaps/duplicates and report them.
- Terminal item payloads contain all evidence needed to reconstruct the manifest item.

### Recommended events

```text
operation-created
scan-started
scan-diagnostic
plan-completed
operation-started
item-started
item-stage
item-terminal
operation-paused
operation-resumed
operation-cancel-requested
operation-terminal
manifest-written
audit-terminal
restore-item-terminal
```

Frequent byte progress SHOULD remain in volatile logs/UI and need not bloat the durable journal. Stage transitions and terminal outcomes are durable.

## 10. Audit record

Audits append evidence; they do not alter archive-time hashes:

```json
{
  "auditId": "f7cedde8-7499-4756-ab39-100a3f47ee35",
  "operationId": "...",
  "timestampUtc": "2027-06-19T12:00:00Z",
  "archiveRelativePath": "Album/Track 01.wv",
  "checks": {
    "archiveHash": "matched",
    "decoderIntegrity": "passed",
    "wholeFileSourceComparison": "not-run"
  },
  "result": "healthy-with-limited-depth"
}
```

Permitted check values must distinguish `passed`, `matched`, `failed`, `mismatched`, `not-run`, `not-applicable`, and `unavailable`.

## 11. Restore record

```json
{
  "restoreItemId": "...",
  "archiveRelativePath": "Album/Track 01.wv",
  "restoredRelativePath": "Album/Track 01.wav",
  "restoredLength": 105840044,
  "restoredCompleteFileHash": {
    "algorithm": "sha256",
    "hex": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
  },
  "expectedOriginalHashMatched": true,
  "status": "verified-restored"
}
```

## 12. Redaction

A redacted exported report may replace absolute roots/user names with labels. It MUST NOT alter relative archive paths or hashes without saying so. The canonical manifest in the archive set should favor relative data and may omit absolute source roots entirely.

Suggested root form:

```json
{
  "sourceLabel": "Source A",
  "sourceRootHint": null,
  "destinationRoot": ".",
  "redacted": true
}
```

## 13. Validation and compatibility

- Parse using size/depth limits to resist hostile manifests.
- Reject duplicate critical JSON properties.
- Reject numbers outside `Int64` range.
- Reject invalid UTC timestamps and hashes.
- Revalidate all paths at use time.
- Version `1.x` readers may ignore unknown additive fields.
- A version `2.x` manifest requires explicit support/migration.
- Never downgrade a newer manifest in place.

## 14. Canonicalization and signatures

Version 1 does not require digital signatures. If signatures are later introduced:

- Define a canonical JSON representation or sign detached file bytes.
- Keep archive SHA-256 independent of manifest signatures.
- Do not imply that a hash alone proves authorship.
- Add a new ADR and versioned extension.

## 15. File naming

Recommended canonical files:

```text
archive-manifest.v1.json
<operation-id>.jsonl
<operation-id>.report.json
<operation-id>.report.html
```

Names are ASCII for tool compatibility. User content paths remain Unicode.

## 16. Sample files

See:

- `../samples/archive-manifest.example.json`
- `../samples/archive-report.example.json`
- `../samples/wavcrusher.settings.example.json`

Examples contain non-real hashes where clearly marked and must never be used as release evidence.
