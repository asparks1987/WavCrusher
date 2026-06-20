# Codex Full-Build Plan for WavCrusher

This is the implementation sequence Codex should follow after this documentation package is placed in a new repository. It is a **plan for building the app**, not permission to skip design gates. `AGENTS.md` and the archive safety specification remain binding.

## Operating model

Use one focused pull request per phase or independently testable slice. Each pull request must include:

- Scope and non-scope.
- Safety impact.
- Acceptance criteria addressed.
- Tests added and exact test commands run.
- Screenshots only when UI behavior changes.
- Any deferred work identified by requirement IDâ€”not vague TODOs.

Do not generate the entire application in one unreviewable change.

## Target repository

```text
WavCrusher.sln
global.json
Directory.Build.props
Directory.Packages.props
src/
  WavCrusher.Domain/
  WavCrusher.Application/
  WavCrusher.Infrastructure/
  WavCrusher.WavPack/
  WavCrusher.WinForms/
tests/
  WavCrusher.Domain.Tests/
  WavCrusher.Application.Tests/
  WavCrusher.Infrastructure.Tests/
  WavCrusher.WavPack.IntegrationTests/
  WavCrusher.EndToEndTests/
tools/
  TestCorpusBuilder/
third_party/wavpack/
  VERSION
  LICENSE
  dependency.json
  win-x64/wavpack.exe
  win-x64/wvunpack.exe
```

Use `net10.0` for portable libraries and `net10.0-windows` for Windows-specific projects. Pin the current supported .NET 10 SDK patch at implementation time; do not copy a stale version from this document without checking the official support table.

---

## Phase 0 â€” Repository reconnaissance and dependency proof

### Objective

Confirm external assumptions before application code depends on them.

### Tasks

1. Read all repository documentation and create an implementation requirements matrix.
2. Confirm the provisional product name is acceptable or replace it consistently.
3. Download WavPack 5.9.0 Windows x64 from the approved upstream release.
4. Record its exact artifact names, sizes, SHA-256 hashes, license, and version output in `third_party/wavpack/dependency.json`.
5. Manually test representative WAVs with the intended encode and decode argument arrays.
6. Establish whether the WavPack output parameter accepts the planned `.partial` suffix without changing behavior; adjust the temp-name convention if needed while retaining same-directory atomic publication.
7. Confirm `wvunpack` with no forced output-format switch restores wrapper bytes for the corpus.
8. Record console encodings, progress/output format, exit codes, overwrite behavior, and cancellation behavior.
9. Write ADR amendments for any discrepancy.

### Deliverables

- `docs/implementation/REQUIREMENTS_MATRIX.md`
- `third_party/wavpack/dependency.json` with real hashes
- `docs/implementation/WAVPACK_CLI_NOTES.md`
- A small redistributable, generated test corpus with provenance

### Gate 0

A script can encode and restore every valid corpus file; `SHA256(original) == SHA256(restored)` for each. No source is modified. Invalid and unsupported files fail clearly.

---

## Phase 1 â€” Solution bootstrap and engineering policy

### Objective

Create a buildable, analyzable, repeatable solution with no business logic yet.

### Tasks

1. Add `global.json` for the selected supported .NET 10 SDK.
2. Create solution/projects and enforce dependency direction.
3. Enable nullable reference types, implicit usings where appropriate, deterministic builds, analyzers, warnings as errors, and code style.
4. Add central package management and lock files.
5. Add test projects and a standard test framework.
6. Add CI for restore, build, unit tests, integration-test opt-in, dependency review, and artifact checksum generation.
7. Add assembly metadata and versioning placeholders that cannot be mistaken for a public release.
8. Add a composition-root smoke test.

### Gate 1

A clean clone builds and runs unit tests in CI and on a documented Windows developer environment. Projects cannot reference layers in the wrong direction.

---

## Phase 2 â€” Domain model and invariants

### Objective

Represent archive intent and outcomes without touching the filesystem or invoking WavPack.

### Suggested types

