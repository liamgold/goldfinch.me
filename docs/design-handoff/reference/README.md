# Reference Assets

Drop these into the Razor project's `wwwroot/`:

| From | To |
|---|---|
| `styles.css` | `wwwroot/styles.css` |
| `fonts/geist-variable.woff2` | `wwwroot/fonts/geist-variable.woff2` |
| `favicon-32x32.png` | `wwwroot/favicon-32x32.png` |

The `@font-face` rule in `styles.css` references `fonts/geist-variable.woff2` with a relative path — that works as long as `styles.css` lives at the same depth as the `fonts/` folder. If you move either, update the `url()` in the font face rule.

## JetBrains Mono

Not bundled — loaded from Google Fonts. Add this to `<head>`:

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&display=swap">
```

## Verifying a clean drop

After copying:

1. Load a page with just `<link rel="stylesheet" href="/styles.css">` and `<h1 class="mono">hello</h1>`.
2. Confirm:
   - Page background is `#0c0d0f`
   - Heading renders in JetBrains Mono (monospace)
   - `body` renders in Geist (sans-serif, not Helvetica)
3. Open devtools → Computed → check `--accent` resolves to `#f5a524`.

If Geist falls back to a system sans, the font path is wrong.
