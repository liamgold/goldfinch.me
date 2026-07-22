using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask;
using Goldfinch.Core.Ask.Models;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Features.Ask;

/// <summary>
/// Backs the site-wide "Ask" feature: grounded, cited Q&amp;A over the blog's own posts.
/// Selects relevant posts, gathers their content, and generates an answer from that content only.
/// Returns 503 when the feature isn't configured (no Azure OpenAI deployment) so the widget stays
/// hidden.
/// </summary>
[ApiController]
[Route("api/ask")]
public class AskApiController : ControllerBase
{
    private const int MaxQuestionLength = 500;

    // Bound client-supplied history so a single request can't be made arbitrarily expensive
    // (the rate limiter caps request count, not size).
    private const int MaxHistoryItems = 12;
    private const int MaxHistoryMessageLength = 2000;

    private readonly IAskChatClient _chatClient;
    private readonly IAskService _askService;

    public AskApiController(IAskChatClient chatClient, IAskService askService)
    {
        _chatClient = chatClient;
        _askService = askService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromBody] AskRequest? request, CancellationToken cancellationToken)
    {
        if (!_chatClient.IsConfigured)
        {
            return StatusCode(503, new { error = "unavailable" });
        }

        var question = request?.Question?.Trim();
        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new { error = "missing_question" });
        }
        if (question.Length > MaxQuestionLength)
        {
            return BadRequest(new { error = "question_too_long" });
        }

        var history = (request?.History ?? [])
            .Where(turn => turn is not null && !string.IsNullOrWhiteSpace(turn.Content))
            .TakeLast(MaxHistoryItems)
            .Select(turn => new AskTurn
            {
                Role = turn.Role == AskTurn.AssistantRole ? AskTurn.AssistantRole : AskTurn.UserRole,
                Content = Truncate(turn.Content!.Trim(), MaxHistoryMessageLength),
            })
            .ToList();

        var result = await _askService.Ask(question, history, cancellationToken);

        return Ok(new
        {
            answered = result.Answered,
            answer = result.Answer,
            sources = result.Sources.Select(source => new { title = source.Title, url = source.Url }),
        });
    }

    private static string Truncate(string value, int maxChars) =>
        value.Length <= maxChars ? value : value[..maxChars];
}
