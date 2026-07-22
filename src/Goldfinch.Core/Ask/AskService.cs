using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

public class AskService : IAskService
{
    /// <summary>Per-post body cap fed into the answer prompt, to bound token cost on long posts.</summary>
    private const int MaxBodyChars = 8000;

    /// <summary>How many recent turns to keep as context, to bound token growth over a conversation.</summary>
    private const int MaxHistoryTurns = 6;

    private const string AnswerSystemPrompt =
        "You are the assistant for Liam Goldfinch's technical blog about Kentico and .NET development. " +
        "Answer using only the information in the blog posts provided below — never use outside knowledge " +
        "or invent details. If the posts don't cover the question, say you don't have anything on the blog " +
        "about that and suggest browsing or searching the blog.\n\n" +
        "Write like a knowledgeable colleague explaining it in your own words — synthesise the information, " +
        "don't quote or paraphrase the posts sentence by sentence. Answer the question directly and get " +
        "straight to the substance.\n\n" +
        "Write in short, flowing prose paragraphs. Your answer is displayed as plain text, so Markdown " +
        "is NOT rendered — bullets, dashes, numbers and '#' headings would show up as literal characters. " +
        "If you need to convey a sequence of steps, describe them in a sentence or two of prose rather than " +
        "a list.\n\n" +
        "Do NOT:\n" +
        "- open with \"Yes\", \"No\", or \"Sure\"\n" +
        "- open with or lean on phrases like \"From the blog post …\", \"According to …\", \"The post says …\"\n" +
        "- name post titles inline or otherwise cite sources in the text — the sources are listed separately " +
        "beneath your answer, so the reader can already see them\n" +
        "- use Markdown, bullet points, numbered lists, dashes, or headings to structure the answer\n" +
        "- output raw URLs\n\n" +
        "Keep it concise and friendly, and build naturally on earlier turns in the conversation rather than " +
        "restating context the reader already has.";

    private const string NoAnswerMessage =
        "I couldn't find anything on the blog about that. Try rephrasing, or browse the posts on the blog.";

    private readonly IAskChatClient _chatClient;
    private readonly IAskPostSelector _selector;
    private readonly IAskSourceGatherer _gatherer;

    public AskService(
        IAskChatClient chatClient,
        IAskPostSelector selector,
        IAskSourceGatherer gatherer)
    {
        _chatClient = chatClient;
        _selector = selector;
        _gatherer = gatherer;
    }

    public async Task<AskResult> Ask(
        string question,
        IReadOnlyList<AskTurn> history,
        CancellationToken cancellationToken = default)
    {
        if (!_chatClient.IsConfigured || string.IsNullOrWhiteSpace(question))
        {
            return new AskResult { Answered = false, Answer = NoAnswerMessage };
        }

        // Keep only the most recent turns so token cost doesn't grow unbounded over a conversation.
        var recentHistory = history.Count > MaxHistoryTurns
            ? history.Skip(history.Count - MaxHistoryTurns).ToList()
            : history;

        var postIds = await _selector.SelectRelevantPostIds(question, recentHistory, cancellationToken);
        if (postIds.Count == 0)
        {
            return new AskResult { Answered = false, Answer = NoAnswerMessage };
        }

        var sources = await _gatherer.GetSources(postIds, cancellationToken);
        if (sources.Count == 0)
        {
            return new AskResult { Answered = false, Answer = NoAnswerMessage };
        }

        // The prior turns give continuity; the final user turn carries the freshly gathered posts
        // so this answer is grounded in (and cites) real content.
        var messages = new List<AskTurn>(recentHistory)
        {
            new() { Role = AskTurn.UserRole, Content = BuildAnswerPrompt(question, sources) },
        };

        var answer = await _chatClient.Complete(AnswerSystemPrompt, messages, cancellationToken);

        return new AskResult
        {
            Answered = true,
            Answer = answer,
            Sources = sources
                .Select(source => new AskSource { Title = source.Title, Url = source.Url })
                .ToList(),
        };
    }

    private static string BuildAnswerPrompt(string question, IReadOnlyList<AskSourcePost> sources)
    {
        var builder = new StringBuilder();
        builder.Append("Question: ").AppendLine(question.Trim());
        builder.AppendLine();
        builder.AppendLine("Blog posts:");

        foreach (var source in sources)
        {
            builder.AppendLine();
            builder.Append("### ").AppendLine(source.Title);
            builder.AppendLine(Truncate(source.Body, MaxBodyChars));
        }

        return builder.ToString();
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
