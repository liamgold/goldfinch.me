using System.Collections.Generic;

namespace Goldfinch.Web.Features.Ask;

/// <summary>Request body for <c>POST /api/ask</c>.</summary>
public class AskRequest
{
    public string? Question { get; set; }

    /// <summary>Prior conversation turns, held client-side and replayed for follow-up context.</summary>
    public List<AskTurnDto>? History { get; set; }
}

/// <summary>A prior conversation turn as sent by the client.</summary>
public class AskTurnDto
{
    public string? Role { get; set; }

    public string? Content { get; set; }
}
