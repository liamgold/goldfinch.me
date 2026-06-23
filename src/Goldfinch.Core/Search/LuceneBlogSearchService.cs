using System.Collections.Generic;
using System.Linq;
using System.Net;
using Kentico.Xperience.Lucene.Core;
using Kentico.Xperience.Lucene.Core.Indexing;
using Kentico.Xperience.Lucene.Core.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Util;

namespace Goldfinch.Core.Search;

public class LuceneBlogSearchService : ILuceneBlogSearchService
{
    // Relevance boosts encode the contract's ranking: title > summary > body.
    private const float TITLE_BOOST = 5f;
    private const float SUMMARY_BOOST = 2f;
    private const float CONTENT_BOOST = 1f;

    private readonly ILuceneSearchService _searchService;
    private readonly ILuceneIndexManager _indexManager;

    public LuceneBlogSearchService(
        ILuceneSearchService searchService,
        ILuceneIndexManager indexManager)
    {
        _searchService = searchService;
        _indexManager = indexManager;
    }

    public IReadOnlyList<BlogSearchResult> SearchPosts(string query, string? tag, int limit)
    {
        var index = _indexManager.GetRequiredIndex(BlogSearchConstants.INDEX_NAME);
        var analyzer = index.LuceneAnalyzer;

        // Boosted text query across the indexed fields (title > summary > body).
        var queryBuilder = new QueryBuilder(analyzer);
        var textQuery = new BooleanQuery();
        AddBoostedClause(textQuery, queryBuilder, BlogSearchConstants.FIELD_TITLE, query, TITLE_BOOST);
        AddBoostedClause(textQuery, queryBuilder, BlogSearchConstants.FIELD_SUMMARY, query, SUMMARY_BOOST);
        AddBoostedClause(textQuery, queryBuilder, BlogSearchConstants.FIELD_CONTENT, query, CONTENT_BOOST);

        // No analyzable tokens (e.g. query was only stop words/punctuation) — nothing to match.
        if (!textQuery.GetClauses().Any())
        {
            return [];
        }

        // Scope to a tag when provided (exact term on the stored tag slug).
        Query finalQuery = textQuery;
        if (!string.IsNullOrWhiteSpace(tag))
        {
            finalQuery = new BooleanQuery
            {
                { textQuery, Occur.MUST },
                { new TermQuery(new Term(BlogSearchConstants.FIELD_TAGS, tag)), Occur.MUST },
            };
        }

        return _searchService.UseSearcher(index, searcher =>
        {
            var topDocs = searcher.Search(finalQuery, limit);

            // Highlight against the text query only (not the tag scope) and over the whole field.
            var highlighter = new Highlighter(
                new SimpleHTMLFormatter("<mark>", "</mark>"),
                new QueryScorer(textQuery))
            {
                TextFragmenter = new NullFragmenter(),
            };

            var results = new List<BlogSearchResult>(topDocs.ScoreDocs.Length);
            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);

                var title = doc.Get(BlogSearchConstants.FIELD_TITLE) ?? string.Empty;
                var summary = doc.Get(BlogSearchConstants.FIELD_SUMMARY) ?? string.Empty;
                var url = doc.Get(BaseDocumentProperties.URL) ?? string.Empty;

                results.Add(new BlogSearchResult
                {
                    Slug = SlugFromUrl(url),
                    Title = title,
                    Summary = summary,
                    Url = url,
                    Date = doc.Get(BlogSearchConstants.FIELD_DATE) ?? string.Empty,
                    Tags = doc.GetValues(BlogSearchConstants.FIELD_TAGS) ?? [],
                    ReadingMinutes = doc.GetField(BlogSearchConstants.FIELD_READING_MINUTES)?.GetInt32Value() ?? 1,
                    HighlightedTitle = Highlight(highlighter, analyzer, BlogSearchConstants.FIELD_TITLE, title),
                    HighlightedSummary = Highlight(highlighter, analyzer, BlogSearchConstants.FIELD_SUMMARY, summary),
                });
            }

            return (IReadOnlyList<BlogSearchResult>)results;
        });
    }

    private static void AddBoostedClause(BooleanQuery target, QueryBuilder queryBuilder, string field, string query, float boost)
    {
        var clause = queryBuilder.CreateBooleanQuery(field, query, Occur.SHOULD);
        if (clause is null)
        {
            return;
        }

        clause.Boost = boost;
        target.Add(clause, Occur.SHOULD);
    }

    /// <summary>
    /// Produces a highlighted fragment with matched terms wrapped in <c>&lt;mark&gt;</c>. The field
    /// text is HTML-encoded first so <c>&lt;mark&gt;</c> is the only markup in the result (the client
    /// renders this via innerHTML and trusts nothing else). Returns null when no term matched.
    /// </summary>
    private static string? Highlight(Highlighter highlighter, Analyzer analyzer, string field, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var encoded = WebUtility.HtmlEncode(text);

        return highlighter.GetBestFragment(analyzer, field, encoded);
    }

    private static string SlugFromUrl(string url) =>
        url.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
}
