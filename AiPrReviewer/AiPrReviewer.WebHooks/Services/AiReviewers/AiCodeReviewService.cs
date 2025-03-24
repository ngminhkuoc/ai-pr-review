using System.Text;
using System.Text.Json;
using AiPrReviewer.WebHooks.Services.AiReviewers.Models;
using AiPrReviewer.WebHooks.Services.Externals.AiAgents.Models;

namespace AiPrReviewer.WebHooks.Services.AiReviewers;

public class AiCodeReviewService : ICodeReviewService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AiCodeReviewService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ReviewCodeResultDto> ReviewCodeAsync(ReviewCodeDto dto)
    {
        var comments = new List<CodeCommentDto>();

        var client = _httpClientFactory.CreateClient("ai-agent");
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true, };

        foreach (var file in dto.Files)
        {
            var prompt = $$"""
Review the below code diff with the following rules:
- response format MUST be Json { "comment": "This is the comment", "fileName": "{{file.FileName}}", "position": 0 } WITHOUT ``` or ```json mark
- if the code is bad then give a comment (as an senior developer)
- if the code is good MUST say LGTM (without any further explanation)
- postion is the line number of the comment takes place
- the diff using unified diff format and content as below:
{{file.Diff}}
""";

            var requestBody = new { model = "qwen2.5-coder", prompt, max_tokens = 100, stream = false };
            var jsonPayload = JsonSerializer.Serialize(requestBody);

            using HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync("http://localhost:11434/api/generate", content);
            var result = await response.Content.ReadAsStringAsync();
           
            try
            {
                var resultObj = JsonSerializer.Deserialize<AiResponseDto>(result, options);
                var codeCommentText = resultObj?.Response ?? string.Empty;

                if (!string.IsNullOrEmpty(codeCommentText))
                {
                    var codeComment = JsonSerializer.Deserialize<CodeCommentDto>(codeCommentText, options);
                    if (!string.IsNullOrEmpty(codeComment?.Comment) && !codeComment.Comment.Equals("LGTM", StringComparison.OrdinalIgnoreCase))
                    {
                        comments.Add(codeComment);
                    }
                }
            }
            catch (JsonException)
            {
                continue;
            }
        }

        return new ReviewCodeResultDto
        {
            Comments = comments
        };
    }
}
