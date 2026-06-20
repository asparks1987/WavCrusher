# Security and Data-Safety Policy

## Supported versions

No application release exists yet. When releases begin, this table must be updated with supported branches and end dates.

## Sensitive issue classes

Please treat these as private/coordinated reports when a contact channel exists:

- Any path that modifies/deletes source files.
- Path traversal or reparse-point containment escape.
- Shell/argument injection.
- Bundled-tool hash/version bypass.
- Unverified or lossy data reported as Verified.
- Existing destination overwrite.
- Malicious manifest leading to out-of-root write or resource exhaustion.
- Release/update supply-chain compromise.

## Reporting

Before public launch, replace this section with a monitored security contact or repository private-vulnerability-reporting procedure. Do not invent an email address.

A useful report includes:

- Affected version/commit and OS.
- Minimal reproducible folder/manifest/fixture.
- Exact expected vs actual behavior.
- Whether source or archive bytes changed.
- Logs with sensitive paths redacted.
- Proposed severity and disclosure constraints.

Do not send private or copyrighted recordings when a generated fixture can reproduce the issue.

## Response goals

The project should acknowledge promptly, preserve evidence, reproduce safely, assess archival impact, and coordinate a fix/advisory. Exact timelines will be published once maintainers and release operations exist.

## Scope notes

WavCrusher protects against application-level accidents and hostile local inputs within its threat model. It cannot protect data from a fully compromised administrator/kernel, malicious firmware, physical loss, or lack of redundant copies.
