using System;

namespace Goldfinch.Core.SiteStats;

/// <summary>
/// Derived site-level stats shared across pages (home hero, about facts, etc.).
/// Kept in Core so Razor views and view components can use the same logic.
/// </summary>
public static class SiteStats
{
    /// <summary>Year Liam won his first Kentico MVP award.</summary>
    public const int FirstMvpYear = 2022;

    /// <summary>
    /// Number of consecutive Kentico MVP awards held, including the current year.
    /// Assumes the run is unbroken from <see cref="FirstMvpYear"/> onwards —
    /// if the streak ever breaks, promote to a Home content-type field.
    /// </summary>
    public static int KenticoMvpCount() =>
        Math.Max(0, DateTime.UtcNow.Year - FirstMvpYear + 1);
}
