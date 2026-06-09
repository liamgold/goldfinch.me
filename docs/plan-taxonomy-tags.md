# Plan: Taxonomy Tag Filtering for BlogPost (Issue #201)

## Goal

Tag filter chips on `/blog` currently link to `?tag=slug` but always return empty.
Wire up Xperience by Kentico's built-in Taxonomies feature so filtering is real.

---

## Phase 1 — Admin Setup (manual steps)

1. Create a `BlogTags` taxonomy in the Kentico admin (`/admin/taxonomies`)
   - Name (code name): `BlogTags`
   - Title: `Blog Tags`
2. Add tags within the taxonomy — one per topic. Suggested set (from the existing hardcoded chips):

   | Code name (slug) | Display title  |
   |------------------|----------------|
   | `xperience`      | Xperience      |
   | `saas`           | Kentico SaaS   |
   | `umbraco`        | Umbraco        |
   | `ai`             | AI tooling     |
   | `open-source`    | Open source    |
   | `dotnet`         | .NET           |
   | `commerce`       | Commerce       |
   | `security`       | Security       |

3. Add a `BlogPostTags` taxonomy field to the `BlogPost` content type
   - Field type: **Tags**
   - Column name: `BlogPostTags`
   - Linked taxonomy: `BlogTags`
4. Tag all existing blog posts in the Content hub
5. Run `dotnet run --kxp-ci-store` from `src/Goldfinch.Web` to serialise to the CI repository
   - New: `App_Data/CIRepository/@global/cms.taxonomy/blogtags.xml`
   - New: `App_Data/CIRepository/@global/cms.tag/*.xml` (one per tag)
   - Updated: `App_Data/CIRepository/@global/cms.contenttype/goldfinch.blogpost.xml`
6. Commit the CI repository files

---

## Phase 2 — Code Generation

Run the Kentico code generator to update the `BlogPost` generated class:

```bash
dotnet run --kxp-codegen -- --type ContentTypes --namespace Goldfinch.Core.ContentTypes
```

Expected result — new property added to `BlogPost.generated.cs`:

```csharp
public IEnumerable<TagReference> BlogPostTags { get; set; }
```

---

## Phase 3 — New `BlogTagViewModel`

**New file:** `src/Goldfinch.Web/Features/BlogList/BlogTagViewModel.cs`

```csharp
namespace Goldfinch.Web.Features.BlogList;

public sealed record BlogTagViewModel(
    string Slug,   // Tag code name — appears in ?tag= query string
    string Label,  // Display title
    int Count      // Number of published posts with this tag
);
```

---

## Phase 4 — New `IBlogTagService` / `BlogTagService`

**New file:** `src/Goldfinch.Core/BlogPosts/IBlogTagService.cs`

```csharp
using CMS.ContentEngine;

public interface IBlogTagService
{
    Task<Guid?> ResolveTagSlugToGuid(string slug, CancellationToken ct = default);

    Task<IReadOnlyList<(Tag Tag, int PostCount)>> GetTagsWithPostCounts(
        string languageName, CancellationToken ct = default);

    Task<IEnumerable<Tag>> GetTagsByGuids(
        IEnumerable<Guid> tagGuids, string languageName, CancellationToken ct = default);
}
```

**New file:** `src/Goldfinch.Core/BlogPosts/BlogTagService.cs`

Key implementation notes:
- `BLOG_TAGS_TAXONOMY_NAME = "BlogTags"` — must match the taxonomy code name from admin
- `ResolveTagSlugToGuid` — calls `ITaxonomyRetriever.RetrieveTaxonomy`, finds tag by `t.Name == slug` (case-insensitive). Cache 1440 min, dependency `TaxonomyInfo.OBJECT_TYPE|all`
- `GetTagsWithPostCounts` — retrieves taxonomy, fetches all posts, counts per tag in memory. Cache 60 min, dependencies: `TaxonomyInfo.OBJECT_TYPE|all` + `cms.contentitemtag|all`. Only returns tags with `count > 0`.
- `GetTagsByGuids` — calls `ITaxonomyRetriever.RetrieveTags(guids, languageName)` for per-post tag label resolution

**Register in** `src/Goldfinch.Core/ServiceConfiguration.cs`:

```csharp
.AddSingleton<IBlogTagService, BlogTagService>()
```

---

## Phase 5 — Extend `IBlogPostService` / `BlogPostService`

**Add to** `IBlogPostService`:

```csharp
Task<IEnumerable<BlogPost>> GetBlogPostsByTag(Guid tagGuid);
```

**Implement in** `BlogPostService` using `WhereContainsTags`:

```csharp
public async Task<IEnumerable<BlogPost>> GetBlogPostsByTag(Guid tagGuid)
{
    return await _progressiveCache.LoadAsync(async cs =>
    {
        cs.CacheDependency = CacheHelper.GetCacheDependency(
        [
            $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|all",
            $"{TaxonomyInfo.OBJECT_TYPE}|all"
        ]);

        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentType(BlogPost.CONTENT_TYPE_NAME, q => q
                .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                .Where(w => w.WhereContainsTags(nameof(BlogPost.BlogPostTags), [tagGuid]))
                .OrderBy([new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending)])
            );

        return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder, new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        });
    },
    new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, _websiteChannelContext.IsPreview,
        nameof(BlogPostService), nameof(GetBlogPostsByTag), tagGuid.ToString()));
}
```

---

