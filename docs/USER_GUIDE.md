# User Guide

> This guide describes WavCrusher v1.0.21a alpha and the version 1 safety model.

## 1. What WavCrusher does

WavCrusher stores WAV files more compactly as lossless WavPack `.wv` files. It keeps the folder layout, restores every candidate archive to temporary storage, and checks that the restored WAV is byte-for-byte identical before calling the archive verified.

Recent real-world WavCrusher v1.0.21a alpha runs are consistently producing archives around **30-50% of the original WAV size** on suitable collections. Your exact result depends on the audio, but the app shows original bytes, archive bytes, and per-file compression ratio so you can see the actual savings.

A successful archive can be recovered later with WavCrusher or the standard `wvunpack` program.

## 2. What you still need for preservation

WavCrusher is an archive-creation and verification tool, not a complete backup strategy. Keep:

- At least two verified archive copies on independent storage.
- At least one copy in another location.
- The manifest, journal, reports, checksums, WavPack decoder, license, and documentation with each archive set.
- A schedule for periodic audits and storage replacement.

Make source-retention decisions only after verified `.wv` creation, verified `.tar.gz` packaging, multiple copies, and a recovery drill. The opt-in cleanup checkbox can delete source files after each file and the package both pass verification.

## 3. Before you begin

Prepare:

1. A source folder containing WAV files.
2. A separate destination folder with enough free space.
3. Time for maximum compression and full verification; this profile intentionally favors size over speed.
4. Stable power and storage where possible.

The destination must not be the source, inside the source, or a parent of the source. This avoids recursion and accidental mixing.

## 4. The Archive workspace

### Step 1 â€” Choose folders

- **Source folder:** WavCrusher reads `.wav` files recursively.
- **Archive folder:** WavCrusher creates mirrored `.wv` files and a `.wavcrusher` evidence folder.

Example:

```text
D:\Audio Masters\Project A\Mix.wav
E:\WAV Archive\Project A\Mix.wv
```

WavCrusher does not follow symbolic links, junctions, or other directory reparse points. Skipped locations appear as warnings.

### Step 2 â€” Scan

Select **Scan folder**. The app shows:

- WAV count and total source size.
- Planned output paths.
- Existing-output conflicts.
- Unreadable/unsupported files.
- Reparse-point skips.
- Destination free space.
- An estimated archive-size range. Recent observed archive sizes are often 30-50% of source bytes, but the estimate remains content-dependent.

The estimate is not a promise. Silence and structured audio may compress greatly; high-resolution noise may compress little.

### Step 3 â€” Review

Resolve destination conflicts before starting. Version 1 never silently overwrites an existing archive. A conflict is not considered a success.

Confirm that:

- The roots are correct.
- The output mapping looks right.
- Important warnings are understood.
- The source is not being actively recorded to or edited.

### Step 4 â€” Start verified archive

Select **Start verified archive**. For each WAV, the app:

1. Hashes the complete original file.
2. Encodes a temporary pure-lossless WavPack archive.
3. Requires WavPackâ€™s own verification.
4. Restores the temporary archive to a temporary WAV.
5. Compares the complete restored WAV hash with the original.
6. Hashes the archive.
7. Publishes the final `.wv` only after all checks pass.
8. Records durable evidence.
9. Builds the final `.tar.gz` package.
10. Extracts and compares the packaged payload bytes before reporting package completion.
11. If enabled, removes source files only for items that passed verification and have successfully built a verified package.

### Step 5 â€” Read status correctly

- **Verified:** Complete round-trip proof passed and evidence was recorded.
- **Failed:** A required stage failed; the reason and safe next action are shown.
- **Conflict:** The intended output already exists or two inputs map to the same output.
- **Skipped:** The app intentionally did not process the item and states why.
- **Cancelled:** Work did not finish and is not verified.

â€œEncodedâ€ is an intermediate stage, not success.

### Pause and cancel

- **Pause:** Stops starting new files. Active files normally finish their current pipeline safely.
- **Cancel:** Stops scheduling, terminates active WavPack processes safely, records cancellation, and removes operation-owned temporary files according to policy.

Cancellation preserves a coherent operation record that can be inspected/resumed.

## 5. Completion and reports

The completion view shows separate counts for verified, failed, conflict, skipped, and cancelled items. Export or retain:

- Manifest JSON.
- Operation journal JSONL.
- JSON report.
- Human-readable HTML report.

Do not rely only on a screenshot or the green status icon. The manifest and archive hashes are the durable evidence.

## 6. Resume an interrupted operation

When WavCrusher finds an incomplete operation under the destinationâ€™s `.wavcrusher` folder, it offers an inspection.

