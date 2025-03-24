using AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;
using Refit;

namespace AiPrReviewer.WebHooks.Services.Externals.GitHub;

public interface IGitHubApiService
{
    [Get("/repos/{owner}/{repoName}/pulls/{prNumber}/files")]
    Task<IList<GitHubFile>> GetChangedFilesAsync(string owner, string repoName, int prNumber);

    [Post("/repos/{owner}/{repoName}/pulls/{prNumber}/comments")]
    Task PostCommentAsync(string owner, string repoName, int prNumber, [Body] GitHubComment comment);
}
