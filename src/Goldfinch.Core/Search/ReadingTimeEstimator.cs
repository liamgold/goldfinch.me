using System;

namespace Goldfinch.Core.Search;

/// <summary>
/// Word-count/200wpm reading-time estimate. Used by the Lucene indexing strategy against the
/// full crawled post body, and reused as a fallback (against the shorter summary) wherever a
/// post hasn't been indexed yet.
/// </summary>
public static class ReadingTimeEstimator
{
    public static int EstimateMinutes(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 1;
        }

        var wordCount = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        return Math.Max(1, (int)Math.Ceiling((double)wordCount / BlogSearchConstants.WORDS_PER_MINUTE));
    }
}
