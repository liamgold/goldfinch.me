using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;

namespace Goldfinch.Core.WebPage;

public abstract class WebPageRepository(WebPageQueryTools tools)
{
    protected IContentQueryExecutor Executor { get; } = tools.Executor;

    protected IWebPageUrlRetriever UrlRetriever { get; } = tools.UrlRetriever;

    protected IWebsiteChannelContext WebsiteChannelContext { get; } = tools.WebsiteChannelContext;

    protected IProgressiveCache ProgressiveCache { get; } = tools.ProgressiveCache;

    protected CacheDependencyBuilder CacheDependencyBuilder { get; } = tools.CacheDependencyBuilder;
}
