using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Web.Middleware;

public enum TrailingSlashMode
{
    DoNothing = 0,
    AlwaysAdd = 1,
    AlwaysRemove = 2,
}

public sealed class TrailingSlashOptions
{
    public TrailingSlashMode Mode { get; set; } = TrailingSlashMode.DoNothing;
}

public sealed class TrailingSlashMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TrailingSlashOptions _options;

    public TrailingSlashMiddleware(RequestDelegate next, IOptions<TrailingSlashOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? new TrailingSlashOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process GET requests to avoid issues with POST/PUT/DELETE
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var pathBase = context.Request.PathBase;
        var path = context.Request.Path;
        var query = context.Request.QueryString;

        // Skip processing for root path or when mode is DoNothing
        if (_options.Mode == TrailingSlashMode.DoNothing || !path.HasValue || path == "/")
        {
            await _next(context);
            return;
        }

        // Validate the path to prevent open redirect vulnerabilities
        if (!IsValidPath(path.Value!))
        {
            await _next(context);
            return;
        }

        var needsRedirect = false;
        PathString newPath = path;

        if (_options.Mode == TrailingSlashMode.AlwaysAdd && !path.Value!.EndsWith("/"))
        {
            // Don't add trailing slash to paths that look like files
            if (!LooksLikeFile(path.Value))
            {
                newPath = path.Add("/");
                needsRedirect = true;
            }
        }
        else if (_options.Mode == TrailingSlashMode.AlwaysRemove && path.Value!.EndsWith("/") && path.Value.Length > 1)
        {
            newPath = new PathString(path.Value.TrimEnd('/'));
            needsRedirect = true;
        }

        if (needsRedirect)
        {
            // Build safe relative URL
            var target = UriHelper.BuildRelative(pathBase, newPath, query);

            // Additional safety check - ensure the target is relative
            if (!target.StartsWith('/') || target.StartsWith("//"))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status308PermanentRedirect;
            context.Response.Headers.Location = target;
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Validates that the path is safe and doesn't contain suspicious patterns
    /// that could be used for open redirect attacks.
    /// </summary>
    private static bool IsValidPath(string path)
    {
        // Check for common open redirect patterns
        if (path.Contains("//") ||
            path.Contains("\\") ||
            path.Contains("%2F%2F", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("%5C", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines if a path looks like it points to a file (has an extension).
    /// </summary>
    private static bool LooksLikeFile(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        var lastSegment = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
        return !string.IsNullOrEmpty(lastSegment) && lastSegment.Contains('.');
    }
}

public static class TrailingSlashMiddlewareExtensions
{
    /// <summary>
    /// Registers the TrailingSlashOptions from configuration.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="config">The application configuration (usually builder.Configuration).</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddTrailingSlash(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TrailingSlashOptions>(config.GetSection("TrailingSlash"));
        return services;
    }

    /// <summary>
    /// Adds the trailing slash middleware to the request pipeline.
    /// </summary>
    /// <param name="builder">The Microsoft.AspNetCore.Builder.IApplicationBuilder to add the middleware to.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseTrailingSlashMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TrailingSlashMiddleware>();
    }
}
