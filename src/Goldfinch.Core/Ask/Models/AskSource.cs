namespace Goldfinch.Core.Ask.Models;

/// <summary>A cited source shown alongside an answer: the post title and a link to it.</summary>
public class AskSource
{
    public required string Title { get; set; }

    public required string Url { get; set; }
}