Resume performs fresh validation. It does not assume that an existing `.wv` is valid. Completed items may be recognized only when evidence and current hashes agree. Incomplete temporary files are classified and never promoted solely because they look large or complete.

A changed source is re-planned or failed according to the current operation rather than silently associated with old evidence.

## 7. The Audit workspace

Audits help detect storage changes over time.

### Archive hash audit

Recalculates SHA-256 of each `.wv` and compares it with the manifest. This detects changed bytes but does not itself decode the file.

### Decoder integrity audit

Uses `wvunpack` to test decoder-level integrity. It is deeper than a simple existence check, but does not compare with the original WAV unless a restore comparison is selected.

### Whole-file source comparison

Where original WAVs are available, WavCrusher decodes each archive to scratch storage and compares complete-file SHA-256 against the original. This is the strongest current-source audit.

### Recovery sample

For offline archive copies where originals are not mounted, decode selected or all `.wv` files to scratch storage and compare the restored hash to the original hash recorded in the manifest.

Audit reports clearly state which depth was run. â€œHash matchedâ€ must not be confused with â€œfull recovery drill completed.â€

## 8. The Restore workspace

### Step 1 â€” Select archive set

Choose the manifest or archive root. WavCrusher validates its format and paths.

### Step 2 â€” Select a separate restore folder

Restore does not overwrite existing WAVs. Review conflicts and available space.

### Step 3 â€” Preview

The app reconstructs the original relative paths and shows all planned targets.

### Step 4 â€” Restore and verify

For each item, `wvunpack` restores the stored original WAV wrapper. WavCrusher hashes the restored file and compares it with the original complete-file SHA-256 in the manifest.

Only a matching file is marked **Verified restored**.

## 9. Emergency recovery without WavCrusher

Each `.wv` is a standard file. From a command prompt in the directory containing the bundled tools:

```powershell
wvunpack.exe "E:\WAV Archive\Project A\Mix.wv" "D:\Recovered\Project A\Mix.wav"
```

For exact original wrapper restoration, do not add options that force a fresh WAV header, normalize float audio, output raw PCM, or convert formats.

Then calculate SHA-256 and compare it with the source hash in the manifest:

```powershell
Get-FileHash "D:\Recovered\Project A\Mix.wav" -Algorithm SHA256
```

## 10. Storage planning

During the archive pipeline, temporary space may be needed for:

- The `.wv` being encoded.
- A full restored WAV used for round-trip verification.
- Journal/report snapshots.

A conservative destination/scratch-space rule is at least the total size of the largest active source WAV plus its expected archive, multiplied by the worker count, with safety margin. The appâ€™s preflight should calculate a better bound from the plan.

The final WavPack archive often uses far less space than the WAV. Current observed WavCrusher runs commonly land around 30-50% of original size, but the exact ratio is content-dependent. Do not begin a huge operation with nearly full storage.

## 11. Common problems

### â€œSource and destination overlapâ€

Select entirely separate folder trees. This is a safety rule, not a warning that can be bypassed.

### â€œDestination conflictâ€

A `.wv` already exists at the planned path. Move the old archive aside after investigating, choose another destination, or audit/reconcile it. WavCrusher will not overwrite it.

### â€œSource changed during processingâ€

An application, recorder, sync client, or user modified the WAV. Stop other activity and retry from a fresh scan.

### â€œRound-trip hash mismatchâ€

The decoded complete file did not match the original. The app must not publish it. Preserve the diagnostic report, verify tool hashes, test storage/RAM health, and retry on known-good storage. Treat repeated mismatches seriously.

### â€œUnsupported WAVâ€

The extension is `.wav`, but the internal codec/layout is not supported by the pinned WavPack toolchain. Keep the original and consult the final support matrix. Do not convert it merely to make the warning disappear unless that conversion is a separately documented preservation action.

### â€œNot enough temporary spaceâ€

Choose a destination/scratch location with enough room for the restored WAV and archive, reduce worker count, or archive smaller batches.

### â€œTool hash mismatchâ€

The bundled WavPack executable differs from approved dependency metadata. Reinstall from a trusted WavCrusher release; do not bypass the check.

## 12. Recommended preservation routine

1. Archive and require all desired items to be Verified.
2. Copy the complete archive set to two or more independent destinations.
3. Verify checksums after each copy.
4. Perform a sample restore with plain `wvunpack`.
5. Store a copy off-site.
6. Audit periodically and after any media error or migration.
7. Keep old media until the replacement copy and recovery drill pass.
8. Keep the decoder, license, source release, and manifest documentation with the collection.
