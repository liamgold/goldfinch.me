using System.Collections.Generic;

namespace Goldfinch.Core.SEO.Constants;

/// <summary>
/// The author's social profile URLs, used for the structured data <c>sameAs</c> list.
/// </summary>
public static class SocialProfiles
{
    public const string GITHUB = "https://github.com/liamgold/";

    public const string LINKEDIN = "https://www.linkedin.com/in/liamgoldfinch/";

    public const string X = "https://x.com/LiamGoldfinch";

    public const string YOUTUBE = "https://www.youtube.com/@Liam_Goldfinch";

    /// <summary>
    /// All profiles, in the order they appear in the <c>sameAs</c> list.
    /// </summary>
    public static readonly IReadOnlyList<string> All = [GITHUB, LINKEDIN, X, YOUTUBE];
}
