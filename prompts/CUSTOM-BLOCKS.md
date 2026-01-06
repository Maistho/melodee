## Problem Statement

We want a lightweight customization mechanism that lets an admin add small, page-specific “blurbs” (custom blocks) that render as part of the native UI.
Example blocks:

- `login.top.md`: rendered on `/account/login` above the page’s normal content.
- `login.bottom.md`: rendered on `/account/login` below the page’s normal content.

The solution should be WYSIWYG-ready later (admin editor), secure by default, and not require recompiling the app.

## Goals

- Allow optional, page-scoped custom blocks to be injected at predefined “slots” (top/bottom/etc.).
- Keep rendering “native” (matches the page layout/spacing/theme).
- Provide a single, consistent mechanism usable across Razor components/pages.
- Ensure content is safe to render (prevent XSS / script injection).
- Support file-backed storage now; enable an easy migration path to DB-backed storage + WYSIWYG later.

## Non-Goals (for initial iteration)

- Full CMS/page-builder.
- Arbitrary insertion at any DOM location (only explicit slots we define).
- Rich scripting support (custom JavaScript execution).
- Per-user personalization (this is admin-controlled global content).

## Key Design Decisions

### Content Format

- **Decision:** Markdown only.
  - Store as Markdown, render it to safe markup for display, and sanitize the rendered output.
  - Configure the Markdown renderer to **disallow raw HTML** in Markdown.
  - Good authoring UX for admins and compatible with many WYSIWYG editors.
- **Not allowed:** raw Razor / dynamic code.
  - Security risk and operational risk (code execution, leaking secrets, server-side template injection).

### Naming and Addressing

Represent blocks as `{page}.{slot}` keys.

- Format: `{page}.{slot}.md`
  - `login.top.md`
  - `login.bottom.md`

Pages should own the mapping from route → `page` key (so we can rename routes without breaking blocks if needed).

### Storage (Phase 1)

File-backed blocks in a configurable directory (mounted volume in Docker).

- Example default path: `${MELODEE_DATA_DIR}/custom-blocks/`
- Files:
  - `${CUSTOM_BLOCKS_DIR}/login.top.md`
  - `${CUSTOM_BLOCKS_DIR}/login.bottom.md`

### Rendering

- Render Markdown → safe markup → sanitize → output as a `MarkupString`.
- Keep blocks wrapped in a consistent container so they inherit styling:
  - e.g., `div.custom-block.custom-block--login-top`

## Security Requirements

Custom blocks are untrusted input.

- Sanitize the rendered output with a strict allow-list (XSS protection).
- Ensure the Markdown renderer is configured to **disallow raw HTML** so users can’t embed `<script>` or similar constructs via Markdown.
- Enforce maximum size per block (e.g., 256KB) to avoid abuse.
- Never allow server-side code execution from blocks.

## UX / Styling Guidance

Blocks should “feel native”:

- Provide recommended markup patterns (e.g., use `<p>` and `<a>`).
- Provide a small set of optional CSS utility classes that match the app’s design system.
- Keep spacing consistent by using a wrapper that applies margin/padding aligned with the page.

## Implementation Plan

### Phase 1 — Infrastructure (file-backed, read-only)

1. **Configuration**
   1. Add `CustomBlocks` configuration section:
      - `Enabled` (bool, default: true)
      - `Directory` (string, default: `${MELODEE_DATA_DIR}/custom-blocks`)
      - `MaxBytes` (int, default: 262144)
      - `CacheSeconds` (int, default: 30)
2. **Domain / App service**
   1. Create `ICustomBlockService`:
      - `Task<CustomBlockResult> GetAsync(string key, CancellationToken ct)`
      - Where `key` is like `login.top` (service resolves to `login.top.md`).
   2. Implement `FileCustomBlockService`:
      - Normalize/validate key (deny path traversal: reject `..`, `/`, `\\`).
      - Resolve to a file path under the configured root.
      - Read file (size-limited).
      - Render Markdown to safe markup for display.
      - Sanitize the rendered output and return as a `MarkupString`.
      - Cache by key + file last-write timestamp (memory cache).
3. **Markdown rendering + sanitization**
   1. Add a Markdown renderer configured to **disallow raw HTML**.
   2. Sanitize the rendered output (allow-list) and centralize sanitizer configuration.
4. **Rendering component**
   1. Add `CustomBlock.razor`:
      - Parameters: `Key`, `CssClass?`, `WrapInCard?` (optional)
      - If block missing → render nothing.
      - If error → log and render nothing (do not break page render).

### Phase 2 — Initial opt-in pages (short list)

Start with a small, high-value set of **explicit opt-in** pages. The app only renders blocks where we intentionally place `<CustomBlock />`.

#### 1) `/account/login`

- Above content: `login.top`
- Below content: `login.bottom`

Example:

```razor
<CustomBlock Key="login.top" CssClass="mb-3" />

<!-- existing login UI -->

<CustomBlock Key="login.bottom" CssClass="mt-3" />
```

#### 2) `/account/register` (if enabled)

- `register.top`
- `register.bottom`

#### 3) Password reset request page (forgot password)

- `forgot-password.top`
- `forgot-password.bottom`

#### 4) Password reset (token entry/new password) page

- `reset-password.top`
- `reset-password.bottom`

3. Confirm missing blocks do not change layout (no empty cards/margins).

### Phase 3 — Expand slot coverage (opt-in)

Add more pages only as needed (still explicit opt-in). Recommended next candidates:

- Access denied / unauthorized page: `access-denied.top`, `access-denied.bottom`
- App-wide post-login banner (dedicated layout slot): `app.banner`

Keep slot names consistent (`top`, `bottom`, `banner`) and document each page’s supported keys.

### Phase 4 — Admin UX (WYSIWYG-ready)

1. Add an admin page to manage blocks:
   - list existing blocks
   - preview rendered output
   - edit content (WYSIWYG)
   - save
2. Storage migration option:
   - Keep file-backed as default, but allow DB-backed provider later.
   - Consider a provider abstraction:

```csharp
interface ICustomBlockStore
{
    Task<CustomBlock?> GetAsync(string key, CancellationToken ct);
    Task SaveAsync(string key, string markdown, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
    Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct);
}
```

3. Always sanitize on save and on render.

## Testing Plan

- Unit tests:
  - Key validation blocks traversal attempts (`../`, absolute paths).
  - Sanitizer removes script injection attempts and unsafe attributes.
  - Missing file → returns “not found” without exceptions.
  - Cache respects last-write timestamp changes.
- Integration tests:
  - Given a custom blocks directory with `login.top.md`, verify it renders on the login page.

## Operational / Deployment Notes

- For Docker deployments, mount the blocks directory as a volume.
- Provide an example `custom-blocks/` folder in docs:

```text
custom-blocks/
  login.top.md
  login.bottom.md
```

- Log at `Information` when blocks are loaded (key + path), and `Warning` on sanitize failures.

## Open Questions

- Do we want per-library/per-tenant blocks (multi-tenant readiness), or global-only?
- Should we allow limited inline styles, or enforce CSS class-only styling?
- Do we need a preview mode that renders with the current theme?
