using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// <see cref="IAskChatClient"/> backed by an Azure OpenAI chat deployment, reached through Azure's
/// OpenAI-compatible "v1" API using the stock <c>OpenAI</c> SDK (the same package Kentico ships).
/// The v1 API is version-less, which avoids the dated <c>api-version</c> mismatches that affect
/// newer models. Builds the client once from configured options; when any option is missing it
/// stays unconfigured (<see cref="IsConfigured"/> is <c>false</c>) rather than throwing at startup.
/// </summary>
public class AzureOpenAIChatClient : IAskChatClient
{
    private readonly ChatClient? _chatClient;

    public AzureOpenAIChatClient(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;

        if (!string.IsNullOrWhiteSpace(settings.Endpoint)
            && !string.IsNullOrWhiteSpace(settings.ApiKey)
            && !string.IsNullOrWhiteSpace(settings.ChatDeploymentName))
        {
            _chatClient = new ChatClient(
                model: settings.ChatDeploymentName,
                credential: new ApiKeyCredential(settings.ApiKey),
                options: new OpenAIClientOptions { Endpoint = BuildV1Endpoint(settings.Endpoint) });
        }
    }

    public bool IsConfigured => _chatClient is not null;

    public async Task<string> Complete(string systemPrompt, IReadOnlyList<AskTurn> messages, CancellationToken cancellationToken = default)
    {
        if (_chatClient is null)
        {
            throw new InvalidOperationException("The Ask feature's Azure OpenAI client is not configured.");
        }

        var chatMessages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
        foreach (var message in messages)
        {
            chatMessages.Add(message.Role == AskTurn.AssistantRole
                ? new AssistantChatMessage(message.Content)
                : new UserChatMessage(message.Content));
        }

        ClientResult<ChatCompletion> result = await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);
        var completion = result.Value;

        return completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;
    }

    /// <summary>
    /// Builds the Azure OpenAI "v1" API base URL from the configured resource endpoint, appending
    /// <c>openai/v1/</c> when it isn't already present so the setting can be just the resource root.
    /// </summary>
    private static Uri BuildV1Endpoint(string endpoint)
    {
        var trimmed = endpoint.TrimEnd('/');

        if (trimmed.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase))
        {
            // Already points at the v1 API.
        }
        else if (trimmed.EndsWith("/openai", StringComparison.OrdinalIgnoreCase))
        {
            trimmed += "/v1";
        }
        else
        {
            trimmed += "/openai/v1";
        }

        return new Uri(trimmed + "/");
    }
}
