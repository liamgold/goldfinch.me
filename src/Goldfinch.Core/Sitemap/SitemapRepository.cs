using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using Sidio.Sitemap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.Sitemap;

public class SitemapRepository : WebPageRepository
{
    public SitemapRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<List<SitemapNode>> GetSitemap()
    {
        return await ProgressiveCache.LoadAsync(async cs =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"webpageitem|bychannel|Goldfinch|all",
            ]);

            var sitemapNodes = await GetSitemapNodes();

            return sitemapNodes;
        }, new CacheSettings(1440, nameof(SitemapRepository), nameof(GetSitemap)));
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
                query.ForWebsite(WebsiteChannelContext.WebsiteChannelName);
            });

        var pages = await Executor.GetMappedWebPageResult<IWebPageFieldsSource>(builder);

        var blogPostCount = pages.OfType<BlogPost>().Count();
        var blogPostPageCount = (int)Math.Ceiling((double)blogPostCount / 6);

        foreach (var page in pages)
        {
            var pageUrl = await UrlRetriever.Retrieve(page);
            var relativeUrl = pageUrl.RelativePath.Replace("~/", "/");
            var absoluteUrl = $"https://www.goldfinch.me{relativeUrl}";

            var metadata = ContentItemLanguageMetadataInfo.Provider.Get()
                .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID), page.SystemFields.ContentItemID)
                .FirstOrDefault();

            var lastModified = metadata.ContentItemLanguageMetadataModifiedWhen;

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
