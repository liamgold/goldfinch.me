using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.ErrorPages;

public class ErrorPageRepository : WebPageRepository
{
    public ErrorPageRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<ErrorPage> GetErrorPage(int webPageItemID)
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(ErrorPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<ErrorPage>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(ErrorPageRepository), nameof(GetErrorPage), $"PageID-{webPageItemID}"));
    }

    /// <summary>
    ///     Returns the site 404 error page document.
    /// </summary>
    public async Task<ErrorPage> Get404Page()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(ErrorPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(ErrorPage.ErrorCode), 404))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<ErrorPage>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(ErrorPageRepository), nameof(Get404Page)));
    }

    /// <summary>
    ///     Returns the site 500 error page document.
    /// </summary>
    public async Task<ErrorPage> Get500Page()
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(ErrorPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(ErrorPage.ErrorCode), 500))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<ErrorPage>(queryBuilder);

            return pages.FirstOrDefault();
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(ErrorPageRepository), nameof(Get500Page)));
    }
}
