# Goldfinch.me

Personal blog on Xperience by Kentico (.NET 10, ASP.NET Core MVC). Public repo: never name employer-internal tooling in commits, PRs or docs.

## Architecture

- Put services, strategies, framework plug-ins (for example Lucene indexing strategies) and helpers in a feature folder in `Goldfinch.Core`, registered in `Core/ServiceConfiguration.cs`. Keep `Goldfinch.Web` thin: controllers, view components, widgets, CMS modules and wiring only.
- Keep `Program.cs` thin. Wrap multi-line service registrations in an `IServiceCollection` extension under `Goldfinch.Web/Infrastructure`.
- Prefer `IContentRetriever` for content retrieval. Use `IContentQueryExecutor` with `IProgressiveCache` only when `IContentRetriever` can't do the job (offset pagination, cross-type queries, tree-path traversal).

## Kentico gotchas

- Blog tags use the built-in `BlogTags` taxonomy. Never create a custom Tag content type.
- Query tags by GUID only (`WhereContainsTags`). Resolve URL slugs with `IBlogTagService.ResolveTagSlugToGuid` first.
- `IWebPageUrlRetriever` returns virtual paths (`~/blog/foo`). Pass them through `ToAbsolutePath()` before using them as an `href` or in a `Uri`.

## Front end

- The public site uses hand-written CSS in `wwwroot/sitefiles/src/global.css`. Use the CSS variables, never raw colour or size values.
- No inline `style` attributes, unless a value is dynamic per item.
- `--fg-dim` is the darkest allowed colour for visible text (WCAG AA).
- Render icons with the `<icon>` tag helper. Add new icons to `IconTagHelper`, never inline SVG.
- All JS is progressive enhancement: every page must work with JS disabled.
- Read `docs/design-handoff/` before making visual changes.
- Building or changing a Page Builder widget? Use the `create-widget` skill (`.claude/skills/create-widget/SKILL.md`).