## Phase 6 — Update `BlogListViewModel` and `BlogPostViewModel`

**`BlogListViewModel`** — add:
```csharp
public IReadOnlyList<BlogTagViewModel> Tags { get; set; } = [];
```

**`BlogPostViewModel`** — add:
```csharp
public IReadOnlyList<BlogTagViewModel> Tags { get; set; } = [];
```

---

## Phase 7 — Update `BlogListController`

1. Inject `IBlogTagService _blogTagService`

2. **Fix filter ordering** — `?tag=` must run before `?q=` (currently reversed in the controller). Swap the two `if` blocks.

3. Replace the tag-filter stub (lines 94–99) with:

```csharp
if (!string.IsNullOrWhiteSpace(tag))
{
    var tagGuid = await _blogTagService.ResolveTagSlugToGuid(tag);
    filtered = tagGuid.HasValue
        ? (await _blogPostService.GetBlogPostsByTag(tagGuid.Value)).OrderByDescending(p => p.BlogPostDate)
        : Enumerable.Empty<BlogPost>();
}
```

4. Populate tag chips after building the view model:

```csharp
var tagsWithCounts = await _blogTagService.GetTagsWithPostCounts(languageName);
viewModel.Tags = tagsWithCounts
    .Select(t => new BlogTagViewModel(t.Tag.Name, t.Tag.Title, t.PostCount))
    .ToList();
```

5. Populate per-post tags in the `foreach` loop:

```csharp
if (blogPost.BlogPostTags?.Any() == true)
{
    var tagGuids = blogPost.BlogPostTags.Select(t => t.Identifier).ToList();
    var resolvedTags = await _blogTagService.GetTagsByGuids(tagGuids, languageName);
    vm.Tags = resolvedTags.Select(t => new BlogTagViewModel(t.Name, t.Title, 0)).ToList();
}
```

---

## Phase 8 — Update `Listing.cshtml`

1. Remove the `/* ... */` comment wrapper from the `@functions` block (delete the static hardcoded array)

2. Restore the tag chip row, driven by `Model.Tags`:

```razor
@if (Model.Tags.Count > 0)
{
    <div class="tag-chip-row">
        <a class="tag-chip mono @(string.IsNullOrWhiteSpace(Model.ActiveTag) ? "tag-chip--active" : "")"
           href="@Model.Url"
           aria-current="@(string.IsNullOrWhiteSpace(Model.ActiveTag) ? "true" : null)">
            <span>All posts</span>
            <span class="tag-chip__count mono">@Model.TotalCount</span>
        </a>
        @foreach (var chip in Model.Tags)
        {
            var isActive = string.Equals(Model.ActiveTag, chip.Slug, StringComparison.OrdinalIgnoreCase);
            <a class="tag-chip mono @(isActive ? "tag-chip--active" : "")"
               href="@Model.Url?tag=@chip.Slug"
               aria-current="@(isActive ? "true" : null)">
                <span class="tag-chip__hash">#</span><span>@chip.Label</span>
                <span class="tag-chip__count mono">@chip.Count</span>
            </a>
        }
    </div>
}
```

3. Restore per-post tags in the list view row (around line 140):

```razor
@if (post.Tags.Count > 0)
{
    <div class="post-list__tags">
        @foreach (var t in post.Tags)
        {
            <a class="post-tag mono" href="@Model.Url?tag=@t.Slug">#@t.Label</a>
        }
    </div>
}
```

---

## Phase 9 — Verification Checklist

- [ ] `/blog` renders tag chip row with real tags and correct counts
- [ ] Clicking a chip filters to matching posts (`?tag=xperience` shows only Xperience posts)
- [ ] "All posts" chip clears the filter
- [ ] `?tag=` + `?q=` combined works (text search applied on top of tag-filtered set)
- [ ] Unknown slug (`?tag=nonexistent`) shows empty state cleanly
- [ ] List view rows show tag chips that link correctly
- [ ] CI repository committed with taxonomy + tag + updated content type files

---

## Files Summary

| Action | File |
|--------|------|
| New | `src/Goldfinch.Web/Features/BlogList/BlogTagViewModel.cs` |
| New | `src/Goldfinch.Core/BlogPosts/IBlogTagService.cs` |
| New | `src/Goldfinch.Core/BlogPosts/BlogTagService.cs` |
| Codegen | `src/Goldfinch.Core/ContentTypes/PageContentTypes/Goldfinch/BlogPost/BlogPost.generated.cs` |
| Update | `src/Goldfinch.Core/BlogPosts/IBlogPostService.cs` |
| Update | `src/Goldfinch.Core/BlogPosts/BlogPostService.cs` |
| Update | `src/Goldfinch.Core/ServiceConfiguration.cs` |
| Update | `src/Goldfinch.Web/Features/BlogList/BlogListViewModel.cs` |
| Update | `src/Goldfinch.Web/Features/BlogList/BlogListController.cs` |
| Update | `src/Goldfinch.Web/Features/BlogDetail/BlogPostViewModel.cs` |
| Update | `src/Goldfinch.Web/Features/BlogList/Listing.cshtml` |
| New (CI) | `App_Data/CIRepository/@global/cms.taxonomy/blogtags.xml` |
| New (CI) | `App_Data/CIRepository/@global/cms.tag/*.xml` |
| Updated (CI) | `App_Data/CIRepository/@global/cms.contenttype/goldfinch.blogpost.xml` |
