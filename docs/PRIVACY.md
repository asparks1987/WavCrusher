# Privacy

## Local-only promise

WavCrusher version 1 is designed to work entirely on the userâ€™s Windows computer. It does not require an account, internet connection, cloud service, analytics SDK, advertising system, or telemetry endpoint.

## Data the app reads

Only after user selection, the app may read:

- Paths and directory structure under selected source/archive/restore roots.
- WAV and WavPack file bytes needed for hashing, encoding, auditing, and restoring.
- File sizes, timestamps, and common attributes.
- Local disk capacity.
- Bundled WavPack executable bytes/version for identity verification.
- Local settings and prior operation evidence.

## Data the app writes

- `.wv` archives under the selected destination.
- `.wavcrusher` manifests, journals, reports, and operation metadata under the destination.
- Restored WAVs under the selected restore root.
- Operation-owned temporary files.
- Per-user settings and bounded diagnostic logs in a documented local app-data folder.

The app never writes to source WAVs by design.

## Network behavior

Version 1 must perform no automatic network requests. The website is static and uses no analytics, remote fonts, CDNs, or third-party scripts.

An About/help link may open a public project page only after the user explicitly activates it. The app itself should not prefetch update information.

## Sensitive paths

File paths can reveal names, clients, projects, or research subjects. The canonical manifest favors relative paths. Reports intended for support/sharing should offer redaction of absolute roots and local user names.

Users should inspect reports before sharing them.

## Logs

Logs are for local diagnostics and are not archival truth. They should:

- Be bounded/rotated.
- Avoid dumping environment variables.
- Avoid unbounded external-tool output.
- Support a redacted support bundle.
- Be removable through documented local cleanup without affecting archive evidence.

## Future changes

Telemetry, crash upload, cloud backup, auto-update, or remote storage would change the privacy model and require explicit opt-in design, a threat review, clear documentation, and an ADR. They are not implied by this plan.
