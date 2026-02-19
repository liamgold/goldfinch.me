using Goldfinch.Web.Components.Widgets.Base;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Goldfinch.Web.Components.Widgets.CodeBlock;

[FormCategory(Label = "Content", Order = 0, Collapsible = true, IsCollapsed = false)]
public class CodeBlockWidgetProperties : IWidgetProperties
{
    [DropDownComponent(Label = "Language", Order = 1, DataProviderType = typeof(DropdownEnumOptionProvider<CodeLanguage>))]
    public string Language { get; set; } = "csharp";

    [TextAreaComponent(Label = "Code", Order = 2)]
    public string Code { get; set; } = string.Empty;
}
