# Progressive Enhancement

Every page must work with JavaScript disabled. JS only adds polish — the core flow is SSR HTML + HTTP.

| Feature | With JS | Without JS |
|---|---|---|
| Page navigation | Real `<a href>` links | Same — works identically |
| Tag filter | Click chip → query-string nav | Same |
| Pagination | Click prev/next → query-string nav | Same |
| Blog view toggle (grid/list) | Click toggle → sets cookie, re-requests page | Toggle buttons are `<button type="submit" form="view-form">`s in a tiny `<form method="get">` that posts `?view=list&…`. Full reload. |
| Search input (blog toolbar) | Debounced fetch → dropdown | The input sits inside `<form method="get" action="/blog">` — submitting reloads `/blog?q=…` as a server-side search |
| Command palette | `⌘K` opens dialog | Does nothing. Users fall back to the search form above. |
| Mobile drawer | Hamburger toggles slide-in | Hamburger button is `hidden` without JS (`.menu-mobile[hidden]` is the default). Desktop nav remains visible at all widths — slightly ugly on phones but functional. |
| TOC on post | Scrollspy highlight + smooth scroll | Plain anchor links. Browser's native `scroll-behavior: smooth` (set on `html`) still gives smooth scroll in modern browsers. |
| Reading progress bar | Fills as you scroll | Stays at 0%. Acceptable — purely decorative. |
| Palette hint on home | Shows unless dismissed | Always visible (no localStorage read). Not a problem — it's static copy. |
| Copy-code buttons on code blocks | Clipboard API | Rendered as disabled `<button>` (or omitted entirely) |

## Techniques used

- **`<form method="get">` for anything that could be a link.** The blog toolbar search form is a real form; if JS hydrates, it prevents default and runs the fetch instead.
- **`hidden` attribute** for JS-managed UI. The mobile drawer and command palette are `hidden` in markup; JS removes the attribute when activating.
- **`aria-expanded` always on toggles.** Starts `false`, flips to `true` when open.
- **No `onclick` attributes.** All behaviour attached via `addEventListener` in the JS modules.

## What's NOT progressive

These features genuinely require JS and have no fallback:

- Live search dropdown (fallback: submit the form for server-side search)
- Command palette (fallback: use the nav + search form)
- TOC scrollspy highlighting (fallback: just click the anchor)
- Reading progress indicator (no fallback needed — purely decorative)

Everything else has a full no-JS path.

## Testing

Load each page with JS disabled in devtools. Checklist:

- [ ] Home loads, featured post clickable
- [ ] `/blog` loads, all chips clickable and produce `/blog?tag=…` URLs
- [ ] `/blog?tag=ai&page=2&q=claude` renders correctly (all three params honoured)
- [ ] `/blog/from-spec-to-stripe` loads with full article
- [ ] TOC links scroll to the right heading
- [ ] Prev/next post links work
- [ ] Submit the blog toolbar search form → reloads with `?q=` and shows results
- [ ] `/rss.xml` returns valid RSS
- [ ] No console errors (hard to have errors with no JS, but check for broken `<noscript>` content etc.)
