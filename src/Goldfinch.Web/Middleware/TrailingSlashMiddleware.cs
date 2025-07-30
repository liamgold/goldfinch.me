using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Goldfinch.Web.Middleware;

public class TrailingSlashMiddleware
{
    private readonly RequestDelegate _next;

    public TrailingSlashMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        try
        {
            /// Modes
            /// 0 - Do nothing
            /// 1 - Always add trailing slash
            /// 2 - Always remove trailing slash
            _ = int.TryParse(configuration["TrailingSlash:Mode"], out int mode);
            if (mode > 0)
            {
                var urlPath = context.Request.Path.ToString();

                if (urlPath.Equals("/"))
                {
                    await _next(context);
                    return;
                }

                if (mode == 1)
                {
                    if (!urlPath.EndsWith("/"))
                    {
                        context.Response.Redirect($"{urlPath}/", true);
                    }
                }
                if (mode == 2)
                {
                    if (urlPath.EndsWith("/"))
                    {
                        context.Response.Redirect(urlPath.TrimEnd('/'), true);
                    }
                }
            }
        }
        catch
        {
        }

        await _next(context);
    }
}

public static class TrailingSlashMiddlewareExtensions
{
    /// <summary>
    ///     Add the trailing slash middleware.
    /// </summary>
    /// <param name="builder">The Microsoft.AspNetCore.Builder.IApplicationBuilder to add the middleware to.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseTrailingSlashMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TrailingSlashMiddleware>();
    }
}
