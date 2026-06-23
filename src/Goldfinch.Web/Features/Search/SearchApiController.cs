using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.Search;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Features.Search;

/// <summary>
/// Backs the command palette (⌘K) and the blog toolbar's live search dropdown.
/// See docs/design-handoff/api-contracts.md for the full contract.
/// </summary>
/// <remarks>
/// Post results come from the <c>BlogPosts</c> Lucene index (title &gt; summary &gt; body ranking,
/// with body content + reading time + <c>&lt;mark&gt;</c> highlights). Tag results are resolved
/// from the BlogTags taxonomy and emitted first on an exact tag match.
/// </remarks>
[ApiController]
[Route("api/search")]
public class SearchApiController : ControllerBase
{
    private readonly ILuceneBlogSearchService _searchService;
    private readonly IBlogTagService _blogTagService;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public SearchApiController(
        ILuceneBlogSearchService searchService,
        IBlogTagService blogTagService,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _searchService = searchService;
        _blogTagService = blogTagService;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? tag = null, [FromQuery] int limit = 8)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "missing_query" });
        }
        if (limit < 1 || limit > 20)
        {
            return BadRequest(new { error = "invalid_limit" });
        }

        var stopwatch = Stopwatch.StartNew();
        var needle = q.Trim();

        var results = new List<object>();

        // Tag results first (contract rank #1): exact match on tag slug or title.
        // Skipped when already scoped to a tag (live search within /blog?tag=…).
        if (string.IsNullOrWhiteSpace(tag))
        {
            results.AddRange(await GetTagResults(needle));
        }

        // Post results from the Lucene index.
        var posts = _searchService.SearchPosts(needle, tag, limit);
        foreach (var post in posts)
        {
            results.Add(new
            {
                kind = "post",
                slug = post.Slug,
                title = post.Title,
                summary = post.Summary,
                url = post.Url,
                date = post.Date,
                tags = post.Tags,
                reading_minutes = post.ReadingMinutes,
                highlights = BuildHighlights(post),
            });
        }

        stopwatch.Stop();

        Response.Headers["Cache-Control"] = "public, max-age=60";
        Response.Headers["Vary"] = "Accept-Encoding";

        return Ok(new
        {
            q = needle,
            total = results.Count,
            took_ms = (int)stopwatch.ElapsedMilliseconds,
            results,
        });
    }

    private async Task<IEnumerable<object>> GetTagResults(string needle)
    {
        var languageName = _preferredLanguageRetriever.Get();
        var tags = await _blogTagService.GetTagsWithPostCounts(languageName);

        return tags
            .Where(t =>
                string.Equals(t.Tag.Name, needle, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Tag.Title, needle, StringComparison.OrdinalIgnoreCase))
            .Select(t => (object)new
            {
                kind = "tag",
                slug = t.Tag.Name,
                label = t.Tag.Title,
                url = $"/blog?tag={t.Tag.Name}",
                post_count = t.PostCount,
            });
    }

    /// <summary>
    /// Builds the optional highlights object, including only the parts that actually matched.
    /// Returns null when there are no highlights so the property is omitted from the response.
    /// </summary>
    private static Dictionary<string, string>? BuildHighlights(BlogSearchResult post)
    {
        var highlights = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(post.HighlightedTitle))
        {
            highlights["title"] = post.HighlightedTitle;
        }
        if (!string.IsNullOrEmpty(post.HighlightedSummary))
        {
            highlights["summary"] = post.HighlightedSummary;
        }

        return highlights.Count > 0 ? highlights : null;
    }
}
