# Frequently Asked Questions

## Is WavPack lossless?

Yes in its default pure-lossless mode. WavCrusher does not enable WavPackâ€™s hybrid/lossy mode. It also independently decodes each candidate archive and compares the entire restored WAV with the original SHA-256 before marking it Verified.

## Does â€œ100% original qualityâ€ mean the file itself is identical?

For an item marked Verified, that is the intended promise: the complete restored WAV byte sequence matches the source WAV observed during archiving. This is stronger than merely comparing decoded audio samples.

## Why compare SHA-256 when WavPack already verifies data?

WavPackâ€™s verification and stored MD5 protect the encoded audio data. WavCrusher also cares about the complete WAV wrapperâ€”headers, metadata chunks, padding, and trailersâ€”so it performs an independent extraction and hashes the whole restored file.

## Will WavCrusher delete my original WAVs?

No. Version 1 has no source deletion, replacement, move, or rename feature. Source files are read-only inputs.

## Can I restore without WavCrusher?

Yes. Each output is a standard `.wv` file. Use the bundled or upstream `wvunpack` tool and compare the restored SHA-256 with the manifest.

## Is `.wv` playable?

Many audio applications support WavPack, but playability is not required for WavCrusherâ€™s storage goal. The archive can always be decoded back to WAV with a compatible WavPack decoder.

## How much space will I save?

It depends on the audio. Structured material often compresses well; high-resolution noise may compress poorly. The official WavPack project describes a broad typical range, but WavCrusher must show estimatesâ€”not guaranteesâ€”and should report actual ratios per file.

## Is WavCrusher guaranteed to make the smallest possible archive?

No. No practical compressor wins for every input, and â€œsmallest possibleâ€ is not a responsible universal claim. WavCrusher chooses WavPack for a strong balance of compression, open recovery, broad WAV support, maintained tooling, and wrapper preservation.

## Why use the slow `-x6` setting?

The product goal prioritizes long-term storage size over encoding speed. Extra analysis can improve compression and does not impose the same cost on decoding. The UI must make the slow operation clear. A future faster profile would need product/safety review and distinct evidence.

## Can I choose lossy or hybrid settings?

No. Those modes undermine the simple archival promise and are intentionally absent.

## Can source and archive folders be on the same drive?

Yes if they are separate, non-nested folder trees and enough temporary space is available. Independent physical storage is strongly recommended for redundancy, but the appâ€™s path rule concerns safe processing topology.

## Why canâ€™t the destination be inside the source?

The scanner could encounter its own outputs, and source/archive responsibilities would become ambiguous. WavCrusher rejects overlapping roots instead of trying to guess safely.

## Does it follow shortcuts, junctions, or symbolic links?

Not in version 1. Directory reparse points are skipped and reported to prevent loops and containment escapes.

## What happens when a `.wv` already exists?

It is a conflict. WavCrusher does not overwrite it. The user may audit/reconcile the existing archive or choose a new destination after investigation.

## What if the computer loses power?

The app writes temporary archives, appends durable journal records, and publishes final names only after verification. On restart, it inspects the valid journal prefix and classifies temporary/orphan files conservatively. No design can eliminate all hardware risk, so redundant copies remain essential.

## Does WavCrusher preserve timestamps and permissions?

The whole-file promise covers file bytes. The app plans to record common timestamps/attributes and may restore them by policy. Full NTFS ACLs, alternate data streams, EFS, hard-link relationships, and all filesystem metadata are outside version 1 scope.

## Does it support every WAV file?

No universal claim is made. The planned scope includes uncompressed PCM and IEEE-float WAV variants supported and proven by the pinned WavPack toolchain. Compressed WAV codecs such as ADPCM may be unsupported and will be reported without modifying the original.

## Is this a backup program?

It can be part of an archival workflow but is not by itself a complete backup system. Keep multiple verified copies, at least one off-site, and audit them periodically.

## Does it upload my audio?

No. Version 1 is local-only and has no telemetry or automatic network access.

## Why WinForms?

WinForms is a mature Windows desktop UI technology with strong local filesystem integration, offline operation, accessibility support through Windows, and a productive designer/tooling ecosystem. The archive engine remains separated from the UI so a future CLI is possible.

## Why .NET 10?

The plan targets the current Long Term Support .NET generation at the time this package was authored. The actual build must pin a currently supported patch and remain updated within its support lifecycle.

## Can I archive network shares?

The final support policy should be decided after testing. Network storage changes failure and atomicity assumptions. A conservative version 1 may allow readable shares but must warn that same-filesystem atomic publication and connectivity behavior need validation; clean local disks are the reference environment.

## What should I keep with my archives?

Keep `.wv` files, `.wavcrusher` evidence, WavPack decoder and license, WavCrusher release/docs, release checksums, and recovery instructions. Test recovery before disposing of any source copy.
