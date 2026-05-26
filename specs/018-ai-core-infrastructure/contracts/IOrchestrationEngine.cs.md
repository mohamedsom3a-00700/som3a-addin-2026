# Contract: IOrchestrationEngine

**Layer**: Som3a.AI.Orchestration
**Purpose**: Central orchestration interface — builds context from domain entities, selects provider via failover routing, manages retry and streaming.

```csharp
namespace Som3a.AI.Orchestration;

public interface IOrchestrationEngine
{
    Task<OrchestrationResult> ExecuteAsync(
        OrchestrationRequest request,
        CancellationToken ct);
    
    IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        OrchestrationRequest request,
        CancellationToken ct);
}

public record OrchestrationRequest(
    string TemplateId,
    IReadOnlyDictionary<string, object> ContextEntities,
    string? OverrideProviderId);

public record OrchestrationResult(
    string ExecutionId,
    string ProviderUsed,
    JsonDocument StructuredOutput,
    TokenUsage Usage,
    TimeSpan Duration,
    string[] FallbackChain);

public record OrchestrationStreamEvent(
    string ExecutionId,
    string? ContentDelta,
    bool IsProgress,
    bool IsFinal,
    TokenUsage? Usage);
```
