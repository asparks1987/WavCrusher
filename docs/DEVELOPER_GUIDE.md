# Developer Guide

## 1. Prerequisites

Planned development environment:

- Supported Windows 10/11 x64 workstation.
- Current supported .NET 10 SDK patch pinned in `global.json`.
- Visual Studio with .NET desktop development workload, or equivalent `dotnet` CLI tooling.
- Git with long-path support configured where appropriate.
- Official pinned WavPack Windows x64 release acquired according to `RELEASE_AND_PACKAGING.md`.

Do not commit unverified third-party executables. Dependency metadata must contain real hashes generated from the approved upstream artifact.

## 2. First setup

Once implementation exists:

```powershell
git clone <repository>
cd WavCrusher
dotnet --info
dotnet restore --locked-mode
dotnet build WavCrusher.sln -c Debug --no-restore
dotnet test WavCrusher.sln -c Debug --no-build
```

A bootstrap script may place approved WavPack binaries under `third_party/wavpack/win-x64`, but it must:

- Require an explicit upstream release asset or approved cache.
- Verify a committed expected SHA-256.
- Never download and execute an unpinned â€œlatestâ€ artifact.
- Retain license/provenance.

Version 1 runtime must not download tools automatically.

## 3. Repository conventions

```text
src/                    Product source
tests/                  Unit, integration, and end-to-end tests
tools/                  Corpus/maintenance tools
third_party/wavpack/     Pinned sidecar dependency and notices
docs/                    Product and engineering documentation
artifacts/               Ignored local build output
TestResults/             Ignored test output
```

Use one class/type per file when it improves clarity; group tiny tightly related records when appropriate. Namespaces match project/folder intent, not necessarily every folder segment.

## 4. Build policy

`Directory.Build.props` should enable:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <Deterministic>true</Deterministic>
  <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
</PropertyGroup>
```

Pin dependencies centrally in `Directory.Packages.props` and commit NuGet lock files. Avoid dependencies where the framework provides a clear implementation.

## 5. Local commands

```powershell
# Format verification
dotnet format WavCrusher.sln --verify-no-changes

# Unit tests
dotnet test tests/WavCrusher.Domain.Tests -c Release
dotnet test tests/WavCrusher.Application.Tests -c Release

# Real WavPack integration tests (environment contract to be defined)
$env:WAVHARBOR_RUN_WAVPACK_INTEGRATION = "1"
dotnet test tests/WavCrusher.WavPack.IntegrationTests -c Release

# Complete release test suite
dotnet test WavCrusher.sln -c Release --collect:"XPlat Code Coverage"
```

Do not make archival correctness dependent on a coverage percentage. Coverage identifies gaps; real corpus round trips and fault tests prove the central promise.

## 6. Coding guidance

### Asynchrony

- All long filesystem, hash, process, and persistence operations are asynchronous where meaningful.
- Propagate `CancellationToken`.
- Never block the WinForms UI thread.
- Use `ConfigureAwait(false)` in library code according to team convention; presentation code returns to the UI context deliberately.

### Memory

- Stream content; never read a large WAV or `.wv` entirely into memory.
- Use pooled bounded buffers when profiling justifies it.
- Retain bounded tool output.
- Use `long` for sizes and progress.

### Paths

- Pass validated path value objects across application boundaries.
- Revalidate filesystem state near use; planning does not eliminate races.
- Do not normalize by lowercasing display paths.
- Test Windows device prefixes, UNC paths if supported, and long-path behavior explicitly.

### Processes

- Only `WavCrusher.WavPack` launches the CLI tools.
- Use `ArgumentList` and absolute executable paths.
- Drain stdout/stderr concurrently.
- Capture exit code before disposing.
- Kill the process tree on cancellation.
- Treat progress parsing as advisory, not evidence.

### Persistence

- Journals append and flush.
- Manifest/report snapshots are transactional.
- JSON is UTF-8 and versioned.
- Do not serialize exceptions wholesale; create bounded diagnostics.

### Errors

- Convert expected operational conditions to typed failure codes.
- Preserve inner exception/Win32 code for support diagnostics.
- User messages explain safe next actions.
- Never catch `Exception` and continue as if success.

## 7. Test architecture

### Unit tests

- Domain invariants/state transitions.
- Relative path validation.
- Planner mapping/collisions.
- Success predicate completeness.
- Manifest validation and compatibility.
- Presenter behavior.

### Component tests

- Real filesystem scanner in temporary trees.
- Atomic persistence and journal recovery.
- Hashing and source-change detection.
- Process adapter with a controllable fake child executable.

### Integration tests

- Real pinned `wavpack.exe` / `wvunpack.exe`.
- Generated fixture corpus.
- Unicode/long/special-character paths.
- Tool tamper/version checks.

### End-to-end tests

- Complete archive, audit, and restore.
- Cancellation and resume.
- Fault injection and clean-machine packaging.

See `TESTING_STRATEGY.md` for the normative matrix.

## 8. Fixture policy

Do not add copyrighted music or user recordings. Generate deterministic fixtures:

- PCM silence, impulses, ramps, tones, pseudorandom noise.
- Float edge cases.
- RIFF chunks/padding/trailers.
- BWF metadata with synthetic text.
- RF64 headers and sparse files where appropriate.

Each fixture generator version should produce documented SHA-256 values. Small hand-crafted binary fixtures may be committed with provenance.

## 9. UI development

- Keep designer-generated code minimal and separate.
- Use `TableLayoutPanel`, `FlowLayoutPanel`, docking, and anchoring for scaling.
- Avoid fixed pixel assumptions.
- Set accessible names/descriptions and logical tab order.
- Use a presenter/view-model layer for testable commands/state.
- Throttle progress to a few UI updates per second while preserving exact final values.
- Test high contrast and 125%, 150%, 200% scaling.

## 10. Documentation changes

Update the relevant requirement and documentation in the same pull request when behavior changes. Safety changes also require an ADR. Schema changes require:

- Version/compatibility analysis.
- Updated samples.
- Validation tests.
- Migration/rejection behavior.
- Changelog entry.

## 11. Pull requests

A pull request should be small enough to review. Include:

- Requirement IDs.
- Safety analysis.
- Before/after behavior.
- Tests and exact results.
- UI screenshots/accessibility notes where relevant.
- Dependency/license impact.
- Remaining risks.

Do not merge with unexplained skipped tests, placeholder hashes, analyzer suppression, or unbounded TODOs in archive-critical code.

## 12. Debugging an archive mismatch

When a whole-file hash differs:

1. Preserve source, temporary archive, restored fixture, journal, tool identity, and bounded diagnostics in an isolated test environment unless reproducing the explicit source-cleanup behavior under a controlled test case.
2. Compare file lengths.
3. Locate first byte difference with a diagnostic tool; do not add this expensive operation to normal success paths.
4. Inspect RIFF chunk structure/wrapper differences.
5. Confirm decoder arguments did not force a fresh wrapper.
6. Confirm source did not change between observations.
7. Reproduce with official CLI directly.
8. Add a minimized, redistributable regression fixture.
9. Never downgrade the failure to audio-only success.

## 13. Release builds

```powershell
dotnet publish src/WavCrusher.WinForms/WavCrusher.WinForms.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:DebugType=embedded `
  -p:PublishReadyToRun=false
```

Final properties should be tested for reproducibility, startup, antivirus reputation, and debugging. Keep WavPack executables/licenses as visible sidecars even if the app is single-file.
