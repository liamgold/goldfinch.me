using Goldfinch.Core.BlogListings;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ErrorPages;
using Goldfinch.Core.HomePages;
using Goldfinch.Core.InnerPages;
using Goldfinch.Core.MediaAssets;
using Goldfinch.Core.PublicSpeaking;
using Goldfinch.Core.SEO;
using Goldfinch.Core.Sitemap;
using Goldfinch.Core.WebPage;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Core;

public static class ServiceConfiguration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<BlogListingRepository>()
            .AddSingleton<BreadcrumbService>()
            .AddSingleton<BlogPostRepository>()
            .AddSingleton<ErrorPageRepository>()
            .AddSingleton<HomeRepository>()
            .AddSingleton<InnerPageRepository>()
            .AddSingleton<MediaAssetRepository>()
            .AddSingleton<PublicSpeakingRepository>()
            .AddSingleton<SitemapRepository>()

            .AddTransient<WebPageQueryTools>();
    }
}
