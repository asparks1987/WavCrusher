# ADR 0004 â€” Require complete-file extraction round-trip verification

- **Status:** Accepted
- **Date:** 2026-06-19
- **Related requirements:** G-2, FR-038â€“042, AS-040â€“046

## Context

Lossless audio codecs commonly prove decoded samples, but a WAV may also contain important RIFF/RF64 metadata, application chunks, padding, and trailers. WavPack preserves wrapper data, yet an application integration error or decoder option could still regenerate a different wrapper.

## Decision

Before publishing a final archive, WavCrusher decodes the temporary `.wv` to an isolated temporary WAV and requires complete-file length and SHA-256 equality with the source. WavPackâ€™s own `-v` pass and optional raw-audio MD5 remain additional layers, not substitutes.

## Alternatives considered

- Trust encoder exit code and `-v`: insufficient to prove app-selected decode/restoration behavior and whole wrapper equality.
- Compare PCM samples only: insufficient for whole-file promise.
- Byte-by-byte comparison only: exact but expensive to retain diagnostics; SHA-256 plus length is efficient, with optional first-difference diagnostics on failure.

## Consequences

- Requires temporary space equal to a restored WAV and additional decode time.
- Provides a straightforward, independently explainable proof.
- Enables strong regression fixtures for unusual metadata.

## Verification

Corpus includes unknown chunks, odd padding, BWF/iXML/cue data, trailers, float, and RF64 cases. Every Verified item has matching source/restored complete-file hashes.
