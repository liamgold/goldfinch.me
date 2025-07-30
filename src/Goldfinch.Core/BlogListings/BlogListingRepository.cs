using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogListings;

public class BlogListingRepository : WebPageRepository
{
    public BlogListingRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<BlogListing> GetBlogListing()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(BlogListing.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<BlogListing>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(BlogListingRepository), nameof(GetBlogListing)));
    }
}
