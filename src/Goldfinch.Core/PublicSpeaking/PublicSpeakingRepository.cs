using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.WebPage;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.PublicSpeaking;

public class PublicSpeakingRepository : WebPageRepository
{
    public PublicSpeakingRepository(WebPageQueryTools tools) : base(tools)
    {
    }

    public async Task<PublicSpeakingModel> GetPublicSpeakingPage(int webPageItemID)
    {
        return await ProgressiveCache.LoadAsync(async (cs) =>
        {
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(PublicSpeakingPage.CONTENT_TYPE_NAME, queryParameters => queryParameters
                    .ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID))
                    .TopN(1)
                );

            var pages = await Executor.GetMappedWebPageResult<PublicSpeakingPage>(queryBuilder);

            var listingPage = pages.FirstOrDefault();

            queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(SpeakingEngagement.CONTENT_TYPE_NAME);

            var speakingEngagements = await Executor.GetMappedResult<SpeakingEngagement>(queryBuilder);

            var groupedSpeakingEngagements = speakingEngagements
                .GroupBy(s => s.EventDate.Year)
                .OrderByDescending(g => g.Key)
                .Select(g => new SpeakingEngagementYear
                {
                    Year = g.Key,
                    SpeakingEngagements = g.OrderByDescending(s => s.EventDate).ToList()
                })
                .ToList();

            var publicSpeakingModel = new PublicSpeakingModel
            {
                Page = listingPage,
                Years = groupedSpeakingEngagements
            };

            return publicSpeakingModel;
        },
        new CacheSettings(60, WebsiteChannelContext.WebsiteChannelName, nameof(PublicSpeakingRepository), nameof(GetPublicSpeakingPage), $"PageID-{webPageItemID}"));
    }
}