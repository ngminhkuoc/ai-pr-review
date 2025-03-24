using System.Text.Json;
using AiPrReviewer.WebHooks.Services.AiReviewers;
using AiPrReviewer.WebHooks.Services.Externals.GitHub;
using AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AiPrReviewer.WebHooks.Handlers;

[ApiController]
[Route("webhook/github")]
public class GitHubWebhookHandler : ControllerBase
{
    private static readonly string[] allowActions = ["opened", "synchronize"];

    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] JsonElement payload,
        [FromServices] IGitHubApiService gitHubApiService,
        [FromServices] IOptions<GitHubApiOptions> gitHubApiOptions,
        [FromServices] ICodeReviewService aiReviewerService
        )
    {
        var action = payload.GetProperty("action").GetString();
        if (allowActions.All(a => !string.Equals(a, action, StringComparison.OrdinalIgnoreCase)))
        {
            return Ok(); // Ignore other events
        }

        var repoName = payload.GetProperty("repository").GetProperty("full_name").GetString();
        if (string.IsNullOrEmpty(repoName))
        {
            return BadRequest();
        }

        var prNumber = payload.GetProperty("pull_request").GetProperty("number").GetInt32();
        if (prNumber <= 0)
        {
            return BadRequest();
        }

        var owner = gitHubApiOptions.Value.Owner;
        var files = await gitHubApiService.GetChangedFilesAsync(owner, repoName, prNumber);
        //var reviewComments = await _aiReviewService.ReviewCode(files);

        //await gitHubApiService.PostCommentAsync(owner, repoName, prNumber, new GitHubComment());
        return Ok();
    }
}
