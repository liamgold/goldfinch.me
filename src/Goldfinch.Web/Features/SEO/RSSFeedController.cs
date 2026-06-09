using CMS.Websites;
using Goldfinch.Core.Extensions;
using Goldfinch.Core.BlogPosts;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Goldfinch.Web.Features.SEO;

public class RSSFeedController : Controller
{
    private readonly IBlogPostService _blogPostService;
    private readonly IBlogTagService _blogTagService;
    private readonly IWebPageUrlRetriever _pageUrlRetriever;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public RSSFeedController(
        IBlogPostService blogPostService,
        IBlogTagService blogTagService,
        IWebPageUrlRetriever pageUrlRetriever,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _blogPostService = blogPostService;
        _blogTagService = blogTagService;
        _pageUrlRetriever = pageUrlRetriever;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public async Task<ActionResult> Index()
    {
        var feed = await RSSFeedInternal();

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        using var stream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            var rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }

    private async Task<SyndicationFeed> RSSFeedInternal()
    {
        var blogPosts = await _blogPostService.GetLatestBlogPosts();

        var feed = new SyndicationFeed(
            "Latest Blog Posts - Liam Goldfinch",
            "I will be sharing knowledge, learnings, and experiences of working with Kentico and the .NET world. I hope that you will find my blog posts valuable, and that they will be useful in assisting with working on your own projects.",
            new Uri("https://www.goldfinch.me/rss"),
            "RSSUrl",
            DateTime.Now)
        {
            Copyright = new TextSyndicationContent($"{DateTime.UtcNow.Year} - Liam Goldfinch")
        };

        var items = new List<SyndicationItem>();
        var languageName = _preferredLanguageRetriever.Get();

        foreach (var blogPost in blogPosts)
        {
            var pageGuid = blogPost.SystemFields.WebPageItemGUID.ToString("N");
            var blogPostUrl = (await _pageUrlRetriever.Retrieve(blogPost)).RelativePath;

            var pageUrl = $"https://www.goldfinch.me{blogPostUrl.ToAbsolutePath()}";

            var item = new SyndicationItem(blogPost.BaseContentTitle, blogPost.BaseContentShortDescription, new Uri(pageUrl), pageGuid, blogPost.BlogPostDate)
            {
                PublishDate = blogPost.BlogPostDate,
            };

            item.Authors.Add(new SyndicationPerson
            {
                Name = "Liam Goldfinch",
            });

            if (blogPost.BlogPostTags?.Any() == true)
            {
                var tags = await _blogTagService.GetTagsByGuids(
                    blogPost.BlogPostTags.Select(t => t.Identifier), languageName);
                foreach (var tag in tags)
                {
                    item.Categories.Add(new SyndicationCategory(tag.Title));
                }
            }

            items.Add(item);
        }

        feed.Items = items;

        return feed;
    }
}
