# Archive Safety Specification

WavCrusher's safety model is simple: a WAV archive is useful only when it can be restored exactly and the user can understand what happened.

## Core promise

An item marked **Verified** has passed the complete archival chain:

1. The original WAV was hashed with SHA-256.
2. WavPack encoded a pure-lossless `.wv` archive.
3. WavPack's verification pass succeeded.
4. WavCrusher restored the archive to temporary storage.
5. Restored length and full-file SHA-256 matched the original.
6. The final `.wv` was published and recorded with evidence.

WavPack audio MD5 is useful evidence, but it does not replace complete-file SHA-256 over the WAV wrapper and all bytes.

## Source handling

Source WAV files are archival inputs. WavCrusher must prove the `.wv` archive and final `.tar.gz` package before reporting success. Users should make multiple verified archive copies and perform recovery drills before making retention decisions. A future V1.0+ option will allow users to delete sources after verification only when explicitly enabled.

## Path safety

WavCrusher rejects overlapping source and destination roots, unsafe relative paths, traversal, reserved metadata collisions, and directory reparse-point recursion. Manifest paths are treated as untrusted and revalidated before restore.

## Output safety

Archives are written to operation-owned temporary names first. A final `.wv` file is published only after verification succeeds. Existing outputs are conflicts, not overwrite targets.

## Package verification

After the final `.tar.gz` is created, WavCrusher must decompress and extract it into a verification workspace, then compare packaged payload hashes against the staged `.wv` and manifest content. Package completion must not be reported if extracted bytes differ.

## Failure language

Failed, skipped, conflict, cancelled, and verified states must remain distinct in the UI, reports, manifests, and documentation. A completed operation with any issue must say so plainly.

## Performance boundary

WavCrusher may use bounded multi-core archival by processing multiple files at once, but concurrency must never weaken per-file verification, cancellation, or evidence recording.
