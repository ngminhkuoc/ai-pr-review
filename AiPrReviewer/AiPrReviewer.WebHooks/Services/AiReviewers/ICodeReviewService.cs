
using AiPrReviewer.WebHooks.Services.AiReviewers.Models;

namespace AiPrReviewer.WebHooks.Services.AiReviewers;

public interface ICodeReviewService
{
    Task<ReviewCodeResultDto> ReviewCodeAsync(ReviewCodeDto dto);
}
