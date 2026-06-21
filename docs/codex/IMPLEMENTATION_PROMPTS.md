# Codex Implementation Prompts

Use these prompts sequentially in a repository that contains this documentation. Replace bracketed repository-specific details only after inspection. Each session must read `AGENTS.md` first and produce a reviewable change, not an all-at-once implementation.

## Standard preamble for every Codex task

```text
Read AGENTS.md, docs/ARCHIVE_SAFETY_SPEC.md, docs/PRODUCT_REQUIREMENTS.md,
and the ADRs relevant to this task. Inspect the current repository before editing.
Safety and whole-file recoverability outrank speed and code volume.

Work only on the requested task. Do not add overwrite, lossy/hybrid
options, free-form WavPack arguments, shell invocation, network behavior, or symlink
following. Add tests and update documentation. Run the applicable commands and report
exact results. If an assumption about WavPack or Windows is unproven, create a failing
or skipped research test / documented blocker rather than pretending it is true.
```

## Prompt 00 â€” Requirements matrix and CLI proof

```text
Create docs/implementation/REQUIREMENTS_MATRIX.md mapping every FR/UX/NFR/AS ID to:
planned component, test layer, status, and evidence link. Create a small research harness
or scripts under tools/ that invoke the approved official WavPack 5.9.0 Windows x64
binaries directly using structured arguments.

Acquire no untrusted binaries and do not invent hashes. Add third_party/wavpack/
dependency.example.json if the real approved binary is not available; make the build
clearly fail or skip integration with an actionable message until real dependency
metadata exists.

Prove on generated WAV fixtures that the intended pure-lossless options encode, verify,
and restore byte-for-byte. Investigate the exact temporary output filename convention,
console encoding/progress, exit codes, no-overwrite behavior, cancellation, and decoder
arguments. Record findings in docs/implementation/WAVPACK_CLI_NOTES.md. Do not build UI.
```

Acceptance:

- Real dependency hashes are recorded only when computed from approved artifacts.
- Whole-file SHA-256 equality is demonstrated for the research corpus.
- No source file changes.
- Temporary output naming is proven, not assumed.

## Prompt 01 â€” Solution bootstrap

```text
Create WavCrusher.sln and projects specified in docs/ARCHITECTURE.md. Target net10.0 for
portable libraries and net10.0-windows for Windows-specific projects. Pin a currently
supported .NET 10 SDK in global.json after checking the official support policy.

Add Directory.Build.props, Directory.Packages.props, lock files, analyzers, nullable,
deterministic builds, and warnings as errors. Add minimal project-reference tests or
architecture rules that prevent forbidden dependency directions. Add test projects but
no archive behavior yet. Update developer docs and CI. Run restore/build/test.
```

Acceptance:

- Clean build and tests.
- Correct layer references.
- No unnecessary third-party packages.
- SDK/package versions pinned.

## Prompt 02 â€” Domain and path invariants

```text
Implement immutable domain value objects and state models for normalized absolute paths,
validated relative paths, archive roots, plan items, hashes, tool identity, profile,
evidence, status/stage, and typed failures. The Domain project must not touch the real
filesystem or reference UI/process/JSON packages.

Encode Windows case-insensitive equality/nesting semantics through injected/explicit
policies without lowercasing display paths. Reject traversal, rooted/drive-relative,
device, ADS, empty, and unsafe persisted paths. Encode the Verified success predicate so
missing evidence cannot produce success. Add exhaustive unit tests and update the
requirements matrix.
```

Acceptance:

- Invalid plans/paths are rejected.
- Success requires every verification field.
- Domain remains pure.

## Prompt 03 â€” Scanner and planner

```text
Implement the Application ports and Infrastructure Windows scanner/planner. Recursively
discover .wav case-insensitively, include readable hidden/system files, skip directory
reparse points with diagnostics, and remain cancellable. Map to a mirrored destination
with only the final extension changed to .wv. Reserve .wavcrusher metadata paths.

Validate source/destination equality and both nesting directions using canonical Windows
behavior, not simple string prefixes. Detect destination existence, case collisions,
access failures, and unsafe paths. Stream results and produce a deterministic immutable
plan summary. Add fake and real temporary-filesystem tests, including junction loops,
Unicode, long paths, inaccessible/disappearing entries, and root races where practical.
```

