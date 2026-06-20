# Archive Safety Specification

**Normative status:** Binding for version 1 implementation.  
Keywords **MUST**, **MUST NOT**, **SHOULD**, and **MAY** are used in their conventional requirements sense.

## 1. Preservation claim

For every item reported as `Verified`, WavCrusher MUST have produced a standard pure-lossless WavPack archive and demonstrated that decoding that archive recreates a WAV file whose complete byte sequence is identical to the source WAV observed during the operation.

The claim applies to file contents. Common filesystem timestamps and attributes MAY be recorded and restored according to explicit policy, but NTFS ACLs, alternate data streams, EFS state, sparse allocation, hard-link relationships, and all filesystem metadata are outside version 1â€™s whole-file identity claim.

## 2. Threats addressed

This specification addresses accidental source destruction, wrong-root processing, directory loops, path traversal, shell injection, tool substitution, partial output publication, silent overwrite, source mutation during processing, decoder incompatibility, crash interruption, misleading success reporting, and corrupted archives.

It does not make storage media immortal. Users still need redundant copies, periodic fixity checks, environmental monitoring, and migration planning.

## 3. Source safety

### AS-001 Read-only source

The application MUST open source WAV content read-only. It MUST NOT invoke any source-path operation that deletes, moves, renames, replaces, truncates, or writes the source.

### AS-002 No destructive encoder flags

The application MUST NOT invoke WavPack with `-d` or any current/future option that deletes or modifies input. The allowed argument set MUST be allowlisted, not only blocklisted.

### AS-003 No post-success cleanup of originals

Version 1 MUST NOT offer â€œdelete originals,â€ â€œreplace with archive,â€ or equivalent automation. Documentation MAY describe a separate user-managed retention decision but MUST recommend retaining verified redundant copies.

### AS-004 Stable input observation

For each item the app MUST capture source length and last-write time before hashing, after hashing, and after encoding. When practical, it SHOULD capture a stable file identity. Any observed change MUST produce `SourceChanged`, prevent final publication, and be journaled.

### AS-005 Source open conflicts

A locked or inaccessible source MUST fail clearly. The app MUST NOT bypass access controls, take ownership, use backup privileges, or silently skip without a recorded outcome.

## 4. Root and path safety

### AS-010 Root separation

Source and destination roots MUST be different and neither may contain the other. Validation MUST use canonical Windows path semantics and MUST consider reparse points; a simple text prefix is insufficient.

### AS-011 Destination containment

Every created directory/file MUST be proven to remain beneath the destination root after canonicalization.

### AS-012 Manifest containment

Paths read from manifests MUST be treated as untrusted. Rooted paths, drive-relative paths, `..`, device paths, alternate data stream syntax, invalid segments, and containment escapes MUST be rejected.

### AS-013 Reparse points

Scanner recursion MUST skip directory reparse points in version 1. The skip MUST be visible in plan/report evidence. Output directory creation MUST NOT traverse an unexpected reparse point; each relevant parent SHOULD be checked before publication.

### AS-014 Name mapping

The destination path MUST preserve the validated relative source path and change only the final `.wav` extension to `.wv`. Case-colliding outputs MUST be conflicts.

### AS-015 Archive metadata location

Manifests, journals, and reports SHOULD live in an operation metadata directory under the destination root using a reserved name that cannot collide with mapped source content. The reserved path MUST be documented and checked during planning.

Recommended reserved path:

```text
.wavcrusher/
  manifests/
  journals/
  reports/
  operations/
```

## 5. Toolchain safety

### AS-020 Pinned executable

The application MUST invoke a bundled executable by validated absolute path. It MUST NOT discover `wavpack.exe` or `wvunpack.exe` through the current directory or system `PATH` for normal operation.

### AS-021 Tool identity

Before processing, the executableâ€™s SHA-256 and reported version MUST match approved dependency metadata. A mismatch MUST fail closed.

### AS-022 Direct invocation

The application MUST use direct process creation with a structured argument list. It MUST NOT invoke a command shell, PowerShell, batch file, or user-provided command template.

### AS-023 Pure-lossless profile

Version 1 MUST expose exactly one archival encoding profile. Its intent MUST be equivalent to:

