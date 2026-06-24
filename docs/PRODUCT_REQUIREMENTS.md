# Product Requirements Document

**Product:** WavCrusher  
**Document status:** v1.0.21a alpha baseline  
**Target release:** v1.0.21a alpha  
**Primary platform:** Windows 10/11 x64  
**UI technology:** Windows Forms on .NET 10 LTS

## 1. Product summary

WavCrusher is an open-source desktop application for reducing the storage footprint of WAV collections without surrendering any source information. It recursively scans a user-selected folder, writes one standard WavPack `.wv` file per WAV into a verified package, independently proves whole-file restoration, and records durable audit evidence. Current v1.0.21a alpha runs on suitable WAV collections are consistently producing archives around 30-50% of original source size.

The product is aimed at recording studios, musicians, researchers, oral-history projects, sound designers, archivists, and individuals who have large WAV collections but do not want proprietary archive lock-in or lossy conversion.

## 2. Problem

WAV is durable and widely understood, but uncompressed PCM consumes substantial space. Generic ZIP/7z compression often misses structure that an audio-aware lossless codec can exploit. Existing command-line tools are powerful but leave users to solve recursive traversal, safe path mapping, interruption handling, independent verification, and long-term evidence themselves.

A credible archival tool must do more than say â€œlossless.â€ It must:

- Leave originals untouched by default.
- Avoid overwriting unrelated destination files.
- Reproduce complete WAV bytes, not only equivalent samples.
- Make failures and skipped files impossible to confuse with successes.
- Produce standard archives recoverable outside the application.
- Leave manifests and checksums suitable for future audits.

## 3. Goals

### G-1 â€” Safe recursive archiving

Process all supported `.wav` files beneath a selected source root while preserving relative directory structure and producing verified archival evidence.

### G-2 â€” Whole-file proof

Require a decoded round trip and SHA-256 equality between each original WAV and restored WAV before publishing a final archive.

### G-3 â€” Open recovery

Produce ordinary WavPack files that can be restored with upstream `wvunpack`, without a proprietary database or container.

### G-4 â€” Auditable operations

Create versioned manifests, append-only journals, checksums, and reports that distinguish every terminal outcome.

### G-5 â€” Approachable Windows UX

Make correct archival settings the only default path, with clear preflight, progress, cancellation, resume, audit, and restore workflows.

### G-6 â€” Long-lived project fundamentals

Use an LTS runtime, open-source licensing, pinned dependencies, reproducible release metadata, offline operation, and maintainable documentation.

## 4. Non-goals for version 1

- Guaranteed globally minimal compressed size.
- Lossy or hybrid encoding.
- Audio editing, metadata editing, playback, transcoding, or normalization.
- Cloud backup or synchronization.
- Preservation of every filesystem-level feature such as ACLs and alternate streams.
- macOS/Linux graphical applications.
- A service, CLI daemon, or unattended scheduler.
- Duplicate detection across different source paths.
- Combining many WAVs into one custom archive.

## 5. Personas

### Studio owner

Has multi-terabyte session exports and masters. Wants space savings, an understandable verification report, and confidence that BWF and application-specific chunks return exactly.

### Independent archivist

Maintains oral histories and field recordings. Wants open recovery, routine fixity audits, clear failure records, and no account or telemetry.

### Technical collector

Comfortable with checksums but does not want to script path traversal and crash recovery. Wants detailed logs, tool provenance, and deterministic behavior.

### Occasional user

Has a large WAV folder and needs a guided workflow that prevents accidental same-folder output or destructive settings.

## 6. Core concepts

- **Source root:** Read-only folder containing WAV files.
- **Destination root:** Separate folder receiving `.wv` archives and operation metadata.
- **Archive set:** A destination tree plus its manifest/journal/report.
- **Archive item:** One source WAV mapped to one `.wv` file.
- **Whole-file verification:** Restoring a WAV and comparing complete-file SHA-256 with the original.
- **Fixity:** Evidence that a file has not changed, normally a cryptographic hash.
- **Audit:** Rechecking archive hashes and decoder integrity after creation.
- **Restore:** Recreating WAV files under a separate destination and checking recorded hashes.

## 7. Functional requirements

### 7.1 Application and dependency readiness

