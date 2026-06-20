# Release Acceptance Checklist

This checklist is evidence-based. Do not mark an item complete with â€œimplementedâ€ alone; link to tests, CI, review records, screenshots, dependency files, or recovery reports.

## 1. Product and scope

- [ ] Provisional name has completed public naming/trademark/package review.
- [ ] Public claims match actual tested support and do not say â€œsmallest possible,â€ â€œfuture-proof,â€ guaranteed savings, or every WAV.
- [ ] Version/status displayed accurately in app and website.
- [ ] Version 1 contains no source delete/move/replace option.
- [ ] Version 1 contains no lossy/hybrid/free-form codec settings.
- [ ] Local-only/no-telemetry behavior verified.

## 2. Toolchain and supply chain

- [ ] Supported .NET 10 SDK patch pinned in `global.json`.
- [ ] NuGet versions centralized and lock files committed.
- [ ] Official WavPack release source documented.
- [ ] Real release asset and extracted executable SHA-256 values recorded.
- [ ] WavPack reported version matches dependency metadata.
- [ ] Runtime tool hash validation tested, including tamper failure.
- [ ] Exact upstream license/notices included.
- [ ] SBOM generated and reviewed.
- [ ] Release checksums generated after packaging.
- [ ] No placeholder hashes, fake contacts, or unresolved license entries.

## 3. Source protection

- [ ] Source files opened read-only.
- [ ] Code/argument audit finds no destructive source path/API/flag.
- [ ] Before/after source snapshots pass across success, failure, cancel, crash, disk-full, and resume tests.
- [ ] Source changes during processing are detected and block publication.
- [ ] No test or production path writes verification output beside sources.
- [ ] Independent reviewer signs off source-immutability design/code.

## 4. Root and path containment

- [ ] Equal roots rejected.
- [ ] Destination beneath source rejected.
- [ ] Source beneath destination rejected.
- [ ] Case-insensitive prefix traps handled.
- [ ] Directory reparse points skipped/reported.
- [ ] Unexpected output-parent reparse points fail closed.
- [ ] Manifest rooted/traversal/device/ADS paths rejected.
- [ ] Case-colliding outputs detected.
- [ ] Reserved `.wavcrusher` namespace collisions handled.
- [ ] Hostile path suite cannot create outside sandbox.
- [ ] Independent reviewer signs off containment logic.

## 5. WavPack invocation

- [ ] Absolute bundled executable path used; no PATH lookup.
- [ ] `UseShellExecute=false` and no shell/batch/PowerShell invocation.
- [ ] `ArgumentList` used; metacharacter filenames remain literal.
- [ ] Exact pure-lossless encoder argument array snapshot approved.
- [ ] Forbidden options impossible through public interfaces.
- [ ] Decoder does not force fresh wrapper, normalization, raw, or conversion.
- [ ] Stdout/stderr drained asynchronously and bounded.
- [ ] Exit codes and diagnostics mapped to typed results.
- [ ] Cancellation terminates full process tree.
- [ ] Wrong/missing/tampered tools fail closed.

## 6. Archive transaction

- [ ] Encoder writes an operation-owned same-directory temporary `.wv` name.
- [ ] Encoder never targets final archive path directly.
- [ ] Existing final outputs are never overwritten.
- [ ] Destination conflict race tested.
- [ ] Encoder verification requested and required.
- [ ] Decoder round trip occurs in isolated owned temporary space.
- [ ] Complete source SHA-256 calculated.
- [ ] Restored length and complete-file SHA-256 match source.
- [ ] Archive SHA-256 calculated.
- [ ] Final publication occurs only after all verification.
- [ ] Failed/cancelled items leave no unverified final name.
- [ ] Cleanup removes only operation-owned temporary files.
- [ ] Disk capacity checked for active archive plus full restore needs.

## 7. Evidence and recovery

- [ ] Every Verified record has complete mandatory evidence.
- [ ] WavPack audio MD5 is labeled separately from complete-file SHA-256.
- [ ] Terminal journal records append and flush.
- [ ] Truncated final JSONL line recovers valid prefix with warning.
- [ ] Earlier journal corruption is not silently ignored.
- [ ] Manifest/report snapshots are transactional.
- [ ] Manifest can be rebuilt from journal.
- [ ] Unknown major versions rejected; additive/minor behavior tested.
- [ ] Malicious/oversized manifest limits tested.
- [ ] Summary counts derive from item outcomes.
- [ ] Redacted report mode does not misrepresent evidence.

