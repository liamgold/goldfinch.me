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

    public Task<IReadOnlyList<AskSourcePost>> GetSources(IReadOnlyList<int> webPageItemIds, CancellationToken cancellationToken = default)
    {
        // Read the posts' stored body/title/URL back from the Lucene index — the body is captured
        // (crawled + sanitised) at index time, so there's no need to re-crawl the page per question.
        var sources = _searchService.GetStoredPosts(webPageItemIds)
            .Where(post => !string.IsNullOrWhiteSpace(post.Body))
            .Select(post => new AskSourcePost
            {
                WebPageItemID = post.WebPageItemID,
                Title = post.Title,
                Url = post.Url,
                Body = post.Body,
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<AskSourcePost>>(sources);
    }
}