```text
-hh -x6 -m -v -t -z0 --no-overwrite
```

The exact ordered argument array MUST be covered by integration tests against the pinned release.

### AS-024 Forbidden transformations

The archival path MUST NOT use hybrid/lossy mode, pre-quantization, wrapper discard, raw mode, normalization, resampling, format conversion, or forced fresh WAV header generation.

### AS-025 Process evidence

The app MUST record tool identity, structured arguments or profile ID, exit code, execution duration, and enough bounded diagnostics to investigate a failure.

### AS-026 Cancellation

Cancellation MUST terminate the complete WavPack process tree when active work cannot finish safely. Output from a cancelled process MUST be considered unverified regardless of exit timing.

## 6. Temporary files and publication

### AS-030 Temporary archive

Encoding MUST target an operation-owned temporary archive in the same directory/filesystem as the final output. Its name MUST not end in the ordinary final `.wv` name and MUST include an operation-unique component.

### AS-031 No final write

The encoder MUST NOT write directly to the final path.

### AS-032 Verification before publication

The app MUST complete the entire verification chain before moving a temporary archive to its final name.

### AS-033 No overwrite

Final publication MUST be a no-overwrite operation. A race that creates the final path after planning MUST result in `DestinationConflict`, not replacement.

### AS-034 Atomicity

Where supported on the same filesystem, publication MUST use an atomic rename/move. If atomic publication cannot be guaranteed, the app MUST fail rather than fall back to copy-overwrite semantics.

### AS-035 Temporary restore

The verification decode MUST target an operation-owned temporary workspace separated from source and final archive trees. The workspace MUST have enough free space for the full restored WAV.

### AS-036 Cleanup

The app MAY remove its own temporary files after terminal evidence is recorded. It MUST NOT delete files based only on a suffix; ownership must be established through operation metadata/name and root containment.

### AS-037 Stale partials

On resume, stale partials MUST be classified. They MUST NOT be promoted based solely on existence or size. They may be deleted only after ownership and non-final status are established and the action is journaled.

## 7. Verification chain

### AS-040 Complete source hash

The app MUST compute SHA-256 over every byte of the source WAV as observed for this operation.

### AS-041 Encoder verification

The app MUST request WavPackâ€™s output verification and require successful process completion. Encoder verification alone is insufficient for final success.

### AS-042 Independent decode

The app MUST use `wvunpack` to decode the temporary `.wv` into a new temporary file. It MUST NOT force a generic WAV wrapper or another format.

### AS-043 Whole-file equality

The restored fileâ€™s byte length and SHA-256 MUST equal the source fileâ€™s byte length and SHA-256. A mismatch MUST be `RoundTripHashMismatch`, MUST prevent publication, and SHOULD preserve bounded diagnostics without exposing audio content.

### AS-044 Archive hash

The app MUST compute SHA-256 over the archive bytes that are published. If the hash is computed before atomic move, the move MUST not alter content; metadata must identify the final path and same digest.

### AS-045 WavPack MD5

The stored WavPack MD5 MAY be recorded. It MUST be labeled as raw-audio verification and MUST NOT be represented as the complete-file hash.

### AS-046 Success predicate

`Verified` is true only when all required stages are true. Code MUST centralize this predicate and tests MUST prove no missing evidence combination can produce success.

## 8. Journal and manifest durability

### AS-050 Append-only journal

Each operation MUST maintain an append-only UTF-8 JSONL journal. Terminal item records MUST be flushed before the coordinator reports an item complete.

### AS-051 Snapshot not authority

The manifest is a convenient portable snapshot. When a snapshot and journal disagree after interruption, the valid journal prefix and actual archive evidence MUST drive reconciliation.

### AS-052 Transactional snapshots

Manifest/report snapshots MUST use write-to-temp, flush, and atomic replace/rename. A partially written snapshot MUST not replace the last valid snapshot.

### AS-053 Explicit incompleteness

An interrupted operation MUST remain marked incomplete. Reports MUST NOT summarize an incomplete run as fully successful.

### AS-054 Stable schema

Manifest and journal records MUST contain schema version information. Unknown major versions MUST be rejected with an actionable message. Compatible unknown fields SHOULD be preserved or safely ignored.

