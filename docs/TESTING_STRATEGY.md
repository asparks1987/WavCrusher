# Testing Strategy

## 1. Purpose

WavCrusher makes a preservation claim. Tests must prove not only that code runs, but that supported WAV files survive the complete pipeline byte-for-byte and that failures cannot publish misleading output.

The strategy combines pure unit tests, Windows filesystem tests, controlled child-process tests, real WavPack integration, generated media corpus tests, fault injection, UI checks, packaging drills, and periodic soak runs.

## 2. Quality gates

| Gate | Required evidence |
|---|---|
| Pull request | Formatting, build, unit tests, affected component tests, documentation review. |
| Archive-engine change | Real WavPack integration on relevant fixtures plus cancellation/fault tests. |
| Release candidate | Full corpus, end-to-end archive/audit/restore, clean VM, accessibility, supply-chain checks. |
| Stable release | Independent recovery drill and complete acceptance checklist. |

No mocked test can substitute for the release corpus round trip.

## 3. Test layers

### 3.1 Domain unit tests

Fast, deterministic, cross-platform where possible:

- Root equality/nesting logic.
- Relative path validation.
- Output mapping.
- Status/stage transitions.
- Success predicate requires every evidence element.
- Byte/ratio calculations and overflow boundaries.
- Failure-code serialization.

### 3.2 Application unit tests

Use controlled ports/fakes:

- Planner diagnostics and collision handling.
- Coordinator bounded scheduling.
- Pause and cancellation semantics.
- Resume decision table.
- Audit-depth classification.
- Restore conflict handling.
- Presenter screen states and actionable messages.

Fakes must model failures explicitly; avoid overly permissive mocks that always succeed.

### 3.3 Infrastructure component tests

Run on Windows temporary folders:

- Recursive scanning and access errors.
- Reparse-point skipping and loop prevention.
- Windows case collisions.
- Long paths and Unicode names.
- Streaming SHA-256.
- Source-change detection.
- Same-directory atomic move and no-overwrite races.
- Journal append/flush/truncated-tail recovery.
- Transactional manifest replacement.
- Disk free-space and temp-workspace behavior.

### 3.4 Process adapter tests

Use two approaches:

1. A purpose-built fake executable that emits controllable stdout/stderr, sleeps, spawns a child, exits with chosen codes, and writes/does not write output.
2. Real pinned WavPack tools.

Prove:

- Arguments stay discrete even with quotes and shell metacharacters in paths.
- No shell is involved.
- Both output streams are drained without deadlock.
- Retained diagnostics are bounded.
- Cancellation kills descendants.
- Tool hash/version mismatch fails before execution.
- Progress parsing failure remains advisory.

### 3.5 Real WavPack integration

For each valid corpus fixture:

1. Compute source length/SHA-256.
2. Encode using the exact product profile to a temporary `.wv` name.
3. Require successful `-v` verification.
4. Decode with the product decoder arguments.
5. Compare restored length/SHA-256.
6. Hash archive.
7. Confirm no source metadata/content was intentionally modified.

For invalid fixtures, assert the expected typed failure and absence of a final archive.

### 3.6 End-to-end tests

Test complete services and real filesystem/toolchain:

- New archive set.
- Mixed valid/invalid collection.
- Existing output conflicts.
- Cancel and resume.
- Interrupted journal recovery.
- Audit at each depth.
- Restore to a new root.
- Emergency plain-`wvunpack` recovery.
- Report generation/redaction.

WinForms UI automation may cover a small smoke path, while presenters hold most behavior tests.

## 4. WAV corpus matrix

### 4.1 Sample representations

- 8-bit unsigned PCM.
- 16-bit signed PCM.
- 24-bit signed PCM.
- 32-bit signed PCM.
- 32-bit IEEE float, normalized.
- Float values near zero, full scale, denormals where valid, peaks above Â±1 when supported.

### 4.2 Channels and rates

- Mono, stereo, 3-channel, 5.1, and a higher multichannel fixture.
- Common rates: 8 kHz, 44.1 kHz, 48 kHz, 96 kHz, 192 kHz.
- Unusual valid integer rates supported by WavPack.

### 4.3 Signal content

- Digital silence.
- Single impulse.
- DC and ramps.
- Sine/square/chirp signals.
- Deterministic speech/music-like synthetic mixtures.
- Correlated stereo.
- Independent-channel noise.
- Cryptographically generated/incompressible-looking deterministic noise.
- Very short and zero-audio-data files where valid.

### 4.4 Container structures

- Basic RIFF/WAVE.
- `WAVEFORMATEX` and `WAVEFORMATEXTENSIBLE`.
- Unknown chunks before `fmt `, between `fmt ` and `data`, and after `data`.
- Odd-sized chunks with padding bytes.
- `LIST/INFO` metadata.
- BWF `bext` chunk.
- `iXML`, `axml`, `cue `, `smpl`, and synthetic application chunks.
- Trailing bytes/ID3 where supported.
- Noncanonical but valid chunk order.
- RF64 with `ds64`.
- Files larger than 4 GiB through generated/sparse test infrastructure, not necessarily committed.

### 4.5 Filesystem names

- Spaces and parentheses.
- Apostrophes and quotes where allowed.
- `&`, `^`, `%`, `!`, semicolon, brackets.
- Unicode from multiple scripts.
- Combining and precomposed Unicode.
- Emoji where supported.
- Very long segments and total paths.
- Names differing only by case to test collision detection.
- Multiple `.wav` suffix patterns such as `mix.final.WAV`.

