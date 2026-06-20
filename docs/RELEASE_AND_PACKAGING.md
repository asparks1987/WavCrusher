# Release and Packaging Plan

## 1. Goals

A WavCrusher release must be:

- Usable offline on a clean supported Windows x64 machine.
- Independently recoverable with visible standard WavPack tools.
- Traceable to source, SDK, and dependency versions.
- Verifiable with release checksums.
- Complete with licenses and local documentation.
- Conservative about auto-update and installation side effects.

## 2. Planned release form

Start with a **portable ZIP**. It is transparent, easy to checksum, easy to preserve with an archive set, and avoids installer upgrade/uninstall risks before they are designed.

Suggested layout:

```text
WavCrusher-1.0.0-win-x64/
  WavCrusher.exe
  wavpack.exe
  wvunpack.exe
  dependency.json
  LICENSE.txt
  THIRD_PARTY_NOTICES.txt
  WavPack-LICENSE.txt
  README.html
  README.md
  docs/
  schemas/
  SHA256SUMS.txt
  SBOM.spdx.json
```

The app may be a self-contained single-file executable. WavPack binaries must remain visible sidecars so users can recover archives even without the app.

## 3. Versioning

Use semantic versioning for application releases:

```text
MAJOR.MINOR.PATCH[-prerelease]
```

- Major: incompatible manifest/profile or major product behavior changes.
- Minor: backward-compatible features.
- Patch: compatible fixes.

Archive profile and manifest versions are separate identifiers. App version changes do not automatically require a manifest major bump.

## 4. .NET runtime

Target .NET 10 LTS and pin a supported patch SDK in `global.json` at release build time. Keep patch dependencies current within the supported .NET 10 lifecycle and rerun the entire release suite after updates.

Suggested publish command, subject to implementation testing:

```powershell
dotnet publish src/WavCrusher.WinForms/WavCrusher.WinForms.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:ContinuousIntegrationBuild=true
```

Do not enable trimming unless the complete WinForms application and serialization paths are tested and required metadata is preserved. Do not enable Native AOT merely for packaging simplicity; assess compatibility and debugging first.

## 5. WavPack dependency acquisition

For the pinned official release:

1. Retrieve the exact Windows x64 asset from the official project release.
2. Verify upstream release information through at least the official project site/repository.
3. Calculate SHA-256 locally.
4. Record:
   - Version.
   - Original asset filename.
   - Source release reference.
   - Download/retrieval date.
   - Asset size/hash.
   - Extracted `wavpack.exe` and `wvunpack.exe` sizes/hashes.
   - Reported versions.
   - License filenames/hashes.
5. Store the complete upstream license unmodified.
6. Require build/startup verification against `dependency.json`.

Never put sample or guessed hashes into a release. CI must reject placeholders such as `TODO`, `REPLACE_ME`, or repeated dummy hex.

## 6. Dependency metadata example

```json
{
  "name": "WavPack",
  "version": "5.9.0",
  "platform": "win-x64",
  "source": "official-upstream-release",
  "releaseAsset": {
    "fileName": "RECORD_REAL_ASSET_NAME",
    "size": 0,
    "sha256": "RECORD_REAL_SHA256"
  },
  "files": [
    {
      "path": "wavpack.exe",
      "sha256": "RECORD_REAL_SHA256"
    },
    {
      "path": "wvunpack.exe",
      "sha256": "RECORD_REAL_SHA256"
    }
  ],
  "license": "WavPack-LICENSE.txt"
}
```

This example is intentionally invalid release metadata until real values replace placeholders.

## 7. Build provenance

Release CI should record:

- Git commit/tag and clean-tree status.
- Runner OS/image.
- `dotnet --info`.
- NuGet lock-file hashes.
- WavPack dependency metadata/hash verification.
- Build commands.
- Test/corpus results.
- Artifact SHA-256.
- SBOM.

Prefer reproducible/deterministic managed builds, but do not claim byte-for-byte reproducibility until independently demonstrated.

## 8. Signing

Code signing is recommended after project ownership, publisher identity, and certificate protection are established. Signing must not replace checksums or provenance.

Document:

- Publisher name.
- Certificate chain and timestamp policy.
- Key custody and rotation.
- What a valid signature means.

Never commit signing secrets. Release workflows should use protected environments and least privilege.

## 9. Update policy

Version 1 should not contain an auto-updater or executable downloader. The About page may provide a plain source/release reference only when the user explicitly opens it.

Users install updates manually after verifying signatures/checksums. A future updater requires a separate threat model, rollback strategy, signed metadata format, and ADR.

## 10. Portable settings and user data

By default:

- Application binaries and WavPack sidecars are read-only install content.
- Per-user preferences/logs live under an appropriate local application-data directory.
- Archive evidence lives under the selected destination root.
- Temporary restore data uses an operation-owned workspace with explicit capacity checks.

A portable-settings mode may be added only if it does not cause writes into a read-only install directory or mingle secrets (none expected).

## 11. Release pipeline

1. Freeze requirements and release notes.
2. Update dependencies and lock files intentionally.
3. Verify official WavPack artifacts and licenses.
4. Run formatting/static analysis.
5. Build Release.
6. Run all unit/component/integration/end-to-end/corpus tests.
7. Run clean-VM offline tests.
8. Run accessibility/manual UX checklist.
9. Generate package, docs, schemas, SBOM, notices.
10. Generate checksums after all files are final.
11. Sign artifacts where available.
12. Independently verify ZIP/checksums/signature.
13. Perform emergency plain-`wvunpack` restore from the release package.
14. Complete acceptance checklist.
15. Publish immutable release assets and source tag.

## 12. GitHub Pages website

The dependency-free site under `/docs` can be deployed from the repository. It must:

- Match actual release status.
- Use no trackers, external fonts, or CDNs.
- Avoid universal compression promises.
- Link to source, release checksums, docs, and recovery instructions once those exist.
- Retain an honest pre-alpha banner before software release.

The provided `.github/workflows/pages.yml` is a starting template and must be reviewed for repository permissions and branch policy.

## 13. Installer future

An installer may follow the portable release after design covers:

- Per-user vs per-machine install.
- Standard-user operation.
- Upgrade rollback.
- Uninstall that never touches archive/source/user content.
- WavPack sidecar location and license visibility.
- File associations, if any, as opt-in.
- Code signing and reputation.

The installer must never delete `.wv`, `.wav`, manifests, reports, or user settings without explicit, narrowly scoped consent.

## 14. Archiving the project itself

For long-term recoverability, each stable release should preserve:

- Source archive and Git tag.
- Complete build documentation.
- SDK/runtime version information.
- NuGet packages/lock metadata as licensing permits.
- Exact WavPack binary/source release and license.
- Manifest schemas and examples.
- Test corpus generators and release evidence.
- Portable binaries and checksums.

Store these in redundant, independently verified locations.
