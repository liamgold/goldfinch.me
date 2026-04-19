# JS Modules — Vanilla JS Budget

Total client JS: **~5 modules, ~400 lines**. All progressive enhancement — pages work with JS off.

Load all with `defer` at the bottom of `_Layout.cshtml`:

```html
<script defer src="~/js/mobile-drawer.js"></script>
<script defer src="~/js/command-palette.js"></script>
<script defer src="~/js/search.js"></script>
@if (ViewBag.ShowTocScrollspy == true) {
  <script defer src="~/js/toc-scrollspy.js"></script>
}
```

No bundler required. Each module is a self-contained IIFE that listens for DOM-ready.

## Module index

| File | Purpose | Lines (approx.) |
|---|---|---|
| [`mobile-drawer.js`](./js/mobile-drawer.js) | Hamburger → slide-in drawer, Esc/backdrop close, focus trap | 70 |
| [`command-palette.js`](./js/command-palette.js) | `⌘K` / `Ctrl+K` palette with ↑↓↵ keyboard nav | 180 |
| [`search.js`](./js/search.js) | Debounced live search for blog toolbar input | 60 |
| [`toc-scrollspy.js`](./js/toc-scrollspy.js) | Highlight active TOC entry + reading-progress bar | 70 |
| [`palette-hint.js`](./js/palette-hint.js) | Dismissible home-page ⌘K hint with localStorage | 20 |

## Rules per module

1. **No globals.** Wrap in `(() => { … })()`.
2. **Feature-detect, don't UA-sniff.** `if (!('IntersectionObserver' in window)) return;` for scrollspy.
3. **Respect `prefers-reduced-motion`**. All transitions should be gated.
4. **All handlers idempotent.** Safe to re-run on turbo/back-forward cache restore.
5. **Accessibility first**: use `aria-expanded`, `aria-controls`, `aria-selected`, `role="dialog"`, focus management.
6. **No external libs.** No `fuse.js`, no `lunr`, no `popper`. Fetch + DOM only.

## How the palette talks to search

Both `command-palette.js` and `search.js` hit the same `/api/search` endpoint. Share a tiny helper:

```js
// Inline at top of both files (or extract to a shared `search-api.js` if you add more callers)
async function searchApi(q, limit = 8, signal) {
  const url = `/api/search?q=${encodeURIComponent(q)}&limit=${limit}`;
  const res = await fetch(url, { signal, headers: { Accept: 'application/json' } });
  if (!res.ok) throw new Error(`Search ${res.status}`);
  return res.json();
}
```

Reuse `AbortController` between keystrokes to cancel in-flight requests.

## Server rendering expectations the JS relies on

- **Mobile drawer**: the `<aside id="mobile-drawer" hidden>` must exist on every page (emitted by `_Header.cshtml`).
- **Command palette**: `<div id="palette-root"></div>` must exist in `_Layout.cshtml`. The palette is built on first open.
- **TOC scrollspy**: the TOC renders `<a href="#{slug}">` links with a known parent selector (`nav.toc-rail`). Each heading in the article gets a matching `id` attribute server-side.
- **Reading progress**: `<div class="reading-progress"><div class="reading-progress__fill"></div></div>` rendered at top of `.md-post-layout`. JS updates `transform: scaleX(N)` on `.reading-progress__fill`.
- **Search input**: `<input data-live-search>` in the blog toolbar; results render into `<div data-live-search-results>` sibling.

## Testing checklist

- Disable JS in devtools → every page still navigates, filters, paginates.
- `⌘K` opens palette; `Esc` closes; `↑↓` walks list; `↵` navigates.
- Mobile drawer: tap hamburger, tap backdrop to close, tap link to close + navigate.
- Post detail: scroll → TOC item highlights; reading progress fills; hits 100% at article bottom.
- Search input: type → results appear after 180ms; Esc clears; ↓ enters result list.
