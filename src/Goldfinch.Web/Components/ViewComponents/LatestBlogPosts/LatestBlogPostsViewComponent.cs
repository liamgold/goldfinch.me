using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.Search;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.ViewComponents.LatestBlogPosts;

public class LatestBlogPostsViewComponent : ViewComponent
{
    private readonly IWebPageUrlRetriever _pageUrlRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IWebPageDataContextRetriever _pageDataContextRetriever;
    private readonly IBlogTagService _blogTagService;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly ILuceneBlogSearchService _luceneBlogSearchService;

    public LatestBlogPostsViewComponent(
        IWebPageUrlRetriever pageUrlRetriever,
        IBlogPostService blogPostService,
        IWebPageDataContextRetriever pageDataContextRetriever,
        IBlogTagService blogTagService,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        ILuceneBlogSearchService luceneBlogSearchService)
    {
        _pageUrlRetriever = pageUrlRetriever;
        _blogPostService = blogPostService;
        _pageDataContextRetriever = pageDataContextRetriever;
        _blogTagService = blogTagService;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _luceneBlogSearchService = luceneBlogSearchService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string title)
    {
        if (!_pageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;
        var languageName = _preferredLanguageRetriever.Get();

        var blogPosts = (await _blogPostService.GetLatestBlogPosts())
            .Where(x => x.SystemFields.WebPageItemID != page.WebPageItemID)
            .Take(4);

        var blogPostViewModels = new List<BlogPostViewModel>();

        foreach (var blogPost in blogPosts)
        {
            var blogPostViewModel = await BlogPostViewModel.GetViewModelAsync(blogPost, _pageUrlRetriever, _luceneBlogSearchService);

            if (blogPost.BlogPostTags?.Any() == true)
            {
                var tagGuids = blogPost.BlogPostTags.Select(t => t.Identifier).ToList();
                var resolvedTags = await _blogTagService.GetTagsByGuids(tagGuids, languageName);
                blogPostViewModel.Tags = resolvedTags.Select(t => new BlogTagViewModel(t.Name, t.Title, 0)).ToList();
            }

            blogPostViewModels.Add(blogPostViewModel);
        }

        var viewModel = new LatestBlogPostsViewModel
        {
            Title = title,
            BlogPosts = blogPostViewModels,
        };

        return View("~/Components/ViewComponents/LatestBlogPosts/LatestBlogPosts.cshtml", viewModel);
    }
}
