using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.ContentTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

public class BlogPostService : IBlogPostService
{
    private const int BLOG_LIST_PAGE_SIZE = 9;
    private const int LATEST_POSTS_COUNT = 4; // Need 4 to ensure 3 after filtering current page

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
        new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, _websiteChannelContext.IsPreview, nameof(BlogPostService), nameof(GetBlogPost), $"PageID-{webPageItemID}"));
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
        new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetLatestBlogPosts)));
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
        new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetBlogPosts), $"PageIndex-{pageIndex}"));
    }

    public async Task<IEnumerable<BlogPost>> GetAllBlogPosts()
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                );

            return await _executor.GetMappedWebPageResult<BlogPost>(queryBuilder);
        },
        new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, nameof(BlogPostService), nameof(GetAllBlogPosts)));
    }

    public async Task<int> GetBlogPageCount()
    {
        var allBlogPosts = (await GetAllBlogPosts()).ToList();

        return (int)Math.Ceiling((double)allBlogPosts.Count / BLOG_LIST_PAGE_SIZE);
    }
}
