using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO.Constants;
using Goldfinch.Core.Extensions;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Schema.NET;

[assembly: RegisterWebPageRoute(
    contentTypeName: BlogListing.CONTENT_TYPE_NAME,
    controllerType: typeof(BlogListController),
    ActionName = nameof(BlogListController.Index)
)]
namespace Goldfinch.Web.Features.BlogList;

public class BlogListController : Controller
{
    private const int PostsPerPage = 6;

    private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IContentRetriever _contentRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public BlogListController(
        IWebPageDataContextInitializer webPageDataContextInitializer,
        IWebPageUrlRetriever webPageUrlRetriever,
        IContentRetriever contentRetriever,
        IBlogPostService blogPostService,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebsiteChannelContext websiteChannelContext)
    {
        _webPageDataContextInitializer = webPageDataContextInitializer;
        _webPageUrlRetriever = webPageUrlRetriever;
        _contentRetriever = contentRetriever;
        _blogPostService = blogPostService;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<IActionResult> Index(int page = 1, string? tag = null, string? q = null, string? view = null)
    {
        var requestedPage = page;

        var blogListing = (await _contentRetriever.RetrievePages<BlogListing>(RetrievePagesParameters.Default)).FirstOrDefault();
        if (blogListing == null)
        {
            return NotFound();
        }

        var languageName = _preferredLanguageRetriever.Get();

        _webPageDataContextInitializer.Initialize(new RoutedWebPage
        {
            WebPageItemGUID = blogListing.SystemFields.WebPageItemGUID,
            WebPageItemID = blogListing.SystemFields.WebPageItemID,
            ContentTypeName = BlogListing.CONTENT_TYPE_NAME,
            LanguageName = languageName,
            WebsiteChannelID = blogListing.SystemFields.WebPageItemWebsiteChannelId,
            WebsiteChannelName = _websiteChannelContext.WebsiteChannelName,
            ContentTypeID = blogListing.SystemFields.ContentItemContentTypeID,
        });

        // Pull the full set once — 13ish posts today, in-memory filter is trivial.
        // TODO: once a Tag content type exists, push ?tag= filtering into the service so
        // the query runs against the Kentico database rather than the in-memory list.
        var allPosts = (await _blogPostService.GetAllBlogPosts())
            .OrderByDescending(p => p.BlogPostDate)
            .ToList();

        var filtered = allPosts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            filtered = filtered.Where(p =>
                (p.BaseContentTitle?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                || (p.BaseContentShortDescription?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // TODO: tag filtering is a no-op until a Tag content type is added. When ?tag= is
        // present we currently return no matches so the UI shows the "no posts tagged" state.
        if (!string.IsNullOrWhiteSpace(tag))
        {
            filtered = Enumerable.Empty<BlogPost>();
        }

        var filteredList = filtered.ToList();

        var totalCount = filteredList.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PostsPerPage));
        var currentPage = Math.Clamp(requestedPage, 1, totalPages);

        var pageItems = filteredList
            .Skip((currentPage - 1) * PostsPerPage)
            .Take(PostsPerPage)
            .ToList();

        var viewModel = await BlogListViewModel.GetViewModelAsync(blogListing, _webPageUrlRetriever, currentPage, totalPages);
        viewModel.ActiveTag = tag;
        viewModel.Query = q;
        viewModel.View = view == "list" ? "list" : "grid";
        viewModel.TotalCount = totalCount;
        viewModel.PageStart = totalCount == 0 ? 0 : (currentPage - 1) * PostsPerPage + 1;
        viewModel.PageEnd = totalCount == 0 ? 0 : viewModel.PageStart + pageItems.Count - 1;

        viewModel.PreviousUrl = currentPage > 1 ? BuildPageUrl(viewModel.Url, currentPage - 1, tag, q, view) : string.Empty;
        viewModel.NextUrl = currentPage < totalPages ? BuildPageUrl(viewModel.Url, currentPage + 1, tag, q, view) : string.Empty;

        if (!string.IsNullOrWhiteSpace(viewModel.PreviousUrl))
        {
            ViewData[SEOConstants.PREVIOUS_URL_KEY] = $"https://www.goldfinch.me{viewModel.PreviousUrl.ToAbsolutePath()}";
        }
        if (!string.IsNullOrWhiteSpace(viewModel.NextUrl))
        {
            ViewData[SEOConstants.NEXT_URL_KEY] = $"https://www.goldfinch.me{viewModel.NextUrl.ToAbsolutePath()}";
        }

        foreach (var blogPost in pageItems)
        {
            var vm = await BlogPostViewModel.GetViewModelAsync(blogPost, _webPageUrlRetriever);
            viewModel.BlogPosts.Add(vm);
        }

        viewModel.Schema = GetSchema(viewModel);

        return View("~/Features/BlogList/Listing.cshtml", viewModel);
    }

    /// <summary>
    /// 301s the legacy path-based pagination URLs (/blog/2) to the canonical
    /// query-string form (/blog?page=2). Pagination lives on ?page= now — keeping
    /// this handler means old links + search engine results still land correctly.
    /// </summary>
    [HttpGet]
    public IActionResult PagedRedirect(int pageIndex)
    {
        if (pageIndex <= 1)
        {
            return RedirectPermanent("/blog");
        }
        return RedirectPermanent($"/blog?page={pageIndex}");
    }

    private static string BuildPageUrl(string basePath, int page, string? tag, string? q, string? view)
    {
        var parts = new List<string>();
        if (page > 1) parts.Add($"page={page}");
        if (!string.IsNullOrWhiteSpace(tag)) parts.Add($"tag={Uri.EscapeDataString(tag)}");
        if (!string.IsNullOrWhiteSpace(q)) parts.Add($"q={Uri.EscapeDataString(q)}");
        if (view == "list") parts.Add("view=list");
        return parts.Count == 0 ? basePath : $"{basePath}?{string.Join('&', parts)}";
    }

    private string GetSchema(BlogListViewModel viewModel)
    {
        var blogPosts = viewModel.BlogPosts.Select(post => new BlogPosting
        {
            Headline = post.Title,
            Url = new Uri($"https://www.goldfinch.me{post.Url.ToAbsolutePath()}"),
            DatePublished = post.BlogPostDate,
            Description = post.Summary,
        }).ToList();

        var itemListElements = blogPosts.Select(post => new ListItem { Item = post }).ToList<IListItem>();

        var mainEntity = new ItemList { ItemListElement = itemListElements };

        var currentUrl = new Uri($"https://www.goldfinch.me{viewModel.Url.ToAbsolutePath()}");
        if (viewModel.PageIndex > 1)
        {
            currentUrl = new Uri($"{currentUrl.GetLeftPart(UriPartial.Path)}?page={viewModel.PageIndex}");
        }

        var webPage = new WebPage
        {
            Id = currentUrl,
            Url = currentUrl,
            Name = $"Liam Goldfinch's Blog - Page {viewModel.PageIndex}",
            IsPartOf = new Blog { Id = new Uri("https://www.goldfinch.me/blog") },
            MainEntity = mainEntity,
        };

        var hasPart = new List<ICreativeWork>();
        if (!string.IsNullOrWhiteSpace(viewModel.PreviousUrl))
        {
            hasPart.Add(new WebPage { Url = new Uri($"https://www.goldfinch.me{viewModel.PreviousUrl.ToAbsolutePath()}"), Name = "Previous page" });
        }
        if (!string.IsNullOrWhiteSpace(viewModel.NextUrl))
        {
            hasPart.Add(new WebPage { Url = new Uri($"https://www.goldfinch.me{viewModel.NextUrl.ToAbsolutePath()}"), Name = "Next page" });
        }
        if (hasPart.Count > 0)
        {
            webPage.HasPart = hasPart;
        }

        return webPage.ToHtmlEscapedString();
    }
}
