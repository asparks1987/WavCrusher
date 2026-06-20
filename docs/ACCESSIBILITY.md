# Accessibility Plan

## 1. Commitment

WavCrusherâ€™s core archive, audit, and restore workflows must be usable without a mouse and understandable without relying on color or animation. Accessibility is a release requirement, not a polish task.

The Windows Forms app should follow Microsoftâ€™s accessibility guidance and target practical conformance with WCAG 2.2 AA concepts where they apply to desktop software. The static website should target WCAG 2.2 AA.

## 2. Windows Forms requirements

### Keyboard

- Every interactive control reachable in a logical tab order.
- Clear keyboard accelerators for common commands.
- Enter/Space activate controls according to Windows conventions.
- Escape closes non-destructive dialogs or cancels an in-progress selectionâ€”not active archive work without confirmation.
- Focus remains visible and moves predictably after scans/errors/completion.
- Data grids support keyboard row navigation, sorting, and accessible details.

### Screen readers and automation

- Set meaningful `AccessibleName`, `AccessibleDescription`, and role where defaults are insufficient.
- Associate labels with editable fields.
- Announce validation errors and operation-state changes without stealing focus repeatedly.
- Progress includes textual stage/count/bytes; a progress bar alone is insufficient.
- Status icons have equivalent text.
- Decorative graphics are ignored by accessibility APIs.

### Scaling and layout

- Use DPI-aware application configuration.
- Prefer layout panels, docking, anchoring, and AutoSize.
- Test 100%, 125%, 150%, and 200% display scaling plus large text.
- Avoid clipped labels and fixed-height text containers.
- Allow the main window to resize and remember a valid layout.

### Color and contrast

- Support Windows high contrast.
- Never communicate Verified/Failed/Conflict only by green/red.
- Use system colors where practical.
- Icons must remain distinguishable in light/dark/high-contrast environments if custom themes are added.

### Motion and timing

- Avoid nonessential animation.
- Respect reduced-motion preferences where detectable.
- Do not impose time limits on reviewing plans/reports.
- Cancellation remains available during long operations.

### Language

- Use plain, specific text.
- Explain â€œwhole-file SHA-256â€ in tooltips/help.
- Avoid ambiguous buttons such as â€œOKâ€ when â€œStart verified archiveâ€ is clearer.
- Error messages identify item, stage, effect, and next safe action.

## 3. Website requirements

- Semantic landmarks: header, nav, main, sections, footer.
- One logical H1 and hierarchical headings.
- Skip-to-content link.
- Keyboard-operable navigation and estimator.
- Visible focus indicators.
- Sufficient contrast.
- Responsive reflow without horizontal scrolling at common narrow widths.
- Reduced-motion media query.
- No autoplay audio/video.
- Form labels and live-region output for the storage estimator.
- No remote fonts or scripts that delay/block content.
- Meaningful SVG title/accessible handling.

## 4. Testing checklist

### Automated

- Static HTML validation/linting.
- Accessibility scanner for the website.
- WinForms presenter tests for accessible status text.
- UI automation checks for accessible names on critical controls where feasible.

### Manual

- Complete Archive, Audit, and Restore with keyboard only.
- Test Narrator and at least one additional common Windows screen reader when available.
- Test high contrast.
- Test scaling/text enlargement.
- Test a mixed-result completion run and ensure failures are obvious.
- Test cancellation and confirmation dialogs.
- Test website at 320 CSS px width and 200% zoom.

Record manual results and screenshots in release evidence. Accessibility defects blocking a core workflow are release blockers.