Acceptance:

- No loop or out-of-root mapping.
- Every skipped/error candidate is visible.
- Repeated unchanged scan yields same logical plan.

## Prompt 04 â€” WavPack adapter

```text
Implement only the WavCrusher.WavPack adapter and its Application interfaces. Verify the
absolute bundled tool path, SHA-256, and reported version against dependency metadata.
Use ProcessStartInfo with UseShellExecute=false, CreateNoWindow=true, redirected streams,
and ArgumentList. Never invoke a shell or search PATH.

Implement exact allowlisted encoder and decoder argument builders derived from the CLI
research. Add asynchronous bounded stdout/stderr capture, advisory progress parsing,
exit mapping, timeout plumbing, and CancellationToken process-tree termination. Add a
controllable fake child process for component tests and real WavPack integration tests.
Snapshot exact arguments and test spaces, Unicode, quotes, &, ^, %, !, semicolons, and
long paths. Reject tampered/wrong-version tools before launch.
```

Acceptance:

- No string-built command line/shell.
- No forbidden option possible through public API.
- Cancellation kills descendants.
- Real encode/decode test passes whole-file equality.

## Prompt 05 â€” One-item transactional pipeline

```text
Implement ArchiveItemPipeline for one planned item. It must hash the source,
observe source state before/after, encode to an operation-owned same-directory temporary
archive name, require encoder verification, decode into an isolated temporary workspace,
compare complete-file length and SHA-256, hash the archive, publish to final with no
overwrite, and return immutable evidence.

Use streaming I/O and long counters. Never write directly to final. Never publish when
cancelled or when any evidence is absent. Clean only operation-owned temporary files.
Add deterministic fault injection at every stage and real end-to-end tests with metadata-
rich fixtures. Snapshot source tree before/after every test.
```

Acceptance:

- Supported fixtures round-trip exactly.
- No source modification.
- No failure/cancel leaves unverified final output.
- Conflict race never overwrites.

## Prompt 06 â€” Journal, manifest, and reporting

```text
Implement the v1 JSONL journal and manifest specification. Append terminal item records
with sequence/event/operation IDs and flush durably. Read a valid journal prefix when the
last line is truncated; reject earlier corruption. Write manifest/report snapshots through
temp files and atomic replacement. Generate JSON and standalone HTML reports, with an
optional absolute-root redaction mode.

Validate every successful item has complete evidence. Add schema/validation tests,
golden examples, unknown-minor/additive-field behavior, unknown-major rejection, hostile
path rejection, duplicate critical property handling, overflow/depth/size limits, and
placeholder hash rejection for release records.
```

Acceptance:

- Manifest can be rebuilt from journal.
- Snapshot failure does not destroy prior evidence.
- No incomplete item serializes as verified.

## Prompt 07 â€” Coordinator, pause, cancel, and resume

```text
Implement bounded multi-item orchestration. Use a small configurable worker count within
validated limits. Pause stops scheduling new items; active items may finish. Cancel stops
scheduling, propagates cancellation, terminates process trees, and preserves coherent
terminal journal evidence.

Implement resume from journal/operation metadata. Revalidate roots, tools, source/archive
state, and containment. Never infer success from a .wv file alone. Handle stale partials,
complete journal/no snapshot, snapshot/no archive, and a final archive published before a
success record as explicit cases. Reconcile an orphan final archive only through full
independent verification. Prove idempotency with restart/fault tests.
```

Acceptance:

- Large plans do not create unbounded tasks/processes.
- Resume is conservative and repeatable.
- Completion summary exactly matches terminal evidence.

## Prompt 08 â€” Audit and restore

