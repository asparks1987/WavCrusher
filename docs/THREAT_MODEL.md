# Threat Model

## 1. Scope

WavCrusher is a local Windows desktop application processing user-selected folders and bundled WavPack executables. The primary assets are source WAV integrity, archive integrity, trustworthy evidence, filesystem containment, and recovery capability.

This model addresses accidental misuse and realistic malicious local inputs. It does not claim to defend a compromised administrator/kernel or malicious storage firmware.

## 2. Assets

- Original WAV bytes and names.
- Destination `.wv` bytes.
- Complete-file hashes and manifests.
- Operation journals and reports.
- Bundled toolchain identity.
- User path privacy.
- Ability to recover without WavCrusher.

## 3. Trust boundaries

```text
User input paths / manifests
        â”‚ untrusted
        â–¼
WavCrusher validation and domain model
        â”‚ controlled arguments
        â–¼
Pinned external wavpack/wvunpack processes
        â”‚ filesystem IO
        â–¼
Source, destination, and temp storage
```

Manifests are untrusted even when produced by WavCrusher because they may be edited or obtained elsewhere. Filesystem topology may change after validation.

## 4. Threats and mitigations

### Source deletion or modification

**Threat:** Bug, unsafe flag, or convenience feature changes originals.  
**Mitigations:** Read-only opens, no destructive API path, allowlisted WavPack profile, no delete UI, source snapshots, immutability tests, independent review.

### Root recursion/overlap

**Threat:** Destination inside source is discovered and recursively reprocessed; source inside destination causes ambiguity.  
**Mitigations:** Canonical equality/nesting rejection, reparse inspection, reserved metadata namespace.

### Path traversal

**Threat:** Malicious manifest path writes outside restore root.  
**Mitigations:** Validated relative-path type, reject rooted/traversal/device/ADS forms, recombine and prove containment immediately before write.

### Junction/symlink substitution

**Threat:** A directory is replaced with a reparse point after planning, redirecting writes.  
**Mitigations:** Skip reparse recursion, recheck relevant destination parents near creation/publication, fail closed on unexpected reparse points, same-directory temp and atomic rename.

### Command injection

**Threat:** A filename containing shell syntax changes the command.  
**Mitigations:** No shell, absolute executable path, `ArgumentList`, tests with metacharacters.

### Executable substitution

**Threat:** Attacker replaces WavPack sidecar.  
**Mitigations:** Bundled fixed location, SHA-256 and version validation, signed package where available, no PATH discovery, no runtime download.

### Lossy mode accident

**Threat:** A setting/argument enables hybrid/lossy encoding.  
**Mitigations:** One immutable profile, allowlisted argument snapshot, block dangerous options, whole-file round-trip hash (which would expose loss), integration tests.

### Wrapper loss

**Threat:** Decoder is forced to generate a new WAV header; audio matches but file bytes differ.  
**Mitigations:** No `--wav`/raw/normalization/conversion options, full-file SHA-256, metadata-rich fixtures.

### Partial archive mistaken for complete

**Threat:** Crash leaves an output with the final name.  
**Mitigations:** Encode to unique partial name, verify before atomic publication, terminal journal evidence, no existence-based success.

### Existing file overwrite

**Threat:** Planning race or user choice overwrites an unrelated archive.  
**Mitigations:** Preflight conflict detection, WavPack no-overwrite, atomic no-replace final move, no overwrite setting.

### Source changes mid-operation

**Threat:** Recorder/editor modifies the input, creating mismatched evidence.  
**Mitigations:** Before/after metadata observations, complete-file source hash, post-encode observation, full round trip, typed `SourceChanged`.

### Storage/RAM corruption

**Threat:** Hardware error changes archive bytes.  
**Mitigations:** Encoder verify reread, full decode comparison, archive SHA-256, periodic audits, redundant copies. Not fully preventable by software.

### Journal/report falsity

**Threat:** Crash or logic error records success without proof.  
**Mitigations:** Central success predicate, immutable evidence record, terminal flush, schema validation, fault injection, summary derived from item evidence.

### Malicious/huge manifest

**Threat:** Excessive memory/CPU, duplicate fields, invalid paths.  
**Mitigations:** Input size/depth/count limits, strict parser, duplicate critical property rejection, streaming where appropriate, path revalidation.

### Privacy leakage

**Threat:** Absolute paths/user names appear in shared reports or telemetry.  
**Mitigations:** No telemetry/network, portable relative manifest, redacted report mode, bounded support bundles.

### Denial of service

**Threat:** Huge tree, pathological WAV, hanging tool, or low disk stalls app.  
**Mitigations:** Streaming scan, bounded workers/output, cancellation, optional reviewed timeout, space checks, responsive UI. Maximum compression is inherently slow and communicated honestly.

## 5. Abuse cases

- Selecting the drive root as source and a child folder as destination: rejected by overlap validation.
- Naming a WAV `& del something.wav`: passed as a literal argument, no shell.
- Editing a manifest path to `../../Windows/...`: rejected before restore plan.
- Replacing `wvunpack.exe`: hash mismatch, operation blocked.
- Copying a random file to an expected final `.wv`: no manifest evidence; audit/reconcile required.
- Killing power after final move but before journal flush: orphan final archive identified and independently reverified; not automatically successful.

## 6. Residual risks

- A malicious administrator can alter app/tools/storage and potentially evidence.
- Hashes detect changes but do not repair them; redundant healthy copies are needed.
- SHA-256 is not a digital signature; manifests can be edited unless future signing is added.
- Filesystem metadata outside content is not fully preserved.
- Future OS/tool compatibility requires maintained decoders and migration drills.
- Unknown WavPack or application defects may exist; open formats, independent decoding, and corpus testing reduce but do not eliminate risk.

## 7. Security review triggers

A fresh review/ADR is mandatory before adding:

- Source deletion/replacement.
- Overwrite behavior.
- Symlink/junction following.
- Auto-update or network code.
- Privileged operations.
- User-supplied command arguments.
- New codecs/formats.
- Manifest signatures/encryption.
- Installer shell integration/file associations.
- Cloud or remote storage.
