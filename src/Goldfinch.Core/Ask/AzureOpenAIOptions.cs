namespace Goldfinch.Core.Ask;

/// <summary>
/// Configuration for the Azure OpenAI deployment backing the "Ask" feature. Bound from the
/// <c>AzureOpenAI</c> configuration section — the endpoint and deployment name live in
/// appsettings (tokenised in Production), the API key in .NET User Secrets locally.
/// When any value is missing the feature treats itself as unconfigured and degrades gracefully.
/// </summary>
public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>The Azure OpenAI resource endpoint, e.g. <c>https://my-resource.openai.azure.com/</c>.</summary>
    public string? Endpoint { get; set; }

    /// <summary>The API key for the resource. Set via User Secrets locally; never commit it.</summary>
    public string? ApiKey { get; set; }

    /// <summary>The name of the deployed chat model (e.g. a <c>gpt-4o-mini</c> deployment).</summary>
    public string? ChatDeploymentName { get; set; }
}
