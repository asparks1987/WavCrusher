# WavCrusher Requirements Matrix

Status values:

- `planned` - assigned to a component and test layer, not implemented yet.
- `in-progress` - scaffolding or partial code exists.
- `blocked` - requires external evidence or a later phase.
- `verified` - implemented with passing evidence.

## Safety specification

| ID range | Planned component | Test layer | Status | Evidence |
|---|---|---|---|---|
| AS-001-AS-005 | Application pipeline, Infrastructure filesystem, WavPack adapter | Unit, component, end-to-end immutability | in-progress | Alpha archive flow verifies `.wv` output and final `.tar.gz` packages before completion. |
| AS-010-AS-015 | Domain path model, Infrastructure root validator, scanner/planner | Domain and Windows filesystem tests | in-progress | Scanner/planner and root validation exist in the alpha application. |
| AS-020-AS-026 | WavCrusher.WavPack | Fake process tests, real WavPack integration | in-progress | `third_party/wavpack/dependency.json` records official 5.9.0 hashes; sidecars are bundled. |
| AS-030-AS-037 | Archive item pipeline, temp workspace, atomic publication | Fault injection and real filesystem tests | in-progress | Alpha pipeline publishes final `.wv` only after verification and packages verified payloads. |
| AS-040-AS-046 | Archive item pipeline, hash service, WavPack decoder | Corpus and integration tests | in-progress | Per-file source/restored/archive hashes and package verification are implemented. |
| AS-050-AS-055 | Journal and manifest repository | Persistence and schema tests | in-progress | Tarball manifest evidence is generated and embedded in verified packages. |
| AS-060-AS-063 | Resume/reconcile use case | Fault and restart tests | planned | Pending resume phase. |
| AS-070-AS-073 | Audit use case | Read-only audit integration tests | planned | Pending audit phase. |
| AS-080-AS-084 | Restore use case | Hostile manifest and restore tests | in-progress | WinForms package load/restore path validates manifests and restored hashes. |
| AS-090-AS-094 | Domain failure model, coordinators, UI presenters | Unit, integration, presenter tests | in-progress | UI reports verified/failed/skipped states, progress, ratios, and cleanup outcomes. |

## Product requirements

| ID range | Planned component | Test layer | Status | Evidence |
|---|---|---|---|---|
| FR-001-FR-005 | WinForms shell, WavPack dependency validator | Smoke, integration, packaging | in-progress | Alpha WinForms app and MSI packaging are implemented with bundled sidecars. |
| FR-010-FR-016 | Archive presenter, RootValidator | Domain, infrastructure, presenter | in-progress | Main UI supports editable roots, browse/clear controls, and root validation. |
| FR-020-FR-028 | Scanner and planner | Fake and real filesystem tests | in-progress | Alpha scanner populates the archive grid and supports recursive discovery. |
| FR-030-FR-048 | Archive item pipeline and coordinator | Fault, cancellation, real WavPack tests | in-progress | Alpha archive flow supports bounded multi-file work, per-file ratios, verified package creation, and optional post-verify source cleanup. |
| FR-050-FR-058 | Journal, manifest, reports | Persistence, schema, golden tests | in-progress | Package manifest is written and final `.tar.gz` payload bytes are verified. |
| FR-060-FR-064 | Resume/reconcile | Restart/idempotency tests | planned | Pending resume phase. |
| FR-070-FR-075 | Audit engine | Audit integration tests | planned | Pending audit phase. |
| FR-080-FR-087 | Restore engine | Restore integration and hostile manifest tests | in-progress | Package restore verifies restored bytes against manifest hashes. |
| FR-090-FR-094 | Settings and WinForms presenters | Unit and UI smoke tests | in-progress | UI exposes safe controls, warning-gated source cleanup, About details, and progress feedback. |
| UX-001-UX-010 | WinForms views and presenters | Presenter, accessibility, manual checks | in-progress | Main UI includes scan/archive/restore controls, detailed progress, ratios, original locations, and per-field clear buttons. |
| NFR-001-NFR-052 | Cross-cutting | Unit, integration, corpus, packaging | in-progress | Evidence accumulates through builds, tests, MSI packaging, and observed 30-50% archive-size runs. |

## Current implementation evidence

- Naming ADR: `docs/adr/0005-rename-to-wavcrusher.md`.
- Official WavPack asset recorded in `third_party/wavpack/dependency.json`.
- CLI research notes started in `docs/implementation/WAVPACK_CLI_NOTES.md`.
- Alpha source projects, WinForms UI, archive/restore/package flows, MSI packaging, and bundled WavPack sidecars are present.
- Current suitable WAV collections are consistently producing observed archive sizes around 30-50% of source bytes.
