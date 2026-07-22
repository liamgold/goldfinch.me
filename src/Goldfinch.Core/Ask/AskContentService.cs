using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;
using Goldfinch.Core.BlogPosts;

namespace Goldfinch.Core.Ask;

public class AskContentService : IAskContentService
{
    private readonly IBlogPostService _blogPostService;

    public AskContentService(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public async Task<IReadOnlyList<AskCandidatePost>> GetCandidates()
    {
        var posts = await _blogPostService.GetAllBlogPosts();

        return posts
            .OrderByDescending(p => p.BlogPostDate)
            .Select(p => new AskCandidatePost
            {
                WebPageItemID = p.SystemFields.WebPageItemID,
                Title = p.BaseContentTitle ?? string.Empty,
                Excerpt = p.GetEffectiveExcerpt(),
            })
            .ToList();
    }
}
