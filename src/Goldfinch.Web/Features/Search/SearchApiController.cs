using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Extensions;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Features.Search;

/// <summary>
/// Backs the command palette (⌘K) and the blog toolbar's live search dropdown.
/// See docs/design-handoff/api-contracts.md for the full contract.
/// </summary>
/// <remarks>
/// TODO: this is a v1 stub. It currently searches BlogPost titles + summaries only.
/// The contract adds ranking (tag > title > summary > body), body search,
/// <mark> highlights, tag results, and a 60s cache header. Implement properly
/// once the Tag content type exists and we can index bodies.
/// </remarks>
[ApiController]
[Route("api/search")]
public class SearchApiController : ControllerBase
{
    private readonly IBlogPostService _blogPostService;
    private readonly IBlogTagService _blogTagService;
    private readonly IWebPageUrlRetriever _urlRetriever;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public SearchApiController(
        IBlogPostService blogPostService,
        IBlogTagService blogTagService,
        IWebPageUrlRetriever urlRetriever,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _blogPostService = blogPostService;
        _blogTagService = blogTagService;
        _urlRetriever = urlRetriever;
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

        var started = DateTime.UtcNow;

        var needle = q.Trim();

        // Scope to a tag when one is active (e.g. live search on /blog?tag=…).
        List<BlogPost> all;
        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagGuid = await _blogTagService.ResolveTagSlugToGuid(tag);
            all = tagGuid.HasValue
                ? (await _blogPostService.GetBlogPostsByTag(tagGuid.Value)).ToList()
                : [];
        }
        else
        {
            all = (await _blogPostService.GetAllBlogPosts())
                .OrderByDescending(p => p.BlogPostDate)
                .ToList();
        }

        var matches = all.Where(p =>
                (p.BaseContentTitle?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                || (p.BaseContentShortDescription?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(limit)
            .ToList();

        var languageName = _preferredLanguageRetriever.Get();

        var results = new object[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            var url = (await _urlRetriever.Retrieve(m)).RelativePath.ToAbsolutePath();

            var tags = Array.Empty<string>();
            if (m.BlogPostTags?.Any() == true)
            {
                var resolvedTags = await _blogTagService.GetTagsByGuids(
                    m.BlogPostTags.Select(t => t.Identifier), languageName);
                tags = resolvedTags.Select(t => t.Name).ToArray();
            }

            results[i] = new
            {
                kind = "post",
                title = m.BaseContentTitle,
                summary = m.BaseContentShortDescription,
                url,
                date = m.BlogPostDate.ToString("yyyy-MM-dd"),
                tags,
                reading_minutes = 4,             // TODO: compute from body
            };
        }

        Response.Headers["Cache-Control"] = "public, max-age=60";
        return Ok(new
        {
            q = needle,
            total = results.Length,
            took_ms = (int)(DateTime.UtcNow - started).TotalMilliseconds,
            results,
        });
    }
}
