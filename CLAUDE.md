# Claude Code Context

This document provides context for Claude Code when working on the Goldfinch.me project.

## Project Overview

Goldfinch.me is a personal website/blog for Liam Goldfinch featuring articles about Kentico development and .NET technologies. The site is built on Xperience by Kentico (a content management system) with ASP.NET Core MVC.

## Technology Stack

- **Framework:** .NET 10.0
- **CMS:** Xperience by Kentico
- **Frontend (public site):** hand-written CSS + vanilla JS progressive-enhancement modules, bundled via Vite
- **Frontend (admin):** React (Kentico admin customisations only — not loaded on the public site)
- **Storage:** Azure Blob Storage (production), local filesystem (development)
- **Database:** Microsoft SQL Server

### Key NuGet Packages

- `Kentico.Xperience.WebApp` - Core Kentico framework
- `Kentico.Xperience.Admin` - Admin interface
- `Kentico.Xperience.AzureStorage` - Azure blob storage integration
- `Schema.NET` - Structured data/schema markup
- `Sidio.Sitemap.AspNetCore` - XML sitemap generation
- `SixLabors.ImageSharp` - Image processing

### Front-end dependencies (`wwwroot/sitefiles`)

- `vite` — bundler. Three entry points: `global.css`, `main.ts`, `codeblock.ts`. Output hash-busted into `wwwroot/sitefiles/dist/assets/` and loaded by `_Layout.cshtml` / `BlogDetail.cshtml` via `asp-href-include` / `asp-src-include` wildcards.
- `typescript` — used by the TS entrypoints; `build` script runs `tsc && vite build`.
- `highlight.js` — syntax highlighting inside code-block widgets on post-detail pages.

Tailwind was removed during the 2026 redesign — all styling is hand-written in `wwwroot/sitefiles/src/global.css`.

## Project Structure

```
src/
├── Goldfinch.Core/          # Shared business logic & data models
│   ├── BlogPosts/           # Blog post service & models
│   ├── ContentTypes/        # Kentico content type definitions
│   ├── Extensions/          # Shared string helpers (ToAbsolutePath)
│   ├── PublicSpeaking/      # Speaking engagement functionality
│   ├── SEO/                 # SEO models (breadcrumbs, meta fields, constants)
│   └── Sitemap/             # Sitemap generation logic
├── Goldfinch.Admin/         # Kentico admin customisations
└── Goldfinch.Web/           # Main web application
    ├── Components/
    │   ├── ViewComponents/  # Header, Footer, SEO, Canonical, PageTitle, LatestBlogPosts
    │   └── Widgets/         # Page builder widgets (Image, Video, YouTubeVideo, CodeBlock)
    ├── Extensions/          # Razor/tag-helper extensions
    ├── Features/            # Feature-based organisation
    │   ├── BlogDetail/      # Post detail page
    │   ├── BlogList/        # Archive — query-string filter + pagination
    │   ├── ErrorPage/       # Custom 404 / 500 page
    │   ├── Home/            # Home page (hero + Now + featured + recent)
    │   ├── InnerPage/       # Generic content page (e.g. /about)
    │   ├── PublicSpeakingPage/
    │   ├── SEO/             # RSSFeed + Sitemap controllers
    │   └── Search/          # /api/search JSON endpoint (stub)
    ├── Infrastructure/      # Cross-cutting concerns (storage, etc.)
    ├── TagHelpers/          # IconTagHelper, ImageAssetTagHelper, PageBuilderModeTagHelper
    ├── Views/Shared/
    │   ├── _Layout.cshtml
    │   └── PartialViews/    # Footer, MobileDrawer, BlogPost card, TalkCardBody
    └── wwwroot/sitefiles/   # Vite project — CSS + JS source
        ├── src/
        │   ├── global.css   # Design tokens + every component style (one file)
        │   ├── main.ts      # Imports every JS enhancement module
        │   ├── scripts/     # mobile-drawer, command-palette, search,
        │   │                #   toc-scrollspy, header-scroll
        │   └── codeblock/   # highlight.js integration (lazy-loaded on post detail)
        └── dist/assets/     # Built output, loaded by Razor
```

