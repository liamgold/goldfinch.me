# Accessibility

Target: **WCAG 2.2 AA**. Lighthouse a11y score ≥ 95.

## Structure

- Exactly one `<h1>` per page.
- Headings in order — no skipping levels (no `<h4>` inside an `<h2>` section without an `<h3>`).
- Landmark roles: `<header>`, `<nav>`, `<main>`, `<aside>`, `<footer>`. One `<main>` per page.
- Skip-to-content link as the first focusable element in `<body>`:
  ```html
  <a class="skip-link" href="#main">Skip to content</a>
  ```
  Visually hidden until focused (uses `.skip-link` class — add to `styles.css` if not present yet).

## Keyboard

- **All interactive elements reachable by Tab.** No `tabindex="-1"` on anything clickable.
- **Focus visible.** The `:focus-visible` ring in `styles.css` covers most; don't override per-element unless needed.
- **Command palette**: `⌘K` / `Ctrl+K` opens. `↑↓` walk. `Enter` navigates. `Esc` closes. Focus returns to the opener.
- **Mobile drawer**: Tab trap inside while open. `Esc` closes. Focus returns to hamburger.
- **Blog toolbar search input**: `ArrowDown` from input moves focus into the results list; `ArrowUp` from first result returns to input.
- **TOC**: plain anchor links — browser handles the keyboard.

## ARIA

| Element | Attributes |
|---|---|
| Mobile drawer | `role="dialog"`, `aria-modal="true"`, `aria-labelledby` |
| Hamburger button | `aria-expanded`, `aria-controls="mobile-drawer"` |
| Command palette dialog | `role="dialog"`, `aria-modal="true"`, `aria-label="Command palette"` |
| Palette input | `aria-controls="palette-listbox"`, `aria-activedescendant="palette-item-N"` |
| Palette list | `role="listbox"` |
| Palette item | `role="option"`, `aria-selected` |
| Active tag chip | `aria-current="true"` |
| Current page nav link | `aria-current="page"` |
| Pagination prev/next | `aria-label="Newer posts"` / `aria-label="Older posts"` (buttons say "Newer" only by default) |
| Disabled pager side | Render as `<span aria-disabled="true">`, not a disabled `<a>` |
| Reading progress | `<progress>` element if possible, else `role="progressbar"` with `aria-valuenow` updated |
| Live search results | `role="listbox"`, with results as `role="option"` |

## Color contrast

The token palette is verified AA across **all four background surfaces** the mock actually uses:

| Text token | vs `--bg` (#0c0d0f) | vs `--bg-1` (#111216) | vs `--bg-2` (#181a1f) | vs `--bg-3` (#22252b) |
|---|---|---|---|---|
| `--fg` `#e8e4d6` | 15.3 AAA | 14.7 AAA | 13.7 AAA | 12.1 AAA |
| `--fg-muted` `#a8a49a` | 7.8 AAA | 7.5 AAA | 7.0 AAA | 6.2 AA |
| `--fg-dim` `#949188` | 6.2 AA | 5.9 AA | 5.5 AA | 4.9 AA |
| `--accent` `#f5a524` | 9.5 AAA | 9.2 AAA | 8.5 AAA | 7.5 AAA |
| `--accent-hot` `#ffc04c` | 11.9 AAA | 11.5 AAA | 10.7 AAA | 9.4 AAA |
| `--cyan` `#7dd3fc` | 11.7 AAA | 11.2 AAA | 10.4 AAA | 9.2 AAA |
| `--green` `#86efac` | 13.8 AAA | 13.3 AAA | 12.4 AAA | 10.9 AAA |
| `--red` `#fca5a5` | 10.2 AAA | 9.9 AAA | 9.2 AAA | 8.1 AAA |

Every text token above clears WCAG 2.2 AA (≥4.5:1 normal text) on every background surface.

**`--fg-dimmer` `#4e4c48`** is at ~2.0–2.3:1 across all surfaces — **decoration only**. Valid uses: inactive item count chips, dot separators, disabled icon tint, placeholder scaffolding in SVG mocks. Never use for prose, metadata, or anything a user needs to read.

## Motion

- All animations respect `prefers-reduced-motion: reduce` — the `fadeup` override in `styles.css` is the pattern.
- No auto-playing media.
- No carousels or parallax.

## Forms

- Every input has an associated `<label>` (visually or via `aria-label`).
- Error messages connected via `aria-describedby`.
- Don't rely on placeholder as label.

## Images

- Every `<img>` has `alt`. Decorative images: `alt=""`.
- Post heroes: alt should describe the image, not repeat the title.
- SVG icons: `aria-hidden="true"` if purely decorative; `role="img"` + `<title>` if meaningful.

## Language

- `<html lang="en-GB">`.
- Explicit `lang` on any code block that differs (usually unnecessary).

## Screen-reader announcements

- Search results update: wrap the results container in `aria-live="polite"`.
- Pagination is a static navigation — no live region needed.

## Testing

1. **Keyboard-only pass** through Home → Blog → Filter → Post → About → Speaking, Tab through everything.
2. **NVDA or VoiceOver** read-through of a post detail page.
3. **Axe DevTools** or Lighthouse — fix everything it flags.
4. **Zoom to 200%** — no horizontal scrolling, no clipped text.
5. **High-contrast mode** (Windows) — outlines still visible, UI doesn't break.
