using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

public class AskPostSelector : IAskPostSelector
{
    /// <summary>Upper bound on how many posts to feed into the answer step.</summary>
    private const int MaxSelected = 5;

    /// <summary>Excerpts are trimmed to keep the selection prompt small and cheap.</summary>
    private const int MaxExcerptChars = 300;

    private const string SystemPrompt =
        "You are a retrieval assistant for a technical blog about Kentico and .NET development. " +
        "You are given a reader's question and a numbered list of blog posts (title + short excerpt). " +
        "Pick the posts most likely to contain the answer — the fewest that suffice, at most five. " +
        "If none are relevant, pick none. " +
        "Respond with ONLY a JSON array of the chosen post numbers (for example [2, 5]) and nothing else. " +
        "If none are relevant, respond with [].";

    private readonly IAskContentService _contentService;
    private readonly IAskChatClient _chatClient;

    public AskPostSelector(IAskContentService contentService, IAskChatClient chatClient)
    {
        _contentService = contentService;
        _chatClient = chatClient;
    }

    public async Task<IReadOnlyList<int>> SelectRelevantPostIds(
        string question,
        IReadOnlyList<AskTurn> history,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return [];
        }

        var candidates = await _contentService.GetCandidates();
        if (candidates.Count == 0)
        {
            return [];
        }

        var userPrompt = BuildUserPrompt(question, history, candidates);
        var response = await _chatClient.Complete(
            SystemPrompt,
            [new AskTurn { Role = AskTurn.UserRole, Content = userPrompt }],
            cancellationToken);

        return MapNumbersToPostIds(response, candidates);
    }

    private static string BuildUserPrompt(string question, IReadOnlyList<AskTurn> history, IReadOnlyList<AskCandidatePost> candidates)
    {
        var builder = new StringBuilder();

        // Put the (large, rarely-changing) candidate list first so it forms a stable prefix that
        // Azure OpenAI's automatic prompt caching can reuse across requests; the variable history
        // and question go last. Reordering these would defeat the cache.
        builder.AppendLine("Posts:");
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            builder
                .Append(i + 1)
                .Append(". ")
                .Append(candidate.Title)
                .Append(" — ")
                .AppendLine(Truncate(candidate.Excerpt, MaxExcerptChars));
        }
        builder.AppendLine();

        // Give the model the recent exchange so a follow-up like "what about a UI tab instead?"
        // can be resolved against what was already discussed.
        if (history.Count > 0)
        {
            builder.AppendLine("Conversation so far:");
            foreach (var turn in history)
            {
                var speaker = turn.Role == AskTurn.AssistantRole ? "Assistant" : "Reader";
                builder.Append(speaker).Append(": ").AppendLine(turn.Content.Trim());
            }
            builder.AppendLine();
        }

        builder.Append("Question: ").AppendLine(question.Trim());

        return builder.ToString();
    }

    /// <summary>
    /// Parses the model's JSON array of 1-based post numbers and maps them back to
    /// <c>WebPageItemID</c>s, ignoring anything out of range or duplicated. Returns an empty list
    /// if the response can't be parsed, so a malformed reply degrades to "no answer" rather than
    /// throwing.
    /// </summary>
    private static IReadOnlyList<int> MapNumbersToPostIds(string response, IReadOnlyList<AskCandidatePost> candidates)
    {
        var json = ExtractJsonArray(response);
        if (json is null)
        {
            return [];
        }

        int[]? numbers;
        try
        {
            numbers = JsonSerializer.Deserialize<int[]>(json);
        }
        catch (JsonException)
        {
            return [];
        }

        if (numbers is null)
        {
            return [];
        }

        var postIds = new List<int>();
        foreach (var number in numbers)
        {
            var index = number - 1;
            if (index < 0 || index >= candidates.Count)
            {
                continue;
            }

            var postId = candidates[index].WebPageItemID;
            if (!postIds.Contains(postId))
            {
                postIds.Add(postId);
            }

            if (postIds.Count == MaxSelected)
            {
                break;
            }
        }

        return postIds;
    }

    /// <summary>
    /// Extracts the outermost JSON array from the model response, tolerating stray prose or code
    /// fences around it. Returns null when no array is present.
    /// </summary>
    private static string? ExtractJsonArray(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var start = response.IndexOf('[');
        var end = response.LastIndexOf(']');

        if (start < 0 || end <= start)
        {
            return null;
        }

        return response.Substring(start, end - start + 1);
    }

    private static string Truncate(string value, int maxChars)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxChars)
        {
            return value;
        }

        return value.Substring(0, maxChars).TrimEnd() + "…";
    }
}
