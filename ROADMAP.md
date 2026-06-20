# Roadmap

Dates are intentionally omitted until maintainers and implementation capacity exist. Safety gates determine release readiness.

## Milestone 0 â€” Research and proof

- Confirm product name.
- Pin .NET 10 SDK patch and WavPack 5.9.0 artifacts.
- Prove whole-file round trips on generated WAV corpus.
- Record tool behavior and real hashes.

## Milestone 1 â€” Archive engine alpha

- Domain/path model.
- Recursive scanner/planner.
- WavPack adapter.
- One-file transactional pipeline.
- Journal/manifest.
- Headless end-to-end tests.

## Milestone 2 â€” Multi-file/resume alpha

- Bounded orchestration.
- Pause/cancel.
- Crash recovery and orphan reconciliation.
- Reports and redaction.
- Fault-injection suite.

## Milestone 3 â€” WinForms beta

- Archive UI.
- Audit and Restore UI.
- Accessibility and high-DPI work.
- Local help/About/license views.
- Settings limited to safe preferences.

## Milestone 4 â€” Release candidate

- Complete corpus/support matrix.
- Clean-machine offline package.
- SBOM, checksums, signing if available.
- Independent source-safety/path review.
- Recovery drill without WavCrusher.
- Naming/legal/claim review.

## Version 1.0

- Every acceptance gate has linked evidence.
- No open critical/high data-safety defect.
- Portable Windows x64 release and source tag.
- Accurate website and recovery documentation.

## Post-1.0 candidates

- Headless CLI using the same Application/Domain layers.
- Scheduled/manual audit reminders without background network access.
- Additional Windows architectures.
- Cross-platform archive/audit/restore CLI.
- Optional parity/recovery data integration.
- Signed manifests.
- Faster pure-lossless profile as a distinct, explicit policy.

## Explicitly not automatically planned

- Source deletion.
- Lossy/hybrid settings.
- Cloud sync.
- Proprietary container.
- User-supplied WavPack arguments.

Each would require a new decision and threat review.
