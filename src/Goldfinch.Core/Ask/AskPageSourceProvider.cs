using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.Ask.Models;
using Goldfinch.Core.Caching;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Extensions;
using Goldfinch.Core.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Core.Ask;

public class AskPageSourceProvider : IAskPageSourceProvider
{
    /// <summary>Excerpt length used for the selection prompt when a page has no short description.</summary>
    private const int MaxExcerptChars = 300;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebsiteChannelContext _channelContext;
    private readonly IProgressiveCache _progressiveCache;
    private readonly WebScraperHtmlSanitizer _htmlSanitizer;

    public AskPageSourceProvider(
        IServiceScopeFactory scopeFactory,
        IWebsiteChannelContext channelContext,
        IProgressiveCache progressiveCache,
        WebScraperHtmlSanitizer htmlSanitizer)
    {
        _scopeFactory = scopeFactory;
        _channelContext = channelContext;
        _progressiveCache = progressiveCache;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<IReadOnlyList<AskCandidate>> GetPageCandidates(CancellationToken cancellationToken = default)
    {
        return await _progressiveCache.LoadAsync(async cs =>
        {
            // A publish/edit anywhere in the channel invalidates this, so the crawl only re-runs when
            // content actually changes (or after a day) — not on every question.
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"webpageitem|bychannel|{_channelContext.WebsiteChannelName}|all",
            ]);

            // Resolve the crawler (a typed HttpClient) and query services from a scope rather than
            // capturing them on this singleton, so the HttpClient handler lifetime is respected.
            using var scope = _scopeFactory.CreateScope();
            var executor = scope.ServiceProvider.GetRequiredService<IContentQueryExecutor>();
            var urlRetriever = scope.ServiceProvider.GetRequiredService<IWebPageUrlRetriever>();
            var crawler = scope.ServiceProvider.GetRequiredService<WebCrawlerService>();

            var candidates = new List<AskCandidate>();
            candidates.AddRange(await BuildInnerPageCandidates(executor, urlRetriever, crawler));
            candidates.AddRange(await BuildSpeakingCandidates(executor, urlRetriever, crawler));

            return (IReadOnlyList<AskCandidate>)candidates;
        },
        new CacheSettings(CacheDuration.Day, _channelContext.WebsiteChannelName, nameof(AskPageSourceProvider), nameof(GetPageCandidates)));
    }

    private async Task<List<AskCandidate>> BuildInnerPageCandidates(
        IContentQueryExecutor executor, IWebPageUrlRetriever urlRetriever, WebCrawlerService crawler)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(InnerPage.CONTENT_TYPE_NAME, parameters => parameters
                .ForWebsite(_channelContext.WebsiteChannelName));

        var pages = await executor.GetMappedWebPageResult<InnerPage>(builder);

        var candidates = new List<AskCandidate>();
        foreach (var page in pages)
        {
            var candidate = await BuildCandidate(
                page, page.BaseContentTitle, page.BaseContentShortDescription, urlRetriever, crawler);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private async Task<List<AskCandidate>> BuildSpeakingCandidates(
        IContentQueryExecutor executor, IWebPageUrlRetriever urlRetriever, WebCrawlerService crawler)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(PublicSpeakingPage.CONTENT_TYPE_NAME, parameters => parameters
                .ForWebsite(_channelContext.WebsiteChannelName));

        var pages = await executor.GetMappedWebPageResult<PublicSpeakingPage>(builder);

        var candidates = new List<AskCandidate>();
        foreach (var page in pages)
        {
            var candidate = await BuildCandidate(
                page, page.BaseContentTitle, page.BaseContentShortDescription, urlRetriever, crawler);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    /// <summary>
    /// Crawls + sanitises a page into a candidate with its body and URL resolved. Returns null when
    /// the crawl yields no text, so an empty source is never offered to the model.
    /// </summary>
    private async Task<AskCandidate?> BuildCandidate(
        IWebPageFieldsSource page, string? title, string? shortDescription,
        IWebPageUrlRetriever urlRetriever, WebCrawlerService crawler)
    {
        var body = _htmlSanitizer.SanitizeHtmlDocument(await crawler.CrawlWebPage(page));
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var url = string.Empty;
        try
        {
            url = (await urlRetriever.Retrieve(page)).RelativePath.ToAbsolutePath();
        }
        catch
        {
            // Leave the URL empty if resolution fails; the body is still usable for grounding.
        }

        var excerpt = !string.IsNullOrWhiteSpace(shortDescription)
            ? shortDescription
            : Truncate(body, MaxExcerptChars);

        return new AskCandidate
        {
            WebPageItemID = page.SystemFields.WebPageItemID,
            Title = title ?? string.Empty,
            Excerpt = excerpt,
            Url = url,
            Body = body,
        };
    }

    private static string Truncate(string value, int maxChars) =>
        value.Length <= maxChars ? value : value.Substring(0, maxChars).TrimEnd() + "…";
}
