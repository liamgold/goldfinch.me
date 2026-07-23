using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Web.Infrastructure;

/// <summary>
/// Configures the Forwarded Headers Middleware for the production proxy chain, where the
/// application runs behind Cloudflare and Azure App Service. This ensures the original request
/// scheme (HTTPS) and the real visitor IP are honoured — so URL/redirect generation and per-IP
/// rate limiting (the /api/ask limiter partitions on the client IP) behave correctly rather than
/// seeing the upstream proxy address.
/// </summary>
public static class ForwardedHeadersConfiguration
{
    public static IServiceCollection AddForwardedHeadersConfiguration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Cloudflare always sets the true visitor IP in CF-Connecting-IP — more reliable than
            // parsing a multi-hop X-Forwarded-For.
            options.ForwardedForHeaderName = "CF-Connecting-IP";

            // The origin is only reachable through the Cloudflare -> Azure App Service edge, and the
            // immediate upstream has no stable IP to pin, so trust the forwarding chain rather than
            // restricting to specific proxy addresses.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
