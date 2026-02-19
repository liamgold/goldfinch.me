using System.ComponentModel;

namespace Goldfinch.Web.Components.Widgets.CodeBlock;

public enum CodeLanguage
{
    [Description("C#")]
    Csharp,
    [Description("JavaScript")]
    Javascript,
    [Description("TypeScript")]
    Typescript,
    [Description("CSS")]
    Css,
    [Description("YAML")]
    Yaml,
    [Description("XML")]
    Xml,
    [Description("Bash")]
    Bash,
}
