using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.ContentEngine;
using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Extensions;
using Kentico.Xperience.Lucene.Core;
using Kentico.Xperience.Lucene.Core.Indexing;
using Lucene.Net.Documents;

namespace Goldfinch.Core.Search;

/// <summary>
/// Lucene indexing strategy for <see cref="BlogPost"/> web pages. Indexes the title, summary,
/// resolved tag slugs, publish date, an estimated reading time, and the full rendered post body
/// (crawled and sanitized to plain text, since the body lives in Page Builder editable areas).
/// </summary>
public class BlogPostSearchIndexingStrategy : DefaultLuceneIndexingStrategy
{
    private readonly IContentQueryExecutor _queryExecutor;
    private readonly IWebPageUrlRetriever _urlRetriever;
    private readonly IBlogTagService _blogTagService;
    private readonly WebCrawlerService _webCrawler;
    private readonly WebScraperHtmlSanitizer _htmlSanitizer;

    public BlogPostSearchIndexingStrategy(
        IContentQueryExecutor queryExecutor,
        IWebPageUrlRetriever urlRetriever,
        IBlogTagService blogTagService,
        WebCrawlerService webCrawler,
        WebScraperHtmlSanitizer htmlSanitizer)
    {
        _queryExecutor = queryExecutor;
        _urlRetriever = urlRetriever;
        _blogTagService = blogTagService;
        _webCrawler = webCrawler;
        _htmlSanitizer = htmlSanitizer;
    }

    public override async Task<Document?> MapToLuceneDocumentOrNull(IIndexEventItemModel item)
    {
        // Only index BlogPost web pages; ignore everything else (and secured items).
        if (item is not IndexEventWebPageItemModel webPageItem ||
            !string.Equals(item.ContentTypeName, BlogPost.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var page = await GetPage(webPageItem.ItemGuid, webPageItem.WebsiteChannelName, webPageItem.LanguageName);
        if (page is null)
        {
            return null;
        }

        var title = page.BaseContentTitle ?? string.Empty;
        var summary = page.BaseContentShortDescription ?? string.Empty;

        // Resolve tag references to their code names (the ?tag= slugs).
        var tagSlugs = Array.Empty<string>();
        if (page.BlogPostTags?.Any() == true)
        {
            var tags = await _blogTagService.GetTagsByGuids(
                page.BlogPostTags.Select(t => t.Identifier), webPageItem.LanguageName);
            tagSlugs = tags.Select(t => t.Name).ToArray();
        }

        // Crawl the rendered page and reduce it to the post body text.
        var rawHtml = await _webCrawler.CrawlWebPage(page);
        var body = _htmlSanitizer.SanitizeHtmlDocument(rawHtml);

        var readingMinutes = EstimateReadingMinutes(body);

        var url = string.Empty;
        try
        {
            url = (await _urlRetriever.Retrieve(page)).RelativePath.ToAbsolutePath();
        }
        catch (Exception)
        {
            // Retrieve can throw if the page was deleted before the queued task ran; index an empty URL.
        }

        var document = new Document
        {
            new TextField(BlogSearchConstants.FIELD_TITLE, title, Field.Store.YES),
            new TextField(BlogSearchConstants.FIELD_SUMMARY, summary, Field.Store.YES),
            // Body is indexed for matching but not stored (results never echo the full body).
            new TextField(BlogSearchConstants.FIELD_CONTENT, body, Field.Store.NO),
            new StringField(BlogSearchConstants.FIELD_DATE, page.BlogPostDate.ToString("yyyy-MM-dd"), Field.Store.YES),
            new StoredField(BlogSearchConstants.FIELD_READING_MINUTES, readingMinutes),
            new StringField(BaseDocumentProperties.URL, url, Field.Store.YES),
        };

        // Tag slugs: indexed as exact terms (for ?tag= scoping) and stored (for result rendering).
        foreach (var slug in tagSlugs)
        {
            document.Add(new StringField(BlogSearchConstants.FIELD_TAGS, slug, Field.Store.YES));
        }

        return document;
    }

    private async Task<BlogPost?> GetPage(Guid itemGuid, string channelName, string languageName)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(BlogPost.CONTENT_TYPE_NAME, config => config
                .WithLinkedItems(1)
                .ForWebsite(channelName)
                .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), itemGuid))
                .TopN(1))
            .InLanguage(languageName);

        var result = await _queryExecutor.GetMappedWebPageResult<BlogPost>(builder);

        return result.FirstOrDefault();
    }

    private static int EstimateReadingMinutes(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return 1;
        }

        var wordCount = body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        return Math.Max(1, (int)Math.Ceiling((double)wordCount / BlogSearchConstants.WORDS_PER_MINUTE));
    }
}
