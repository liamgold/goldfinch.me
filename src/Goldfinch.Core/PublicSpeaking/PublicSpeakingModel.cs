using Goldfinch.Core.ContentTypes;
using System.Collections.Generic;

namespace Goldfinch.Core.PublicSpeaking;

public class PublicSpeakingModel
{
    public PublicSpeakingPage Page { get; set; }

    public IReadOnlyList<SpeakingEngagementYear> Years { get; set; }
}

public class SpeakingEngagementYear
{
    public int Year { get; set; }

    public IReadOnlyList<SpeakingEngagement> SpeakingEngagements { get; set; }
}