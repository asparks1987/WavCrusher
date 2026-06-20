# Contributing

Thanks for helping build a safer open lossless WAV archive tool.

## Before starting

Read:

1. `README.md`
2. `AGENTS.md`
3. `docs/ARCHIVE_SAFETY_SPEC.md`
4. Relevant product requirements and ADRs

Archive safety rules outrank convenience and compression benchmarks.

## Find or propose work

Use an issue describing:

- User problem.
- Requirement IDs affected.
- Preservation/safety impact.
- Proposed behavior and non-goals.
- Test strategy.

Changes to source deletion, overwrite, path/reparse handling, archive profile, manifest compatibility, toolchain, network behavior, or recovery guarantees require an ADR before implementation.

## Development workflow

1. Create a focused branch.
2. Add/update tests with the behavior.
3. Keep layer dependencies valid.
4. Run formatting, build, and applicable tests.
5. Update docs/examples/changelog.
6. Open a small reviewable pull request using the template.

Suggested commands once implementation exists:

```powershell
dotnet restore --locked-mode
dotnet format WavCrusher.sln --verify-no-changes
dotnet build WavCrusher.sln -c Release --no-restore
dotnet test WavCrusher.sln -c Release --no-build
```

Archive pipeline changes must run real pinned WavPack integration/corpus tests.

## Commit and PR guidance

- Explain why, not only what.
- Reference requirement/issue IDs.
- Do not mix large formatting rewrites with behavior changes.
- Do not check in copyrighted recordings, personal data, untrusted binaries, or placeholder dependency hashes.
- State tests not run and why.
- Include accessibility evidence for UI changes.
- Include license/SBOM impact for dependencies.

## Reporting defects

Data-integrity, path-escape, source-modification, dependency-substitution, or misleading-success defects may be security/safety sensitive. Follow `SECURITY.md` rather than publishing exploitable details immediately.

## Documentation and website

Keep claims qualified and test-backed. Do not advertise â€œsmallest possible,â€ â€œfuture-proof,â€ â€œsupports every WAV,â€ or guaranteed savings. Preserve the websiteâ€™s local-only/no-tracker design.

## Code of conduct

Participation is governed by `CODE_OF_CONDUCT.md`.
