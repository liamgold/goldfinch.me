using System.Collections.Generic;

namespace Goldfinch.Core.Ask.Models;

/// <summary>The outcome of an "Ask" query: the generated answer and the posts it drew from.</summary>
public class AskResult
{
    /// <summary>
    /// True when the answer was generated from matching posts; false when nothing relevant was
    /// found (in which case <see cref="Answer"/> holds a friendly "not covered" message).
    /// </summary>
    public bool Answered { get; set; }

    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<AskSource> Sources { get; set; } = [];
}
