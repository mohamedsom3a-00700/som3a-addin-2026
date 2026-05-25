using Som3a.Contracts;

namespace Som3a.AI.Providers
{
    public abstract class AIProviderBase : IAIProvider
    {
        public abstract string ProviderId { get; }
        public abstract string ProviderName { get; }
        public abstract bool IsAvailable { get; }

        public abstract Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken ct = default);
        public abstract IAsyncEnumerable<AIStreamChunk> StreamPromptAsync(AIRequest request, CancellationToken ct = default);

        public virtual async Task<bool> HealthCheckAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await ExecutePromptAsync(new AIRequest
                {
                    SystemPrompt = "You are a health check.",
                    UserPrompt = "Ping",
                    MaxTokens = 10,
                    Temperature = 0
                }, ct);
                return response.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }
}
