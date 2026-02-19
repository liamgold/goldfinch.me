using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogDetail;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Schema.NET;
using System;
using System.Collections.Generic;
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
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;

    public BlogDetailController(
        IBlogPostService blogPostService,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _blogPostService = blogPostService;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _webPageUrlRetriever = webPageUrlRetriever;
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
                Name = "Principal Systems Developer",
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