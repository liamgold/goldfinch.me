using Goldfinch.Core.ContentTypes;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Core.MediaAssets;

/// <summary>
/// Provides access to media asset content items stored in the content hub.
/// </summary>
public interface IMediaAssetService
{
    /// <summary>
    /// Retrieves a media asset content item by its content item GUID.
    /// </summary>
    /// <param name="identifier">The GUID of the media asset content item.</param>
    /// <returns>The matching <see cref="MediaAssetContent"/>, or <c>null</c> if not found.</returns>
    Task<MediaAssetContent?> GetMediaAssetContent(Guid identifier);
}
