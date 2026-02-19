using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Web.Features.BlogDetail;
using Kentico.Content.Web.Mvc;
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

    public LatestBlogPostsViewComponent(
        IWebPageUrlRetriever pageUrlRetriever,
        IBlogPostService blogPostService,
        IWebPageDataContextRetriever pageDataContextRetriever)
    {
        _pageUrlRetriever = pageUrlRetriever;
        _blogPostService = blogPostService;
        _pageDataContextRetriever = pageDataContextRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(string title)
    {
        if (!_pageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;

        var blogPosts = (await _blogPostService.GetLatestBlogPosts())
            .Where(x => x.SystemFields.WebPageItemID != page.WebPageItemID)
            .Take(3);

        var blogPostViewModels = new List<BlogPostViewModel>();

        foreach (var blogPost in blogPosts)
        {
            var blogPostViewModel = await BlogPostViewModel.GetViewModelAsync(blogPost, _pageUrlRetriever);

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