- `ArchiveRoots`
- `NormalizedPath`
- `ValidatedRelativePath`
- `ArchivePlan`
- `ArchivePlanItem`
- `OperationId`
- `ArchiveProfile`
- `FileFingerprint`
- `HashDigest`
- `ToolIdentity`
- `ArchiveEvidence`
- `ArchiveItemResult`
- `ArchiveItemStatus`
- `FailureCode`
- `ConflictPolicy`

### Tasks

1. Encode root non-equality/nesting invariants.
2. Encode a single immutable archive profile; do not make dangerous options configurable.
3. Define stable failure/status codes.
4. Define state transitions for item and operation lifecycles.
5. Define arithmetic for bytes and progress without overflow.
6. Add exhaustive unit tests, including Windows case-insensitive path cases and traversal strings.

### Gate 2

Invalid plans are impossible or explicitly rejected. Domain tests require no filesystem, process, JSON, or UI dependency.

---

## Phase 3 â€” Scanner and deterministic planner

### Objective

Recursively discover candidate WAVs and produce a reviewable plan.

### Tasks

1. Implement a streaming scanner behind `IFileTreeScanner`.
2. Match `.wav` case-insensitively.
3. Include hidden/system WAV files when readable, but surface attributes in the plan.
4. Skip reparse-point directories and record warnings.
5. Handle access-denied, disappearing directories, long paths, and cancellation per item without collapsing the entire scan.
6. Generate output paths by changing only the final extension to `.wv` under a mirrored relative tree.
7. Detect case collisions and existing destinations before processing.
8. Sort plan display deterministically while allowing streaming discovery for large trees.
9. Estimate source bytes, expected temporary space conservatively, and destination free space.
10. Add fake-filesystem tests plus real Windows filesystem tests.

### Gate 3

A scan of a hostile synthetic tree cannot escape roots, loop through junctions, or silently omit an error. Repeated scans of an unchanged tree yield the same logical plan.

---

## Phase 4 â€” WavPack process adapter

### Objective

Provide a narrow, tested adapter around pinned `wavpack.exe` and `wvunpack.exe`.

### Interfaces

```csharp
public interface IWavPackEncoder
{
    Task<WavPackEncodeResult> EncodeAsync(
        WavPackEncodeRequest request,
        IProgress<ToolProgress>? progress,
        CancellationToken cancellationToken);
}

public interface IWavPackDecoder
{
    Task<WavPackDecodeResult> DecodeAsync(
        WavPackDecodeRequest request,
        IProgress<ToolProgress>? progress,
        CancellationToken cancellationToken);
}
```

### Tasks

1. Verify executable hash and version against dependency metadata.
2. Construct structured argument lists from validated paths.
3. Set direct process execution, redirected streams, no window, explicit encoding where supported.
4. Drain output asynchronously and cap retained diagnostic bytes.
5. Parse progress defensively; progress parsing failure must not imply operation failure.
6. Map exit status and expected diagnostics to typed results.
7. Implement cancellation with process-tree termination and cleanup.
8. Add timeouts only as configurable operational safeguards, not silent aborts.
9. Add real integration tests for paths containing spaces, Unicode, punctuation, and long names.
10. Snapshot the exact allowed argument array in tests.

### Gate 4

No code path invokes a shell or emits a forbidden option. A tampered or wrong-version executable is rejected before processing.

---

## Phase 5 â€” Transactional archive engine

### Objective

Implement the full evidence-producing pipeline for one item, then bounded multi-item orchestration.

### Single-item stages

```text
Planned
  -> FingerprintingSource
  -> EncodingTemporaryArchive
  -> DecoderRoundTrip
  -> ComparingWholeFile
  -> HashingArchive
  -> Publishing
  -> Journaled
  -> Succeeded
```

Every stage can transition to `Failed` or `Cancelled`; none may bypass verification.

### Tasks

