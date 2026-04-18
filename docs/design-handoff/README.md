# Goldfinch.me v2 — Claude Code Handoff

This folder is the handoff package for rebuilding **goldfinch.me** in Xperience by Kentico + Razor, using the design mock in the parent folder as the visual reference.

## How to use this package

1. **Start with [`HANDOFF.md`](./HANDOFF.md)** — it's the master doc. It explains the stack, principles, scope, and points at every other file.
2. **Visual reference**: open `../index.html` in a browser — the React mock is the source of truth for layout, spacing, colour, and interaction behaviour. Treat the React/JSX as a design artifact, not a blueprint; the production site is SSR Razor.
3. **Build order**: follow `HANDOFF.md` § "Build order" — it sequences the work so you have working pages end-to-end before polishing.

## Files in this package

| File | Purpose |
|---|---|
| [`HANDOFF.md`](./HANDOFF.md) | Master doc. Read first. |
| [`routes.md`](./routes.md) | URL map + query-string contract for filter/pagination |
| [`content-types.md`](./content-types.md) | Xperience content type definitions (BlogPost, Tag, Talk, etc.) |
| [`components.md`](./components.md) | Razor partial inventory + markup skeletons |
| [`css-tokens.md`](./css-tokens.md) | Design tokens reference (colors, spacing, type, motion) |
| [`js-modules.md`](./js-modules.md) | Vanilla JS budget — what needs scripting, what doesn't |
| [`api-contracts.md`](./api-contracts.md) | JSON endpoints (search) |
| [`progressive-enhancement.md`](./progressive-enhancement.md) | No-JS behaviour per feature |
| [`accessibility.md`](./accessibility.md) | A11y requirements (keyboard, ARIA, contrast) |
| [`reference/`](./reference/) | Assets to copy into the Razor project: `styles.css`, fonts, favicon |
| [`js/`](./js/) | Vanilla JS modules (drop into `wwwroot/js/`) |

## Ground rules

- **SSR first.** Every navigation and filter is a real link with real query strings. No SPA routing.
- **Progressive enhancement.** The site must work with JavaScript disabled — JS only adds polish (command palette, scrollspy, live search, mobile drawer animation).
- **Dark mode only.** No light theme. Don't add a toggle.
- **No build pipeline assumed.** Hand-written CSS + a few `<script>` tags. If the Kentico project already has a bundler, feel free to use it, but don't introduce one for this.
- **No external JS frameworks.** No React, Vue, Alpine, jQuery, Tailwind. Vanilla JS + Razor + the provided `styles.css`.
