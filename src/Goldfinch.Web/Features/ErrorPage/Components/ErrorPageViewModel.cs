namespace Goldfinch.Web.Features.ErrorPage;

/// <param name="Page">The error page content item.</param>
/// <param name="RequestedPath">
/// The path the visitor asked for, when the error page is rendered through
/// status-code re-execution. Null when it isn't available. Untrusted input:
/// render it through Razor's encoding only, never Html.Raw.
/// </param>
public record ErrorPageViewModel(Core.ContentTypes.ErrorPage Page, string? RequestedPath);
