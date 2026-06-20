# Glossary

**Archive set** â€” Destination tree containing WavPack archives and WavCrusher evidence files.

**Atomic publication** â€” Renaming/moving a verified temporary archive to its final name as one filesystem operation without exposing partial content.

**BWF** â€” Broadcast Wave Format, a WAV extension that adds professional metadata, commonly in a `bext` chunk.

**Complete-file hash** â€” A cryptographic digest over every byte of a file, including container metadata and padding.

**Conflict** â€” A condition such as an existing destination or path collision that prevents safe processing without user investigation.

**Compression ratio** - The archive size compared with the original source size. WavCrusher reports this per file so users can see actual storage savings.

**Fixity** â€” Evidence that digital content has not changed, commonly established through cryptographic hashes.

**Hash** â€” A deterministic digest such as SHA-256. It detects changed bytes but does not by itself prove authorship or repair corruption.

**Hybrid mode** â€” WavPack mode that creates a lossy stream, optionally with a correction file. WavCrusher version 1 does not use it.

**Journal** â€” Append-only JSONL operation evidence used for crash recovery.

**Manifest** â€” Versioned JSON snapshot describing archive items, paths, hashes, tools, operations, and verification results.

**PCM** â€” Pulse-code modulation, the common uncompressed sample representation in WAV files.

**Pure lossless** â€” Encoding mode that discards no source information needed for exact restoration.

**Reparse point** â€” Windows filesystem object used by symbolic links, junctions, mount points, and other redirection features. Version 1 does not recurse into them.

**RF64** â€” An extension of RIFF/WAVE that supports files larger than the usual 4 GiB RIFF limit.

**RIFF chunk** â€” A typed section of a WAV/RIFF file, such as `fmt `, `data`, `LIST`, or `bext`.

**Round-trip verification** â€” Encode, decode, and compare the restored result with the source.

**SHA-256** â€” A widely used 256-bit cryptographic hash. WavCrusher uses it for complete source, restored, and archive files.

**SourceChanged** â€” Failure indicating the inputâ€™s observed state changed during processing.

**WAV wrapper** â€” The non-audio container bytes around PCM/float data: RIFF/RF64 headers, chunks, padding, metadata, and trailers.

**WavPack** â€” Open lossless/hybrid audio compression format and toolchain. WavCrusher uses its pure-lossless mode.

**Whole-file lossless** â€” Restoration in which every file-content byte matches, not only decoded audio samples.

**Worker pool** - The bounded set of concurrent archival workers WavCrusher uses to process multiple files at once without launching unbounded WavPack processes.

**`wavpack`** â€” Encoder executable used to create `.wv` files.

**`wvunpack`** â€” Decoder executable used to test and restore `.wv` files.
