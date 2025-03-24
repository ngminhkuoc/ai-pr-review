namespace AiPrReviewer.WebHooks.Services.AiReviewers.Models;

public record ReviewCodeDto
{
    public IList<ReviewingFileDto> Files { get; init; } = [];
}