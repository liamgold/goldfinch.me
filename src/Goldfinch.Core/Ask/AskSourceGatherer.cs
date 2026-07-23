using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;
using Goldfinch.Core.Search;

namespace Goldfinch.Core.Ask;

public class AskSourceGatherer : IAskSourceGatherer
{
    private readonly ILuceneBlogSearchService _searchService;

    public AskSourceGatherer(ILuceneBlogSearchService searchService)
    {
        _searchService = searchService;
    }

    public Task<IReadOnlyList<AskSourcePost>> GetSources(IReadOnlyList<AskCandidate> candidates, CancellationToken cancellationToken = default)
    {
        // Blog posts don't carry a body — read it (plus title/URL) back from the Lucene index, where
        // it was captured (crawled + sanitised) at index time, so there's no re-crawl per question.
        var blogIds = candidates
            .Where(candidate => string.IsNullOrWhiteSpace(candidate.Body))
            .Select(candidate => candidate.WebPageItemID)
            .ToList();

        var storedById = (blogIds.Count > 0 ? _searchService.GetStoredPosts(blogIds) : [])
            .ToDictionary(post => post.WebPageItemID);

        // Preserve the model's selection order; use each candidate's own body (pages) or the stored
        // body (posts), and drop anything that resolves to no text.
        var sources = new List<AskSourcePost>();
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate.Body))
            {
                sources.Add(new AskSourcePost
                {
                    WebPageItemID = candidate.WebPageItemID,
                    Title = candidate.Title,
                    Url = candidate.Url ?? string.Empty,
                    Body = candidate.Body,
                });
            }
            else if (storedById.TryGetValue(candidate.WebPageItemID, out var stored)
                && !string.IsNullOrWhiteSpace(stored.Body))
            {
                sources.Add(new AskSourcePost
                {
                    WebPageItemID = stored.WebPageItemID,
                    Title = stored.Title,
                    Url = stored.Url,
                    Body = stored.Body,
                });
            }
        }

        return Task.FromResult<IReadOnlyList<AskSourcePost>>(sources);
    }
}
