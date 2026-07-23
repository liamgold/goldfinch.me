using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Thin abstraction over the chat-completion model used by the "Ask" feature. Keeps the OpenAI
/// SDK out of the feature's selection/answer logic and lets the endpoint check
/// <see cref="IsConfigured"/> to hide the feature when no model is configured.
/// </summary>
public interface IAskChatClient
{
    /// <summary>True when an endpoint, key, and deployment are all configured.</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Sends a system prompt plus a sequence of conversation turns (ending with the current user
    /// turn) to the model and returns the completion text. A single-turn call passes a one-item list.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when the client is not configured.</exception>
    Task<string> Complete(string systemPrompt, IReadOnlyList<AskTurn> messages, CancellationToken cancellationToken = default);
}
