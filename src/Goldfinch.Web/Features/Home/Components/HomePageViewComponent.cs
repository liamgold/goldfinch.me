using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.SiteStats;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewComponent : ViewComponent
{
    // 8 tiles the 4-column desktop grid cleanly (2 rows) — 6 would leave two
    // orphans on the second row.
    private const int RecentPostCount = 8;

    // First day coding (started college, Sept 2005) — drives the "years coding" stat.
    private static readonly System.DateTime FirstCodingDate = new(2005, 9, 1);

    private readonly IContentRetriever _contentRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IWebPageUrlRetriever _urlRetriever;
    private readonly IBlogTagService _blogTagService;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public HomePageViewComponent(
        IContentRetriever contentRetriever,
        IBlogPostService blogPostService,
        IWebPageUrlRetriever urlRetriever,
        IBlogTagService blogTagService,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _contentRetriever = contentRetriever;
        _blogPostService = blogPostService;
        _urlRetriever = urlRetriever;
        _blogTagService = blogTagService;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, HomePageTemplateProperties props)
    {
        var home = await _contentRetriever.RetrieveCurrentPage<Core.ContentTypes.Home>();
        var languageName = _preferredLanguageRetriever.Get();

        // Pull the full set — cheap on a small blog, lets us derive totals + first-year
        // for the hero stats without a second query.
        // TODO: once content modelling lands, use a real BlogPost.Featured flag (see docs/design-handoff/content-types.md).
        var allPosts = (await _blogPostService.GetAllBlogPosts())
            .OrderByDescending(p => p.BlogPostDate)
            .ToList();

        HomeFeaturedPost? featured = null;
        var recent = new List<HomeRecentPost>();

        if (allPosts.Count > 0)
        {
            var top = allPosts[0];
            var topUrl = (await _urlRetriever.Retrieve(top)).RelativePath;
            var topTags = await ResolveTags(top.BlogPostTags, languageName);
            featured = new HomeFeaturedPost(
                Title: top.BaseContentTitle,
                Summary: top.BaseContentShortDescription,
                Url: topUrl,
                Filename: BlogPostViewModel.FilenameFromUrl(topUrl),
                PublishedOn: top.BlogPostDate,
                ReadingMinutes: top.GetEffectiveReadingMinutes(),
                Tags: topTags);

            foreach (var post in allPosts.Skip(1).Take(RecentPostCount))
            {
                var url = (await _urlRetriever.Retrieve(post)).RelativePath;
                var tags = await ResolveTags(post.BlogPostTags, languageName);
                recent.Add(new HomeRecentPost(
                    Title: post.BaseContentTitle,
                    Summary: post.BaseContentShortDescription,
                    Url: url,
                    Filename: BlogPostViewModel.FilenameFromUrl(url),
                    PublishedOn: post.BlogPostDate,
                    ReadingMinutes: post.GetEffectiveReadingMinutes(),
                    Tags: tags));
            }
        }

        var viewModel = new HomePageViewModel
        {
            Page = home,
            FeaturedPost = featured,
            RecentPosts = recent,
            TotalPostCount = allPosts.Count,
            FirstPostYear = allPosts.Count > 0 ? allPosts.Min(p => p.BlogPostDate).Year : null,
            KenticoMvpCount = SiteStats.KenticoMvpCount(),
            YearsCoding = CalculateFullYears(FirstCodingDate, System.DateTime.UtcNow),
        };

        return View("~/Features/Home/Components/HomePage.cshtml", viewModel);
    }

    /// <summary>
    /// Full years between two dates — only counts a year once the anniversary has passed.
    /// </summary>
    private static int CalculateFullYears(System.DateTime from, System.DateTime to)
    {
        var years = to.Year - from.Year;
        if (to.Month < from.Month || (to.Month == from.Month && to.Day < from.Day))
        {
            years--;
        }
        return System.Math.Max(0, years);
    }

    private async System.Threading.Tasks.Task<IReadOnlyList<BlogTagViewModel>> ResolveTags(
        System.Collections.Generic.IEnumerable<CMS.ContentEngine.TagReference>? tagRefs,
        string languageName)
    {
        if (tagRefs == null || !tagRefs.Any()) return [];
        var guids = tagRefs.Select(t => t.Identifier).ToList();
        var tags = await _blogTagService.GetTagsByGuids(guids, languageName);
        return tags.Select(t => new BlogTagViewModel(t.Name, t.Title, 0)).ToList();
    }
}
