# Release and Packaging Plan

## 1. Goals

A WavCrusher release must be:

- Usable offline on a clean supported Windows x64 machine.
- Independently recoverable with visible standard WavPack tools.
- Traceable to source, SDK, and dependency versions.
- Verifiable with release checksums.
- Complete with licenses and local documentation.
- Conservative about auto-update and installation side effects.

## 2. Release form

The v1.0.0a build produces a **self-contained Windows x64 MSI installer** plus a published application folder under `artifacts/`. The installer gives normal users a familiar setup flow, desktop shortcut, and themed icon while preserving the recovery principle: WavPack sidecars remain visible in the installed payload.

Current installed layout:

```text
C:\Program Files\WavCrusher\
  WavCrusher.WinForms.exe
  WavCrusher.*.dll
  wavpack.exe
  wvunpack.exe
  third_party/wavpack/
    LICENSE
    VERSION
    dependency.json
    win-x64/
      wavpack.exe
      wvunpack.exe
```

The MSI is generated at:

```text
artifacts/installer/WavCrusher.Setup.<version>.msi
```

A portable ZIP can still be added later as a secondary artifact because it is convenient to preserve beside archive sets.

## 3. Build command

```powershell
.\buildwavcrusher.ps1 -NoRestore
```

The script publishes `win-x64` self-contained, generates the themed application icon, copies WavPack sidecars, bootstraps WiX Toolset locally under `artifacts/tools/`, and builds the MSI.

Do not enable trimming unless the complete WinForms application and serialization paths are tested. Do not enable Native AOT merely for packaging simplicity.

## 4. Versioning

Use semantic versioning for application releases:

```text
MAJOR.MINOR.PATCH[-prerelease]
```

Archive profile and manifest versions are separate identifiers. App version changes do not automatically require a manifest major bump.

The MSI uses Windows Installer's numeric three-part product version only for package metadata. WavCrusher release labels are user-defined, so the installer is configured to replace an existing WavCrusher install regardless of MSI's numeric newer/older comparison.

## 5. WavPack dependency acquisition

For the pinned official release:

1. Retrieve the exact Windows x64 asset from the official project release.
2. Calculate SHA-256 locally.
3. Record version, source, asset filename, size, hashes, reported versions, and license files.
4. Store the complete upstream license unmodified.
5. Require build/startup verification against `dependency.json`.

Never put sample or guessed hashes into a release.

## 6. Build provenance

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

## 7. Signing

Code signing is recommended after project ownership, publisher identity, and certificate protection are established. Signing must not replace checksums or provenance.

Never commit signing secrets. Release workflows should use protected environments and least privilege.

## 8. Update policy

Version 1 should not contain an auto-updater or executable downloader. The About page may provide a plain source/release reference only when the user explicitly opens it.

## 9. Installer policy

The MSI installer is now part of the alpha release tooling. It must continue to satisfy:

- Per-machine install to `C:\Program Files\WavCrusher`.
- Desktop shortcut creation.
- Standard-user app operation after install.
- Uninstall that never touches archive/source/user content.
- Visible bundled WavPack tools and license/provenance metadata.
- No file associations unless explicitly designed later.
- Code signing when publisher identity and certificate custody are ready.

The installer must keep uninstall behavior scoped to installed application files and must not treat archive sets, manifests, reports, or user-created audio as installer-owned content.

## 10. Website and release copy

The dependency-free site under `/docs` can be deployed from the repository. It must:

- Match actual release status.
- Use no trackers, external fonts, or CDNs.
- Sell the observed 30-50% archive-size results for suitable WAV collections while avoiding guaranteed compression promises.
- Link to source, release checksums, docs, and recovery instructions once those exist.
- Keep "Verified means byte-for-byte restored" precise and prominent.

## 11. Archiving the project itself

For long-term recoverability, each stable release should preserve source, build documentation, SDK/runtime version information, NuGet lock metadata, exact WavPack binary/source release and license, manifest schemas/examples, test corpus generators, release evidence, binaries, and checksums.
