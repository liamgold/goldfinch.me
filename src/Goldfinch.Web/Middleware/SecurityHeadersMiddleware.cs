using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Goldfinch.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    ///     Add security headers (not-CSP) middleware.
    /// </summary>
    /// <param name="builder">The Microsoft.AspNetCore.Builder.IApplicationBuilder to add the middleware to.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