### AS-055 Evidence immutability

A later audit MUST add audit evidence rather than rewriting historical archive-time hashes or tool identity.

## 9. Resume safety

### AS-060 Revalidation

Resume MUST revalidate roots, tool identity, operation identity, paths, archive existence, and source evidence as applicable.

### AS-061 No existence inference

A `.wv` fileâ€™s existence MUST NOT imply success. It must be connected to complete journal/manifest evidence and current archive SHA-256, or independently reverified.

### AS-062 Orphan final archive

If final publication occurred but success journaling failed, resume MUST treat the final archive as an orphan. It may reconcile only after validating source relationship, tool/profile evidence where available, archive hash, and a full round-trip comparison. Otherwise it remains a conflict requiring user action.

### AS-063 Idempotency

Repeating resume on unchanged data MUST not duplicate final archives, overwrite files, or create contradictory success records.

## 10. Audit safety

### AS-070 Read-only audit

Audit MUST open source/archive content read-only and MUST NOT change tags, timestamps intentionally, or file bytes.

### AS-071 Audit levels

Reports MUST distinguish archive-hash checks, decoder-integrity checks, and whole-file source comparisons. Passing one level MUST NOT be described as passing a deeper level.

### AS-072 Changed archives

An archive hash mismatch MUST be reported as changed/corrupt even if the decoder can still extract audio.

### AS-073 Missing originals

When original WAVs are absent, the app MAY verify archive fixity and decoder integrity but MUST state that current whole-file comparison to the original was not performed.

## 11. Restore safety

### AS-080 Separate restore root

Restore output MUST be beneath a validated restore root separate from the archive root.

### AS-081 No overwrite

Existing restore targets MUST be conflicts unless a future ADR defines an explicit safe replacement workflow.

### AS-082 Hash before success

A restored WAV MUST match the archive-time original complete-file SHA-256 before status becomes `VerifiedRestored`.

### AS-083 Metadata policy

Optional timestamp/attribute restoration MUST happen only after content verification and MUST not affect the content hash claim. Failures to restore metadata must be reported separately.

### AS-084 Emergency recovery

The distribution MUST include plain `wvunpack` instructions. The proprietary application MUST NOT be necessary to recover the WAV content.

## 12. Failure handling

### AS-090 Fail closed

When required evidence is unavailable, ambiguous, or contradictory, the item MUST not be successful.

### AS-091 Typed failures

Expected failures MUST have stable machine codes and user-readable explanations. An unknown exception MUST retain context and fail the operation/item safely.

### AS-092 Continue policy

A single-item failure MAY allow other independent items to continue. Root-level, tool-integrity, journal-durability, or containment failures MUST stop the operation.

### AS-093 Disk full

Insufficient space during any stage MUST fail cleanly. The app MUST not delete source or unrelated destination content to recover space.

### AS-094 Log truthfulness

Progress and completion messages MUST not overstate guarantees. â€œEncodedâ€ and â€œVerifiedâ€ are distinct states.

## 13. Required release tests

A release MUST prove:

1. No source bytes change under all normal/fault/cancel paths.
2. Every supported corpus WAV restores with identical length and SHA-256.
3. No unverified final name remains after injected failure at each pipeline stage.
4. Tool tampering is detected.
5. Shell metacharacters in paths remain literal arguments.
6. Reparse loops and manifest traversal cannot escape roots.
7. Existing outputs are never overwritten.
8. Journal truncation recovers the valid prefix.
9. Resume is idempotent.
10. Plain upstream/bundled `wvunpack` independently restores sample archives.

## 14. Claim language

Approved:

- â€œLossless WAV storage.â€
- â€œVerified byte-for-byte WAV restoration for files marked Verified.â€
- â€œStandard WavPack archives recoverable with `wvunpack`.â€
- â€œOriginal source files remain untouched by WavCrusher.â€

Disallowed without additional evidence/qualification:

- â€œSmallest possible.â€
- â€œImpossible to lose data.â€
- â€œFuture-proof forever.â€
- â€œSupports every WAV.â€
- â€œBackup replacement.â€
- â€œGuaranteed storage savings.â€