## Design System

The public site uses a dark-only, terminal/IDE-inspired aesthetic. The full design handoff lives in `docs/design-handoff/` — read it alongside the code when making visual changes.

### Tokens

All colours, typography, spacing tokens live as CSS custom properties at the top of `wwwroot/sitefiles/src/global.css`. Use the variables, never the raw values.

- `--bg`, `--bg-1`, `--bg-2`, `--bg-3` — background layers (darkest → elevated)
- `--fg`, `--fg-muted`, `--fg-dim` — text colours (primary → decoration-only); `--fg-dim` is the WCAG AA floor — don't go darker for any visible text
- `--accent` / `--accent-hot` / `--accent-soft` — amber brand colour (only brand colour; cyan/green/magenta are semantic echoes, use sparingly)
- `--font-mono` — JetBrains Mono (headings, labels, code, chrome)
- `--font-sans` — Geist Variable (body prose)

Both fonts are **self-hosted** as variable woff2s in `wwwroot/fonts/` and preloaded from `_Layout.cshtml`. No Google Fonts request on any page. JetBrains Mono ships as a Latin-only subset (31 KB) — non-Latin codepoints fall back to Consolas/Menlo/monospace.
- `--radius`, `--radius-sm`, `--radius-lg` — corner radii
- `--dur`, `--dur-fast`, `--ease` — motion timing

### Responsive

Single breakpoint: **768px**. Mobile overrides live in a single `@media (max-width: 768px)` block at the bottom of `global.css`. Desktop-first — mobile rules override desktop.

### Icons

Inline SVG icons are rendered via the `<icon>` tag helper — never inline raw SVG or repeat the switch-statement-of-paths pattern in multiple views.

```html
<icon name="book" size="15" />
<icon name="arrowR" />   @* size defaults to 14 *@
```

The icon set lives in `Goldfinch.Web.TagHelpers.IconTagHelper`. Add new icons to the `Paths` dictionary there rather than putting SVG elsewhere.

## Client JS Modules

All client JS is vanilla, progressive-enhancement only — every page works with JS disabled. Modules are bundled into a single `main.ts` via Vite.

| Module | Purpose |
|---|---|
| `header-scroll.js` | Toggles `data-scrolled` on `<header>` for the blur/border transition |
| `mobile-drawer.js` | Hamburger → slide-in drawer, Esc/backdrop close, focus trap |
| `command-palette.js` | ⌘K / Ctrl+K palette with ↑↓↵ keyboard nav; hits `/api/search` |
| `search.js` | Debounced live search for the blog toolbar input |
| `toc-scrollspy.js` | Highlights active TOC entry + updates reading-progress bar |

Each module is a self-contained IIFE that no-ops if its expected DOM isn't present — safe to ship on every page.

## Architecture & Patterns

### Content Management

- **Kentico Content Types:** Defined in `Goldfinch.Core/ContentTypes/`
- **Service Pattern:** Data access is abstracted through service classes (e.g., `BlogPostService`, `ErrorPageService`)
- **Progressive Caching:** Complex services use Kentico's `IProgressiveCache` for performance; simple retrieval uses `IContentRetriever` which caches automatically
- **CI/CD:** Uses Kentico's Continuous Integration system — objects stored as serialised files in `App_Data/CIRepository/`

### Image Handling

The project uses Xperience by Kentico's native **Image Variant** functionality (Standard Media Dimensions) for responsive image delivery:

- **Standard Media Dimensions:** Defined in `App_Data/CIRepository/@global/cms.standardmediadimensions/`
- **Current Variants:**
  - `480Width` - 480px wide (mobile)
  - `800Width` - 800px wide (tablet)
  - `1000Width` - 1000px wide (desktop)
  - `SocialMediaCard` - 1200x630px (Open Graph)
