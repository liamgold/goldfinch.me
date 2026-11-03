using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.ViewComponents.RelatedBlogPosts;

/// <summary>
/// "Keep reading" cards under a blog post: posts sharing the most tags with it, topped up
/// with the latest posts.
/// </summary>
public class RelatedBlogPostsViewComponent : ViewComponent
{
    private const int RELATED_POSTS_COUNT = 4;

    private readonly IWebPageUrlRetriever _pageUrlRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IWebPageDataContextRetriever _pageDataContextRetriever;
    private readonly IBlogTagService _blogTagService;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public RelatedBlogPostsViewComponent(
        IWebPageUrlRetriever pageUrlRetriever,
        IBlogPostService blogPostService,
        IWebPageDataContextRetriever pageDataContextRetriever,
        IBlogTagService blogTagService,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _pageUrlRetriever = pageUrlRetriever;
        _blogPostService = blogPostService;
        _pageDataContextRetriever = pageDataContextRetriever;
        _blogTagService = blogTagService;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(string title)
    {
        if (!_pageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var currentPost = await _blogPostService.GetBlogPost(data.WebPage.WebPageItemID);

        if (currentPost is null)
        {
            return Content(string.Empty);
        }

        var languageName = _preferredLanguageRetriever.Get();
        var blogPosts = await _blogPostService.GetRelatedBlogPosts(currentPost, RELATED_POSTS_COUNT);

        var blogPostViewModels = new List<BlogPostViewModel>();

        foreach (var blogPost in blogPosts)
        {
            var blogPostViewModel = await BlogPostViewModel.GetViewModelAsync(blogPost, _pageUrlRetriever);

            if (blogPost.BlogPostTags?.Any() == true)
            {
                var tagGuids = blogPost.BlogPostTags.Select(t => t.Identifier).ToList();
                var resolvedTags = await _blogTagService.GetTagsByGuids(tagGuids, languageName);
                blogPostViewModel.Tags = resolvedTags.Select(t => new BlogTagViewModel(t.Name, t.Title, 0)).ToList();
            }

            blogPostViewModels.Add(blogPostViewModel);
        }

        if (blogPostViewModels.Count == 0)
        {
            return Content(string.Empty);
        }

        var viewModel = new RelatedBlogPostsViewModel
        {
            Title = title,
            BlogPosts = blogPostViewModels,
        };

        return View("~/Components/ViewComponents/RelatedBlogPosts/RelatedBlogPosts.cshtml", viewModel);
    }
}
