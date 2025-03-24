
namespace AiPrReviewer.WebHooks.Services.AiReviewers.Models;

public record ReviewCodeResultDto
{
    public IList<CodeCommentDto> Comments { get; init; } = [];
}