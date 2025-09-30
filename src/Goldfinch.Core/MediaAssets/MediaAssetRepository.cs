using CMS.ContentEngine;
using CMS.Helpers;
using Goldfinch.Core.ContentTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.MediaAssets;

public class MediaAssetRepository
{
    private readonly IContentQueryExecutor _executor;
    private readonly IProgressiveCache _progressiveCache;

    public MediaAssetRepository(IProgressiveCache progressiveCache, IContentQueryExecutor executor)
    {
        _progressiveCache = progressiveCache;
        _executor = executor;
    }

    public async Task<MediaAssetContent?> GetMediaAssetContent(Guid identifier)
    {
        return await _progressiveCache.LoadAsync(async cs =>
        {
            var query = new ContentItemQueryBuilder()
                            .ForContentType(MediaAssetContent.CONTENT_TYPE_NAME,
                                  config => config
                                    .WithLinkedItems(1)
                                    .Where(where => where
                                            .WhereEquals(nameof(IContentQueryDataContainer.ContentItemGUID), identifier)));

            var results = await _executor.GetMappedResult<MediaAssetContent>(query);

            return results.FirstOrDefault();
        }, new CacheSettings(1440, nameof(MediaAssetRepository), nameof(GetMediaAssetContent), identifier));
    }
}
