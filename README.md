# WavCrusher

**Open-source, lossless WAV storage for Windows.**

> Current official alpha release: **v1.0.21a** for Windows 10/11 x64.

WavCrusher is a Windows desktop application that recursively discovers `.wav` files, compresses each file into a standard WavPack `.wv` archive, verifies that the archive can reproduce the **entire original WAV file byte-for-byte**, and records an auditable manifest.

The design is deliberately conservative:

- Pure lossless WavPack onlyâ€”never hybrid or lossy modes.
- Each `.wv` is restored and compared byte-for-byte before it is treated as verified.
- The source directory structure is mirrored under a separate destination.
- Every successful archive is independently restored to a temporary WAV and compared with the original using SHA-256.
- Output is a normal `.wv` file recoverable with upstream `wvunpack`; WavCrusher is not required for future extraction.
- Interrupted work leaves resumable journal records and never publishes a partially written archive as complete.
- Current real-world archive runs are consistently landing around **30-50% of the original WAV size** on suitable collections, while still staying pure lossless.

This repository package contains the WinForms application, product requirements, safety specification, architecture, UX plan, manifest format, test strategy, release plan, project governance files, sample records, and a dependency-free website under [`/docs`](docs/).

## Project status

**Official alpha v1.0.21a is implemented with WinForms UI, verified archive packaging, restore support, MSI packaging, optional verified source cleanup, and bundled WavPack sidecars.**

The build plan targets:

- Windows 10/11 x64 first.
- .NET 10 LTS and Windows Forms (`net10.0-windows`).
- WavPack 5.9.0 command-line tools pinned and distributed with their license and hashes.
- A self-contained application distribution that does not require a system-wide WavPack installation.

The v1.0.21a release is an alpha, not a stable/production guarantee. Stable release language should wait until all gates in [`docs/codex/ACCEPTANCE_CHECKLIST.md`](docs/codex/ACCEPTANCE_CHECKLIST.md) pass.

## Product promise

> **Your WAVs, smaller. Every byte comes back.**

Observed archive output is commonly around **30-50% of the original WAV bytes** for the collections tested so far. Exact compression remains content-dependent, so WavCrusher reports per-file original size, archive size, and compression ratio instead of hiding behind a single marketing number.

â€œLosslessâ€ can mean two different things:

1. **Audio-sample lossless:** decoded sample values are unchanged.
2. **Whole-file lossless:** every byte of the original fileâ€”including RIFF/RF64 headers, metadata chunks, padding, and trailersâ€”is reproduced.

WavCrusherâ€™s default archival workflow requires both. WavPackâ€™s own verification protects encoded audio; WavCrusher adds a complete extraction round trip and compares SHA-256 hashes of the original and restored WAV files.

## Canonical archive workflow

For each discovered source file:

1. Normalize and validate source and destination roots.
2. Enumerate `.wav` files recursively without following directory reparse points.
3. Record source identity, length, timestamps, and SHA-256.
4. Encode to a same-directory temporary `.partial` file with a pinned WavPack binary.
5. Require a successful encoder exit code and WavPack verification pass.
6. Decode the temporary `.wv` to an isolated temporary WAV using `wvunpack` without forcing a new wrapper.
7. SHA-256 the restored WAV and require exact equality with the original WAV hash.
8. SHA-256 the `.wv` archive.
9. Atomically move the verified temporary archive to its final path.
10. Append a durable journal record and update the versioned manifest/report.

The intended WavPack profile is equivalent to:

```text
wavpack -hh -x6 -m -v -t -z0 --no-overwrite <source.wav> <temporary-name.partial.wv>
```

Arguments are supplied with `ProcessStartInfo.ArgumentList`; the application must never construct a shell command string. The adapter behavior is tied to the pinned WavPack release shipped with the installer.

### Forbidden options

The application must reject or never expose settings that could undermine the promise:

```text
-b                 hybrid/lossy mode
-c                 hybrid correction-file workflow
--pre-quantize     discards source precision
-r                 discards the original wrapper
-d                 deletes the source after success
-i                 ignores certain input length errors
wvunpack --wav     generates a new WAV wrapper instead of restoring the saved one
```

## User workflow

### Archive

Choose a source folder and a separate destination folder. WavCrusher scans the tree, previews the planned output paths, flags conflicts or unsupported files, estimates free-space requirements, and starts only after the user confirms their retention settings.

The result is a mirrored tree such as:

```text
Source                         Destination
D:\Masters\Album\Track.wav  -> E:\Archive\Album\Track.wv
```

### Audit

Select an archive root or manifest. WavCrusher checks archive SHA-256, invokes `wvunpack` integrity verification, optionally performs full extraction comparisons where source WAVs are available, and emits a signed-off report suitable for periodic storage scrubs.

### Restore

Select a manifest/archive root and a separate restore destination. WavCrusher restores the original relative paths and requires the whole-file SHA-256 recorded at archive time to match before marking an item restored.

## Safety invariants

These are non-negotiable:

- Each `.wv` and the final `.tar.gz` package are verified after compression/packaging before completion is reported.
- Source and destination roots cannot be equal.
- Neither root may be nested inside the other.
- Directory reparse points are skipped unless a future, separately reviewed feature enables them.
- Relative paths cannot escape a chosen root.
- Existing destination files are never silently overwritten.
- A final `.wv` name is published only after full verification.
- Cancellation stops scheduling new work and safely terminates current child processes.
- Files that change during processing fail with a clear â€œsource changedâ€ result.
- Logs and manifests never claim success without all required evidence.
- The app works offline and performs no telemetry or network transfer.
- Optional source cleanup is off by default and may run only after both per-file `.wv` verification and final `.tar.gz` verification pass, with each deletion recorded in the operation report.

