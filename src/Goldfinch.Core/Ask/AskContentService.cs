using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;
using Goldfinch.Core.BlogPosts;

namespace Goldfinch.Core.Ask;

public class AskContentService : IAskContentService
{
    private readonly IBlogPostService _blogPostService;
    private readonly IAskPageSourceProvider _pageSourceProvider;

    public AskContentService(IBlogPostService blogPostService, IAskPageSourceProvider pageSourceProvider)
    {
        _blogPostService = blogPostService;
        _pageSourceProvider = pageSourceProvider;
    }

    public async Task<IReadOnlyList<AskCandidate>> GetCandidates()
    {
        var posts = await _blogPostService.GetAllBlogPosts();

        var candidates = posts
            .OrderByDescending(p => p.BlogPostDate)
            .Select(p => new AskCandidate
            {
                WebPageItemID = p.SystemFields.WebPageItemID,
                Title = p.BaseContentTitle ?? string.Empty,
                Excerpt = p.GetEffectiveExcerpt(),
            })
            .ToList();

        // Pages (About, Public Speaking, …) round out the corpus so questions about Liam or his
        // talks are answerable, not just blog topics. They already carry their body + URL.
        candidates.AddRange(await _pageSourceProvider.GetPageCandidates());

        return candidates;
    }
}
