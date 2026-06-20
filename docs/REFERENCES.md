# Primary References

This project should prefer current primary sources and exact pinned-release documentation. Verify all dependency details again during implementation and release.

## WavPack

- WavPack official site: <https://www.wavpack.com/>
- WavPack 5.9.0 user documentation: <https://www.wavpack.com/wavpack_doc.html>
- Official source repository and releases: <https://github.com/dbry/WavPack>
- WavPack release downloads: <https://www.wavpack.com/downloads.html>
- Source documentation files, including file format and library guides: <https://github.com/dbry/WavPack/tree/master/doc>

Relevant upstream behavior to confirm against the pinned binary:

- Default mode is pure lossless unless hybrid/lossy options are supplied.
- The `.wv` file can retain original extension/header/trailer information for restoration.
- `-hh` selects the highest compression mode.
- `-x6` selects the maximum extra analysis level and is extremely slow.
- `-m` stores an MD5 of raw uncompressed audio.
- `-v` performs an output verification pass after write.
- `--no-overwrite` avoids replacing existing files.
- Decoder options that force WAV/raw/normalization can discard or regenerate wrapper details and are inappropriate for whole-file proof.

Do not copy command options from old 3.x documentation; use the exact 5.9.0/current manual tied to the shipped release.

## .NET and Windows Forms

- Official .NET support policy: <https://dotnet.microsoft.com/en-us/platform/support/policy>
- Windows Forms overview: <https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/>
- What’s new in Windows Forms for .NET 10: <https://learn.microsoft.com/en-us/dotnet/desktop/winforms/whats-new/net100>
- .NET single-file deployment: <https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview>
- `ProcessStartInfo.ArgumentList`: <https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.argumentlist>
- `File.Move`: <https://learn.microsoft.com/en-us/dotnet/api/system.io.file.move>
- Windows application accessibility: <https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/windows-forms-accessibility-improvements>

At the time this package was prepared, .NET 10 was an active LTS release. The implementation must pin a currently supported patch rather than treating this document as a live version feed.

## Hashing and security

- NIST Secure Hash Standard, FIPS PUB 180-4: <https://csrc.nist.gov/pubs/fips/180-4/upd1/final>
- Microsoft cryptographic services / SHA-256 APIs: <https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256>
- Microsoft path/file APIs: <https://learn.microsoft.com/en-us/dotnet/standard/io/file-path-formats>
- Microsoft reparse-point concepts: <https://learn.microsoft.com/en-us/windows/win32/fileio/reparse-points>

## Digital preservation

- NDSA Levels of Digital Preservation: <https://www.ndsa.org/publications/levels-of-digital-preservation/>
- Digital Preservation Coalition Handbook — fixity and checksums: <https://www.dpconline.org/handbook/technical-solutions-and-tools/fixity-and-checksums>
- Library of Congress Sustainability of Digital Formats: <https://www.loc.gov/preservation/digital/formats/>
- Library of Congress WAVE format description: <https://www.loc.gov/preservation/digital/formats/fdd/fdd000001.shtml>

These sources reinforce that format conversion/compression is only one part of preservation. Multiple copies, fixity monitoring, documentation, recovery testing, and migration remain necessary.

## Web accessibility

- WCAG 2.2: <https://www.w3.org/TR/WCAG22/>
- WAI-ARIA Authoring Practices: <https://www.w3.org/WAI/ARIA/apg/>
- `prefers-reduced-motion`: <https://developer.mozilla.org/en-US/docs/Web/CSS/@media/prefers-reduced-motion>

## Licensing

- WavPack repository license files: <https://github.com/dbry/WavPack>
- MIT License text: <https://opensource.org/license/mit>

Bundled third-party notices must be copied from the exact distributed artifacts/source release, not reconstructed from this reference list.