```text
Implement Audit and Restore use cases without UI. Audit supports distinct levels: archive
SHA-256, WavPack decoder integrity, full current-source round trip, and recovery-sample
comparison against the manifestâ€™s original hash. Reports must state the exact depth run.

Restore validates all manifest paths, uses a separate root, never overwrites, decodes
without wrapper/normalization/format-forcing options, and requires the archive-time
complete-file SHA-256 before VerifiedRestored. Add hostile manifest, corrupt/missing
archive, wrong hash, insufficient space, cancel, and plain-wvunpack interoperability tests.
```

Acceptance:

- Hostile manifest cannot escape root.
- Existing restore targets remain unchanged.
- Every restored success matches original complete-file hash.

## Prompt 09 â€” WinForms shell and Archive UX

```text
Implement a responsive WinForms shell and Archive workspace over existing Application use
cases. Use passive views/presenters or an equivalent testable pattern. Add source/destination
folder selection with editable fields, validation, cancellable scan, preflight summary,
efficient item grid, clear conflicts/warnings, confirmation, progress/stages, pause,
cancel, completion, and report access.

Do not add archive logic to forms. Throttle progress updates, marshal to the UI thread
safely, and preserve responsiveness. Implement keyboard navigation, accessible names,
status text/icons, high DPI/layout scaling, high contrast, and no color-only meaning.
Add presenter tests and manual accessibility notes/screenshots.
```

Acceptance:

- Entire Archive workflow keyboard-operable.
- UI remains responsive under a large synthetic plan.
- Verified/failed/conflict/skipped/cancelled are unmistakable.

## Prompt 10 â€” Audit, Restore, Activity, Settings, About UX

```text
Build the Audit and Restore workspaces and an Activity/Reports view. Add only safe settings:
worker count within tested limits, report redaction, temp workspace policy, UI preferences,
and last locations. Do not expose arbitrary WavPack arguments, lossy/hybrid modes, source
deletion, overwrite, wrapper discard, or verification bypass.

About must show app/tool versions, dependency hash status, licenses, source reference, and
o-telemetry/offline statement. Include local help and emergency wvunpack recovery steps.
Add presenter/UI/accessibility tests.
```

Acceptance:

- Dangerous settings absent by construction.
- Tool/license/recovery information available offline.
- All primary workflows have coherent cancel/error/completion states.

## Prompt 11 â€” Corpus, security, and fault hardening

```text
Create tools/TestCorpusBuilder to generate deterministic legal WAV fixtures covering the
matrix in TESTING_STRATEGY.md, including PCM widths, float, channels/rates, metadata chunks,
odd padding, trailers, RF64 strategy, Unicode/long names, silence, and noise. Record fixture
provenance and hashes.

Complete all fault-injection, path-security, source-immutability, journal truncation,
resume, tool tamper, disk-full, locked/changing file, process crash/hang, and soak tests.
Fix defects; do not weaken expected outcomes. Update the supported-input matrix from actual
evidence and record qualified benchmarks.
```

Acceptance:

- All supported fixtures exact.
- No unverified final outputs across injected faults.
- No source changes.
- Claims match measured evidence.

## Prompt 12 â€” Packaging and release candidate

```text
Implement the portable win-x64 release pipeline. Publish the app self-contained; retain
wavpack.exe, wvunpack.exe, exact upstream license, dependency metadata, docs, schemas, and
recovery instructions as visible sidecars. Generate SBOM and SHA256SUMS after packaging.
Do not add an auto-updater.

Test on clean offline Windows VMs, including removing WavCrusher.exe and restoring with the
bundled decoder alone. Complete the release acceptance checklist with links/evidence,
perform dependency/license/accessibility/source-safety reviews, update website status and
claims, and produce release notes. Do not call it stable while any gate lacks evidence.
```

Acceptance:

- Clean-machine archive/audit/restore succeeds.
- Plain decoder recovery succeeds.
- Checksums/licenses/SBOM complete.
- Acceptance checklist fully evidenced.

## Handoff format after every prompt

```text
Task:
Commit / branch:
Requirement IDs addressed:
Files changed:
Key decisions:
Safety impact:
Tests run and exact results:
Tests not run:
Known risks / blockers:
Documentation updated:
Next recommended task:
```
