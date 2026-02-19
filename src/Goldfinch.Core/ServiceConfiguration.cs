using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ErrorPages;
using Goldfinch.Core.MediaAssets;
using Goldfinch.Core.PublicSpeaking;
using Goldfinch.Core.SEO;
using Goldfinch.Core.Sitemap;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Core;

public static class ServiceConfiguration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IBreadcrumbService, BreadcrumbService>()
            .AddSingleton<IBlogPostService, BlogPostService>()
            .AddScoped<IErrorPageService, ErrorPageService>()
            .AddSingleton<IMediaAssetService, MediaAssetService>()
            .AddSingleton<IPublicSpeakingService, PublicSpeakingService>()
            .AddSingleton<ISitemapService, SitemapService>();
    }
}
