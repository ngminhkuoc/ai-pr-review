namespace AiPrReviewer.WebHooks.Services.AiReviewers.Models;

public record ReviewingFileDto
{
    public string FileName { get; init; } = string.Empty;
    public string Diff { get; init; } = string.Empty;
}