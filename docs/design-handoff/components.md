# Components — Razor partial inventory

Each partial below corresponds to a well-defined region in the [bundled mock (`mock.html`)](./mock.html). File paths assume `Views/Shared/` unless noted. Markup shown is the target shape, not final — lift class names verbatim from the mock to keep `styles.css` matches working.

> **⚠️ Where the component CSS lives:** `reference/styles.css` only defines tokens, base styles, utilities, and the mobile-override block. The actual per-component styling is embedded as inline `style` objects in the React source inside `mock.html`. For each partial below, open `mock.html` in a browser, inspect the component in DevTools, and lift the computed styles into a named CSS class in `wwwroot/css/components.css`. The class names referenced in `styles.css`'s mobile block (e.g. `.palette-dialog`, `.nav-desktop`, `.featured-card`, `.post-article`, `.blog-toolbar`, `.blog-pagination`, `.speaking-stats`, `.speaking-grid`, `.about-stack`, `.footer-grid`, `.md-post-layout`, etc.) are the **class-name contract** — use those exact names so the responsive overrides apply.

> **Pattern:** one view model per partial, passed in via `@model` or `@Html.Partial(name, model)`. Keep partials presentation-only — query Kentico in the action, pass plain DTOs in.

## Layout

### `_Layout.cshtml`
- `<head>`: title, meta description, canonical, OG tags, `preconnect` for Google Fonts, JetBrains Mono font link, `<link rel="stylesheet" href="~/styles.css">`, `<link rel="icon" href="~/favicon-32x32.png">`
- Skip-to-content link (`<a class="skip-link" href="#main">Skip to content</a>`)
- `@Html.Partial("_Header")`
- `<main id="main">@RenderBody()</main>`
- `@Html.Partial("_Footer")`
- `<div id="palette-root"></div>` — empty until `command-palette.js` hydrates
- Scripts at end, all `defer`: `mobile-drawer.js`, `command-palette.js`, (conditional) `toc-scrollspy.js`, `search.js`

## Chrome

### `_Header.cshtml` (`header.header` in the mock)
- Left: brand logo (`Goldfinch` wordmark + `.me` in accent) + small tagline on desktop
- Middle: desktop nav (`Home`, `Blog`, `About`, `Speaking`) — real `<a>` links, active state via `aria-current="page"` on whichever matches the current route
- Right: search trigger (`⌘K` button on desktop, search icon on mobile), hamburger button on mobile (`aria-expanded`, `aria-controls="mobile-drawer"`)
- Mobile drawer lives here too (`aside#mobile-drawer`), hidden by default; toggled by `mobile-drawer.js`
- Class hooks already in `styles.css`: `.nav-desktop`, `.search-desktop`, `.search-mobile`, `.menu-mobile`, `.brand-sub`, `.icon-btn`

### `_Footer.cshtml`
- Three columns on desktop (`.footer-grid`), stacked on mobile
- Bottom bar with © year + build SHA + RSS link (`.footer-bottom`)

### `_MobileDrawer.cshtml` (can inline in header)
- `<aside id="mobile-drawer" role="dialog" aria-modal="true" aria-labelledby="drawer-title" hidden>`
- Contains nav links (same as desktop), search form, social links
- Closes on Esc, backdrop click, or link click (all wired in `mobile-drawer.js`)

## Home

### `Views/Home/Index.cshtml`
Composition, top to bottom:
1. `_Hero.cshtml` — terminal-style intro block with animated cursor
2. `_NowPanel.cshtml` — labelled rows (`Reading`, `Shipping`, `Playing with`, `Listening`). Data from a JSON file in the repo initially (`App_Data/now.json`) until it's worth modelling
3. `_FeaturedPost.cshtml` — single card, pulled by `BlogPost.Featured = true` → fallback to newest
4. `_RecentPosts.cshtml` — next 6 non-featured posts in a grid (`grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))`)
5. `_SpeakingCallout.cshtml` — small teaser linking `/speaking`
6. `_PaletteHint.cshtml` — "Press ⌘K for anywhere." Dismissible via localStorage key `gf2_palette_hint_dismissed`

## Blog archive

### `Views/Blog/Index.cshtml`
ViewModel: `BlogIndexViewModel`
```csharp
public record BlogIndexViewModel(
    IReadOnlyList<PostCardDto> Posts,
    IReadOnlyList<TagChipDto> Tags,
    string? ActiveTag,
    string? Query,
    string View,            // "grid" or "list" — persisted via cookie
    int Page,
    int TotalPages,
    int TotalCount,
    int PageStart,          // 1-indexed item number of first on page
    int PageEnd             // 1-indexed item number of last on page
);
```

Composition:
1. Heading (`~/blog` label, "Archive" title, post count)
2. `_BlogToolbar.cshtml` — search input, filter count, view toggle (grid/list)
3. `_TagChips.cshtml` — chips with counts; each chip is an `<a href="?tag={slug}">`; "All posts" clears the filter
4. Results:
   - If `Posts.Count == 0` → `_EmptyState.cshtml`
   - If `View == "grid"` → `_PostGrid.cshtml` (list of `_PostCard.cshtml`)
   - If `View == "list"` → `_PostListView.cshtml` (condensed rows, wrapped in `.mobile-scroll-x` for narrow viewports)
