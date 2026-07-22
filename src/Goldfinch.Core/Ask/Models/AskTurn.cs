namespace Goldfinch.Core.Ask.Models;

/// <summary>
/// A single prior turn in an Ask conversation. The transcript is held client-side and sent with
/// each request (the server stays stateless), so follow-up questions can be resolved in context.
/// </summary>
public class AskTurn
{
    /// <summary><c>"user"</c> or <c>"assistant"</c>.</summary>
    public required string Role { get; set; }

    public required string Content { get; set; }

    public const string UserRole = "user";
    public const string AssistantRole = "assistant";
}
