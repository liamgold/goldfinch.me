using System.Collections.Generic;
using System.Linq;
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
    // Relevance boosts encode the contract's ranking: title > summary > body, and within each
    // field an exact/near-exact phrase match outranks scattered individual term matches.
    private const float TITLE_PHRASE_BOOST = 10f;
    private const float TITLE_TERM_BOOST = 5f;
    private const float SUMMARY_PHRASE_BOOST = 4f;
    private const float SUMMARY_TERM_BOOST = 2f;
    private const float CONTENT_PHRASE_BOOST = 2f;
    private const float CONTENT_TERM_BOOST = 1f;

    // Allows a couple of intervening words so near-phrase matches still count as a phrase hit.
    private const int PHRASE_SLOP = 2;

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

        // Boosted text query across the indexed fields. Each field contributes a high-boost phrase
        // clause (exact/near-exact match) and a lower-boost term clause (any word), so a full-phrase
        // hit ranks above scattered term hits, and title beats summary beats body.
        var queryBuilder = new QueryBuilder(analyzer);
        var textQuery = new BooleanQuery();
        AddFieldClauses(textQuery, queryBuilder, BlogSearchConstants.FIELD_TITLE, query, TITLE_PHRASE_BOOST, TITLE_TERM_BOOST);
        AddFieldClauses(textQuery, queryBuilder, BlogSearchConstants.FIELD_SUMMARY, query, SUMMARY_PHRASE_BOOST, SUMMARY_TERM_BOOST);
        AddFieldClauses(textQuery, queryBuilder, BlogSearchConstants.FIELD_CONTENT, query, CONTENT_PHRASE_BOOST, CONTENT_TERM_BOOST);

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

    public int? GetReadingMinutes(string url)
    {
        var index = _indexManager.GetRequiredIndex(BlogSearchConstants.INDEX_NAME);

        return _searchService.UseSearcher(index, searcher =>
        {
            var topDocs = searcher.Search(new TermQuery(new Term(BaseDocumentProperties.URL, url)), 1);
            if (topDocs.ScoreDocs.Length == 0)
            {
                return (int?)null;
            }

            var doc = searcher.Doc(topDocs.ScoreDocs[0].Doc);
            return doc.GetField(BlogSearchConstants.FIELD_READING_MINUTES)?.GetInt32Value();
        });
    }

    private static void AddFieldClauses(BooleanQuery target, QueryBuilder queryBuilder, string field, string query, float phraseBoost, float termBoost)
    {
        // Phrase clause: rewards an exact/near-exact match of the whole query in this field.
        var phrase = queryBuilder.CreatePhraseQuery(field, query, PHRASE_SLOP);
        if (phrase is not null)
        {
            phrase.Boost = phraseBoost;
            target.Add(phrase, Occur.SHOULD);
        }

        // Term clause: matches any of the query words, for recall.
        var terms = queryBuilder.CreateBooleanQuery(field, query, Occur.SHOULD);
        if (terms is not null)
        {
            terms.Boost = termBoost;
            target.Add(terms, Occur.SHOULD);
        }
    }

    /// <summary>
    /// Produces a highlighted fragment with matched terms wrapped in literal <c>&lt;mark&gt;</c> tags
    /// over the raw field text. The client re-escapes everything except <c>&lt;mark&gt;</c> before
    /// rendering, so encoding here would double-encode (e.g. an apostrophe would render as
    /// <c>&amp;#39;</c>). Returns null when no term matched.
    /// </summary>
    private static string? Highlight(Highlighter highlighter, Analyzer analyzer, string field, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return highlighter.GetBestFragment(analyzer, field, text);
    }

    private static string SlugFromUrl(string url) =>
        url.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
}