1. Implement streaming SHA-256 with progress and cancellation.
2. Capture source metadata before/after hash and encode.
3. Generate a collision-resistant temp name in the final directory.
4. Check free space before encoding and again before round-trip extraction.
5. Encode with the canonical profile.
6. Decode to an operation-owned temp directory, never beside source data.
7. Compare size and SHA-256; optionally use byte-by-byte diagnostics only on mismatch.
8. Hash the archive.
9. Atomically move to the final path with no overwrite.
10. Persist terminal evidence.
11. Cleanup temp restored WAV immediately after its evidence is recorded.
12. Retain failed `.partial` files only behind a diagnostic policy; default to safe cleanup and log the decision.
13. Add bounded parallelism. Default conservatively; allow user selection only within tested limits.
14. Ensure pause stops new scheduling and cancel terminates active work coherently.
15. Add fault injection at every boundary.

### Gate 5

Power loss/process termination simulation at each stage cannot produce an unverified final `.wv` or alter a source. Resume classifies and continues work without falsely reporting prior incomplete items as successful.

---

## Phase 6 â€” Manifest, journal, and reports

### Objective

Create durable, portable evidence and crash recovery.

### Tasks

1. Implement manifest v1 from `docs/MANIFEST_SPEC.md`.
2. Implement append-only JSONL records with sequence and operation IDs.
3. Flush each terminal record to stable storage as far as the platform permits.
4. Rebuild a manifest snapshot from journal records.
5. Use temp-write and atomic replace for manifest/report snapshots.
6. Generate a concise HTML and JSON report with counts, hashes, failures, and tool identity.
7. Add optional path redaction for reports shared outside the machine.
8. Preserve forward-compatible extension data or reject unsupported major versions.
9. Add JSON schema fixtures and golden-file tests.

### Gate 6

A deliberately truncated final journal line is ignored with a warning; all preceding records remain recoverable. A manifest never contains a success lacking complete evidence.

---

## Phase 7 â€” Audit and restore engines

### Objective

Make verification and recovery first-class workflows, not afterthoughts.

### Audit modes

- **Archive fixity:** compare current `.wv` SHA-256 with manifest.
- **WavPack integrity:** invoke decoder verification/test mode where supported.
- **Full source comparison:** decode and compare whole-file SHA-256 when source WAV is available.
- **Recovery sample:** decode selected/all archives to scratch space and compare recorded original SHA-256.

### Restore tasks

1. Validate every manifest relative path and destination containment.
2. Never overwrite an existing restored WAV by default.
3. Decode without format-forcing or normalization switches.
4. Compare whole-file hash with manifest before success.
5. Restore common timestamps/attributes only after content verification and according to an explicit policy.
6. Emit a restore report and journal.
7. Add tests for malicious manifests, corrupted archives, missing files, and mixed versions.

### Gate 7

Every archive produced by the app can be restored using both WavCrusher and plain `wvunpack`, and the whole-file hash matches. A hostile manifest cannot write outside the restore root.

---

## Phase 8 â€” Windows Forms application

### Objective

Build a responsive, accessible UI over proven application services.

### Shell

- Header with product identity and offline/open-source status.
- Navigation: Archive, Audit, Restore, Activity/Reports, Settings, About.
- Persistent operation status strip.

### Archive page

- Source and destination folder pickers plus editable fields.
- Safety validation inline.
- Scan button and cancellable scan.
- Summary cards: files, source size, predicted range, conflicts, warnings.
- Virtualized/efficient item grid with path, size, output, status, ratio, verification.
- â€œStart verified archiveâ€ confirmation dialog summarizing immutable-source and no-overwrite rules.
- Progress, current stage, pause scheduling, cancel, and report export.

### Audit page

- Select manifest/root.
- Choose audit depth.
- Show last-known and current fixity, errors, and recommended actions.

### Restore page

- Select manifest/root and separate destination.
- Preview paths/conflicts.
- Restore with whole-file verification.

### Tasks

