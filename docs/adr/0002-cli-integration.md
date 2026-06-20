# ADR 0002 — Integrate WavPack through pinned command-line sidecars

- **Status:** Accepted for version 1 plan
- **Date:** 2026-06-19
- **Related requirements:** FR-002–005, NFR-020–023

## Context

WavPack provides a C library and mature command-line tools. Direct library binding could improve progress/control and reduce child-process overhead, but requires native interop design, packaging, and a larger correctness surface. The CLI already implements encoding, decoding, and verification.

## Decision

Version 1 uses official pinned Windows x64 `wavpack.exe` and `wvunpack.exe` sidecars. The app invokes them directly by absolute path with `ProcessStartInfo.ArgumentList`, verifies their hashes/version, captures bounded diagnostics, and never uses a shell.

## Alternatives considered

- P/Invoke/native wrapper: potentially appropriate later after parity tests and security review.
- Reimplement codec: rejected.
- Require system-installed WavPack/PATH: rejected because it weakens provenance and reproducibility.

## Consequences

- Recovery tools remain visible and independently usable.
- Child-process lifecycle, output parsing, and cancellation require careful tests.
- CLI text must not be the sole correctness signal.
- The product package contains multiple executables and licenses.

## Verification

Argument snapshots, metacharacter path tests, process-tree cancellation tests, tool-tamper tests, and real encode/decode integration tests.
