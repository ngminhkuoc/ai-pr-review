namespace AiPrReviewer.WebHooks.Services.AiReviewers.Models;

public record CodeCommentDto
{
    public string FileName { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public int Position { get; init; }
}