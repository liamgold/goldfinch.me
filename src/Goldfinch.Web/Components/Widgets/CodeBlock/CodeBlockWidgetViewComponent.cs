using Goldfinch.Web.Components.Shared;
using Goldfinch.Web.Components.Widgets.Base;
using Goldfinch.Web.Components.Widgets.CodeBlock;
using Goldfinch.Web.Extensions;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWidget(
    identifier: CodeBlockWidgetViewComponent.IDENTIFIER,
    viewComponentType: typeof(CodeBlockWidgetViewComponent),
    name: CodeBlockWidgetViewComponent.DISPLAY_NAME,
    propertiesType: typeof(CodeBlockWidgetProperties),
    Description = "Displays a code block.",
    IconClass = KenticoIcons.RECTANGLE_PARAGRAPH)]

namespace Goldfinch.Web.Components.Widgets.CodeBlock;

public class CodeBlockWidgetViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.CodeBlockWidget";
    public const string DISPLAY_NAME = "Code Block";
    private const string ViewName = "~/Components/Widgets/CodeBlock/CodeBlockWidget.cshtml";

    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public CodeBlockWidgetViewComponent(IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public IViewComponentResult Invoke(CodeBlockWidgetProperties properties)
    {
        if (properties == null || string.IsNullOrWhiteSpace(properties.Code))
        {
            return _pageBuilderDataContext.IsEditMode()
                ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Add code in the widget properties to display this block.")
                : Content(string.Empty);
        }

        var viewModel = new CodeBlockWidgetViewModel
        {
            CodeClassName = properties.Language,
            CodeText = properties.Code,
        };

        return View(ViewName, viewModel);
    }
}