- **Custom Tag Helper:** `ImageAssetTagHelper` in `Goldfinch.Web/TagHelpers/`
  - Automatically generates responsive `srcset` and `sizes` attributes
  - Prevents upscaling by filtering out variants larger than the original image
  - Caps `sizes` at the original image width to prevent browser upscaling
  - Always includes the original image URL in the srcset
- **Migration Note:** Previously used `XperienceCommunity.ImageProcessing` (now removed in favor of native functionality)

Example usage in views:
```html
<img gf-image-asset="@Model.ContentItemAsset" alt="@Model.Description" loading="lazy" />
```

The tag helper automatically generates:
- `src` attribute with the original image URL (fallback)
- `srcset` attribute with all applicable variants (skips any larger than original)
- `sizes` attribute: `(max-width: 600px) 480px, (max-width: 1000px) 800px, {maxSize}px`
  - `maxSize` is capped at min(1000px, original image width)

Examples:
- **345px image**: srcset contains only `345w`, sizes capped at `345px` (no upscaling)
- **1500px image**: srcset contains `480w, 800w, 1000w, 1500w`, sizes capped at `1000px` (optimized variants used, never serves full 1500px)

### Web Layer

- **Feature Folders:** Organized by feature rather than technical layer
- **View Components:** Used extensively for reusable UI components
- **Page Builder:** Kentico's page builder with custom widgets for content editing
- **Controllers:** MVC controllers handle routing and orchestrate services/view models

### Routes

Blog archive pagination is **query-string only**: `/blog?page=2&tag=saas&q=stripe&view=list`. Legacy path-based URLs (`/blog/2`) are 301-redirected to the canonical form by `BlogListController.PagedRedirect`. `<link rel="prev">` / `<link rel="next">` are emitted by `CanonicalViewComponent` from `ViewData[SEOConstants.PREVIOUS_URL_KEY]` / `NEXT_URL_KEY` — controllers populate those when they want pagination hints in the head.

RSS lives at `/rss.xml` (canonical) with `/rss` kept as an alias for back-compat.

### URL normalisation

Kentico's `IWebPageUrlRetriever` returns paths in virtual form (`~/blog/foo`). Always pass them through `Goldfinch.Core.Extensions.StringExtensions.ToAbsolutePath()` before using as an `href` or inside a `new Uri(...)`. Sitemap, breadcrumbs, RSS, `/api/search`, and the blog schema all use this helper.

### Data Access Patterns

**Prefer `IContentRetriever`** for page and content retrieval — it handles channel context, preview mode, and caching automatically.

In view components and controllers where the page context is already set, use `RetrieveCurrentPage`:
```csharp
var home = await _contentRetriever.RetrieveCurrentPage<Core.ContentTypes.Home>();
```

> **Note:** If the content type name matches the enclosing feature namespace (e.g. `Home` inside `Goldfinch.Web.Features.Home`), use the partial qualification `Core.ContentTypes.Home` to avoid the compiler treating it as a namespace.

For queries where the context isn't set yet (e.g. finding an error page by code before routing), use `RetrievePages` with a where clause and explicit cache settings:
```csharp
var errorPages = await _contentRetriever.RetrievePages<ErrorPage>(
    RetrievePagesParameters.Default,
    query => query.Where(x => x.WhereEquals(nameof(ErrorPage.ErrorCode), errorCode)),
    cacheSettings: new RetrievalCacheSettings(
        cacheItemNameSuffix: $"{nameof(ErrorPage)}|code|{errorCode}",
        cacheExpiration: TimeSpan.FromMinutes(30)));
```

For widgets that need to fetch a content hub item by GUID (e.g. a selected asset), use `RetrieveContentByGuids` directly in the ViewComponent — no separate service needed:
```csharp
var results = await _contentRetriever.RetrieveContentByGuids<MediaAssetContent>(
    new List<Guid> { asset.Identifier },
    new RetrieveContentParameters { LinkedItemsMaxLevel = 1 });
var item = results.FirstOrDefault();
```