| ID | Requirement |
|---|---|
| FR-001 | The app shall run as a Windows desktop application without requiring a network connection. |
| FR-002 | The distribution shall include approved `wavpack.exe` and `wvunpack.exe` binaries, their license, version, and SHA-256 metadata. |
| FR-003 | The app shall verify dependency identity before the first archive/audit/restore operation of a session. |
| FR-004 | The About view shall display app version, WavPack version, dependency hash status, license links, and source repository information. |
| FR-005 | The app shall fail closed when required WavPack tools are missing, modified, unsupported, or unexecutable. |

### 7.2 Root selection and validation

| ID | Requirement |
|---|---|
| FR-010 | The Archive view shall accept a source folder and destination folder using both folder dialogs and editable path fields. |
| FR-011 | The app shall canonicalize and validate roots before scan or processing. |
| FR-012 | Source and destination shall not be the same folder. |
| FR-013 | Neither root shall be contained within the other. |
| FR-014 | The app shall show an actionable error for nonexistent, inaccessible, or unsuitable roots. |
| FR-015 | The destination shall be writable and free-space information shall be shown when available. |
| FR-016 | The app shall not require administrative privileges for normal operation. |

### 7.3 Discovery and planning

| ID | Requirement |
|---|---|
| FR-020 | The app shall recursively enumerate files whose extension equals `.wav` using ordinal case-insensitive comparison. |
| FR-021 | Directory reparse points shall not be followed. |
| FR-022 | Hidden/system WAV files shall be included when readable and identified in the plan. |
| FR-023 | Access-denied, disappearing, invalid, or too-long paths shall be recorded as warnings or failures without erasing other scan results. |
| FR-024 | Each candidate shall map to the same relative path under the destination with only the final extension changed to `.wv`. |
| FR-025 | The planner shall detect existing destinations, output collisions, unsafe relative paths, and unsupported candidates before processing. |
| FR-026 | The preview shall display file count, total source bytes, conflicts, warnings, destination free space, and estimated output range clearly labeled as an estimate. |
| FR-027 | The plan shall be stable and reviewable before the Start action is enabled. |
| FR-028 | The user shall be able to cancel an in-progress scan. |

### 7.4 Archiving

| ID | Requirement |
|---|---|
| FR-030 | The app shall create verified `.wv` archives and retain evidence needed to support user-controlled source retention decisions. |
| FR-030A | The app may offer an explicit user-controlled option to delete source files only after that file and the final package have passed required verification checks. |
| FR-031 | The app shall use one fixed pure-lossless WavPack archival profile in version 1. |
| FR-032 | The profile shall request highest compression, maximum extra analysis, stored audio MD5, output verification, timestamp copy where supported, and no overwrite. |
| FR-033 | The app shall create missing destination directories only beneath the validated destination root. |
| FR-034 | The encoder shall write to an operation-owned temporary file in the final archive directory. |
| FR-035 | A final `.wv` path shall not exist until all mandatory verification stages succeed. |
| FR-036 | Existing `.wv` files shall not be overwritten. |
| FR-037 | The app shall detect source changes during processing and fail the item instead of publishing it. |
| FR-038 | The app shall compute SHA-256 over the complete source WAV. |
| FR-039 | The app shall require successful WavPack process completion and output verification. |
| FR-040 | The app shall decode the temporary archive into an isolated temporary WAV without forcing a new wrapper or format conversion. |
| FR-041 | The app shall compare restored and source file lengths and SHA-256 values. |
| FR-042 | The app shall compute SHA-256 over the published `.wv` archive. |
| FR-043 | The app shall record input bytes, archive bytes, compression ratio, duration, hashes, tool identity, and terminal result. |
| FR-044 | Failed or cancelled items shall never be represented as successful. |
| FR-045 | Processing concurrency shall be bounded and configurable only within safe tested limits. |
| FR-046 | Pause shall stop new items from starting; active child processes may finish. |
| FR-047 | Cancel shall stop new work, terminate active process trees safely, clean operation-owned temporary files according to policy, and preserve the journal. |
| FR-048 | A completed run shall display totals for verified, skipped, conflicts, failures, and cancelled items. |

### 7.5 Journal, manifest, and reporting

