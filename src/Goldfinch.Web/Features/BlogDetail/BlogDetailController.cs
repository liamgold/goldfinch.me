using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Schema.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWebPageRoute(
    contentTypeName: BlogPost.CONTENT_TYPE_NAME,
    controllerType: typeof(BlogDetailController),
    ActionName = nameof(BlogDetailController.Index)
)]
namespace Goldfinch.Web.Features.BlogDetail;

public class BlogDetailController : Controller
{
    private readonly IBlogPostService _blogPostService;
    private readonly IBlogTagService _blogTagService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public BlogDetailController(
        IBlogPostService blogPostService,
        IBlogTagService blogTagService,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IWebPageUrlRetriever webPageUrlRetriever,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _blogPostService = blogPostService;
        _blogTagService = blogTagService;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _webPageUrlRetriever = webPageUrlRetriever;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public async Task<IActionResult> Index()
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var data))
        {
            return NotFound();
        }

        var page = data.WebPage;

        var currentPage = await _blogPostService.GetBlogPost(page.WebPageItemID);

        if (currentPage == null)
        {
            return NotFound();
        }

        var viewModel = await BlogPostViewModel.GetViewModelAsync(currentPage, _webPageUrlRetriever);

        if (currentPage.BlogPostTags?.Any() == true)
        {
            var tagGuids = currentPage.BlogPostTags.Select(t => t.Identifier).ToList();
            var resolvedTags = await _blogTagService.GetTagsByGuids(tagGuids, _preferredLanguageRetriever.Get());
            viewModel.Tags = resolvedTags.Select(t => new BlogTagViewModel(t.Name, t.Title, 0)).ToList();
        }

        viewModel.Schema = GetSchema(viewModel);

        return View("~/Features/BlogDetail/BlogDetail.cshtml", viewModel);
    }

    private string GetSchema(BlogPostViewModel viewModel)
    {
        var author = new Person
        {
            Name = "Liam Goldfinch",
            Url = new Uri("https://www.goldfinch.me/"),
            SameAs = new List<Uri>
            {
                new("https://github.com/liamgold/"),
                new("https://www.linkedin.com/in/liamgoldfinch/"),
                new("https://x.com/LiamGoldfinch")
            },
            HasOccupation = new Occupation
            {
                Name = "Principal Systems Engineer",
                OccupationLocation = new City
                {
                    Name = "Leeds, UK"
                }
            },
            WorksFor = new Organization
            {
                Name = "IDHL"
            }
        };

        var blogPostUrl = new Uri($"https://www.goldfinch.me{Url.Content(viewModel.Url)}");

        var blogPost = new BlogPosting
        {
            Headline = viewModel.Title,
            Url = blogPostUrl,
            DatePublished = viewModel.BlogPostDate,
            Description = viewModel.Summary,
            Author = author
        };

        if (viewModel.Tags.Count > 0)
        {
            blogPost.Keywords = string.Join(", ", viewModel.Tags.Select(t => t.Label));
        }

        var webPage = new WebPage
        {
            Id = blogPostUrl,
            Url = blogPostUrl,
            Name = viewModel.Title,
            IsPartOf = new Blog
            {
                Id = new Uri("https://www.goldfinch.me/blog"),
            },
            MainEntity = blogPost
        };

        var jsonLd = webPage.ToHtmlEscapedString();

        return jsonLd;
    }
}