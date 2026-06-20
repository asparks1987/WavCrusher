# AGENTS.md â€” WavCrusher Engineering Contract

This file is binding for Codex, GitHub Copilot agents, scripted refactoring tools, and human contributors working in this repository. Read it before planning or changing code.

## 1. Mission

Build a trustworthy Windows Forms application that recursively archives WAV files into standard, pure-lossless WavPack files and proves that every successful archive can restore the original WAV **byte-for-byte**.

The priorities, in order, are:

1. Protect source data.
2. Prove recoverability.
3. Produce interoperable standard `.wv` files.
4. Make failure states explicit and resumable.
5. Deliver an accessible, understandable Windows UI.
6. Improve compression ratio without compromising any item above.

Never trade a higher compression ratio or a prettier UI for weaker data safety.

## 2. Source of truth

When requirements appear to conflict, use this precedence:

1. `docs/ARCHIVE_SAFETY_SPEC.md`
2. This file
3. `docs/PRODUCT_REQUIREMENTS.md`
4. Architecture decision records under `docs/adr/`
5. `docs/ARCHITECTURE.md`
6. `CODEX_BUILD_PLAN.md`
7. Other documentation and issue text

Stop and create an ADR when a change would alter a safety invariant, archive format, manifest compatibility, or recovery guarantee.

## 3. Product boundary

### Included in version 1

- Windows 10/11 x64 desktop application.
- .NET 10 LTS Windows Forms UI.
- Recursive, case-insensitive discovery of `.wav` files.
- Mirrored relative output directories under a separate destination root.
- Pure-lossless WavPack encoding through pinned `wavpack.exe`.
- Full-file extraction verification through pinned `wvunpack.exe`.
- Archive, Audit, and Restore workflows.
- JSON manifest, append-only JSONL journal, human-readable report.
- Resume after cancellation or process interruption.
- Local-only operation with no accounts, telemetry, or network requirement.

### Explicitly excluded from version 1

- Deleting, moving, renaming, or replacing source WAV files.
- Hybrid or lossy WavPack modes.
- Converting sample rate, bit depth, channel layout, normalization, or metadata.
- Editing WAV metadata.
- Following directory junctions, symbolic links, or other reparse points.
- Cloud upload, synchronization, media playback, tagging, or catalog management.
- A proprietary aggregate archive container.
- Administrator-only features, services, scheduled tasks, or shell extensions.
- Preservation of NTFS ACLs, alternate data streams, EFS state, sparse-file allocation, or every filesystem attribute. Common timestamps and attributes may be recorded as metadata, but whole-file byte identity refers to file content.

## 4. Non-negotiable safety rules

### 4.1 Source immutability

- Open source files with read access only.
- Never call delete, move, replace, truncate, set-length, or write APIs on a source path.
- Never pass WavPack `-d` or any option that can remove input.
- Do not provide â€œdelete originals after successâ€ in version 1â€”even behind an advanced toggle.
- Treat any code path capable of modifying a source as a release-blocking defect.

### 4.2 Root isolation

Before scanning:

- Canonicalize source and destination roots.
- Reject equal roots.
- Reject a destination nested below the source.
- Reject a source nested below the destination.
- Compare with Windows path semantics and case-insensitivity.
- Resolve relative segments before comparison.
- Account for directory reparse points; do not rely on string-prefix tests alone.

### 4.3 Path containment

- Derive output only from a validated relative path.
- Reject rooted relative paths, `..` traversal, drive-qualified fragments, invalid characters, and alternate data stream syntax.
- After combining, canonicalize again and prove the path remains under the destination root.
- Never trust paths loaded from a manifest without the same validation.

### 4.4 Reparse points

- Do not recurse into directories with `FileAttributes.ReparsePoint`.
- Record skipped paths and reasons.
- Do not add an override until there is a dedicated threat review and test matrix.

### 4.5 Output publication

- Write each archive to a unique temporary file in the final destination directory, for example `Track.wv.<operation-id>.partial`.
- Never write directly to the final `.wv` path.
- Require all verification stages before final publication.
- Move the temporary file to the final name only when final does not already exist.
- A failed or cancelled item must not leave a file with a final archive name.
- Cleanup of stale `.partial` files must be explicit, logged, and limited to files created by WavCrusher.

### 4.6 Existing outputs

- Default behavior is **never overwrite**.
- If a destination `.wv` exists, classify it as a conflict until its relationship to the source is proven by manifest and verification.
- â€œSkip existingâ€ is permitted only when the item is clearly reported as skipped, not successful.
- Replacement behavior requires a future ADR and backup/rollback design.

### 4.7 Changing inputs

Capture source length and last-write time before hashing, after hashing, and after WavPack finishes reading. Prefer a stable file identity when practical. If evidence changes, fail the item as `SourceChanged`; do not publish the archive.

