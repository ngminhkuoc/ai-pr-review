namespace AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;

public record GitHubFile
{
    public string Filename { get; set; } = string.Empty;
}