The manual `IContentQueryExecutor` + `IProgressiveCache` approach is only used in services with complex requirements not supported by `IContentRetriever` (e.g. pagination with offsets, cross-content-type queries, tree path traversal). All calling code must handle nullable returns from `FirstOrDefault()` appropriately.

## Code Conventions

### Nullable Reference Types

- **Enabled project-wide** as of 2025 (see `Directory.Build.props`)
- Use `required` modifier for essential properties that must be set during initialization
- Use default values (e.g., `string.Empty`) for optional properties
- Service methods returning `FirstOrDefault()` should have nullable return types

### View Models

```csharp
public class BlogPostViewModel
{
    public required string Title { get; set; }        // Essential - required
    public required string Url { get; set; }          // Essential - required
    public string Schema { get; set; } = string.Empty; // Optional - default value
}
```

### Naming Conventions

- **Services:** `{EntityName}Service` (e.g., `BlogPostService`)
- **ViewModels:** `{Feature}ViewModel` (e.g., `BlogPostViewModel`)
- **View Components:** `{Name}ViewComponent` (e.g., `HeaderViewComponent`)
- **Widgets:** `{Name}Widget` (e.g., `ImageWidget`)

### Project Settings

- **Target Framework:** net10.0
- **Nullable:** enable
- **ImplicitUsings:** disable (explicit usings required)
- **WarningsAsErrors:** nullable

### Styling — no inline styles

Component styles live in `wwwroot/sitefiles/src/global.css`. Don't sprinkle `style="…"` attributes in Razor views — add a class instead. The one legitimate exception is when a value is genuinely dynamic per-iteration and can't be expressed as a class variant.

### No Tailwind

The 2026 redesign removed Tailwind and all its classes. If a Razor view still contains `bg-zinc-*`, `text-yellow-*`, `rounded-lg`, `grid-cols-*`, etc. it's leftover from before the rewrite and should be migrated to the hand-written CSS.

## Common Tasks

### Adding a New Content Type

1. Define in Kentico admin UI
2. Create strongly-typed class in `Goldfinch.Core/ContentTypes/`
3. For simple retrieval (single page by context, or basic query), use `IContentRetriever` directly in the controller or view component — no separate service class needed.
4. For complex retrieval (pagination, custom ordering, cross-content queries), create a service and interface in an appropriate folder (e.g., `BlogPosts/IBlogPostService.cs` + `BlogPosts/BlogPostService.cs`). Inject Kentico services directly — no base class. Common dependencies:
   - `IContentQueryExecutor` — query execution
   - `IWebsiteChannelContext` — channel name and preview flag
   - `IProgressiveCache` — caching
   - `IWebPageUrlRetriever` — URL resolution (only when needed)
5. Register the service via its interface in `ServiceConfiguration.cs` (e.g., `AddSingleton<IBlogPostService, BlogPostService>()`)

### Adding a New Widget

Every widget follows the same 4-file pattern in `Goldfinch.Web/Components/Widgets/{WidgetName}/`:

```
{WidgetName}WidgetViewComponent.cs   # Widget registration + data wiring
{WidgetName}WidgetProperties.cs      # Admin UI form fields (IWidgetProperties)
{WidgetName}WidgetViewModel.cs       # Data passed to the view
{WidgetName}Widget.cshtml            # Razor view
```

If the widget has dropdown fields, also create `{WidgetName}WidgetEnums.cs` in the same folder.

#### ViewComponent

- `[assembly: RegisterWidget(...)]` goes **above** the namespace declaration
- Every ViewComponent must declare two public constants:
  ```csharp
  public const string IDENTIFIER = "Goldfinch.WidgetName";
  public const string DISPLAY_NAME = "Widget Display Name";
  ```
  Use both in the `RegisterWidget` attribute — never hardcode the name as a string.
