using System;
using System.Collections.Generic;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewModel
{
    public required Core.ContentTypes.Home Page { get; init; }

    public HomeFeaturedPost? FeaturedPost { get; init; }

    public IReadOnlyList<HomeRecentPost> RecentPosts { get; init; } = [];

    /// <summary>Total number of published blog posts — drives the hero stats row.</summary>
    public int TotalPostCount { get; init; }

    /// <summary>Year of the oldest published blog post, or null if there are none.</summary>
    public int? FirstPostYear { get; init; }

    /// <summary>Number of consecutive Kentico MVP awards held, including the current year.</summary>
    public int KenticoMvpCount { get; init; }

    /// <summary>Full years spent coding since first picking it up.</summary>
    public int YearsCoding { get; init; }
}

public record HomeFeaturedPost(
    string Title,
    string Summary,
    string Url,
    string Filename,
    DateTime PublishedOn,
    int ReadingMinutes);

public record HomeRecentPost(
    string Title,
    string Summary,
    string Url,
    string Filename,
    DateTime PublishedOn,
    int ReadingMinutes);