5. `_Pagination.cshtml` — GitHub-style prev/next with `Page X of Y` and item range

### `_PostCard.cshtml`
Props: `PostCardDto { Slug, Title, Summary, Tags, PublishedOn, ReadingMinutes, HeroUrl? }`
- Entire card is an `<a href="/blog/@Model.Slug">` so the whole thing is clickable and keyboard-focusable once
- Reuse `.post-card-*` classes from `styles.css` (they're embedded in the mock's `PostCard` component — lift them into dedicated class names during extraction)

### `_TagChip.cshtml`
```cshtml
<a href="@Model.Href"
   class="tag-chip @(Model.Active ? "is-active" : "")"
   aria-current="@(Model.Active ? "true" : null)">
  <span class="tag-chip__label">@Model.Label</span>
  <span class="tag-chip__count mono">@Model.Count</span>
</a>
```

### `_Pagination.cshtml`
Props: `PaginationViewModel { Page, TotalPages, TotalCount, PageStart, PageEnd, PrevHref?, NextHref? }`
- Three-column grid: `Newer` button left, status centre, `Older` button right
- Disabled side renders as a `<span>` with `aria-disabled="true"` styled like the disabled state — do **not** render an `<a>` at all when there's no prev/next
- Status: `Page <accent>N</accent> of M` + `X–Y of Total`

## Post detail

### `Views/Blog/Post.cshtml`
ViewModel:
```csharp
public record PostDetailViewModel(
    PostDto Post,
    IReadOnlyList<TocEntryDto> Toc,    // extracted from Body h2/h3 at render
    PostCardDto? Prev,                  // newer post
    PostCardDto? Next,                  // older post
    IReadOnlyList<PostCardDto> Related  // up to 3 by shared tag overlap
);
```

Composition:
1. `_PostPathBar.cshtml` — breadcrumb-ish `~/blog/{slug}.md` terminal bar
2. Three-column grid (`.md-post-layout`): left TOC rail, centre article, right info rail
3. Left: `_TocRail.cshtml` (server-rendered; JS adds scroll-spy active state)
4. Centre: `_Article.cshtml` — `<article class="post-article">` with `<h1>`, summary, body, author card, prev/next
5. Right: `_PostInfoRail.cshtml` — reading time, tags, share links, back-to-top
6. Reading progress bar at top (`<div class="reading-progress" />`) — `0%` server-rendered, JS updates width on scroll

### `_Article.cshtml`
Prose styling already defined via `.post-article *` rules in `styles.css`. The body is rendered from Kentico rich text — ensure the rich-text editor outputs bare semantic HTML (no inline styles or tracking-style classes). Code blocks should be `<pre><code class="language-{lang}">` so a future syntax highlighter has a hook.

### `_AuthorCard.cshtml`
Static content partial. Used on post detail and linked from About.

### `_PrevNextNav.cshtml`
Two adjacent cards: `← Newer: <title>` | `Older: <title> →`. Either can be absent (first/last post).

### `_RelatedPosts.cshtml`
`<h2>` + grid of `_PostCard.cshtml`. Hide if `Related.Count == 0`.

## About

### `Views/About/Index.cshtml`
- Intro block
- Stack grid (`.about-stack` — two columns on desktop, one on mobile) listing tools, stack items
- Social links
- Contact prompt

## Speaking

### `Views/Speaking/Index.cshtml`
ViewModel: `SpeakingViewModel { IReadOnlyList<SpeakingYearGroupDto> Years, SpeakingStatsDto Stats }`
1. Hero with stats grid (`.speaking-stats` — 4 cols desktop, 2x2 mobile): total talks, countries, hours, recordings
2. Year groups: `<section>` per year with `<h2>` then `_TalkCard.cshtml` grid (`.speaking-grid`)

### `_TalkCard.cshtml`
- Title, event + location, date
- Type pill (conference/webinar/meetup/podcast)
- "Recording" badge if `HasRecording`
- Clickable card if `RecordingUrl` set — whole card is an `<a>`. Otherwise render as `<article>` with no link.

## Shared

### `_CommandPalette.cshtml`
Empty shell — the `<div id="palette-root">` in `_Layout.cshtml` is sufficient. `command-palette.js` injects the dialog on first open.

### `_EmptyState.cshtml`
Terminal-style "`> no results`" block with a "clear filters" link.

### `_SkipLink.cshtml`
Single `<a class="skip-link" href="#main">Skip to content</a>` — visible only on focus.

### `_Seo.cshtml`
A small partial that renders `<title>`, `<meta name="description">`, canonical, OG/Twitter tags from an `SeoViewModel`. Every page composes one and hands it to `_Layout.cshtml` via `ViewData`.

---

**Read next:** [`css-tokens.md`](./css-tokens.md) for the design tokens reference.
