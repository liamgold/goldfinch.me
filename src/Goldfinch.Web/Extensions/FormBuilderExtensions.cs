using Microsoft.AspNetCore.Http;

namespace Goldfinch.Web.Extensions;

public static class FormBuilderExtensions
{
    private const string UseUnobtrusiveValidationKey = "UseUnobtrusiveValidation";

    public static bool IsUnobstrusiveValidationEnabled(this HttpContext context)
    {
        return context.Items.ContainsKey(UseUnobtrusiveValidationKey);
    }

    public static void EnableUnobstrusiveValidation(this HttpContext context)
    {
        if (!context.IsUnobstrusiveValidationEnabled())
        {
            context.Items[UseUnobtrusiveValidationKey] = true;
        }
    }
}