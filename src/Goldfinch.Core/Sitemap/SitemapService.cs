using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using Goldfinch.Core.Extensions;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.ContentTypes;
using Sidio.Sitemap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.Sitemap;

public class SitemapService : ISitemapService
{
    private readonly IContentQueryExecutor _executor;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IProgressiveCache _progressiveCache;
    private readonly IWebPageUrlRetriever _urlRetriever;

    public SitemapService(
        IContentQueryExecutor executor,
        IWebsiteChannelContext websiteChannelContext,
        IProgressiveCache progressiveCache,
        IWebPageUrlRetriever urlRetriever)
    {
        _executor = executor;
        _websiteChannelContext = websiteChannelContext;
        _progressiveCache = progressiveCache;
        _urlRetriever = urlRetriever;
    }

    public async Task<List<SitemapNode>> GetSitemap()
    {
        return await _progressiveCache.LoadAsync(async cs =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"webpageitem|bychannel|Goldfinch|all",
            ]);

            return await GetSitemapNodes();
        }, new CacheSettings(1440, nameof(SitemapService), nameof(GetSitemap)));
    }

    private async Task<List<SitemapNode>> GetSitemapNodes()
    {
        var sitemapNodes = new List<SitemapNode>();

        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(query =>
            {
                query.OfContentType([
                    Home.CONTENT_TYPE_NAME,
                    BlogListing.CONTENT_TYPE_NAME,
                    InnerPage.CONTENT_TYPE_NAME,
                    BlogPost.CONTENT_TYPE_NAME,
                    PublicSpeakingPage.CONTENT_TYPE_NAME,
                ]);
                query.ForWebsite(_websiteChannelContext.WebsiteChannelName);
            });

        var pages = await _executor.GetMappedWebPageResult<IWebPageFieldsSource>(builder);

        var blogPostCount = pages.OfType<BlogPost>().Count();
        var blogPostPageCount = (int)Math.Ceiling((double)blogPostCount / 6);

        foreach (var page in pages)
        {
            var pageUrl = await _urlRetriever.Retrieve(page);
            var absoluteUrl = $"https://www.goldfinch.me{pageUrl.RelativePath.ToAbsolutePath()}";

            var metadata = ContentItemLanguageMetadataInfo.Provider.Get()
                .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), page.SystemFields.ContentItemID)
                .FirstOrDefault();

            var lastModified = metadata?.ContentItemLanguageMetadataModifiedWhen ?? DateTime.UtcNow;

            sitemapNodes.Add(new SitemapNode(absoluteUrl)
            {
                LastModified = lastModified.ToUniversalTime(),
            });

            if (page is BlogListing)
            {
                for (int i = 2; i <= blogPostPageCount; i++)
                {
                    sitemapNodes.Add(new SitemapNode($"{absoluteUrl}/{i}")
                    {
                        LastModified = lastModified.ToUniversalTime(),
                    });
                }
            }
        }

        return sitemapNodes;
    }
}
