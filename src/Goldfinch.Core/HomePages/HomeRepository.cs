using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.HomePages;

public class HomeRepository : WebPageRepository
{
    public HomeRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<Home> GetHome()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            cs.CacheDependency = CacheDependencyBuilder
                .ForWebPageItems()
                    .ByContentType<Home>(WebsiteChannelContext.WebsiteChannelName)
                    .Builder()
                .Build();

            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(Home.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<Home>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(HomeRepository), nameof(GetHome)));
    }
}
