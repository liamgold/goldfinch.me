using System.Threading.Tasks;

namespace Goldfinch.Core.PublicSpeaking;

/// <summary>
/// Provides access to public speaking content.
/// </summary>
public interface IPublicSpeakingService
{
    /// <summary>
    /// Retrieves the public speaking page by its web page item ID.
    /// </summary>
    /// <param name="webPageItemID">The web page item ID of the public speaking page.</param>
    /// <returns>The matching <see cref="PublicSpeakingModel"/>, or <c>null</c> if not found.</returns>
    Task<PublicSpeakingModel?> GetPublicSpeakingPage(int webPageItemID);
}
