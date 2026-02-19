# Claude Code Context

This document provides context for Claude Code when working on the Goldfinch.me project.

## Project Overview

Goldfinch.me is a personal website/blog for Liam Goldfinch featuring articles about Kentico development and .NET technologies. The site is built on Xperience by Kentico (a content management system) with ASP.NET Core MVC.

## Technology Stack

- **Framework:** .NET 10.0
- **CMS:** Xperience by Kentico
- **Frontend:** Tailwind CSS, Vite, React (admin customizations)
- **Storage:** Azure Blob Storage (production), local filesystem (development)
- **Database:** Microsoft SQL Server

### Key NuGet Packages

- `Kentico.Xperience.WebApp` - Core Kentico framework
- `Kentico.Xperience.Admin` - Admin interface
- `Kentico.Xperience.AzureStorage` - Azure blob storage integration
- `Schema.NET` - Structured data/schema markup
- `Sidio.Sitemap.AspNetCore` - XML sitemap generation
- `SixLabors.ImageSharp` - Image processing

## Project Structure

```
src/
├── Goldfinch.Core/          # Shared business logic & data models
│   ├── BlogPosts/           # Blog post service & models
│   ├── ContentTypes/        # Kentico content type definitions
│   ├── PublicSpeaking/      # Speaking engagement functionality
│   ├── SEO/                 # SEO models (breadcrumbs, meta fields)
│   └── Sitemap/             # Sitemap generation logic
├── Goldfinch.Admin/         # Kentico admin customizations
└── Goldfinch.Web/           # Main web application
    ├── Components/
    │   ├── ViewComponents/  # Reusable view components (Header, SEO, etc.)
    │   └── Widgets/         # Page builder widgets (Image, Video, CodeBlock, etc.)
    ├── Features/            # Feature-based organization
    │   ├── BlogDetail/      # Blog post detail pages
    │   ├── BlogList/        # Blog listing/pagination
    │   ├── ErrorPage/       # Error page handling (404, 500)
    │   └── PublicSpeakingPage/
    └── Infrastructure/      # Cross-cutting concerns (storage, etc.)
```

## Architecture & Patterns

### Content Management

- **Kentico Content Types:** Defined in `Goldfinch.Core/ContentTypes/`
- **Service Pattern:** Data access is abstracted through service classes (e.g., `BlogPostService`, `ErrorPageService`)
- **Progressive Caching:** Complex services use Kentico's `IProgressiveCache` for performance; simple retrieval uses `IContentRetriever` which caches automatically
- **CI/CD:** Uses Kentico's Continuous Integration system - objects stored as serialized files in `App_Data/CIRepository/`

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
# First time setup - restore CI objects
cd src/Goldfinch.Web
dotnet run --kxp-ci-restore

# Normal development
dotnet run
```

**Local URL:** https://localhost:52623

**Admin Login:** admin / Test123! (local only)

## Testing & Build

- Build command: `dotnet build`
- E2E tests: Playwright tests in `tests/Goldfinch.Tests.E2E/`
- Test command: `dotnet test tests/Goldfinch.Tests.E2E/Goldfinch.Tests.E2E.csproj`

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

## Git Workflow

- Main branch: `main`
- Feature branches: `feature/{feature-name}`

## Contact & Resources

- **Author:** Liam Goldfinch
- **Website:** https://www.goldfinch.me
- **License:** MIT
- **Kentico Docs:** https://docs.kentico.com/x/6wocCQ
