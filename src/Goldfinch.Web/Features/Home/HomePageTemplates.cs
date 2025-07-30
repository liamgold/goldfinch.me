using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.Home;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

[assembly: RegisterPageTemplate(
    identifier: "Goldfinch.HomePage_Default",
    name: "Home Page - Default",
    propertiesType: typeof(HomePageTemplateProperties),
    customViewName: "~/Features/Home/HomePage_Default.cshtml",
    ContentTypeNames = [Home.CONTENT_TYPE_NAME],
    Description = "",
    IconClass = ""
)]

namespace Goldfinch.Web.Features.Home;

public class HomePageTemplateProperties : IPageTemplateProperties
{
}