- **`IconClass`** must use a constant from `KenticoIcons` (`Goldfinch.Web.Components.Shared`). Never use raw icon strings. Common examples: `KenticoIcons.PICTURE`, `KenticoIcons.MEDIA_PLAYER`, `KenticoIcons.PARAGRAPH`, `KenticoIcons.RECTANGLE_PARAGRAPH`.
- **Always** guard against missing/empty properties and return an edit-mode warning:
  ```csharp
  if (properties == null || string.IsNullOrWhiteSpace(properties.RequiredField))
  {
      return _pageBuilderDataContext.IsEditMode()
          ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Descriptive hint for the editor.")
          : Content(string.Empty);
  }
  ```
  `WidgetPlaceholder` is in `Goldfinch.Web.Components.Widgets.Base`. `IsEditMode()` is an extension on `IPageBuilderDataContextRetriever` in `Goldfinch.Web.Extensions` — inject `IPageBuilderDataContextRetriever` and add `using Goldfinch.Web.Extensions;`.

  For widgets that select content items, add a second guard after retrieval:
  ```csharp
  if (retrievedItems == null || !retrievedItems.Any())
  {
      return _pageBuilderDataContext.IsEditMode()
          ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "The selected asset could not be retrieved. Ensure it is published.")
          : Content(string.Empty);
  }
  ```

Full example:
```csharp
using Goldfinch.Web.Components.Shared;
using Goldfinch.Web.Components.Widgets.Base;
using Goldfinch.Web.Components.Widgets.MyWidget;
using Goldfinch.Web.Extensions;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWidget(
    identifier: MyWidgetViewComponent.IDENTIFIER,
    viewComponentType: typeof(MyWidgetViewComponent),
    name: MyWidgetViewComponent.DISPLAY_NAME,
    propertiesType: typeof(MyWidgetProperties),
    Description = "Describes what the widget renders.",
    IconClass = KenticoIcons.PICTURE)]

namespace Goldfinch.Web.Components.Widgets.MyWidget;

public class MyWidgetViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.MyWidget";
    public const string DISPLAY_NAME = "My Widget";
    private const string ViewName = "~/Components/Widgets/MyWidget/MyWidget.cshtml";

    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public MyWidgetViewComponent(IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public IViewComponentResult Invoke(MyWidgetProperties properties)
    {
        if (properties == null || string.IsNullOrWhiteSpace(properties.Heading))
        {
            return _pageBuilderDataContext.IsEditMode()
                ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Add a heading in the widget properties.")
                : Content(string.Empty);
        }

        var viewModel = new MyWidgetViewModel { Heading = properties.Heading };
        return View(ViewName, viewModel);
    }
}
```

#### Properties (IWidgetProperties)

- **Always** add at least one `[FormCategory]` on the class, even for a single group — this sets a consistent pattern and makes it easy to add more groups later:
  ```csharp
  [FormCategory(Label = "Content", Order = 0, Collapsible = true, IsCollapsed = false)]
  [FormCategory(Label = "Appearance", Order = 10, Collapsible = true, IsCollapsed = false)]
  public class MyWidgetProperties : IWidgetProperties { ... }
  ```
  Always `IsCollapsed = false`. Space category `Order` values by 10; field `Order` values are sequential integers within each group.

- **Dropdowns** must use `DropdownEnumOptionProvider<TEnum>` — never use the `Options = "value;Label\r\n..."` string approach:
  ```csharp
  [DropDownComponent(Label = "Theme", Order = 1,
      DataProviderType = typeof(DropdownEnumOptionProvider<MyWidgetTheme>))]
  public string? Theme { get; set; } = "blue";
  ```
  The stored value is the kebab-case of the enum member name (`Blue` → `"blue"`, `DarkBlue` → `"dark-blue"`). Default values must be the kebab-case string. Define enums in `{WidgetName}WidgetEnums.cs`. Use `[Description("Label")]` on enum members to customise the displayed label.

  **Exception:** when stored values must match an external identifier (e.g. highlight.js language classes), name enum members as single words to avoid hyphens (`Csharp` → `"csharp"`, not `CSharp` → `"c-sharp"`).

#### Key namespaces for widgets

