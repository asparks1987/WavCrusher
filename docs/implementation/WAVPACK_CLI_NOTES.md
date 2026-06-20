# WavPack CLI Notes

## Dependency source

- Upstream project: `https://github.com/dbry/WavPack`
- Release tag: `5.9.0`
- Windows x64 asset: `wavpack-5.9.0-x64.zip`
- Download URL: `https://github.com/dbry/WavPack/releases/download/5.9.0/wavpack-5.9.0-x64.zip`

The asset was downloaded from the official upstream GitHub release and extracted locally. Runtime sidecars copied into `third_party/wavpack/win-x64/`:

- `wavpack.exe`
- `wvunpack.exe`

The upstream license was copied to `third_party/wavpack/LICENSE`.

## Recorded versions

`wavpack.exe --version`:

```text
wavpack 5.9.0
libwavpack 5.9.0
```

`wvunpack.exe --version`:

```text
wvunpack 5.9.0
libwavpack 5.9.0
```

## Intended argument arrays

Encoder profile under test:

```text
-hh
-x6
-m
-v
-t
-z0
--no-overwrite
<source.wav>
<temporary-output.partial.wv>
```

Decoder round-trip restore:

```text
<temporary-output.partial.wv>
-o
<isolated-restored.wav>
```

No shell, command string, PATH discovery, `--wav`, raw output, lossy/hybrid, overwrite, or delete-source options are permitted.

## Open research items

## Initial research result

The `tools/WavPackCliResearch/WavCrusher.WavPackResearch` harness generated a one-second 16-bit PCM WAV with spaces and `&` in its filename, invoked both tools through `ProcessStartInfo.ArgumentList`, decoded the archive, and confirmed:

- Source SHA-256 before and after processing matched.
- Restored WAV SHA-256 matched the source SHA-256.
- Encoder and decoder exit codes were `0`.
- Encoder diagnostics were written to stderr and included WavPack's "created (and verified)" text.

Observed source/restored SHA-256:

```text
8033c9c459b80d3616131baaf9dd0a698a98cf3d307f013188093586c4f2812e
```

When the requested output path was `*.wv.research.partial`, WavPack created `*.wv.research.partial.wv`. Therefore the archive pipeline should use operation-owned temporary names that already end in `.wv`, such as:

```text
Track.<operation-id>.partial.wv
```

This preserves the "not final archive name" rule while avoiding implicit extension rewriting by the tool.

## Open research items

- Confirm no-overwrite behavior and exit codes with existing output files.
- Confirm cancellation behavior and process-tree termination from the .NET process adapter.
- Record console encoding/progress behavior from both tools.
