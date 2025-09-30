using Goldfinch.Core.ContentTypes;
using System.Collections.Generic;

namespace Goldfinch.Core.PublicSpeaking;

public class PublicSpeakingModel
{
    public required PublicSpeakingPage Page { get; set; }

    public required IReadOnlyList<SpeakingEngagementYear> Years { get; set; }
}

public class SpeakingEngagementYear
{
    public int Year { get; set; }

    public required IReadOnlyList<SpeakingEngagement> SpeakingEngagements { get; set; }
}