# WavCrusher v1.0.21a Release Notes

WavCrusher v1.0.21a is the official alpha release of the Windows desktop app for verified pure-lossless WAV archiving.

This release focuses on a simple promise: make large WAV collections much smaller while proving every verified file can restore byte-for-byte.

## Highlights

- Creates verified pure-lossless WavPack `.wv` archives from WAV folders.
- Observed archive sizes commonly land around **30-50% of original WAV size** on suitable collections.
- Verifies each `.wv` by restoring it and comparing whole-file SHA-256 evidence.
- Builds and verifies final `.tar.gz` packages after archive creation.
- Shows per-file original bytes, archive bytes, and compression ratio.
- Uses bounded multi-file archival to take better advantage of multi-core systems.
- Loads restore packages and shows each file's original location.
- Restores WAV files and verifies restored bytes against the manifest.
- Provides optional, warning-gated source cleanup after `.wv` and `.tar.gz` verification succeeds.
- Shows detailed overall and current-item progress during long jobs.
- Ships as a self-contained Windows MSI with bundled `wavpack.exe` and `wvunpack.exe`.

## Safety Notes

WavCrusher does not treat "encoded" as success. A file is marked Verified only after a full restore comparison proves the original WAV bytes can come back exactly.

Optional source deletion is off by default. When enabled, WavCrusher rechecks verification evidence and deletes only after the package has been validated.

## Included Artifact

- `WavCrusher.Setup.1.0.21a.msi`

The installer targets:

```text
C:\Program Files\WavCrusher\
```

## Known Boundaries

- This is an alpha release, not a stable/production guarantee.
- Compression ratios are content-dependent; the 30-50% range is an observed result for suitable WAV collections, not a universal promise.
- WavCrusher is an archive creation, verification, and restore tool. It is not a complete backup strategy by itself.
