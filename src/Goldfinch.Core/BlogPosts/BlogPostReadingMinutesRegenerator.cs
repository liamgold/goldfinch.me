using System;
using System.Threading;
using System.Threading.Tasks;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using Goldfinch.Core.Search;

namespace Goldfinch.Core.BlogPosts;

/// <inheritdoc cref="IBlogPostReadingMinutesRegenerator"/>
public class BlogPostReadingMinutesRegenerator : IBlogPostReadingMinutesRegenerator
{
    private readonly IInfoProvider<ContentItemInfo> _contentItemInfoProvider;
    private readonly IContentItemDataInfoRetriever _contentItemDataInfoRetriever;
    private readonly IContentLanguageRetriever _contentLanguageRetriever;

    public BlogPostReadingMinutesRegenerator(
        IInfoProvider<ContentItemInfo> contentItemInfoProvider,
        IContentItemDataInfoRetriever contentItemDataInfoRetriever,
        IContentLanguageRetriever contentLanguageRetriever)
    {
        _contentItemInfoProvider = contentItemInfoProvider;
        _contentItemDataInfoRetriever = contentItemDataInfoRetriever;
        _contentLanguageRetriever = contentLanguageRetriever;
    }

    public async Task<int> RegenerateAsync(int contentItemId, string languageName, CancellationToken cancellationToken = default)
    {
        var contentItem = await _contentItemInfoProvider.GetAsync(contentItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Content item {contentItemId} was not found.");

        var language = await _contentLanguageRetriever.GetContentLanguageOrThrow(languageName, cancellationToken);

        // isLatest: true picks up the draft currently being edited, falling back to the published
        // version once there's no pending draft — matches what the editor sees on screen.
        var data = await _contentItemDataInfoRetriever.GetContentItemData(
            contentItem, language.ContentLanguageID, isLatest: true, cancellationToken);

        data.TryGetValue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataVisualBuilderWidgets), out var widgetsValue);

        var body = PageBuilderTextExtractor.ExtractText(widgetsValue as string);

        return ReadingTimeEstimator.EstimateMinutes(body);
    }
}
