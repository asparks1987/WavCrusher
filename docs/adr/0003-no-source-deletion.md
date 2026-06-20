# ADR 0003 — Never delete or replace source WAVs in version 1

- **Status:** Accepted
- **Date:** 2026-06-19
- **Related requirements:** FR-030, AS-001–003

## Context

Automatic deletion could make storage savings immediate, but a preservation tool has an asymmetric risk: one defect can destroy irreplaceable recordings. Verification does not prove that all copies, metadata policies, and user retention requirements are satisfied.

## Decision

Version 1 has no source deletion, replacement, move, or rename feature. It does not expose WavPack’s delete-source option. Users manage retention outside the app after creating multiple verified copies and performing recovery drills.

## Consequences

- The app temporarily requires both source and archive storage.
- Users receive a simpler, stronger safety promise.
- Source mutation code is absent and can be audited.
- A future destructive workflow would require a new product boundary, ADR, backups/rollback design, and explicit release review; it is not a routine feature request.

## Verification

Static argument/API checks and source-tree before/after snapshots in every end-to-end fault path.
