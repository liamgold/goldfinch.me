using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Goldfinch.Web.Features.SEO;

public class RSSFeedController : Controller
{
    private readonly BlogPostRepository _blogPostRepository;
    private readonly IWebPageUrlRetriever _pageUrlRetriever;

    public RSSFeedController(BlogPostRepository blogPostRepository, IWebPageUrlRetriever pageUrlRetriever)
    {
        _blogPostRepository = blogPostRepository;
        _pageUrlRetriever = pageUrlRetriever;
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
        var blogPosts = await _blogPostRepository.GetLatestBlogPosts();

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

        foreach (var blogPost in blogPosts)
        {
            var pageGuid = blogPost.SystemFields.WebPageItemGUID.ToString("N");
            var blogPostUrl = (await _pageUrlRetriever.Retrieve(blogPost)).RelativePath;

            var relativeUrl = blogPostUrl.Replace("~/", "/");
            var pageUrl = $"https://www.goldfinch.me{relativeUrl}";

            var item = new SyndicationItem(blogPost.BaseContentTitle, blogPost.BaseContentShortDescription, new Uri(pageUrl), pageGuid, blogPost.BlogPostDate)
            {
                PublishDate = blogPost.BlogPostDate,
            };

            item.Authors.Add(new SyndicationPerson
            {
                Name = "Liam Goldfinch",
            });

            items.Add(item);
        }

        feed.Items = items;

        return feed;
    }
}
