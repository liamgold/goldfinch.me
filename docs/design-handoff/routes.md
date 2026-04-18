# Routes — URL map

All routes are server-side rendered. Filter, search, and pagination state live in the query string so every URL is shareable.

## Public routes

| URL | Controller action / page | Notes |
|---|---|---|
| `/` | `HomeController.Index` → `Views/Home/Index.cshtml` | Hero, "Now", featured post, recent posts, speaking callout |
| `/blog` | `BlogController.Index` | Archive with toolbar, chips, pagination |
| `/blog?tag={slug}` | same | Filter by tag. `tag` = slug from `Tag` content type |
| `/blog?q={query}` | same | Server-side text search (title/summary/tag match). Same markup as filtered view. |
| `/blog?page={n}` | same | 1-indexed. Combine freely with `tag` and `q`. |
| `/blog/{slug}` | `BlogController.Post(string slug)` | Post detail. 404 if no match. |
| `/about` | `AboutController.Index` | Static content |
| `/speaking` | `SpeakingController.Index` | Grouped by year |
| `/rss.xml` | `FeedController.Rss` | RSS 2.0, all posts, newest first |
| `/robots.txt` | Static | Reference sitemap |
| `/sitemap.xml` | `SitemapController.Index` | All posts + static pages |

## API routes

| URL | Purpose | Contract |
|---|---|---|
| `/api/search?q={query}&limit=8` | Live search for the command palette and blog toolbar | See [`api-contracts.md`](./api-contracts.md) |

## 404 + error pages

- `/404` — custom 404 in the IDE terminal aesthetic. Title: `404 — file not found`. Body: "The resource `{path}` could not be found." Include a command-palette prompt and links to Home / Blog.
- `/500` — generic error page with same aesthetic. Don't leak stack traces.

## URL rules

- **Lowercase, hyphenated slugs.** `/blog/from-spec-to-stripe`, not `/blog/FromSpecToStripe`.
- **No trailing slashes.** 301 if one arrives.
- **Tag slugs lowercase**, single-word where possible: `xperience`, `saas`, `ai`, `open-source`, `.net`, `commerce`, `security`, `umbraco`, `stripe`, `dx`, `dxp`, `devops`, `migration`, `beginner`, `sustainability`, `tooling`, `testing`.
- **Canonical links** on every page. Especially important for `/blog?tag=…` — canonical should be `/blog` (or `/blog/tag/{slug}` if we decide to promote tag pages later; for v1 the chips filter in-place and the canonical points at `/blog`).
- **Pagination canonicals**: `rel="prev"` / `rel="next"` link tags on paginated archive pages.

## Old URL compatibility

The current site's post URLs use the same `/blog/{slug}` pattern, so no redirects are needed if slugs stay identical. Double-check before launch — any mismatched slug needs a 301 rule.

## Query-string behaviour details

- **Changing `tag` resets `page` to 1** (handled server-side — the tag chip link builds `?tag=x` without `page`).
- **Changing `q` resets `page` to 1** similarly.
- **`page=1`** should not appear in URLs — omit it from generated links so the canonical home-of-page-1 is clean.
- **Invalid `page` values** (non-numeric, out of range) → clamp to last valid page; don't 404.
- **Invalid `tag` values** → render the page with a banner "No posts tagged `{value}`" + the full tag list. Don't 404.
