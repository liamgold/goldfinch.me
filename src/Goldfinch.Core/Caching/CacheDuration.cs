namespace Goldfinch.Core.Caching;

/// <summary>
/// Cache durations in minutes, for use with Kentico's <c>CacheSettings</c>. Use these named
/// constants rather than magic numbers so cache lifetimes are consistent and self-documenting.
/// </summary>
public static class CacheDuration
{
    /// <summary>30 minutes — widget-level content and page-specific data.</summary>
    public const int HalfHour = 30;

    /// <summary>1 hour.</summary>
    public const int Hour = 60;

    /// <summary>12 hours.</summary>
    public const int HalfDay = 720;

    /// <summary>24 hours — global/shared data that changes infrequently (header, footer, SEO, taxonomy).</summary>
    public const int Day = 1440;
}
