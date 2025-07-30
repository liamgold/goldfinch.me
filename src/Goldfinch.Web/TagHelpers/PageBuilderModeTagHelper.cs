using Goldfinch.Web.Extensions;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Goldfinch.Web.TagHelpers;

public class PageBuilderModeTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public PageBuilderModeTagHelper(IHttpContextAccessor httpContextAccessor, IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isFormPresent = _httpContextAccessor.HttpContext?.IsUnobstrusiveValidationEnabled() ?? false;
        var mode = _pageBuilderDataContext.Retrieve().GetMode();

        output.TagName = null;

        if (mode == PageBuilderMode.Off && !isFormPresent)
        {
            output.SuppressOutput();
        }

        return;
    }
}
