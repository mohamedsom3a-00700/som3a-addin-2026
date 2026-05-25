namespace Som3a.Contracts
{
    public interface IAIProvider
    {
        string ProviderId { get; }
        string ProviderName { get; }
        bool IsAvailable { get; }

        Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken ct = default);
        IAsyncEnumerable<AIStreamChunk> StreamPromptAsync(AIRequest request, CancellationToken ct = default);
        Task<bool> HealthCheckAsync(CancellationToken ct = default);
    }

    public class AIRequest
    {
        public string SystemPrompt { get; set; } = string.Empty;
        public string UserPrompt { get; set; } = string.Empty;
        public string? JsonSchema { get; set; }
        public Dictionary<string, string>? Parameters { get; set; }
        public int MaxTokens { get; set; } = 4096;
        public float Temperature { get; set; } = 0.3f;
    }

    public class AIResponse
    {
        public string Content { get; set; } = string.Empty;
        public string? ParsedJson { get; set; }
        public TokenUsage Usage { get; set; } = new();
        public string ProviderId { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AIStreamChunk
    {
        public string Delta { get; set; } = string.Empty;
        public int Index { get; set; }
        public bool IsFinal { get; set; }
    }

    public class TokenUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens => PromptTokens + CompletionTokens;
    }
}
