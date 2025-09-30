using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Goldfinch.Web.Components.Widgets.CodeBlock
{
    public class CodeBlockWidgetProperties : IWidgetProperties
    {
        public const string LanguageDataSource = "csharp;C#\r\njavascript;JavaScript\r\ntypescript;TypeScript\r\ncss;CSS\r\nyaml;YAML\r\nxml;XML\r\nbash;Bash";

        [DropDownComponent(Label = "Language", Order = 1, Options = LanguageDataSource)]
        public string Language { get; set; } = "csharp";

        [TextAreaComponent(Label = "Code", Order = 2)]
        public string Code { get; set; } = string.Empty;
    }
}