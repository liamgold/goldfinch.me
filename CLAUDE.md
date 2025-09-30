# Claude Code Context

This document provides context for Claude Code when working on the Goldfinch.me project.

## Project Overview

Goldfinch.me is a personal website/blog for Liam Goldfinch featuring articles about Kentico development and .NET technologies. The site is built on Xperience by Kentico (a content management system) with ASP.NET Core MVC.

## Technology Stack

- **Framework:** .NET 9.0
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
│   ├── BlogPosts/           # Blog post repository & models
│   ├── ContentTypes/        # Kentico content type definitions
│   ├── MediaAssets/         # Media/asset management
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
- **Repository Pattern:** Data access is abstracted through repository classes (e.g., `BlogPostRepository`, `ErrorPageRepository`)
- **Progressive Caching:** All repositories use Kentico's `IProgressiveCache` for performance
- **CI/CD:** Uses Kentico's Continuous Integration system - objects stored as serialized files in `App_Data/CIRepository/`

### Web Layer

- **Feature Folders:** Organized by feature rather than technical layer
- **View Components:** Used extensively for reusable UI components
- **Page Builder:** Kentico's page builder with custom widgets for content editing
- **Controllers:** MVC controllers handle routing and orchestrate repositories/view models

### Data Access Patterns

All repository methods that use `FirstOrDefault()` return nullable types:

```csharp
public async Task<BlogPost?> GetBlogPost(int webPageItemID)
{
    return await ProgressiveCache.LoadAsync(async (cs) =>
    {
        var pages = await Executor.GetMappedWebPageResult<BlogPost>(queryBuilder);
        return pages.FirstOrDefault(); // Can be null
    }, cacheSettings);
}
```

Calling code must handle null returns appropriately.

## Code Conventions

### Nullable Reference Types

- **Enabled project-wide** as of 2025 (see `Directory.Build.props`)
- Use `required` modifier for essential properties that must be set during initialization
- Use default values (e.g., `string.Empty`) for optional properties
- Repository methods returning `FirstOrDefault()` should have nullable return types

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

- **Repositories:** `{EntityName}Repository` (e.g., `BlogPostRepository`)
- **ViewModels:** `{Feature}ViewModel` (e.g., `BlogPostViewModel`)
- **View Components:** `{Name}ViewComponent` (e.g., `HeaderViewComponent`)
- **Widgets:** `{Name}Widget` (e.g., `ImageWidget`)

### Project Settings

- **Target Framework:** net9.0
- **Nullable:** enable
- **ImplicitUsings:** disable (explicit usings required)
- **WarningsAsErrors:** nullable

## Common Tasks

### Adding a New Content Type

1. Define in Kentico admin UI
2. Create strongly-typed class in `Goldfinch.Core/ContentTypes/`
3. Create repository in appropriate folder (e.g., `BlogPosts/BlogPostRepository.cs`)
4. Inherit from `WebPageRepository` for web page types

### Adding a New Widget

1. Create folder in `Goldfinch.Web/Components/Widgets/{WidgetName}/`
2. Create `{WidgetName}WidgetProperties.cs` (IWidgetProperties)
3. Create `{WidgetName}WidgetViewModel.cs`
4. Create `{WidgetName}WidgetViewComponent.cs` (ViewComponent)
5. Create `{WidgetName}Widget.cshtml` (view)
6. Register with `[RegisterWidget]` attribute

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
- No automated tests currently in the project
- Manual testing via local environment

## Deployment

- **Production:** https://www.goldfinch.me
- Uses Azure Blob Storage for media assets in production
- Continuous Deployment via Kentico CI/CD system

## Important Notes

- **No database backups in repo** - use CI files to restore Kentico objects
- **Connection strings** stored in `connectionstrings.json` (gitignored)
- **Do not commit** `.claude/` directory (in .gitignore)
- **Admin credentials** in README are for local development only
- **Contributions** not expected - personal project

## Git Workflow

- Main branch: `main`
- Feature branches: `feature/{feature-name}`
- Recent work: Enabled nullable reference types across entire project

## Contact & Resources

- **Author:** Liam Goldfinch
- **Website:** https://www.goldfinch.me
- **License:** MIT
- **Kentico Docs:** https://docs.kentico.com/x/6wocCQ
