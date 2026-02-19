using Goldfinch.Core.ContentTypes;
using System.Threading.Tasks;

namespace Goldfinch.Core.ErrorPages;

/// <summary>
/// Provides access to error page content items.
/// </summary>
public interface IErrorPageService
{
    /// <summary>
    /// Retrieves the error page configured for the given HTTP status code.
    /// </summary>
    /// <param name="errorCode">The HTTP status code (e.g. 404, 500).</param>
    /// <returns>The matching <see cref="ErrorPage"/>, or <c>null</c> if none is configured for that code.</returns>
    Task<ErrorPage?> GetErrorPageByCode(int errorCode);

    /// <summary>
    /// Retrieves an error page by its web page item ID.
    /// </summary>
    /// <param name="webPageItemID">The web page item ID of the error page.</param>
    /// <returns>The matching <see cref="ErrorPage"/>, or <c>null</c> if not found.</returns>
    Task<ErrorPage?> GetErrorPageById(int webPageItemID);
}
