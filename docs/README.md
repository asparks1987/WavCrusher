# WavCrusher `/docs` Site

This directory is both the dependency-free static marketing site and the main technical-documentation collection for WavCrusher v1.0.0a.

WavCrusher is a local Windows tool for turning large WAV libraries into verified pure-lossless WavPack archives. It is aimed at musicians, collectors, studios, field recordists, archivists, and anyone with a lossless music collection that is too large to keep as raw WAV forever but too valuable to compress carelessly.

## Preview locally

From the repository root:

```powershell
python -m http.server 8080 --directory docs
```

Then open `http://localhost:8080/`. A local server is preferable to `file://` when checking manifest and browser behavior.

## Deploy

The site can be published from `/docs` with GitHub Pages or the included Pages workflow. Before public deployment:

- Confirm the displayed release status matches the current build.
- Add real repository/release links; do not invent them.
- Run HTML/CSS/JavaScript and accessibility checks.
- Confirm every relative documentation link resolves.
- Keep the site free of trackers, remote fonts, CDNs, and third-party scripts.
- Keep compression figures qualified as estimates.
- Keep source-retention language clear: WavCrusher verifies archives first; users remain responsible for backups and may enable post-verify source cleanup only if they want it.

## Files

- `index.html` - one-page product site.
- `styles.css` - responsive visual design.
- `app.js` - storage estimator and progressive enhancement.
- `favicon.svg`, `manifest.webmanifest`, `404.html`, `.nojekyll` - deployment support.
- Markdown files - product, user, engineering, safety, release, and recovery documentation.

## Current product copy

The website may say that a file marked Verified has passed byte-for-byte restoration because the app performs an encode, restore, and complete-file hash comparison before treating an item as successful.

Do not promise universal savings, every WAV variant, future-proof storage, or backup replacement. Preferred wording:

- "Save serious disk space on large WAV collections."
- "Pure-lossless WavPack archives."
- "Verified means restored byte-for-byte."
- "Standard `.wv` files, no proprietary vault."
- "Local-only: no account, cloud upload, trackers, or telemetry."
