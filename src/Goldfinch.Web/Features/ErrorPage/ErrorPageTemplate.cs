using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.ErrorPage;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

[assembly: RegisterPageTemplate(
    identifier: "Goldfinch.ErrorPage_Default",
    name: "Error Page - Default",
    propertiesType: typeof(ErrorPageTemplateProperties),
    customViewName: "~/Features/ErrorPage/ErrorPage_Default.cshtml",
    ContentTypeNames = [ErrorPage.CONTENT_TYPE_NAME],
    Description = "",
    IconClass = ""
)]

namespace Goldfinch.Web.Features.ErrorPage;

public class ErrorPageTemplateProperties : IPageTemplateProperties
{
}