A full-file SHA-256 equality check is required even if the source appears stable.

## 5. Canonical WavPack behavior

Use a pinned, reviewed WavPack distribution. Version 1 plans for WavPack 5.9.0, but code must read the expected version from dependency metadata rather than scattering a string literal.

The default encoder profile is equivalent to:

```text
-hh -x6 -m -v -t -z0 --no-overwrite
```

Confirm option spelling and behavior against the exact pinned binaries in automated integration tests.

### Required intent

- Pure lossless mode: no bitrate/hybrid option.
- Highest compression mode: `-hh`.
- Maximum extra analysis: `-x6`.
- Store raw-audio MD5: `-m`.
- Verify output after write: `-v`.
- Copy file timestamps where supported: `-t`.
- Suppress title-bar manipulation for GUI integration: `-z0`.
- Never overwrite: `--no-overwrite`.

### Forbidden encoder options

- `-b` or combined hybrid syntax.
- `-c` correction-file behavior.
- `--pre-quantize`.
- `-r` wrapper discard.
- `-d` source deletion.
- `-i` input length-error suppression.
- Raw PCM mode unless a future feature is explicitly scoped.
- Any output-to-stdout pipeline for archive creation.

### Forbidden decoder behavior

- Do not force a new WAV header with `--wav`.
- Do not normalize floats.
- Do not output raw PCM for archival verification.
- Do not convert formats.
- Do not write restored verification files beside source data.

### Process invocation

- Invoke executables directlyâ€”never through `cmd.exe`, PowerShell, batch files, or a shell.
- Use `ProcessStartInfo.ArgumentList`; never interpolate a command string.
- Set `UseShellExecute = false`.
- Redirect and asynchronously drain stdout and stderr.
- Use explicit working directories.
- Validate executable identity and SHA-256 before first use.
- Capture executable version, arguments as a structured array, exit code, duration, and bounded output.
- Kill the complete process tree on cancellation or timeout.
- Never log secrets; the app has none by design. Paths may be sensitive, so reports should support a redacted mode.

## 6. Required verification chain

An item is `Succeeded` only when every mandatory stage succeeds:

1. Source path and root containment validated.
2. Source SHA-256 calculated over the complete original file.
3. WavPack process exits successfully.
4. WavPackâ€™s requested verification pass reports no failure.
5. Temporary `.wv` exists, is nonzero, and is inside the destination root.
6. `wvunpack` restores the temporary archive to an isolated temporary WAV.
7. Restored full-file length equals source full-file length.
8. Restored full-file SHA-256 equals source full-file SHA-256.
9. Archive SHA-256 calculated over the final archive bytes.
10. Temporary archive atomically published under the final `.wv` path.
11. Durable success journal entry flushed.
12. Manifest/report updated without losing the journal evidence.

WavPackâ€™s stored MD5 is useful but covers raw audio, not necessarily every WAV wrapper byte. It never replaces the complete-file SHA-256 comparison.

## 7. Architecture and dependency rules

Target solution shape:

```text
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
```

Dependency direction:

```text
Domain <- Application <- WinForms
                  ^
                  â”œâ”€ Infrastructure
                  â””â”€ WavPack
```

- `Domain` has no UI, filesystem, process, JSON, or WavPack dependency.
- `Application` defines use cases and interfaces/ports.
- `Infrastructure` implements filesystem, hashing, persistence, clock, and platform services.
- `WavPack` owns CLI-specific invocation and parsing.
- `WinForms` composes dependencies and renders state; it does not contain archive logic.
- Use built-in .NET facilities unless a third-party package has a documented, reviewed need.
- Centralize package versions and enable lock files.

## 8. Coding standards

- C# latest language version supported by the pinned .NET 10 SDK.
- Nullable reference types enabled; warnings treated as errors in CI.
- Prefer immutable records for plans, evidence, and results.
- Use `CancellationToken` throughout asynchronous operations.
- Use `IAsyncEnumerable<T>` for streaming large scans where appropriate.
- Do not block the UI thread with `.Result`, `.Wait()`, or synchronous process reads.
- All public APIs have XML documentation when behavior is not obvious.
- Represent bytes with `long`, not `int`.
- Use UTC ISO-8601 timestamps in records; display local time in the UI.
- Persist enum-like values as stable strings with unknown-value handling.
- Use deterministic JSON property naming and ordering where practical.
- Avoid global mutable state, static service locators, and hidden environment dependencies.
- Never swallow exceptions. Translate expected failures into typed outcomes and preserve diagnostic context.
- Keep logs structured; do not use logs as the authoritative manifest.

## 9. UI rules

The Windows Forms app has three primary workspaces:

