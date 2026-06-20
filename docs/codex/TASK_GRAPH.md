# Codex Task Graph

This graph decomposes the application into reviewable tasks. IDs are stable planning references, not issue numbers.

## Dependency overview

```text
R0 Research/tool proof
 └─ B0 Solution bootstrap
     ├─ D0 Domain model
     │   └─ S0 Scanner/planner
     ├─ W0 WavPack adapter
     └─ P0 Persistence foundations

D0 + S0 + W0 + P0
 └─ A0 Single-item archive pipeline
     ├─ A1 Journal/manifest/report
     ├─ A2 Coordinator/concurrency
     └─ A3 Resume/reconciliation

A1 + A3
 ├─ U0 Audit engine
 └─ U1 Restore engine

A0 + A2 + U0 + U1
 └─ UI0 WinForms shell
     ├─ UI1 Archive workspace
     ├─ UI2 Audit workspace
     ├─ UI3 Restore workspace
     └─ UI4 Activity/settings/about/accessibility

All above
 ├─ T0 Full corpus/fault hardening
 ├─ PK0 Packaging/supply chain
 └─ RC0 Release candidate evidence
```

## Task inventory

| ID | Task | Depends on | Primary output | Gate |
|---|---|---|---|---|
| R0 | Verify official WavPack dependency/CLI | Docs | Real hashes, CLI notes, research round trips | Gate 0 |
| R1 | Naming and claim review record | None | Naming decision/claim language | Public release |
| B0 | Solution/projects/global SDK | R0 | Clean build | Gate 1 |
| B1 | CI/analyzers/lock files | B0 | Enforced policy | Gate 1 |
| D0 | Path/root/hash/profile value objects | B0 | Pure domain model | Gate 2 |
| D1 | State machines/failure codes/evidence | D0 | Terminal outcome model | Gate 2 |
| D2 | Verified predicate | D1 | Proof completeness validation | Gate 2 |
| S0 | Root validator | D0 | Safe roots | Gate 3 |
| S1 | Recursive scanner | S0 | Candidate stream | Gate 3 |
| S2 | Output planner/collision detector | S1 | Immutable plan | Gate 3 |
| S3 | Space estimator | S2 | Preflight bounds | UI1 |
| W0 | Dependency metadata validator | R0, B0 | Trusted tool identity | Gate 4 |
| W1 | Process runner | W0 | Safe child lifecycle | Gate 4 |
| W2 | Encoder argument builder/adapter | W1 | Pure-lossless encode | Gate 4 |
| W3 | Decoder/test adapter | W1 | Exact restore/integrity | Gate 4 |
| P0 | Streaming SHA-256 | B0 | Hash service | Gate 5 |
| P1 | File observations/identity | B0 | Source-change evidence | Gate 5 |
| P2 | Atomic persistence helpers | B0 | Safe snapshots/publication | Gate 5/6 |
| P3 | Temp workspace manager | B0 | Owned cleanup | Gate 5 |
| A0 | One-item pipeline skeleton | D2,S2,W2,W3,P0-P3 | Stage orchestration | Gate 5 |
| A1 | Full verification/publish | A0 | Verified archive evidence | Gate 5 |
| A2 | Append-only journal | P2,D1 | Durable events | Gate 6 |
| A3 | Manifest v1 repository | A2 | Portable snapshot | Gate 6 |
| A4 | JSON/HTML report generator | A3 | Reports/redaction | Gate 6 |
| A5 | Bounded multi-item coordinator | A1,A2 | Archive operation | Gate 5/6 |
| A6 | Pause/cancel | A5,W1 | Controlled interruption | Gate 5 |
| A7 | Resume/reconcile | A2,A3,A5 | Crash recovery | Gate 7 |
| U0 | Audit engine | A3,W3,P0 | Audit levels/report | Gate 7 |
| U1 | Restore planner | D0,A3 | Safe restore plan | Gate 7 |
| U2 | Restore pipeline | U1,W3,P0,A2 | Verified restore | Gate 7 |
| UI0 | Shell/navigation/composition | B0 | Launchable app | Gate 8 |
| UI1 | Archive setup/scan/preflight | S2,S3,UI0 | Plan UX | Gate 8 |
| UI2 | Archive operation/progress | A5,A6,A7,UI1 | Execution UX | Gate 8 |
| UI3 | Audit workspace | U0,UI0 | Audit UX | Gate 8 |
| UI4 | Restore workspace | U1,U2,UI0 | Restore UX | Gate 8 |
| UI5 | Activity/reports/settings/about | A4,UI0 | Supporting UX | Gate 8 |
| UI6 | Accessibility/high DPI | UI1-UI5 | Accessible primary flows | Gate 8 |
| T0 | Fixture generator | R0,B0 | Deterministic corpus | Gate 9 |
| T1 | Metadata/RF64/malformed corpus | T0,A1 | Support evidence | Gate 9 |
| T2 | Path/reparse security suite | S0-S2,U1 | Containment proof | Gate 9 |
| T3 | Fault-injection suite | A1-A7,U0-U2 | Failure safety proof | Gate 9 |
| T4 | Soak/performance benchmark | A5,UI2,T0 | Qualified results | Gate 9 |
| PK0 | Self-contained portable package | UI6,W0 | Release folder/ZIP | Gate 10 |
| PK1 | License/SBOM/checksums | PK0 | Supply-chain evidence | Gate 10 |
| PK2 | Clean VM/offline drill | PK1 | Deployment evidence | Gate 10 |
| RC0 | Acceptance checklist | All | Linked release evidence | Gate 11 |
| RC1 | Independent reviews | All | Safety/accessibility/license review | Gate 11 |
| RC2 | Website/release docs final | RC0,RC1 | Accurate public release | Gate 11 |

## Parallelization guidance

Safe early parallel tracks after B0:

- Domain (`D*`).
- WavPack adapter (`W*`) after R0.
- Persistence primitives (`P*`).
- Corpus generator (`T0`).

Do not parallelize competing definitions of path/evidence/status models. Do not build WinForms workflows before Application contracts are stable enough to consume. Do not postpone real WavPack integration until after the UI.

## Critical path

```text
R0 → B0 → D0/D1/D2 → S0/S1/S2 → W0/W1/W2/W3 → P0-P3
→ A0/A1 → A2/A3 → A5/A6/A7 → U0/U1/U2
→ UI0-UI6 → T1-T4 → PK0-PK2 → RC0-RC2
```

## Review boundaries

Prefer one pull request per row or tightly coupled pair. A PR may cross rows only when tests cannot express value independently. Never combine the first archive pipeline with the entire UI and packaging stack.