## 8. Cancellation and resume

- [ ] Pause stops new scheduling and is represented accurately.
- [ ] Cancel stops new work and active process trees.
- [ ] Cancellation at every pipeline stage tested.
- [ ] Resume revalidates roots, tools, source/archive state, and paths.
- [ ] `.wv` existence alone never implies success.
- [ ] Stale partial policy is explicit and safe.
- [ ] Published-orphan/no-success-record window reconciles only by full verification.
- [ ] Resume idempotency proven.
- [ ] Completion status never hides incomplete/cancelled work.

## 9. Audit and restore

- [ ] Archive SHA-256 audit implemented.
- [ ] Decoder integrity audit implemented.
- [ ] Full current-source/recovery-sample depth distinguished clearly.
- [ ] Audit is read-only.
- [ ] Restore uses validated separate root and no overwrite.
- [ ] Every restored success matches manifest original complete-file SHA-256.
- [ ] Corrupt/missing/changed archives are classified accurately.
- [ ] Hostile manifest restore cannot escape root.
- [ ] Plain bundled/upstream `wvunpack` independently restores samples.
- [ ] Emergency recovery instructions work as written.

## 10. Corpus and reliability

- [ ] 8/16/24/32-bit integer PCM exact round trips.
- [ ] 32-bit float exact round trips.
- [ ] Mono/stereo/multichannel exact round trips.
- [ ] Common/unusual rates tested.
- [ ] Silence, tones, correlated material, and noise tested.
- [ ] Unknown chunks, odd padding, LIST/INFO, BWF, iXML/axml/cue/smpl, and trailers tested where supported.
- [ ] RF64/large-file strategy tested.
- [ ] Unicode, combining characters, metacharacters, spaces, and long paths tested.
- [ ] Malformed/unsupported/locked/changing files fail safely.
- [ ] Process crash/hang, tool tamper, low disk, permission loss, and journal failure injected.
- [ ] 100,000-file scan/plan soak target met without unbounded memory.
- [ ] Qualified performance/compression report records corpus/hardware/settings; no universal claims.

## 11. Windows Forms UX and accessibility

- [ ] UI remains responsive during scan/hash/encode/decode/reporting.
- [ ] Archive, Audit, and Restore workflows complete by keyboard.
- [ ] Logical tab order and visible focus.
- [ ] Critical controls/status have accessible names and descriptions.
- [ ] Status not conveyed by color alone.
- [ ] High DPI at 100/125/150/200% tested.
- [ ] Windows high contrast tested.
- [ ] Screen-reader smoke test completed.
- [ ] Large collection grid/progress remains usable.
- [ ] Errors name item/stage/effect/safe next action.
- [ ] Completion page exposes failures/conflicts/skips immediately.
- [ ] About shows app/tool versions, hash status, licenses, source, and privacy statement.

## 12. Packaging and deployment

- [ ] Portable win-x64 package built self-contained.
- [ ] `wavpack.exe` and `wvunpack.exe` visible as sidecars.
- [ ] Local docs, schemas, licenses, notices, checksums, and SBOM included.
- [ ] Clean supported Windows VM test passes without developer tools.
- [ ] Offline operation passes and no unexpected network request observed.
- [ ] Standard-user operation passes.
- [ ] Paths with spaces/Unicode pass.
- [ ] Read-only install location passes with proper writable data locations.
- [ ] Removing WavCrusher executable still permits sample recovery with sidecar decoder.
- [ ] Release ZIP checksum/signature (if available) independently verified.

## 13. Website and documentation

- [ ] `/docs` website works without external dependencies or network.
- [ ] Website keyboard/focus/reflow/reduced-motion/accessibility checks pass.
- [ ] Storage estimator labels results as estimates.
- [ ] Website release status is accurate.
- [ ] README, user, developer, safety, manifest, testing, release, privacy, threat, and recovery docs match implementation.
- [ ] References point to current primary sources.
- [ ] Sample manifests/settings validate.
- [ ] Changelog and release notes complete.
- [ ] No fake repository/contact/download links remain.

## 14. Final approvals

- [ ] Product owner approval.
- [ ] Archive-safety independent review.
- [ ] Security/path review.
- [ ] Accessibility review.
- [ ] Dependency/license review.
- [ ] Clean-machine recovery drill witnessed/documented.
- [ ] No open critical/high data-integrity defect.
- [ ] All unchecked items are either completed or version 1 is not released as stable.
