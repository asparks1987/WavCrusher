# Repository Guidelines

## Project Structure & Module Organization
Source code lives under `src/`:
- `WavCrusher.Domain` for core value objects and rules.
- `WavCrusher.Application` for use cases and ports.
- `WavCrusher.Infrastructure` for filesystem and scanning services.
- `WavCrusher.WavPack` for CLI integration.
- `WavCrusher.WinForms` for the desktop UI.

Tests mirror the source layout under `tests/`. Shared assets, samples, and release metadata live in `docs/`, `samples/`, `third_party/`, and `tools/`.

## Build, Test, and Development Commands
- `dotnet restore WavCrusher.sln` restores all projects and lock files.
- `dotnet build WavCrusher.sln --no-restore` builds the full solution.
- `dotnet test WavCrusher.sln --no-build` runs the test suite.
- `.\buildwavcrusher.ps1` publishes the WinForms app and packages release artifacts.

## Coding Style & Naming Conventions
Use modern C# with nullable reference types enabled. Keep indentation to 4 spaces. Use PascalCase for types, public members, and files that define them; use camelCase for locals and private fields. Prefer immutable records for data transfer objects and pass `CancellationToken` through async flows. Keep JSON field names stable and lowercase where the schema requires it.

## Testing Guidelines
Use xUnit for unit and integration tests. Name tests with the pattern `MethodName_ExpectedBehavior`. Add or update tests for any behavior change, especially archive safety, path validation, restore behavior, and WavPack command-line integration. Prefer the lowest useful test layer, but include integration coverage for archive/restore paths.

## Commit & Pull Request Guidelines
Use short, imperative commit messages with a scope when helpful, such as `feat: add archive progress details` or `fix: resolve manifest lookup`. Pull requests should summarize the change, note any safety impact, and list commands run. Include screenshots for UI changes and call out any tests not run.

Every completed task must automatically end with a git commit unless the user explicitly says not to commit. The commit must include a full summary of the work performed, including changed behavior, documentation updates, safety impact, and any validation or tests that were run or intentionally not run. Commit only the files changed for the completed task; do not stage unrelated workspace changes.

## Security & Configuration Tips
Keep source and destination roots separate, validate paths before use, and treat bundled WavPack tools as pinned dependencies under `third_party/wavpack/`. A successful archival workflow must verify each `.wv` by byte-for-byte restore and must verify the final `.tar.gz` package by extracting it and comparing packaged payload bytes before reporting completion.
