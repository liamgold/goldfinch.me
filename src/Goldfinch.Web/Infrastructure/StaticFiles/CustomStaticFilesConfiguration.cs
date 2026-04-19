using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Goldfinch.Web.Infrastructure.StaticFiles;

public static class CustomStaticFilesConfiguration
{
    private static readonly IReadOnlyCollection<string> _staticPaths =
    [
        "/sitefiles/dist/assets",
        "/fonts",
    ];

    /// <summary>
    ///     Configures the <see cref="StaticFileOptions"/> to allow custom MIME types and add 'Cache-Control' headers
    ///     for hash-busted asset bundles and self-hosted fonts.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure the options within.</param>
    /// <param name="configuration">The application configuration.</param>
    public static IServiceCollection AddCustomStaticFilesConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var cacheOptions = configuration
            .GetSection(StaticFilesCacheOptions.SectionName)
            .Get<StaticFilesCacheOptions>() ?? new StaticFilesCacheOptions();

        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".webmanifest"] = "application/manifest+json";

        var cacheControl = cacheOptions.Immutable
            ? $"public, max-age={cacheOptions.CacheDurationSeconds}, immutable"
            : $"public, max-age={cacheOptions.CacheDurationSeconds}";

        return serviceCollection.Configure<StaticFileOptions>(options =>
        {
            options.ContentTypeProvider = provider;
            options.OnPrepareResponse = context =>
            {
                if (!cacheOptions.Enabled)
                {
                    return;
                }

                foreach (var path in _staticPaths)
                {
                    if (context.Context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Context.Response.Headers.CacheControl = cacheControl;
                        return;
                    }
                }
            };
        });
    }
}
