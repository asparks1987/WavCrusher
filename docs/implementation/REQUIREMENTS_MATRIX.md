# WavCrusher Requirements Matrix

Status values:

- `planned` - assigned to a component and test layer, not implemented yet.
- `in-progress` - scaffolding or partial code exists.
- `blocked` - requires external evidence or a later phase.
- `verified` - implemented with passing evidence.

## Safety specification

| ID range | Planned component | Test layer | Status | Evidence |
|---|---|---|---|---|
| AS-001-AS-005 | Application pipeline, Infrastructure filesystem, WavPack adapter | Unit, component, end-to-end immutability | planned | Archive engine not implemented yet. |
| AS-010-AS-015 | Domain path model, Infrastructure root validator, scanner/planner | Domain and Windows filesystem tests | in-progress | Domain bootstrap will encode reserved `.wavcrusher` path and unsafe relative path rejection. |
| AS-020-AS-026 | WavCrusher.WavPack | Fake process tests, real WavPack integration | in-progress | `third_party/wavpack/dependency.json` records official 5.9.0 hashes. |
| AS-030-AS-037 | Archive item pipeline, temp workspace, atomic publication | Fault injection and real filesystem tests | planned | Pending pipeline phase. |
| AS-040-AS-046 | Archive item pipeline, hash service, WavPack decoder | Corpus and integration tests | planned | Pending pipeline phase. |
| AS-050-AS-055 | Journal and manifest repository | Persistence and schema tests | planned | Pending persistence phase. |
| AS-060-AS-063 | Resume/reconcile use case | Fault and restart tests | planned | Pending resume phase. |
| AS-070-AS-073 | Audit use case | Read-only audit integration tests | planned | Pending audit phase. |
| AS-080-AS-084 | Restore use case | Hostile manifest and restore tests | planned | Pending restore phase. |
| AS-090-AS-094 | Domain failure model, coordinators, UI presenters | Unit, integration, presenter tests | planned | Pending application phases. |

## Product requirements

| ID range | Planned component | Test layer | Status | Evidence |
|---|---|---|---|---|
| FR-001-FR-005 | WinForms shell, WavPack dependency validator | Smoke, integration, packaging | in-progress | WavPack metadata recorded; app not implemented yet. |
| FR-010-FR-016 | Archive presenter, RootValidator | Domain, infrastructure, presenter | planned | Pending scanner/planner. |
| FR-020-FR-028 | Scanner and planner | Fake and real filesystem tests | planned | Pending scanner/planner. |
| FR-030-FR-048 | Archive item pipeline and coordinator | Fault, cancellation, real WavPack tests | planned | Pending archive engine. |
| FR-050-FR-058 | Journal, manifest, reports | Persistence, schema, golden tests | planned | Pending evidence phase. |
| FR-060-FR-064 | Resume/reconcile | Restart/idempotency tests | planned | Pending resume phase. |
| FR-070-FR-075 | Audit engine | Audit integration tests | planned | Pending audit phase. |
| FR-080-FR-087 | Restore engine | Restore integration and hostile manifest tests | planned | Pending restore phase. |
| FR-090-FR-093 | Settings and WinForms presenters | Unit and UI smoke tests | planned | Pending UI phase. |
| UX-001-UX-010 | WinForms views and presenters | Presenter, accessibility, manual checks | planned | Pending UI phase. |
| NFR-001-NFR-052 | Cross-cutting | Unit, integration, corpus, packaging | planned | Evidence accumulates through gates. |

## Current implementation evidence

- Naming ADR: `docs/adr/0005-rename-to-wavcrusher.md`.
- Official WavPack asset recorded in `third_party/wavpack/dependency.json`.
- CLI research notes started in `docs/implementation/WAVPACK_CLI_NOTES.md`.
- Source projects and tests begin in Phase 1/2.
