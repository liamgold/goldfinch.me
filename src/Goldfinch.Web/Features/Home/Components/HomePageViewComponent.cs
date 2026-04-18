using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewComponent : ViewComponent
{
    private const int RecentPostCount = 6;

    private readonly IContentRetriever _contentRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IWebPageUrlRetriever _urlRetriever;

    public HomePageViewComponent(
        IContentRetriever contentRetriever,
        IBlogPostService blogPostService,
        IWebPageUrlRetriever urlRetriever)
    {
        _contentRetriever = contentRetriever;
        _blogPostService = blogPostService;
        _urlRetriever = urlRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, HomePageTemplateProperties props)
    {
        var home = await _contentRetriever.RetrieveCurrentPage<Core.ContentTypes.Home>();

        // First page gives us up to 9 posts — use the newest as the featured card,
        // followed by up to six more for the recent grid.
        // TODO: once content modelling lands, use a real BlogPost.Featured flag (see docs/design-handoff/content-types.md).
        var firstPage = (await _blogPostService.GetBlogPosts(1)).ToList();

        HomeFeaturedPost? featured = null;
        var recent = new List<HomeRecentPost>();

        if (firstPage.Count > 0)
        {
            var top = firstPage[0];
            var topUrl = (await _urlRetriever.Retrieve(top)).RelativePath;
            featured = new HomeFeaturedPost(
                Title: top.BaseContentTitle,
                Summary: top.BaseContentShortDescription,
                Url: topUrl,
                Filename: BuildFilename(top.BaseContentTitle),
                PublishedOn: top.BlogPostDate,
                ReadingMinutes: EstimateReadingMinutes(top.BaseContentShortDescription));

            foreach (var post in firstPage.Skip(1).Take(RecentPostCount))
            {
                var url = (await _urlRetriever.Retrieve(post)).RelativePath;
                recent.Add(new HomeRecentPost(
                    Title: post.BaseContentTitle,
                    Summary: post.BaseContentShortDescription,
                    Url: url,
                    Filename: BuildFilename(post.BaseContentTitle),
                    PublishedOn: post.BlogPostDate,
                    ReadingMinutes: EstimateReadingMinutes(post.BaseContentShortDescription)));
            }
        }

        var viewModel = new HomePageViewModel
        {
            Page = home,
            FeaturedPost = featured,
            RecentPosts = recent,
        };

        return View("~/Features/Home/Components/HomePage.cshtml", viewModel);
    }

    /// <summary>
    /// Slugifies the title into a terminal-style "name.md" filename used in card chrome.
    /// </summary>
    private static string BuildFilename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "post.md";
        }

        var slug = new string(title.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }
        slug = slug.Trim('-');
        if (slug.Length > 40)
        {
            slug = slug[..40].TrimEnd('-');
        }
        return $"{slug}.md";
    }

    /// <summary>
    /// Rough reading-time estimate based on summary length.
    /// TODO: compute from full post body once we have a consistent way to read it.
    /// </summary>
    private static int EstimateReadingMinutes(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary)) return 4;
        var words = summary.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
        // Summary is ~40 words; multiply to approximate a full article length.
        var estimated = System.Math.Max(3, (int)System.Math.Round(words * 0.15));
        return System.Math.Min(15, estimated);
    }
}
