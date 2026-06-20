# Documentation Package Contents

This archive is a repo-ready specification package for the planned WavCrusher application. It contains no compiled application and no WavPack executable.

## Root

- `README.md` â€” canonical overview and operating contract.
- `README.me` â€” compatibility pointer requested by the project owner.
- `AGENTS.md` â€” binding instructions for Codex and other coding agents.
- `CODEX_BUILD_PLAN.md` â€” phased implementation plan with gates.
- `LICENSE` â€” MIT license for WavCrusher-authored material.
- `THIRD_PARTY_NOTICES.md` â€” WavPack and dependency handling.
- `CONTRIBUTING.md`, `SECURITY.md`, `SUPPORT.md`, `CODE_OF_CONDUCT.md` â€” governance.
- `ROADMAP.md`, `CHANGELOG.md` â€” planned evolution and package history.
- `SHA256SUMS.txt` â€” package-file checksums, generated last.

## `/docs`

A static, dependency-free marketing website plus the full product, architecture, preservation, testing, privacy, and release documentation. It can be hosted from GitHub Pages as-is.

## `/docs/codex`

Implementation prompts, a dependency-aware task graph, and a release acceptance checklist. These files complementâ€”but do not replaceâ€”the repository-level `AGENTS.md` rules.

## `/samples`

Illustrative JSON files for settings, manifests, and reports. They are examples, not frozen schemas; the normative field definitions are in `docs/MANIFEST_SPEC.md`.

## `/.github`

Issue templates, a pull request template, a starter CI workflow, and a GitHub Pages deployment workflow.
