using Som3a.AI.Orchestration;
using Som3a.AI.Prompts;
using Som3a.AI.Parsing;
using Som3a.AI.Tracking;
using Som3a.Contracts;

namespace Som3a.AI.Orchestration;

public class OrchestrationEngine
{
    private readonly ProviderRouter _router;
    private readonly RetryHandler _retryHandler;
    private readonly RequestQueue _requestQueue;
    private readonly StreamingHandler _streamingHandler;
    private readonly TokenTracker _tokenTracker;
    private readonly ContextBuilder _contextBuilder;
    private readonly StructuredOutputParser _outputParser;

    public OrchestrationEngine(
        ProviderRouter router,
        RetryHandler retryHandler,
        RequestQueue requestQueue,
        StreamingHandler streamingHandler,
        TokenTracker tokenTracker,
        ContextBuilder contextBuilder,
        StructuredOutputParser outputParser)
    {
        _router = router;
        _retryHandler = retryHandler;
        _requestQueue = requestQueue;
        _streamingHandler = streamingHandler;
        _tokenTracker = tokenTracker;
        _contextBuilder = contextBuilder;
        _outputParser = outputParser;
    }

    public async Task<OrchestrationResult> ExecuteAsync(
        string templateId,
        IReadOnlyDictionary<string, object> contextEntities,
        CancellationToken ct = default)
    {
        var execution = new PromptExecutionContext
        {
            TemplateId = templateId,
            Status = ExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        var contextText = _contextBuilder.BuildContext(contextEntities.Values.FirstOrDefault()!);
        var request = new AIRequest
        {
            SystemPrompt = "Execute the requested AI operation.",
            UserPrompt = contextText,
            Temperature = 0.3f,
            MaxTokens = 4096
        };

        var available = _router.GetAvailableProviders();
        if (available.Count == 0)
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = "No AI providers available.";
            return new OrchestrationResult(execution, null);
        }

        List<string> fallbackChain = new();
        AIResponse? lastResponse = null;

        foreach (var provider in available)
        {
            if (!await _requestQueue.TryAcquireAsync(provider.ProviderId, ct))
            {
                fallbackChain.Add($"{provider.ProviderId}: rate limited");
                continue;
            }

            fallbackChain.Add(provider.ProviderId);
            execution.FallbackChain = fallbackChain;
            execution.ProviderId = provider.ProviderId;

            try
            {
                lastResponse = await _retryHandler.ExecuteWithRetryAsync(
                    () => provider.ExecutePromptAsync(request, ct), ct);

                if (lastResponse != null && lastResponse.IsSuccess)
                {
                    _router.RecordSuccess(provider.ProviderId);
                    _tokenTracker.RecordUsage(provider.ProviderId, lastResponse.Usage);

                    execution.Status = ExecutionStatus.Completed;
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.DurationMs = (long)(DateTime.UtcNow - execution.StartedAt).TotalMilliseconds;
                    execution.TokensPrompt = lastResponse.Usage.PromptTokens;
                    execution.TokensCompletion = lastResponse.Usage.CompletionTokens;

                    return new OrchestrationResult(execution, lastResponse);
                }

                _router.RecordFailure(provider.ProviderId);
            }
            catch (Exception ex)
            {
                _router.RecordFailure(provider.ProviderId);
                fallbackChain.Add($"{provider.ProviderId}: {ex.Message}");
            }
        }

        execution.Status = ExecutionStatus.Failed;
        execution.ErrorMessage = $"All providers failed. Chain: {string.Join(" -> ", fallbackChain)}";
        execution.CompletedAt = DateTime.UtcNow;

        return new OrchestrationResult(execution, lastResponse);
    }

    public async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        string templateId,
        IReadOnlyDictionary<string, object> contextEntities,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var executionId = Guid.NewGuid().ToString();
        var contextText = _contextBuilder.BuildContext(contextEntities.Values.FirstOrDefault()!);
        var request = new AIRequest
        {
            SystemPrompt = "Execute the requested AI operation.",
            UserPrompt = contextText,
            Temperature = 0.3f,
            MaxTokens = 4096
        };

        var available = _router.GetAvailableProviders();
        var provider = available.FirstOrDefault();
        if (provider == null)
        {
            yield return new OrchestrationStreamEvent
            {
                ExecutionId = executionId,
                IsFinal = true,
                ContentDelta = "No AI providers available."
            };
            yield break;
        }

        await foreach (var chunk in _streamingHandler.StreamAsync(provider, request, executionId, ct))
        {
            yield return chunk;
        }
    }
}

public class OrchestrationResult
{
    public PromptExecutionContext Execution { get; }
    public AIResponse? Response { get; }
    public bool IsSuccess => Execution.Status == ExecutionStatus.Completed;

    public OrchestrationResult(PromptExecutionContext execution, AIResponse? response)
    {
        Execution = execution;
        Response = response;
    }
}
