# ADR 0005 - Rename product to WavCrusher

- **Status:** Accepted for implementation
- **Date:** 2026-06-19
- **Related requirements:** R1, FR-004, NFR-042

## Context

The initial planning package used the provisional name WavCrusher's predecessor name throughout documentation, sample manifests, and planned project names. The implementation repository and product direction now use `WavCrusher`.

Keeping two names would create avoidable confusion in manifests, namespaces, release artifacts, UI copy, and recovery instructions. The rename is not a safety-model change, but it touches evidence identifiers and reserved metadata paths.

## Decision

Use `WavCrusher` consistently for the product, solution, namespaces, executable, documentation, sample records, and visible UI. Use lowercase `wavcrusher` in stable machine identifiers where the prior provisional name used a lowercase token, including manifest/report formats and the reserved destination metadata folder.

The reserved archive metadata directory is:

```text
.wavcrusher/
  manifests/
  journals/
  reports/
  operations/
```

## Consequences

- Existing pre-implementation samples and docs are updated before source code is generated.
- Version 1 manifests use `wavcrusher-archive-manifest`, not the provisional identifier.
- Any future compatibility work must treat the provisional identifier as documentation-only unless a released artifact actually used it.
- Public naming/trademark review is still required before a stable public release.

## Verification

Repository searches for the provisional name must return no project-authored references after the rename, excluding downloaded upstream/provenance files if any.