| ID | Requirement |
|---|---|
| FR-050 | Each operation shall have a unique ID and start/end metadata. |
| FR-050A | After final `.tar.gz` creation, the app shall extract the package and compare packaged payload bytes against staged archive content before reporting completion. |
| FR-051 | Terminal item outcomes shall be appended to an operation journal and flushed before another item is considered complete. |
| FR-052 | The app shall produce a versioned JSON manifest that follows `MANIFEST_SPEC.md`. |
| FR-053 | The manifest shall use relative paths and include hashes with algorithm names. |
| FR-054 | The app shall be able to rebuild an operation snapshot from a valid journal prefix after interruption. |
| FR-055 | Snapshot/report writes shall be transactional. |
| FR-056 | The app shall produce JSON and human-readable reports. |
| FR-057 | Reports shall optionally redact source-root details while retaining relative paths and evidence. |
| FR-058 | Exported reports shall identify incomplete, unverified, or failed work prominently. |

### 7.6 Resume

| ID | Requirement |
|---|---|
| FR-060 | On opening a destination with an incomplete operation, the app shall offer to inspect and resume it. |
| FR-061 | Resume shall revalidate roots, tool identity, source state, destination state, and journal evidence. |
| FR-062 | Items with complete verified evidence may be recognized without recompression only when archive hash and manifest relationship are valid. |
| FR-063 | Incomplete temporary files shall not be promoted; the user shall be told whether they will be removed or retained for diagnosis. |
| FR-064 | Resume shall never infer success solely from the presence of a `.wv` file. |

### 7.7 Audit

| ID | Requirement |
|---|---|
| FR-070 | The Audit view shall load a compatible manifest and resolve archives beneath its selected root. |
| FR-071 | The user shall be able to run archive SHA-256 checks. |
| FR-072 | The user shall be able to run WavPack decoder integrity tests. |
| FR-073 | Where original sources remain available, the user shall be able to run full decoded whole-file comparisons. |
| FR-074 | The audit report shall classify missing, changed, corrupt, healthy, unsupported, and unverified items separately. |
| FR-075 | Auditing shall not modify archive or source content. |

### 7.8 Restore

| ID | Requirement |
|---|---|
| FR-080 | Restore shall require a destination root separate from the archive root. |
| FR-081 | Restore shall validate all manifest relative paths and containment before decoding. |
| FR-082 | Existing restored files shall never be silently overwritten. |
| FR-083 | `wvunpack` shall restore the stored original wrapper without conversion switches. |
| FR-084 | Each restored WAV shall be compared with the original whole-file SHA-256 recorded in the manifest. |
| FR-085 | An item shall not be marked restored until the hash matches. |
| FR-086 | Restore shall produce its own journal and report. |
| FR-087 | The UI and documentation shall include a plain-`wvunpack` emergency recovery procedure. |

### 7.9 Settings

| ID | Requirement |
|---|---|
| FR-090 | Settings may include worker count, temporary-space location policy, report redaction, UI preferences, and last-used folders. |
| FR-091 | Settings shall not expose lossy/hybrid modes, wrapper discard, unbounded overwrite, or verification bypass. |
| FR-092 | Settings shall have safe defaults and a reset action. |
| FR-093 | Invalid or future-version settings shall be ignored safely with a warning, not crash startup. |
| FR-094 | Source cleanup can be enabled intentionally and defaults off, with clear confirmation and auditable deletion records. |

## 8. User experience requirements

| ID | Requirement |
|---|---|
| UX-001 | First-run content shall explain that originals remain untouched and outputs require a separate location. |
| UX-002 | â€œVerifiedâ€ shall mean the complete round-trip chain passed, not merely that an encoder exited successfully. |
| UX-003 | Status shall use icon, text, and accessible nameâ€”not color alone. |
| UX-004 | Destructive actions shall be explicit, off by default, confirmed with a permanence warning, and gated behind completed verification. |
| UX-005 | Errors shall identify the affected item, failed stage, likely cause, and safe next action. |
| UX-006 | The UI shall remain responsive during scans, hashing, encode/decode, reporting, and cancellation. |
| UX-007 | Large collections shall use virtualized or efficient list presentation and throttled progress updates. |
| UX-008 | Navigation, dialogs, and grids shall be keyboard-operable with visible focus. |
| UX-009 | The UI shall support Windows high DPI, text scaling, high contrast, and screen-reader labels. |
| UX-010 | Completion screens shall never hide failures behind an overall success percentage. |

## 9. Non-functional requirements

### Reliability

