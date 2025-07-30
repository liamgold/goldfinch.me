using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

public class BlogPostRepository : WebPageRepository
{
    public BlogPostRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<BlogPost> GetBlogPost(int webPageItemID)
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<BlogPost>(queryBuilder, new ContentQueryExecutionOptions
            {
                ForPreview = WebsiteChannelContext.IsPreview,
            });

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, WebsiteChannelContext.IsPreview, nameof(BlogPostRepository), nameof(GetBlogPost), $"PageID-{webPageItemID}"));
    }

    public async Task<IEnumerable<BlogPost>> GetLatestBlogPosts()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .TopN(4)
                    .OrderBy(new[] { new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending) })
                );

            var pages = await Executor.GetMappedWebPageResult<BlogPost>(queryBuilder);

            return pages;
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(BlogPostRepository), nameof(GetLatestBlogPosts)));
    }

    public async Task<IEnumerable<BlogPost>> GetBlogPosts(int pageIndex)
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var pageSize = 6;
            var offSet = pageSize * (pageIndex - 1);

            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Offset(offSet, pageSize)
                    .OrderBy(new[] { new OrderByColumn(nameof(BlogPost.BlogPostDate), OrderDirection.Descending) })
                );

            var pages = await Executor.GetMappedWebPageResult<BlogPost>(queryBuilder);

            return pages;
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(BlogPostRepository), nameof(GetBlogPosts), $"PageIndex-{pageIndex}"));
    }

    public async Task<IEnumerable<BlogPost>> GetAllBlogPosts()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogPost.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                );

            var pages = await Executor.GetMappedWebPageResult<BlogPost>(queryBuilder);

            return pages;
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(BlogPostRepository), nameof(GetAllBlogPosts)));
    }

    public async Task<int> GetBlogPageCount()
    {
        var allBlogPosts = (await GetAllBlogPosts()).ToList();

        return (int)Math.Ceiling((double)allBlogPosts.Count / 6);
    }
}
