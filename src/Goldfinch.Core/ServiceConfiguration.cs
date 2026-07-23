using Goldfinch.Core.Ask;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ErrorPages;
using Goldfinch.Core.PublicSpeaking;
using Goldfinch.Core.Search;
using Goldfinch.Core.SEO;
using Goldfinch.Core.Sitemap;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Core;

public static class ServiceConfiguration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IBreadcrumbService, BreadcrumbService>()
            .AddSingleton<IBlogPostService, BlogPostService>()
            .AddSingleton<IBlogTagService, BlogTagService>()
            .AddSingleton<IAskContentService, AskContentService>()
            .AddSingleton<IAskPageSourceProvider, AskPageSourceProvider>()
            .AddSingleton<IAskPostSelector, AskPostSelector>()
            .AddSingleton<IAskSourceGatherer, AskSourceGatherer>()
            .AddSingleton<IAskService, AskService>()
            .AddSingleton<IBlogPostReadingMinutesRegenerator, BlogPostReadingMinutesRegenerator>()
            .AddScoped<IErrorPageService, ErrorPageService>()
            .AddSingleton<IPublicSpeakingService, PublicSpeakingService>()
            .AddSingleton<ISitemapService, SitemapService>()
            .AddSingleton<WebScraperHtmlSanitizer>()
            .AddSingleton<ILuceneBlogSearchService, LuceneBlogSearchService>();

        // Typed HttpClient used by the search indexing strategy to crawl rendered post bodies.
        services.AddHttpClient<WebCrawlerService>();

        return services;
    }
}
