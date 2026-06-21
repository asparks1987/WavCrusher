# ADR 0003 - Source retention is user-controlled after verification

- **Status:** Accepted
- **Date:** 2026-06-19
- **Updated:** 2026-06-21
- **Related requirements:** FR-030, FR-050A

## Context

WavCrusher's primary value is proving that archives restore correctly. Earlier planning language treated source retention as an application-level prohibition. The project direction now separates archive verification from user retention policy.

## Decision

WavCrusher must first prove each `.wv` by full byte-for-byte restore verification and must prove the final `.tar.gz` package by extracting it and comparing packaged payload bytes against staged content. After that evidence exists, source retention is a user-controlled policy decision outside the verification predicate.

Documentation should recommend conservative retention practices: keep redundant verified copies and perform restore drills before discarding any source copy.

## Consequences

- The success predicate remains based on evidence, not convenience.
- Documentation can discuss user-controlled source cleanup without weakening `.wv` or package verification requirements.
- Any automated cleanup feature must be explicit, opt-in, and tied to completed verification evidence.

## Verification

Release evidence must show both per-file `.wv` byte-for-byte restore verification and final `.tar.gz` extract-and-compare package verification.
