using System;
using System.Collections.Generic;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewModel
{
    public required Core.ContentTypes.Home Page { get; init; }

    public HomeFeaturedPost? FeaturedPost { get; init; }

    public IReadOnlyList<HomeRecentPost> RecentPosts { get; init; } = [];
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
