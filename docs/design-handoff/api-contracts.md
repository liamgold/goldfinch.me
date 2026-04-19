# API Contracts

Only one API is needed for this rebuild: search. Everything else is server-rendered via query strings.

## `GET /api/search`

Live search used by the command palette (`⌘K`) and the blog archive toolbar's search input.

### Request

```
GET /api/search?q={query}&limit=8
```

| Param | Type | Required | Default | Notes |
|---|---|---|---|---|
| `q` | string | ✓ | — | Search query. Trim whitespace. Reject empty with 400. |
| `limit` | int | | 8 | Max results to return. Clamp to `[1, 20]`. |

**Headers:**
- Request: standard. No auth.
- Response: `Content-Type: application/json; charset=utf-8`, `Cache-Control: public, max-age=60`, `Vary: Accept-Encoding`.

### Response shape

```json
{
  "q": "stripe",
  "total": 2,
  "took_ms": 14,
  "results": [
    {
      "kind": "post",
      "slug": "from-spec-to-stripe",
      "title": "From Spec to Stripe: Building a Payment Provider for Xperience by Kentico",
      "summary": "How a Markdown spec and AI tools like ChatGPT and Claude Code…",
      "url": "/blog/from-spec-to-stripe",
      "date": "2025-10-21",
      "tags": ["xperience", "stripe", "ai", "dx"],
      "reading_minutes": 11,
      "highlights": {
        "title": "From Spec to <mark>Stripe</mark>: Building a Payment Provider…",
        "summary": "… building a <mark>Stripe</mark> integration end to end …"
      }
    },
    {
      "kind": "tag",
      "slug": "stripe",
      "label": "Stripe",
      "url": "/blog?tag=stripe",
      "post_count": 2
    }
  ]
}
```

### Field rules

- `kind` is `"post"` or `"tag"`. Keep the door open for `"page"` (about, speaking) in future — clients should ignore unknown kinds gracefully.
- `url` is always a real, shareable URL — the client just sets `href` on an `<a>`. No client-side routing.
- `highlights.*` are optional. If present, values contain safe `<mark>` wrapping the matched substring(s). The wrapping characters are the only HTML allowed — everything else is escaped server-side. Client renders via `innerHTML` but **only trusts `<mark>`**. If you can't guarantee sanitisation server-side, omit `highlights` and the client falls back to plain text.
- `date` is ISO 8601 (`YYYY-MM-DD`), site-local zone. The client formats for display.

### Ranking expectations

1. Exact tag match first (returns the tag item + top 3 posts for that tag).
2. Title match second.
3. Summary match third.
4. Body match fourth (cap at 5 body-only hits).

De-duplicate: if a post already appears via title, don't re-emit it for a body hit.

### Error responses

| Status | Body | When |
|---|---|---|
| 400 | `{"error": "missing_query"}` | `q` empty or whitespace-only |
| 400 | `{"error": "invalid_limit"}` | `limit` out of range |
| 429 | `{"error": "rate_limited", "retry_after": 10}` | Throttling (optional v1) |
| 500 | `{"error": "internal"}` | Search index not reachable etc. |

### Performance

Target: **p95 < 120ms** cold, **< 30ms** warm. The blog has ~13 posts today — an in-memory index is plenty. Lucene/XbK search is overkill but also fine.

Debounce on the client: **180ms** after last keystroke.

### Future extensions (not v1)

- `/api/search/suggest?q=stri` → returns only `suggestion` strings for typeahead
- Filter params (`?tag=ai&from=2025-01-01`) — not needed yet; the blog archive already supports these via its own query strings
