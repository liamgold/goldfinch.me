using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.InnerPages;

public class InnerPageRepository : WebPageRepository
{
    public InnerPageRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<InnerPage?> GetInnerPage(int webPageItemID)
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(InnerPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<InnerPage>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(InnerPageRepository), nameof(GetInnerPage), $"PageID-{webPageItemID}"));
    }

    public async Task<IEnumerable<InnerPage>> GetAllInnerPages()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(InnerPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                );

            var pages = await Executor.GetMappedWebPageResult<InnerPage>(queryBuilder);

            return pages.ToList();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(InnerPageRepository), nameof(GetAllInnerPages)));
    }
}