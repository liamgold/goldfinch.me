# Content Types — Xperience by Kentico

All content types below are **reusable content items** unless noted. Field names are suggested; adjust to match any existing convention in the Kentico instance. Guids are omitted — let Xperience assign them.

> **Before creating these:** diff against the existing instance. The current goldfinch.me already has content types for posts, tags, and talks. This doc is the target state — use it as a checklist, not an unconditional create script.

## `BlogPost`

| Field | Type | Required | Notes |
|---|---|---|---|
| `Title` | Text | ✓ | Used in `<h1>`, meta title, OG title |
| `Slug` | Text | ✓ | URL segment. Lowercase, hyphenated. Unique. |
| `PublishedOn` | DateTime | ✓ | Drives ordering, archive grouping, RSS pubDate |
| `Summary` | Long text | ✓ | 1–2 sentences. Shown on cards + meta description |
| `Body` | Rich text | ✓ | Full post markdown/html. Render with the prose styles in `styles.css` |
| `Tags` | Reference (many → `Tag`) | ✓ | 1+ tags |
| `ReadingMinutes` | Integer | | Computed at save time (≈200 wpm). Optional — UI shows "~N min" |
| `Hero` | Asset / Reference | | Optional hero image. If absent, post page shows no hero |
| `Featured` | Boolean | | Only one post should be `true` at a time. The home page pulls the most recent `Featured: true`, falls back to newest if none |
| `OpenGraphImage` | Asset | | Optional. Defaults to site-wide OG |
| `CanonicalUrl` | Text | | Optional. For syndicated posts |

**Validation:**
- `Slug` must match `^[a-z0-9]+(-[a-z0-9]+)*$`.
- `Summary` max 240 chars (soft cap; UI truncates at ~200).

## `Tag`

| Field | Type | Required | Notes |
|---|---|---|---|
| `Label` | Text | ✓ | Display label, e.g. "Kentico SaaS" |
| `Slug` | Text | ✓ | URL segment, e.g. `saas`. Lowercase. |
| `Description` | Long text | | Optional. Reserved for future tag detail pages |
| `Color` | Text | | Optional. Hex or CSS custom-property reference. Unused in v1. |

Keep the seeded tag list minimal — only create tags when a post uses them. Current set: `xperience, saas, umbraco, ai, open-source, .net, commerce, security, stripe, dx, dxp, devops, migration, beginner, sustainability, tooling, testing`.

## `Talk` (Speaking entry)

| Field | Type | Required | Notes |
|---|---|---|---|
| `Title` | Text | ✓ | |
| `Event` | Text | ✓ | "Kentico Connection 2024" |
| `Location` | Text | ✓ | "Prague, CZ" or "Online" |
| `EventDate` | DateTime | ✓ | Drives year grouping + sort order |
| `Summary` | Long text | ✓ | 1–2 sentences |
| `Type` | Choice (`conference`, `webinar`, `meetup`, `podcast`) | ✓ | Drives icon on the card |
| `HasRecording` | Boolean | | If true, shows the `play` badge |
| `RecordingUrl` | Text (URL) | | If `HasRecording`, link the card |
| `SlidesUrl` | Text (URL) | | Optional |

## `Author` (Razor partial content, v1)

For v1 the author block is a Razor partial — no content type. Fields to parameterise if/when we do promote it:
- `Name`, `Role`, `Bio`, `AvatarUrl`, `Socials[]` (kind + url + handle)

## `SitePage` (for About)

If About needs to be CMS-editable:

| Field | Type | Required |
|---|---|---|
| `Title` | Text | ✓ |
| `Body` | Rich text | ✓ |
| `Sections` | Repeated (heading + body) | — optional, for structured pages |

Otherwise keep About as a static Razor view with the copy hand-written.

## Indexes Kentico should build

- `BlogPost.Slug` — unique, for direct URL lookup
- `BlogPost.PublishedOn` desc — archive queries
- `BlogPost.Featured + PublishedOn desc` — home page featured lookup
- `BlogPost` × `Tag` join — tag filter
- Search index (full-text): `Title`, `Summary`, body `Body`, `Tags.Label` → feeds `/api/search`