| ID | Requirement |
|---|---|
| NFR-001 | No unverified final archive may be created by an expected failure or cancellation path. |
| NFR-002 | Operation evidence shall survive process termination up to the last fully flushed journal record. |
| NFR-003 | All file sizes and byte counters shall support values beyond 2 GiB. |
| NFR-004 | The application shall handle at least 100,000 planned files without loading file contents into memory. |

### Performance

| ID | Requirement |
|---|---|
| NFR-010 | Hashing and file copying shall stream using bounded buffers. |
| NFR-011 | Worker count shall avoid unbounded WavPack processes and disk thrashing. |
| NFR-012 | UI progress rendering shall be throttled independently from evidence recording. |
| NFR-013 | Maximum-compression mode is expected to be slow; the UI shall prioritize transparency over optimistic time estimates. |
| NFR-014 | Public copy may cite observed 30-50% archive sizes for suitable WAV collections, but the UI shall report actual per-file ratios and avoid guaranteed savings. |

### Security

| ID | Requirement |
|---|---|
| NFR-020 | No shell shall be used for process invocation. |
| NFR-021 | Untrusted path text shall never become an unvalidated filesystem target or executable argument. |
| NFR-022 | Bundled executable hashes shall be validated. |
| NFR-023 | The app shall not download or execute updates in version 1. |
| NFR-024 | The app shall operate as a standard user. |

### Privacy

| ID | Requirement |
|---|---|
| NFR-030 | No telemetry, analytics, account, advertising, or network upload shall exist in version 1. |
| NFR-031 | Reports shall make path disclosure explicit and offer redaction. |
| NFR-032 | Website assets shall contain no trackers, remote fonts, or third-party scripts. |

### Maintainability

| ID | Requirement |
|---|---|
| NFR-040 | Core workflows shall be testable without opening WinForms windows. |
| NFR-041 | Business logic shall not parse ad hoc UI strings or WavPack console text directly. |
| NFR-042 | Dependency and manifest versions shall be centralized. |
| NFR-043 | CI shall treat warnings as errors and run applicable tests. |

### Interoperability

| ID | Requirement |
|---|---|
| NFR-050 | Output shall be standard WavPack files, one per WAV. |
| NFR-051 | Recovery shall remain possible with upstream `wvunpack` and the manifest alone. |
| NFR-052 | Manifest JSON shall use UTF-8 and stable, documented field names. |

## 10. Supported input definition

Version 1 shall support the set of PCM/IEEE-float WAV variants proven by the pinned WavPack version and the project corpus. Expected coverage includes 8â€“32-bit integer PCM, 32-bit float, mono/stereo/multichannel, and files with preserved header/trailer chunks.

The application must not claim support based solely on extension. It shall surface WavPack rejection as `UnsupportedWave` or a more precise typed failure. Compressed WAV codecs such as ADPCM are not assumed supported.

The public support matrix must be generated from tests and list known exceptions.

## 11. Operation states

### Item statuses

```text
Planned
ScanningWarning
Ready
Running
Verified
Skipped
Conflict
Failed
Cancelled
```

A separate `stage` describes current work. Persist stable machine codes and render localized/user-friendly text separately.

### Operation statuses

```text
Created
Scanning
AwaitingConfirmation
Running
Pausing
Paused
Cancelling
Completed
CompletedWithIssues
Cancelled
Failed
```

## 12. Success metrics

Release readinessâ€”not user surveillanceâ€”shall be measured with offline test evidence:

- 100% byte-identical restoration across the supported corpus.
- No unexpected source modification in fault-injection tests; optional source cleanup is only tested when explicitly enabled and must occur after all required verification checks.
- Zero unverified final filenames after cancellation/crash tests.
- 100% typed terminal outcomes in test runs.
- Clean-machine offline archive/restore success.
- Keyboard-complete primary workflows.
- Dependency license and hash completeness.
- Qualified compression evidence showing observed 30-50% archive sizes on suitable WAV collections without claiming universal savings.

No production telemetry is required or permitted in version 1.

## 13. Release acceptance

Version 1 may be called stable only when:

- All P0/P1 functional requirements are implemented or explicitly re-scoped before release.
- The acceptance checklist has linked evidence.
- An independent reviewer confirms path containment, `.wv` verification, and `.tar.gz` package verification.
- The corpus and clean-machine recovery drill pass.
- The website uses accurate, qualified claims.
- Naming and license reviews are complete.
