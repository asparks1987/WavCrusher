# WavCrusher `/docs` Site

This directory is both a dependency-free static marketing site and the main technical-documentation collection.

## Preview locally

From the repository root:

```powershell
python -m http.server 8080 --directory docs
```

Then open `http://localhost:8080/`. A local server is preferable to `file://` when checking manifest and browser behavior.

## Deploy

The site can be published from `/docs` with GitHub Pages or the included Pages workflow. Before public deployment:

- Replace pre-alpha status only when an actual tested release exists.
- Add real repository/release links; do not invent them.
- Run HTML/CSS/JavaScript and accessibility checks.
- Confirm every relative documentation link resolves.
- Keep the site free of trackers, remote fonts, CDNs, and third-party scripts.
- Keep compression figures qualified as estimates.

## Files

- `index.html` â€” one-page product site.
- `styles.css` â€” responsive visual design.
- `app.js` â€” accessible mobile navigation, storage estimator, and reduced-motion-aware reveals.
- `favicon.svg`, `manifest.webmanifest`, `404.html`, `.nojekyll` â€” deployment support.
- Markdown files â€” product and engineering documentation.

## Copy rule

The website may say that a file marked Verified has passed byte-for-byte restoration **only after the implementation and release tests enforce that state**. Until then, use planned/future language and retain the pre-alpha banner.