1. Implement passive views/presenters so workflows are testable headlessly.
2. Marshal updates to UI thread safely and throttle high-frequency progress.
3. Persist non-dangerous preferences only.
4. Implement high DPI, text scaling, high contrast, keyboard navigation, focus order, accessible names, and status announcements.
5. Add first-run explanation and links to local documentation.
6. Show WavPack version/hash and licenses in About.
7. Add screenshot automation where stable and manual accessibility checklist.

### Gate 8

The UI remains responsive during scans, hashes, and child processes; cancellation works; all workflows are usable by keyboard; status is never communicated by color alone.

---

## Phase 9 â€” Preservation corpus and destructive-condition testing

### Objective

Prove the applicationâ€™s claim across real file structures and failure conditions.

### Tasks

1. Build deterministic WAV fixture generators for PCM/float, metadata chunks, odd padding, trailers, and RF64.
2. Include source-code generators rather than large binary files where practical.
3. Add a corpus manifest with generator version and expected hash.
4. Run whole-file round trips for every fixture.
5. Test Unicode normalization, long paths, locked files, files modified mid-run, junction loops, access denied, low disk, process kill, corrupt tools, corrupt archives, and cancellation.
6. Re-run with 1 and multiple workers.
7. Benchmark compression/time/memory on a documented, redistributable synthetic corpus. Do not market benchmark results as universal.
8. Add soak tests for tens or hundreds of thousands of tiny files.

### Gate 9

All supported corpus files round-trip byte-for-byte. All injected failures produce typed outcomes and no unverified final outputs.

---

## Phase 10 â€” Packaging and supply chain

### Objective

Ship an independently recoverable, inspectable Windows package.

### Tasks

1. Publish self-contained win-x64 application files.
2. Prefer a single-file app executable but keep `wavpack.exe`, `wvunpack.exe`, their exact license, and `dependency.json` visible as sidecars.
3. Validate bundled tool hashes on startup.
4. Include local HTML/Markdown documentation and sample recovery commands.
5. Generate `SHA256SUMS.txt` for every release file.
6. Produce a portable ZIP first; consider an installer only after upgrade/uninstall safety is designed.
7. Add SBOM and build provenance.
8. Sign release artifacts when project identity and certificate infrastructure exist.
9. Test on clean, supported Windows virtual machines without developer tooling or network access.

### Gate 10

A clean offline machine can archive, audit, and restore. The user can extract `.wv` files with the visibly bundled `wvunpack.exe` even if the WavCrusher executable is removed.

---

## Phase 11 â€” Release candidate and public readiness

### Objective

Close claims, documentation, and operational gaps before calling version 1 stable.

### Tasks

1. Complete every acceptance item with linked CI/manual evidence.
2. Run independent review of source immutability and path containment.
3. Run an accessibility review.
4. Run dependency/license review.
5. Complete naming/trademark review.
6. Replace pre-alpha website language with accurate release language.
7. Document supported WAV variants and known limitations from actual tests.
8. Publish recovery drills and a disaster-recovery checklist.
9. Tag and archive the exact source and dependency versions used.

### Gate 11

No open severity-1/2 defect, no unverified safety claim, and no missing recovery documentation. The release artifact passes clean-machine tests and a third party can reproduce a sample restore without the app.

---

## Suggested Codex session sequence

Use the detailed prompts in `docs/codex/IMPLEMENTATION_PROMPTS.md`. A practical sequence is:

1. Repository matrix and CLI proof.
2. Solution bootstrap.
3. Domain/path invariants.
4. Scanner/planner.
5. WavPack adapter.
6. One-file transactional pipeline.
7. Journal/manifest.
8. Multi-file orchestration and resume.
9. Audit/restore.
10. WinForms shell and Archive UX.
11. Audit/Restore UX.
12. Corpus/fault testing.
13. Packaging/release hardening.

After each session, ask Codex to provide a compact handoff containing changed files, decisions, test commands/results, unresolved risks, and the next task ID. Store handoffs under `docs/implementation/handoffs/` during development.

## Final instruction to Codex

Do not optimize for producing the most code. Optimize for producing the smallest reviewable change that proves one more part of the preservation promise. When evidence is missing, fail closed and make the missing evidence visible.
