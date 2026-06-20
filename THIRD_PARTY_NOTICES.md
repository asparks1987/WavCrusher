# Third-Party Notices

## WavPack

WavCrusher is designed to distribute and invoke the WavPack command-line tools as separate sidecar executables.

- Project: WavPack
- Upstream repository: <https://github.com/dbry/WavPack>
- Official site: <https://www.wavpack.com/>
- Planned version for initial implementation: 5.9.0
- Upstream license: BSD-style / BSD-3-Clause files supplied by the WavPack project

This documentation package does **not** include WavPack binaries or reproduce the exact upstream license file. A real application release must include the unmodified license/notices taken from the exact WavPack source/binary distribution and record real SHA-256 hashes in dependency metadata.

WavCrusher must not imply that WavPackâ€™s authors endorse WavCrusher.

## .NET

The planned application targets Microsoft .NET. .NET runtime and libraries have their own licenses and notices. Self-contained publication must preserve any notices required by the exact runtime and NuGet packages shipped.

## NuGet and other dependencies

Before each release:

1. Generate an inventory/SBOM from the lock files and publish output.
2. Review every direct/transitive package license.
3. Include required notices/source offers.
4. Remove packages without a documented need or compatible redistribution terms.
5. Do not copy placeholder license text into release artifacts.

## Website assets

The provided static website uses only repository-authored HTML, CSS, JavaScript, and SVG. It intentionally has no remote fonts, icon libraries, analytics scripts, or CDN assets.
