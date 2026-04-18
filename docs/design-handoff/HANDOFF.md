# HANDOFF.md — Goldfinch.me v2

> Rebuild of goldfinch.me with a new visual design.
> Stack: **Xperience by Kentico 30+ (SaaS)** + **Razor** + vanilla JS.
> Replaces the existing goldfinch.me design 1:1.

## 1. Context

- Personal blog + speaking portfolio for Liam Conroy (Kentico MVP, IDHL).
- Currently runs on Xperience by Kentico SaaS; source is at `github.com/liam-conroy/goldfinch.me` (open source).
- ~13 published posts today, single author. Content will keep growing.
- Traffic pattern: mostly organic search to individual posts, some referrals from Kentico community. **SEO matters.**

## 2. Design reference

The React mock at `../index.html` is the frozen reference. When anything in this doc is ambiguous, **the mock wins**. Compare against it visually page by page.

Pages to implement (in `../index.html`, cycle via the nav or hash routes):
- `#home` — hero + featured post + "Now" panel + recent posts + speaking callout + command-palette hint
- `#blog` — archive with toolbar (search input, filter count, grid/list toggle), tag chips, paginated results
- `#post` — post detail with TOC rail, reading progress, author card, prev/next, related posts
- `#about` — bio, stack, social links
- `#speaking` — stats grid + talks grouped by year

The mock is intentionally **dark-only** and uses a JetBrains Mono + Geist type pairing — keep both.

## 3. Stack decisions

| Decision | Choice | Rationale |
|---|---|---|
| Framework | Xperience by Kentico + Razor | Dogfooding; CMS already models content |
| Rendering | Fully server-side | Simpler, better SEO, no hydration cost for a blog |
| Routing | Real URLs with query strings | `/blog?tag=ai&page=2` — indexable, shareable, no JS needed |
| Client JS | Vanilla, progressive enhancement | Tiny budget (~5 modules, ~300 lines total) |
| Search | JSON API + JS widget | `/api/search?q=…` returns posts/tags; results rendered in a dropdown |
| Styling | Hand-written CSS with custom properties | No framework. `reference/styles.css` drops in verbatim. |
| Theme | Dark only | Per design decision — no toggle |
| Comments | None | Not in scope |
| Analytics | None yet | Leave hooks ready for Plausible or similar later |

## 4. Build order

Recommended sequence — keeps the site navigable end-to-end at every step.

1. **Layout shell** — `_Layout.cshtml` with `<Header>`, `<Footer>`, `styles.css` linked, fonts loaded.
2. **Home page** — static sections first (hero, "Now" panel, featured, recent). Real data wired via Kentico content type queries.
3. **Blog archive** — no filter, no pagination. Just render all posts. Add the toolbar UI but leave it non-functional.
4. **Post detail** — markup + prose styles. Static TOC list (server-rendered from headings in the body).
5. **Tag filter + pagination** — server-side via query strings. Re-render the archive with filtered posts.
6. **About + Speaking** pages — mostly content, low risk.
7. **Mobile drawer** — JS module, progressive enhancement.
8. **TOC scrollspy + reading progress** — JS module.
9. **Command palette** — JS module + `/api/search` endpoint.
10. **Live search input** in the blog archive toolbar — same endpoint.
11. **RSS feed** — `/rss.xml` endpoint.
12. **Polish** — `prefers-reduced-motion`, focus rings, skip-to-content link, 404 page.

## 5. Scope boundaries

### In scope
- All five pages listed above, responsive (768px breakpoint).
- Dark theme only.
- Pagination (6 posts/page), tag filtering, search — all query-string driven except search dropdown.
- Command palette (⌘K) with keyboard nav.
- RSS feed.
- Single-author site — author info is a Razor partial (`_Author.cshtml`) with hard-coded content for now.

### Out of scope (for this rebuild)
- Multi-author support.
- Comments.
- Tag detail pages (tag chips filter the archive in place).
- Newsletter signup.
- Analytics integration (leave a comment in `_Layout.cshtml` where the tag would go).
- Cookie banner (no cookies to disclose).
- Light mode / theme toggle.
- i18n.
- Post-series grouping.
- PWA / offline support.

## 6. What Claude Code will receive

From this folder:
- `routes.md` — the URL ↔ controller/view map
- `content-types.md` — Kentico content types + fields
- `components.md` — Razor partials inventory with markup skeletons
- `css-tokens.md` — design tokens (already used throughout `styles.css`)
- `js-modules.md` — JS behaviour list
- `api-contracts.md` — search endpoint shape
- `progressive-enhancement.md` — no-JS fallback per feature
- `accessibility.md` — a11y checklist
- `reference/` — `styles.css`, `fonts/geist-variable.woff2`, `favicon-32x32.png` — copy into `wwwroot/`
- `js/` — vanilla JS modules ready to drop into `wwwroot/js/`

Everything needed for visuals is in **`reference/styles.css` (~248 lines)** and the existing mock — no additional design Figma is required.

## 7. Fonts

- **JetBrains Mono** — used for headings, labels, code, terminal chrome. Load from Google Fonts (already wired in `../index.html`):
  ```
  https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&display=swap
  ```
- **Geist Variable** — body text. Served locally from `reference/fonts/geist-variable.woff2` (already referenced in `styles.css` via `@font-face`).

Keep both `preconnect` hints on `<head>` (`fonts.googleapis.com` + `fonts.gstatic.com`).

## 8. Favicon + assets

- `reference/favicon-32x32.png` — copy to `wwwroot/`.
- Social/OG image: reuse the existing goldfinch.me OG image; not in this package.

## 9. Content migration

- Existing ~13 posts are in the current Xperience instance. **No migration required** — this rebuild is design-only; content types may need adjusting to match `content-types.md`. Do a dry-run diff between the existing BlogPost content type and the spec before touching anything.
- If content types change, write a one-off migration script (separate task, not in this package).

## 10. Deploy target

Same Azure infrastructure as today. CI/CD already exists — if the rebuild adds new NuGet packages or static assets, update the pipeline accordingly.

## 11. Definition of done

- All five pages match the mock at desktop (1280px canvas) and mobile (402px).
- Works with JS disabled (see `progressive-enhancement.md`).
- Lighthouse: Performance ≥ 90, Accessibility ≥ 95, SEO ≥ 95 on a cold post-detail page.
- `⌘K` opens the command palette; ↑↓↵ navigate; Esc closes.
- `/rss.xml` validates against the W3C validator.
- No horizontal scrollbar on any page at 402px.
- Skip-to-content link works.
- All nav + filter + pagination URLs are shareable and deep-linkable.

---

**Read next:** [`routes.md`](./routes.md) for the URL map.
