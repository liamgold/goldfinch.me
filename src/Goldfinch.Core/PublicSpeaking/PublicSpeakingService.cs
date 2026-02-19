using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.ContentTypes;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.PublicSpeaking;

public class PublicSpeakingService : IPublicSpeakingService
{
    private readonly IContentQueryExecutor _executor;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IProgressiveCache _progressiveCache;

    public PublicSpeakingService(
        IContentQueryExecutor executor,
        IWebsiteChannelContext websiteChannelContext,
        IProgressiveCache progressiveCache)
    {
        _executor = executor;
        _websiteChannelContext = websiteChannelContext;
        _progressiveCache = progressiveCache;
    }

    public async Task<PublicSpeakingModel?> GetPublicSpeakingPage(int webPageItemID)
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(PublicSpeakingPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await _executor.GetMappedWebPageResult<PublicSpeakingPage>(queryBuilder);

            var listingPage = pages.FirstOrDefault();

            if (listingPage == null)
            {
                return null;
            }

            queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(SpeakingEngagement.CONTENT_TYPE_NAME);

            var speakingEngagements = await _executor.GetMappedResult<SpeakingEngagement>(queryBuilder);

            var groupedSpeakingEngagements = speakingEngagements
                .GroupBy(s => s.EventDate.Year)
                .OrderByDescending(g => g.Key)
                .Select(g => new SpeakingEngagementYear
                {
                    Year = g.Key,
                    SpeakingEngagements = g.OrderByDescending(s => s.EventDate).ToList()
                })
                .ToList();

            return new PublicSpeakingModel
            {
                Page = listingPage,
                Years = groupedSpeakingEngagements
            };
        },
        new CacheSettings(60, _websiteChannelContext.WebsiteChannelName, nameof(PublicSpeakingService), nameof(GetPublicSpeakingPage), $"PageID-{webPageItemID}"));
    }
}
