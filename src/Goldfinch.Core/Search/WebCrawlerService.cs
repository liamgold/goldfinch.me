using System;
using System.Net.Http;
using System.Threading.Tasks;
using CMS.Core;
using CMS.Websites;

namespace Goldfinch.Core.Search;

/// <summary>
/// Fetches the rendered HTML of a web page so its Page Builder body can be extracted for indexing.
/// The base URL is read from the <c>WebCrawlerBaseUrl</c> app setting and must match the website
/// channel being indexed (localhost in development, the public URL in production).
/// </summary>
public class WebCrawlerService
{
    private readonly HttpClient _httpClient;
    private readonly IEventLogService _log;
    private readonly IWebPageUrlRetriever _urlRetriever;

    public WebCrawlerService(
        HttpClient httpClient,
        IEventLogService log,
        IWebPageUrlRetriever urlRetriever,
        IAppSettingsService appSettingsService)
    {
        var baseUrl = appSettingsService["WebCrawlerBaseUrl"];

        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GoldfinchSearchCrawler");

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        _log = log;
        _urlRetriever = urlRetriever;
    }

    /// <summary>
    /// Crawls the rendered HTML for a web page item. Returns an empty string on any failure
    /// (logged to the event log) so indexing of other items is unaffected.
    /// </summary>
    public async Task<string> CrawlWebPage(IWebPageFieldsSource page)
    {
        try
        {
            var url = await _urlRetriever.Retrieve(page);
            var path = url.RelativePath.TrimStart('~').TrimStart('/');

            return await CrawlPage(path);
        }
        catch (Exception ex)
        {
            _log.LogException(
                nameof(WebCrawlerService),
                nameof(CrawlWebPage),
                ex,
                $"Tree Path: {page.SystemFields.WebPageItemTreePath}");
        }

        return string.Empty;
    }

    public async Task<string> CrawlPage(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _log.LogException(
                nameof(WebCrawlerService),
                nameof(CrawlPage),
                ex,
                $"Url: {url}");
        }

        return string.Empty;
    }
}
