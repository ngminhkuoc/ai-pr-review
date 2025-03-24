namespace AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;

public record GitHubComment
{
    public string Body { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Position { get; set; } = 1; // Default to first line

}
