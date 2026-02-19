using Kentico.PageBuilder.Web.Mvc;

namespace Goldfinch.Web.Extensions;

public static class PageBuilderExtensions
{
    public static bool IsEditMode(this IPageBuilderDataContextRetriever retriever) =>
        retriever.Retrieve().GetMode() == PageBuilderMode.Edit;
}
