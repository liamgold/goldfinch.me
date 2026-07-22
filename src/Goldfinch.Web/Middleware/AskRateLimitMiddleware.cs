using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Goldfinch.Web.Middleware;

/// <summary>
/// Lightweight in-memory per-IP fixed-window rate limiter for the public <c>POST /api/ask</c>
/// endpoint, bounding abuse of the paid AI endpoint. Adequate for a single-instance deployment;
/// swap for a shared store if the site ever scales out. Non-ask requests pass straight through.
/// </summary>
public class AskRateLimitMiddleware
{
    private const int PermitLimit = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    private static readonly ConcurrentDictionary<string, Counter> Counters = new();

    private readonly RequestDelegate _next;

    public AskRateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAskRequest(context) && IsRateLimited(context))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = ((int)Window.TotalSeconds).ToString();
            await context.Response.WriteAsJsonAsync(new { error = "rate_limited" });
            return;
        }

        await _next(context);
    }

    private static bool IsAskRequest(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/api/ask", StringComparison.OrdinalIgnoreCase);

    private static bool IsRateLimited(HttpContext context)
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTimeOffset.UtcNow;

        var counter = Counters.GetOrAdd(key, _ => new Counter());
        lock (counter)
        {
            if (now - counter.WindowStart >= Window)
            {
                counter.WindowStart = now;
                counter.Count = 0;
            }

            counter.Count++;
            return counter.Count > PermitLimit;
        }
    }

    private sealed class Counter
    {
        public DateTimeOffset WindowStart { get; set; } = DateTimeOffset.UtcNow;
        public int Count { get; set; }
    }
}

public static class AskRateLimitMiddlewareExtensions
{
    /// <summary>Adds the per-IP rate limiter for the <c>/api/ask</c> endpoint.</summary>
    public static IApplicationBuilder UseAskRateLimit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AskRateLimitMiddleware>();
    }
}
