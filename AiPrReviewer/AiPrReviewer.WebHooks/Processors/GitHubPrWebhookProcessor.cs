using AiPrReviewer.WebHooks.Services.AiReviewers;
using AiPrReviewer.WebHooks.Services.AiReviewers.Models;
using AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Octokit;
using Octokit.Internal;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;
using Octokit.Webhooks.Events.PullRequestReview;
using Octokit.Webhooks.Events.PullRequestReviewComment;
using Octokit.Webhooks.Events.PullRequestReviewThread;
using PullRequestReviewEvent = Octokit.Webhooks.Events.PullRequestReviewEvent;

namespace AiPrReviewer.WebHooks.Processors;

public sealed class GitHubPrWebhookProcessor : WebhookEventProcessor
{
    private readonly GitHubApiOptions _gitHubApiOptions;
    private readonly ICodeReviewService _aiReviewerService;
    private readonly GitHubClient _gitHubClient;

    public GitHubPrWebhookProcessor(ICodeReviewService aiReviewerService, IOptions<GitHubApiOptions> gitHubApiOptions)
    {
        _aiReviewerService = aiReviewerService;
        _gitHubApiOptions = gitHubApiOptions.Value;

        var credentials = new InMemoryCredentialStore(new Credentials(_gitHubApiOptions.Token));
        _gitHubClient = new GitHubClient(new ProductHeaderValue("Quoc-AI-Reviewer"), credentials);
    }

    public override async Task ProcessWebhookAsync(IDictionary<string, StringValues> headers, string body)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(body);

        var webhookHeaders = WebhookHeaders.Parse(headers);
        var webhookEvent = this.DeserializeWebhookEvent(webhookHeaders, body);

        await this.ProcessWebhookAsync(webhookHeaders, webhookEvent);
    }

    protected override async Task ProcessPullRequestWebhookAsync(WebhookHeaders headers, PullRequestEvent pullRequestEvent, PullRequestAction action)
    {
        var repoName = pullRequestEvent.Repository?.Name;
        if (string.IsNullOrEmpty(repoName))
        {
            return;
        }

        var prNumber = (int)pullRequestEvent.PullRequest.Number;
        if (prNumber <= 0)
        {
            return;
        }

        var owner = _gitHubApiOptions.Owner;

        var pullRequest = await _gitHubClient.PullRequest.Get(owner, repoName, (int)prNumber);
        var files = await _gitHubClient.PullRequest.Files(owner, repoName, (int)prNumber);

        var reviewingFiles = new List<ReviewingFileDto>();
        foreach (var file in files)
        {
            reviewingFiles.Add(new()
            {
                FileName = file.FileName,
                Diff = file.Patch,
            });
        }

        var reviewCodeResult = await _aiReviewerService.ReviewCodeAsync(new() { Files = reviewingFiles });

        foreach (var codeComment in reviewCodeResult.Comments)
        {
            await _gitHubClient.PullRequest.ReviewComment.Create(owner, repoName, prNumber, new(codeComment.Comment, pullRequest.Head.Sha, codeComment.FileName, codeComment.Position));
        }
    }

    protected override Task ProcessPullRequestReviewCommentWebhookAsync(WebhookHeaders headers, PullRequestReviewCommentEvent pullRequestReviewCommentEvent, PullRequestReviewCommentAction action)
    {
        return base.ProcessPullRequestReviewCommentWebhookAsync(headers, pullRequestReviewCommentEvent, action);
    }

    protected override Task ProcessPullRequestReviewThreadWebhookAsync(WebhookHeaders headers, PullRequestReviewThreadEvent pullRequestReviewThreadEvent, PullRequestReviewThreadAction action)
    {
        return base.ProcessPullRequestReviewThreadWebhookAsync(headers, pullRequestReviewThreadEvent, action);
    }

    protected override Task ProcessPullRequestReviewWebhookAsync(WebhookHeaders headers, PullRequestReviewEvent pullRequestReviewEvent, PullRequestReviewAction action)
    {
        return base.ProcessPullRequestReviewWebhookAsync(headers, pullRequestReviewEvent, action);
    }
}