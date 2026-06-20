# ADR 0001 â€” Use standard one-file-per-WAV WavPack archives

- **Status:** Accepted for version 1 plan
- **Date:** 2026-06-19
- **Related requirements:** G-3, NFR-050, NFR-051

## Context

The product needs audio-aware lossless compression while allowing recovery decades later without a proprietary WavCrusher database or executable. A custom aggregate container could reduce per-file overhead and centralize metadata, but would create lock-in, larger corruption blast radius, and a new format maintenance burden.

## Decision

Create one ordinary pure-lossless `.wv` file per source `.wav`, preserving the relative directory tree. Keep manifests/journals as sidecar evidence. Do not wrap `.wv` files in a proprietary container.

## Alternatives considered

- FLAC with foreign-metadata preservation: standardized and widely supported, but the planned WAV coverage includes float and wrapper cases that WavPack directly targets; comparative corpus work may revisit this in a future profile.
- Generic 7z/xz: open and byte-preserving but generally less audio-aware.
- Custom database/container: rejected for recovery lock-in and failure-domain concerns.

## Consequences

- Users can recover with `wvunpack` alone.
- Individual file corruption/loss affects one archive rather than a monolith.
- Directory and per-file overhead remain.
- Manifests are useful but not required to decode a single file; they remain required to prove original whole-file hashes and structure.

## Verification

Release tests restore sample `.wv` files after removing the WavCrusher executable and compare SHA-256 with the manifest.
