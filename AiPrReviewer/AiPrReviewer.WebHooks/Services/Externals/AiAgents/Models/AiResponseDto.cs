using System.Text.Json.Serialization;

namespace AiPrReviewer.WebHooks.Services.Externals.AiAgents.Models;

public record AiResponseDto()
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; init; }

    [JsonPropertyName("done_reason")]
    public string DoneReason { get; init; } = string.Empty;

    [JsonPropertyName("context")]
    public int[] Context { get; init; } = [];

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; init; }

    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; init; }

    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; init; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; init; }

    [JsonPropertyName("eval_count")]
    public int EvalCount { get; init; }

    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; init; }
}
