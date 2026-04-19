using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.Extensions;
using Kentico.Content.Web.Mvc;
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
    private readonly IWebPageUrlRetriever _urlRetriever;

    public SearchApiController(
        IBlogPostService blogPostService,
        IWebPageUrlRetriever urlRetriever)
    {
        _blogPostService = blogPostService;
        _urlRetriever = urlRetriever;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int limit = 8)
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
        var all = (await _blogPostService.GetAllBlogPosts())
            .OrderByDescending(p => p.BlogPostDate)
            .ToList();

        var matches = all.Where(p =>
                (p.BaseContentTitle?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                || (p.BaseContentShortDescription?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(limit)
            .ToList();

        var results = new object[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            var url = (await _urlRetriever.Retrieve(m)).RelativePath.ToAbsolutePath();
            results[i] = new
            {
                kind = "post",
                slug = url.TrimEnd('/').Split('/').Last(),
                title = m.BaseContentTitle,
                summary = m.BaseContentShortDescription,
                url,
                date = m.BlogPostDate.ToString("yyyy-MM-dd"),
                tags = Array.Empty<string>(),    // TODO: wire up once Tag content type exists
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
