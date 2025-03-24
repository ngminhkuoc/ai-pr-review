using AiPrReviewer.WebHooks.Services.AiReviewers.Models;

namespace AiPrReviewer.WebHooks.Services.AiReviewers;

public class AiCodeReviewService : ICodeReviewService
{
    //private readonly IAiAgentClient _aiAgentClient;

    //public AiCodeReviewService(IAiAgentClient aiAgentClient)
    //{
    //    _aiAgentClient = aiAgentClient;
    //}

    public async Task<ReviewCodeResultDto> ReviewCodeAsync(ReviewCodeDto dto)
    {
        await Task.Delay(5);
        //var comments = new List<(string, string)>();

        //foreach (var file in files)
        //{
        //    string prompt = $"Review this code and suggest improvements:\n\n{file}";
        //    var requestBody = new { model = "codet5", prompt, max_tokens = 100 };

        //    string jsonPayload = JsonSerializer.Serialize(requestBody);
        //    var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        //    var response = await _aiAgentClient.PostAsync("http://localhost:5000/generate", content);
        //    string aiResponse = await response.Content.ReadAsStringAsync();

        //    comments.Add((file, aiResponse));
        //}

        return new ReviewCodeResultDto
        {
            //Comments = comments
        };
    }
}