| Using | Provides |
|---|---|
| `Goldfinch.Web.Components.Shared` | `KenticoIcons` |
| `Goldfinch.Web.Components.Widgets.Base` | `WidgetPlaceholder`, `DropdownEnumOptionProvider<T>` |
| `Goldfinch.Web.Extensions` | `IsEditMode()` extension on `IPageBuilderDataContextRetriever` |
| `Kentico.Content.Web.Mvc` | `IContentRetriever`, `RetrieveContentParameters` |
| `Kentico.PageBuilder.Web.Mvc` | `IWidgetProperties`, `RegisterWidget`, `IPageBuilderDataContextRetriever` |
| `Kentico.Xperience.Admin.Base.FormAnnotations` | `TextInputComponent`, `DropDownComponent`, `CheckBoxComponent`, `TextAreaComponent`, `ContentItemSelectorComponent`, `FormCategory`, etc. |

### Running the Project

```bash
# Build the front-end bundle once (or when CSS / JS changes)
cd src/Goldfinch.Web/wwwroot/sitefiles
npm install
npm run build

# First-time setup — restore CI objects
cd src/Goldfinch.Web
dotnet run --kxp-ci-restore

# Normal development
dotnet run
```

**Local URL:** https://localhost:52623

**Admin Login:** admin / Test123! (local only)

## Testing & Build

- Build command: `dotnet build`
- Front-end build: `cd src/Goldfinch.Web/wwwroot/sitefiles && npm run build`
- E2E tests: Playwright tests in `tests/Goldfinch.Tests.E2E/`
- Test command: `dotnet test tests/Goldfinch.Tests.E2E/Goldfinch.Tests.E2E.csproj`

## Outstanding TODOs (from the 2026 redesign)

Search `TODO` in the repo for the full list. The notable ones:

- **Tag content type** — tag filter chips on `/blog` currently link to `?tag=slug` but the slug isn't matched against any real taxonomy (returns empty). Needs a `Tag` reference field on `BlogPost`.
- **`/api/search`** — stubs title + summary match only. Adding body search, ranking, `<mark>` highlights, tag results, and the documented cache headers is the full v1 per `docs/design-handoff/api-contracts.md`.
- **Reading-minutes** on blog posts — currently estimated from summary length; should be computed from body.
- **TOC rail** on post detail — the scrollspy JS hydrates whenever `.toc-rail a[href^="#"]` + article H2/H3 ids both exist, but we don't emit those yet because bodies are Page Builder widgets rather than a rich-text field.
- **"Now" panel** on home + stack/timeline on About — rows are hard-coded in the Razor views; move to CMS fields or a JSON file in `App_Data` when the content changes often enough to matter.
- **Copy-link share button** on post detail — deferred (needs a tiny inline `navigator.clipboard` script).
- **Featured flag** on `BlogPost` — home currently treats the newest post as "featured". A real `Featured: bool` flag would let an editor pin any post.

## Deployment

- **Production:** https://www.goldfinch.me
- **CI/CD:** GitHub Actions (`.github/workflows/deploy.yml`)
- Uses Azure Blob Storage for media assets in production
- Deploys to Azure Web App using publish profile

## Important Notes

- **No database backups in repo** - use CI files to restore Kentico objects
- **Connection strings** stored in `connectionstrings.json` (gitignored)
- **Do not commit** `.claude/` directory (in .gitignore)
- **Admin credentials** in README are for local development only
- **Contributions** not expected - personal project
- **Design handoff** lives in `docs/design-handoff/` — read it before making visual changes. `mock.html` is the bundled React reference, `reference/styles.css` is the token base, the `*.md` files document routes / content types / components / a11y / progressive enhancement.

## Git Workflow

- Main branch: `main`
- Feature branches: `feature/{feature-name}`

## Contact & Resources

- **Author:** Liam Goldfinch
- **Website:** https://www.goldfinch.me
- **License:** MIT
- **Kentico Docs:** https://docs.kentico.com/x/6wocCQ
