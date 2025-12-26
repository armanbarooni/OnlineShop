# Frontend Source of Truth

## Current Layout
- `presentation/`: canonical source for all public/client HTML, CSS, JS, assets.
- `src/WebAPI/wwwroot/fa/`: runtime copy served by ASP.NET (must be generated, not edited manually).

## Audit Summary (2025-11-24)
- Previously, several pages only existed under `wwwroot/fa` (admin & seller panels, misc helpers).
- Missing files have been copied into `presentation/`:
  - `admin-panel-*.html`
  - `seller-panel-*.html`
  - `blank.html`, `blog*.html`, `cart-empty.html`, `dashboard.html`, `documentation-components.html`, `landing.html`, `page-404.html`, `skeleton-index.html`.
- Assets (`assets/...`) are already identical; no divergent files detected.

## Decision
1. **Edit only under `presentation/`.**
2. Use automated sync (see upcoming script) to push content into `wwwroot/fa`.
3. Treat `wwwroot/fa` as build output; any direct edits there will be overwritten.

This document should be updated if new directories are introduced or the source-of-truth changes.

