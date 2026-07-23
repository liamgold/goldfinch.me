using Goldfinch.Core.Ask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Goldfinch.Web.Infrastructure.Ai;

public static class AskConfiguration
{
    /// <summary>
    /// Wires the "Ask" AI Q&A feature: binds the Azure OpenAI options and registers the chat client.
    /// The content/selection/orchestration services are registered in <c>AddCoreServices</c>; per-IP
    /// rate limiting is applied by <c>AskRateLimitMiddleware</c>. Spend is bounded outside the app
    /// (Azure deployment TPM quota + Cost Management budget). The feature hides when unconfigured.
    /// </summary>
    public static IServiceCollection AddAskFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));
        services.AddSingleton<IAskChatClient, AzureOpenAIChatClient>();

        return services;
    }
}
