using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;

namespace Goldfinch.Core.WebPage;

public class WebPageQueryTools(
    IContentQueryExecutor executor,
    IWebPageUrlRetriever urlRetriever,
    IWebsiteChannelContext websiteChannelContext,
    IProgressiveCache progressiveCache,
    ICacheDependencyBuilderFactory cacheDependencyBuilderFactory)
{
    public IContentQueryExecutor Executor { get; } = executor;

    public IWebPageUrlRetriever UrlRetriever { get; } = urlRetriever;

    public IWebsiteChannelContext WebsiteChannelContext { get; } = websiteChannelContext;

    public IProgressiveCache ProgressiveCache { get; } = progressiveCache;

    public CacheDependencyBuilder CacheDependencyBuilder { get; } = cacheDependencyBuilderFactory.Create();
}