1. **Archive** â€” select roots, scan, review plan, run, and export report.
2. **Audit** â€” verify manifests and `.wv` files without modifying them.
3. **Restore** â€” select archives/manifest and restore to a separate root.

Required behaviors:

- Folder selection plus editable path fields.
- Explicit preflight summary before processing.
- Per-item state, overall progress, current stage, bytes processed, and elapsed time.
- Pause means stop scheduling new files; do not attempt to suspend a child process in an unsafe state.
- Cancel is responsive and leaves a coherent journal.
- All critical commands keyboard-accessible.
- Do not rely on color alone for status.
- Respect high DPI, Windows text scaling, high contrast, and reduced motion.
- UI text must distinguish `Verified`, `Skipped`, `Conflict`, `Failed`, and `Cancelled`.
- Never use celebratory success language when any item failed or remained unverified.

Use a presenter/view-model style that allows workflow tests without opening native windows.

## 10. Persistence rules

- `manifest.json` is a versioned snapshot for portability and human inspection.
- `journal.jsonl` is append-only operation evidence for crash recovery.
- Flush each terminal item record durably before moving to the next item.
- Use write-temp, flush, and atomic replace for snapshot files.
- Preserve unknown manifest fields when feasible or reject unsupported major versions clearly.
- Hash fields include algorithm names and lowercase hexadecimal values.
- Relative paths use `/` in persisted JSON independent of Windows separators.
- Never write unvalidated absolute restore targets from manifest content.

Follow `docs/MANIFEST_SPEC.md` exactly. Schema changes require compatibility tests and a migration note.

## 11. Testing requirements

Every behavioral change must include tests at the lowest useful layer. Archive pipeline changes also need integration or end-to-end coverage.

Minimum corpus coverage:

- 8-, 16-, 24-, and 32-bit integer PCM WAV.
- 32-bit IEEE float WAV.
- Mono, stereo, and multichannel.
- Silence, impulses, tones, speech/music-like material, and incompressible noise.
- RIFF chunks before and after audio, odd-sized chunks, padding, BWF metadata, LIST/INFO, cue data, and trailing bytes.
- RF64 / large-file fixtures where practical; sparse/synthetic builders may avoid checking huge binaries into Git.
- Unicode, combining characters, spaces, punctuation, long paths, and case variants.
- Empty, truncated, malformed, locked, inaccessible, and changing files.
- Destination conflict, out-of-space simulation, process crash, corrupt `.wv`, cancellation at every stage, and stale partial files.
- Reparse-point loops and path traversal attempts.

Release tests must perform real WavPack encode/decode operations using the pinned binaries and compare whole-file SHA-256.

Do not use only mocks to claim archival correctness.

## 12. Security and supply chain

- Pin the .NET SDK in `global.json`.
- Pin NuGet dependencies centrally and commit lock files.
- Acquire WavPack only from the approved upstream release source.
- Record exact filename, version, source, size, and SHA-256 in dependency metadata.
- Verify hashes during build and at application startup.
- Bundle the complete upstream license and notices.
- CI must run secret scanning, dependency vulnerability review, static analysis, unit tests, integration tests, and artifact hash generation.
- Release signing is strongly recommended. Do not invent publisher identity or certificate details.
- The application must not auto-download executable code in version 1.

## 13. Agent workflow

For each task:

1. Read the relevant requirements and ADRs.
2. Restate the taskâ€™s safety impact in the work log or pull request.
3. Inspect existing code before proposing new abstractions.
4. Make the smallest coherent change that advances one acceptance criterion.
5. Add or update tests first when practical.
6. Run formatting, build, focused tests, then the full applicable suite.
7. Update documentation and manifest examples if behavior changes.
8. Report exact commands run and any tests not run.
9. Never mark a task done while placeholders, fake hashes, silent fallbacks, or unhandled safety cases remain.

### Do not

- Rewrite unrelated files.
- Change the archive promise without an ADR.
- add a â€œtemporaryâ€ destructive mode.
- bypass validation to make a test pass.
- depend on undocumented WavPack console text when exit codes or machine-checkable evidence are available.
- claim a benchmark or compression ratio without a reproducible corpus and results.
- generate binary dependencies from untrusted mirrors.
- commit compiled proprietary fixtures or copyrighted audio.

## 14. Definition of done

A feature is done only when:

- Acceptance criteria are implemented.
- Safety invariants remain true.
- Unit and applicable integration/end-to-end tests pass.
- Cancellation and failure behavior are tested.
- User-facing error text is actionable.
- Documentation, sample records, and changelog are updated.
- No analyzer warnings, placeholders, or unexplained ignored tests remain.
- A reviewer can recover archived WAVs with plain `wvunpack` and verify them independently.

A release is done only when every item in `docs/codex/ACCEPTANCE_CHECKLIST.md` is checked with linked evidence.
