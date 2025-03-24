namespace AiPrReviewer.WebHooks.Services.Externals.GitHub.Models;

public record GitHubApiOptions
{
    public const string Section = "GitHub";

    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}
