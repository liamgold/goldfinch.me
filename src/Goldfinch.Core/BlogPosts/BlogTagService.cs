using CMS.ContentEngine;
using CMS.Helpers;
using Goldfinch.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

public class BlogTagService : IBlogTagService
{
    /// <summary>
    /// Code name of the taxonomy created in the Kentico admin.
    /// </summary>
    private const string BLOG_TAGS_TAXONOMY_NAME = "BlogTags";

    private readonly ITaxonomyRetriever _taxonomyRetriever;
    private readonly IBlogPostService _blogPostService;
    private readonly IProgressiveCache _progressiveCache;

    public BlogTagService(
        ITaxonomyRetriever taxonomyRetriever,
        IBlogPostService blogPostService,
        IProgressiveCache progressiveCache)
    {
        _taxonomyRetriever = taxonomyRetriever;
        _blogPostService = blogPostService;
        _progressiveCache = progressiveCache;
    }

    public async Task<Guid?> ResolveTagSlugToGuid(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency($"{TaxonomyInfo.OBJECT_TYPE}|all");

            var taxonomyData = await _taxonomyRetriever.RetrieveTaxonomy(BLOG_TAGS_TAXONOMY_NAME, "en", cancellationToken);

            var tag = taxonomyData.Tags.FirstOrDefault(t =>
                string.Equals(t.Name, slug, StringComparison.OrdinalIgnoreCase));

            return tag?.Identifier;
        },
        new CacheSettings(CacheDuration.Day, nameof(BlogTagService), nameof(ResolveTagSlugToGuid), slug.ToLowerInvariant()));
    }

    public async Task<IReadOnlyList<(Tag Tag, int PostCount)>> GetTagsWithPostCounts(string languageName, CancellationToken cancellationToken = default)
    {
        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"{TaxonomyInfo.OBJECT_TYPE}|all",
                $"{TagInfo.OBJECT_TYPE}|all",
            ]);

            var taxonomyData = await _taxonomyRetriever.RetrieveTaxonomy(BLOG_TAGS_TAXONOMY_NAME, languageName, cancellationToken);

            // At this site's scale (~25 posts) counting in memory over the already-cached
            // full post set is cheaper than a query per tag.
            var allPosts = (await _blogPostService.GetAllBlogPosts()).ToList();

            var results = new List<(Tag Tag, int PostCount)>();
            foreach (var tag in taxonomyData.Tags)
            {
                var count = allPosts.Count(p =>
                    p.BlogPostTags?.Any(r => r.Identifier == tag.Identifier) == true);

                if (count > 0)
                {
                    results.Add((tag, count));
                }
            }

            return (IReadOnlyList<(Tag Tag, int PostCount)>)results;
        },
        new CacheSettings(CacheDuration.Hour, nameof(BlogTagService), nameof(GetTagsWithPostCounts), languageName));
    }

    public async Task<IEnumerable<Tag>> GetTagsByGuids(IEnumerable<Guid> tagGuids, string languageName, CancellationToken cancellationToken = default)
    {
        var guidList = tagGuids.ToList();
        if (guidList.Count == 0)
        {
            return Enumerable.Empty<Tag>();
        }

        return await _progressiveCache.LoadAsync(async (cs) =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency($"{TaxonomyInfo.OBJECT_TYPE}|all");

            return await _taxonomyRetriever.RetrieveTags(guidList, languageName, cancellationToken);
        },
        new CacheSettings(CacheDuration.Day, nameof(BlogTagService), nameof(GetTagsByGuids), languageName, string.Join(",", guidList.OrderBy(g => g))));
    }
}
