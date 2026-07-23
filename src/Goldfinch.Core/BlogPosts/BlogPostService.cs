using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.Caching;
using Goldfinch.Core.ContentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

public class BlogPostService : IBlogPostService
{
    private const int BLOG_LIST_PAGE_SIZE = 9;
    private const int LATEST_POSTS_COUNT = 5; // Need 5 to ensure 4 after filtering current page

    private readonly IContentQueryExecutor _executor;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IProgressiveCache _progressiveCache;

    public BlogPostService(
        IContentQueryExecutor executor,
        IWebsiteChannelContext websiteChannelContext,
        IProgressiveCache progressiveCache)
    {
        _executor = executor;
        _websiteChannelContext = websiteChannelContext;
        _progressiveCache = progressiveCache;
    }

    public async Task<BlogPost?> GetBlogPost(int webPageItemID)
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder, new ContentQueryExecutionOptions
            {
                ForPreview = _websiteChannelContext.IsPreview,
            });

            return pages.FirstOrDefault();
        },
        new CacheSettings(CacheDuration.Hour, _websiteChannelContext.WebsiteChannelName, _websiteChannelContext.IsPreview, nameof(BlogPostService), nameof(GetBlogPost), $"PageID-{webPageItemID}"));
    }

    public async Task<IEnumerable<BlogPost>> GetLatestBlogPosts()
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .TopN(LATEST_POSTS_COUNT)
                    .OrderBy([new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending)])
                );

            return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder);
        },
        new CacheSettings(CacheDuration.Hour, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetLatestBlogPosts)));
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPosts(int pageIndex)
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var pageSize = BLOG_LIST_PAGE_SIZE;
            var offSet = pageSize * (pageIndex - 1);

            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .Offset(offSet, pageSize)
                    .OrderBy([new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending)])
                );

            return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder);
        },
        new CacheSettings(CacheDuration.Hour, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetBlogPosts), $"PageIndex-{pageIndex}"));
    }

    public async Task<IEnumerable<BlogPost>> GetAllBlogPosts()
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            // Cache for a day but depend on the channel's web pages, so a publish/edit invalidates
            // it immediately rather than serving stale content until the TTL expires. The full set
            // rarely changes, so this saves the "select all posts" query on the vast majority of
            // requests (blog list, home, tag counts, and the Ask candidate list all share it).
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|all",
            ]);

            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                );

            return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder);
        },
        new CacheSettings(CacheDuration.Day, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetAllBlogPosts)));
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPostsByTag(Guid tagGuid)
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereContainsTags(nameof(BlogPost.BlogPostTags), [tagGuid]))
                    .OrderBy([new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending)])
                );

            return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder, new ContentQueryExecutionOptions
            {
                ForPreview = _websiteChannelContext.IsPreview,
            });
        },
        new CacheSettings(CacheDuration.Hour, _websiteChannelContext.WebsiteChannelName, _websiteChannelContext.IsPreview, nameof(BlogPostService), nameof(GetBlogPostsByTag), tagGuid.ToString()));
    }

    public async Task<int> GetBlogPageCount()
    {
        var allBlogPosts = (await GetAllBlogPosts()).ToList();

        return (int)Math.Ceiling((double)allBlogPosts.Count / BLOG_LIST_PAGE_SIZE);
    }
}
