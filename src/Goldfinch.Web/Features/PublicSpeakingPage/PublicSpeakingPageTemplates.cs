using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.PublicSpeakingPage;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

[assembly: RegisterPageTemplate(
    identifier: "Goldfinch.PublicSpeakingPage_Default",
    name: "Public Speaking Page - Default",
    propertiesType: typeof(PublicSpeakingPageTemplateProperties),
    customViewName: "~/Features/PublicSpeakingPage/PublicSpeakingPage_Default.cshtml",
    ContentTypeNames = [PublicSpeakingPage.CONTENT_TYPE_NAME],
    Description = "",
    IconClass = ""
)]

namespace Goldfinch.Web.Features.PublicSpeakingPage;

public class PublicSpeakingPageTemplateProperties : IPageTemplateProperties
{
}
