# Design Tokens

All tokens are defined as CSS custom properties in `reference/styles.css` under `:root`. The Razor project should **use those variables, not the raw values** — it keeps the site themeable later.

## Colors

### Backgrounds
| Token | Value | Use |
|---|---|---|
| `--bg` | `#0c0d0f` | Page background |
| `--bg-1` | `#111216` | Cards, toolbars, header when scrolled |
| `--bg-2` | `#181a1f` | Elevated surfaces (active palette row, hover) |
| `--bg-3` | `#22252b` | Active toolbar pills, view-toggle selected |

### Borders
| Token | Value | Use |
|---|---|---|
| `--border` | `#2a2d33` | Default 1px borders |
| `--border-soft` | `#1f2126` | Subtle dividers |
| `--border-hot` | `#3a3e46` | Hover + active border state |

### Text
| Token | Value | Use |
|---|---|---|
| `--fg` | `#e8e4d6` | Primary text (warm off-white) |
| `--fg-muted` | `#a8a49a` | Secondary copy |
| `--fg-dim` | `#949188` | Tertiary / meta labels (WCAG AA on every `--bg-N` surface) |
| `--fg-dimmer` | `#4e4c48` | Quaternary / disabled |

### Accent (amber)
| Token | Value | Use |
|---|---|---|
| `--accent` | `#f5a524` | Brand highlight, links, active chip, terminal cursor |
| `--accent-hot` | `#ffc04c` | Accent on hover, code highlights |
| `--accent-soft` | `rgba(245,165,36,0.12)` | Accent tint fill |
| `--accent-soft-2` | `rgba(245,165,36,0.22)` | Selection, active tag |
| `--accent-ring` | `rgba(245,165,36,0.35)` | Focus ring |

### Syntax echo colors (used sparingly)
| Token | Value | Use |
|---|---|---|
| `--cyan` | `#7dd3fc` | Selected item echoes (links inside prose) |
| `--cyan-soft` | `rgba(125,211,252,0.12)` | |
| `--green` | `#86efac` | Reserved: "recording" / "online" dots only |
| `--green-soft` | `rgba(134,239,172,0.12)` | |
| `--magenta` | `#f0abfc` | Keyword accent (very rare) |
| `--red` | `#fca5a5` | Errors / destructive |

**Rule:** the only brand color is amber. Cyan/green/magenta are semantic echoes — use each one for exactly the meaning shown above, never decoratively.

## Typography

### Families
| Token | Stack | Use |
|---|---|---|
| `--font-mono` | `'JetBrains Mono', 'Berkeley Mono', ui-monospace, SFMono-Regular, Menlo, Consolas, monospace` | Headings, labels, code, terminal chrome |
| `--font-sans` | `'Geist', ui-sans-serif, system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif` | Body prose |

### Scale (used in the mock — not tokenised but consistent)
| Context | Size | Weight | Family |
|---|---|---|---|
| Hero H1 | `clamp(30px, 9vw, 48px)` | 600 | mono |
| Page H1 (archive, post) | `48px` desktop / `28px` mobile | 600 | mono |
| Section H2 | `22px` mobile / up to `28px` desktop | 600 | mono |
| Card title | `18px` → `22px` for featured | 500 | mono |
| Body prose | `16px–17px` | 400 | sans |
| Body lead / summary | `17px–18px` | 400 | sans |
| Meta / label | `11px–12px` | 400 | mono, often `letter-spacing: 0.08em; text-transform: uppercase` |
| Code inline | `0.92em` of parent | 400 | mono |
| Code block | `12px–13px` | 400 | mono |
| Nav link | `13.5px` | 500 | mono |

### Feature settings
Body (mono for headings uses default). Inline `.mono` class sets `font-feature-settings: "calt" 0, "liga" 0` — intentional: we don't want JetBrains ligatures in UI labels.

## Spacing

No dedicated spacing scale in `styles.css` — numbers are inline in components. Use 4px base where inventing new values. Common values in use: `4, 6, 8, 10, 12, 14, 16, 20, 24, 32, 40, 48, 64, 80`.

Container max-width: **1280px** with **32px gutter** on desktop, **16px** on mobile.

## Radii
| Token | Value | Use |
|---|---|---|
| `--radius` | `6px` | Default (chips, buttons) |
| `--radius-sm` | `4px` | Small pills, code inline |
| `--radius-lg` | `10px` | Large cards, palette dialog |

Additional ad-hoc values in use: `3px` (keyboard kbd chips), `8px` (toolbar, icon buttons).

## Motion
| Token | Value | Use |
|---|---|---|
| `--dur` | `220ms` | Default transition |
| `--dur-fast` | `140ms` | Hovers, button states |
| `--ease` | `cubic-bezier(0.4, 0, 0.2, 1)` | Default easing |

Respect `@media (prefers-reduced-motion: reduce)` — see the `fadeup` override in `styles.css` for the pattern.

## Breakpoints

Single breakpoint: **768px**. Below = mobile treatments (see `@media (max-width: 768px)` block at the bottom of `styles.css`).

Design canvases:
- Desktop: **1280px** wide
- Mobile: **402px** (iPhone 14 Pro width)

## Shadows
Only one in use — the command palette:
```
box-shadow: 0 30px 80px -20px rgba(0,0,0,0.6);
```

Cards don't use shadows; they rely on `1px` borders against a slightly-lighter background (`--bg-1` on `--bg`).

## Focus ring

```css
:focus-visible {
  outline: 2px solid var(--accent-ring);
  outline-offset: 2px;
  border-radius: 3px;
}
```

Don't override per-element unless there's a visual reason (palette item already styles via `box-shadow: inset` border).

## Scrollbars

Custom webkit scrollbars: 10px, rounded track, `--border` thumb that brightens to `--border-hot` on hover. Firefox falls back to default — acceptable.

## Selection

```css
::selection { background: var(--accent-soft-2); color: var(--accent-hot); }
```

---

**Read next:** [`components.md`](./components.md) or [`js-modules.md`](./js-modules.md).
