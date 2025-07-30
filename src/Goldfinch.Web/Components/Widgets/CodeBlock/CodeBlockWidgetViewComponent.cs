using Goldfinch.Web.Components.Widgets.CodeBlock;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

[assembly: RegisterWidget(CodeBlockWidgetViewComponent.IDENTIFIER, typeof(CodeBlockWidgetViewComponent), "Code Block", typeof(CodeBlockWidgetProperties), Description = "Displays a code block.", IconClass = "icon-rectangle-paragraph")]
namespace Goldfinch.Web.Components.Widgets.CodeBlock
{
    public class CodeBlockWidgetViewComponent : ViewComponent
    {
        public const string IDENTIFIER = "Goldfinch.CodeBlockWidget";

        private readonly string ViewName = "~/Components/Widgets/CodeBlock/CodeBlockWidget.cshtml";

        public CodeBlockWidgetViewComponent()
        {
        }

        public ViewViewComponentResult Invoke(CodeBlockWidgetProperties properties)
        {
            var viewModel = new CodeBlockWidgetViewModel
            {
                CodeClassName = properties.Language,
                CodeText = properties.Code,
            };

            return View(ViewName, viewModel);
        }
    }
}