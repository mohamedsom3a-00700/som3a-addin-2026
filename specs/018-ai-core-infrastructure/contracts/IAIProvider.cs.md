# Contract: IAIProvider

**Layer**: Som3a.AI.Providers
**Purpose**: Provider adapter interface — each AI provider implements this to present a uniform interface to the orchestration engine.

```csharp
namespace Som3a.AI.Providers;

public interface IAIProvider
{
    string ProviderId { get; }
    
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct);
    
    Task<ProviderResponse> ExecuteAsync(
        ProviderRequest request,
        CancellationToken ct);
    
    IAsyncEnumerable<StreamChunk> ExecuteStreamingAsync(
        ProviderRequest request,
        CancellationToken ct);
}

public record ProviderRequest(
    string SystemPrompt,
    string UserPrompt,
    JsonDocument? OutputSchema,
    int? MaxTokens,
    double? Temperature);

public record ProviderResponse(
    string Content,
    int TokensPrompt,
    int TokensCompletion,
    TimeSpan Duration);

public record StreamChunk(
    string ContentDelta,
    bool IsFinal,
    int? TokensPrompt,
    int? TokensCompletion);

public record HealthCheckResult(
    bool IsAvailable,
    TimeSpan Latency,
    string? ErrorMessage);
```