See [`docs/ARCHIVE_SAFETY_SPEC.md`](docs/ARCHIVE_SAFETY_SPEC.md) for the normative specification.

## Repository layout

```text
/
â”œâ”€â”€ README.md                         Project overview
â”œâ”€â”€ README.me                         Compatibility pointer for requested filename
â”œâ”€â”€ AGENTS.md                         Rules for Codex and other coding agents
â”œâ”€â”€ CODEX_BUILD_PLAN.md               Ordered implementation plan
â”œâ”€â”€ CONTRIBUTING.md                   Contribution workflow
â”œâ”€â”€ SECURITY.md                       Security reporting and boundaries
â”œâ”€â”€ THIRD_PARTY_NOTICES.md            WavPack distribution requirements
â”œâ”€â”€ docs/
â”‚   â”œâ”€â”€ index.html                    Static product website
â”‚   â”œâ”€â”€ PRODUCT_REQUIREMENTS.md       Functional and non-functional requirements
â”‚   â”œâ”€â”€ ARCHITECTURE.md               System design and project boundaries
â”‚   â”œâ”€â”€ ARCHIVE_SAFETY_SPEC.md        Data-integrity contract
â”‚   â”œâ”€â”€ MANIFEST_SPEC.md              JSON manifest and journal format
â”‚   â”œâ”€â”€ USER_GUIDE.md                 Intended end-user documentation
â”‚   â”œâ”€â”€ DEVELOPER_GUIDE.md            Local development conventions
â”‚   â”œâ”€â”€ TESTING_STRATEGY.md           Corpus, fault injection, and release gates
â”‚   â”œâ”€â”€ RELEASE_AND_PACKAGING.md      Reproducible shipping plan
â”‚   â”œâ”€â”€ THREAT_MODEL.md               Risks and mitigations
â”‚   â”œâ”€â”€ ACCESSIBILITY.md              WinForms and website accessibility targets
â”‚   â”œâ”€â”€ PRIVACY.md                    Local-only privacy promise
â”‚   â”œâ”€â”€ FAQ.md                        Product and technical answers
â”‚   â”œâ”€â”€ REFERENCES.md                 Primary-source references
â”‚   â”œâ”€â”€ adr/                          Architecture decision records
â”‚   â””â”€â”€ codex/                        Task graph, prompts, and acceptance gates
â”œâ”€â”€ samples/                          Example settings, manifest, and report
â””â”€â”€ .github/                          Issue, PR, CI, and Pages templates
```

## Documentation map

| Need | Start here |
|---|---|
| Understand the product | [`docs/PRODUCT_REQUIREMENTS.md`](docs/PRODUCT_REQUIREMENTS.md) |
| Implement it with Codex | [`CODEX_BUILD_PLAN.md`](CODEX_BUILD_PLAN.md) and [`AGENTS.md`](AGENTS.md) |
| Review archival correctness | [`docs/ARCHIVE_SAFETY_SPEC.md`](docs/ARCHIVE_SAFETY_SPEC.md) |
| Review architecture | [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| Build the manifest model | [`docs/MANIFEST_SPEC.md`](docs/MANIFEST_SPEC.md) |
| Build the UI | [`docs/USER_GUIDE.md`](docs/USER_GUIDE.md) and product requirements |
| Test and release | [`docs/TESTING_STRATEGY.md`](docs/TESTING_STRATEGY.md) and [`docs/RELEASE_AND_PACKAGING.md`](docs/RELEASE_AND_PACKAGING.md) |
| Release notes | [`docs/RELEASE_NOTES.md`](docs/RELEASE_NOTES.md) |
| Preview marketing copy | [`docs/index.html`](docs/index.html) |

## Build outline

```powershell
dotnet restore WavCrusher.sln
dotnet build WavCrusher.sln --no-restore
dotnet test WavCrusher.sln --no-build
.\buildwavcrusher.ps1 -ProductVersion 1.0.21a -NoRestore
```

Exact SDK versions belong in `global.json`; dependency versions and WavPack artifact hashes must be pinned in source control. Never insert guessed hashes into release metadata.

## Recovery without WavCrusher

A core design goal is avoiding application lock-in. A user can recover an individual file with the bundled or upstream WavPack decoder:

```powershell
wvunpack.exe "E:\Archive\Album\Track.wv" "D:\Restored\Album\Track.wav"
```

Do not add `--wav`, `--raw`, normalization, format-conversion, or metadata-altering options when exact wrapper restoration is required.

## Licensing

WavCrusher documentation and application source are offered under the MIT License. WavPack is a separate project distributed under its own BSD-style license. Bundled WavPack executables must retain upstream notices, version provenance, and cryptographic hashes. See [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

## Name and claim review

Before a stable public launch:

- Perform trademark and package-name searches for â€œWavCrusher.â€
- Replace provisional logos or copy where necessary.
- Have preservation claims reviewed against the implemented test evidence.
- Avoid claims such as â€œsmallest possibleâ€ or â€œfuture-proof.â€ It is fine to cite observed 30-50% archive sizes for suitable WAV collections, but compression ratios remain content-dependent and long-term preservation also requires redundant storage, fixity checks, migration planning, and maintained recovery tools.

## Contributing

Read [`CONTRIBUTING.md`](CONTRIBUTING.md), [`AGENTS.md`](AGENTS.md), and the safety specification before changing archive behavior. A 30-50% archive-size result is excellent, but a smaller output file is never worth weakening recoverability or source safety.
