using Som3a.Contracts;

namespace Som3a.AI.Orchestration
{
    public class AIOrchestrator
    {
        private readonly List<IAIProvider> _providers;
        private readonly RetryHandler _retryHandler;
        private readonly ContextBuilder _contextBuilder;

        public AIOrchestrator(IEnumerable<IAIProvider> providers, RetryHandler retryHandler, ContextBuilder contextBuilder)
        {
            _providers = providers.OrderBy(p => p.ProviderId).ToList();
            _retryHandler = retryHandler;
            _contextBuilder = contextBuilder;
        }

        public async Task<AIResponse> ExecuteAsync(AIRequest request, CancellationToken ct = default)
        {
            var available = _providers.Where(p => p.IsAvailable).ToList();
            if (available.Count == 0)
                return new AIResponse { IsSuccess = false, ErrorMessage = "No AI providers available." };

            var errors = new List<string>();

            foreach (var provider in available)
            {
                try
                {
                    var response = await _retryHandler.ExecuteWithRetryAsync(
                        () => provider.ExecutePromptAsync(request, ct), ct);

                    if (response != null && response.IsSuccess)
                        return response;

                    if (response != null)
                        errors.Add($"{provider.ProviderName}: {response.ErrorMessage}");
                }
                catch (Exception ex)
                {
                    errors.Add($"{provider.ProviderName}: {ex.Message}");
                }
            }

            return new AIResponse
            {
                IsSuccess = false,
                ErrorMessage = $"All providers failed. Errors: {string.Join("; ", errors)}"
            };
        }

        public string BuildContext<T>(T entity) where T : class
        {
            return _contextBuilder.BuildContext(entity);
        }
    }
}
