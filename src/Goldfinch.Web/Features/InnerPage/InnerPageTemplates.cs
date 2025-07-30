using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.InnerPage;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

[assembly: RegisterPageTemplate(
    identifier: "Goldfinch.InnerPage_Default",
    name: "Inner Page - Default",
    propertiesType: typeof(InnerPageTemplateProperties),
    customViewName: "~/Features/InnerPage/InnerPage_Default.cshtml",
    ContentTypeNames = [InnerPage.CONTENT_TYPE_NAME],
    Description = "",
    IconClass = ""
)]

namespace Goldfinch.Web.Features.InnerPage;

public class InnerPageTemplateProperties : IPageTemplateProperties
{
}