### 4.6 Invalid and unsupported inputs

- Empty file.
- Random bytes named `.wav`.
- Truncated RIFF header.
- Declared length greater/less than actual.
- Missing `fmt ` or `data`.
- Unsupported compressed WAV codecs such as ADPCM.
- Corrupt chunk sizes.
- Read-locked/inaccessible file.
- File deleted or replaced mid-scan/process.
- File appended while hashing or encoding.

## 5. Fault-injection matrix

Inject a failure before and after each side effect:

| Stage | Faults | Required result |
|---|---|---|
| Root validation | inaccessible root, overlap, reparse substitution | Stop before scan/write. |
| Directory creation | access denied, path becomes file | Typed failure; no out-of-root writes. |
| Source hash | read error, cancellation, source changes | No encoder start/final output. |
| Encoder launch | missing/tampered tool, process start error | Fail closed. |
| Encoding | nonzero exit, crash, hang, disk full, cancellation | No final `.wv`; partial classified/cleaned. |
| Decoder launch | missing/tampered decoder | No publication. |
| Round trip | decode error, restored disk full, hash mismatch | No publication. |
| Archive hash | read error/cancellation | No publication. |
| Publication | conflict race, permission loss, cross-volume surprise | No overwrite; typed failure. |
| Success journal | disk full/crash after move | Orphan reconciliation path; no false completed operation. |
| Manifest snapshot | write/replace failure | Journal remains authoritative. |

Use an injectable filesystem/process/persistence boundary for deterministic faults and verify selected scenarios on a real Windows filesystem.

## 6. Source immutability tests

For every end-to-end scenario:

- Snapshot source bytes/hashes before.
- Snapshot source directory/file identities, names, sizes, and relevant timestamps/attributes.
- Execute success/failure/cancel paths.
- Confirm content, names, count, and locations unchanged.
- Investigate timestamp changes caused merely by reads on unusual filesystems; do not make unsupported claims.

A release-blocking test scans source code and WavPack argument snapshots for destructive flags/APIs.

## 7. Path security tests

Test:

- `..` and rooted paths from hostile manifests.
- Drive-relative forms (`C:foo`).
- UNC/device paths when outside supported policy.
- Alternate data stream syntax.
- Trailing spaces/dots normalization.
- Reserved device names.
- Destination parent replaced with junction between validation and write.
- Reparse-point loops.
- Case-insensitive prefix traps (`C:\Audio` vs `C:\Audio2`).
- Symlink/junction in restore tree.

No test may create content outside its disposable sandbox.

## 8. Resume tests

Create operation states at every pipeline stage and restart:

- Journal only.
- Truncated final journal line.
- Temporary archive exists.
- Temporary restored WAV exists.
- Final archive published but no success journal.
- Success journal exists but snapshot absent.
- Manifest exists but archive missing/changed.
- Source changed since interruption.
- Tool version/hash changed.

Resume must be deterministic, idempotent, and conservative.

## 9. Manifest compatibility tests

- Round-trip v1 manifest.
- Unknown additive fields.
- Unknown minor version.
- Unknown major version rejection.
- Duplicate critical property rejection.
- Invalid hash lengths/characters.
- Numeric overflow.
- Excessive nesting/size limits.
- Malicious paths.
- Placeholder dependency hash rejection in release mode.
- Summary mismatches detected/recomputed.

Golden examples should be reviewed rather than automatically rewritten without diff inspection.

## 10. Performance and soak tests

Measure, do not guess:

- Scan 100,000+ tiny files.
- Archive multi-hour synthetic files.
- Compare worker counts on HDD, SATA SSD, and NVMe where available.
- Track peak working set and handle count.
- Confirm logs/journals remain bounded/reasonable.
- Confirm UI update rate remains responsive.
- Run cancellation after hours of work and resume.

Report corpus, hardware, settings, WavPack/tool/app versions, and raw results. Marketing may quote only qualified, reproducible measurements.

## 11. UI and accessibility tests

Automated presenter tests plus manual Windows checks:

- Complete primary workflows by keyboard.
- Visible focus and logical tab order.
- Screen-reader names/roles/status announcements.
- 100%, 125%, 150%, 200% scaling.
- High contrast themes.
- Narrow/minimum window and long localized strings.
- No status by color alone.
- Cancel/pause controls enabled only in valid states.
- Completion summary exposes issues immediately.

## 12. Packaging tests

On clean supported VMs:

- Run without .NET preinstalled (self-contained package).
- Run offline.
- Verify dependency hashes and About information.
- Archive/audit/restore sample tree.
- Remove/rename WavCrusher app and restore with bundled `wvunpack` alone.
- Validate licenses, SBOM, checksums, and no unexpected network connections.
- Run as standard user from paths containing spaces/Unicode.
- Test read-only install location with writable user-data/archive locations.

## 13. Test evidence retention

Release evidence should include:

- CI run IDs and commit/tag.
- SDK/tool/dependency versions and hashes.
- Corpus generator version and fixture hashes.
- Machine/OS details for clean-VM and performance tests.
- Test reports and acceptance checklist links.
- Known limitations.

Do not retain copyrighted source audio or sensitive user paths in public evidence.
