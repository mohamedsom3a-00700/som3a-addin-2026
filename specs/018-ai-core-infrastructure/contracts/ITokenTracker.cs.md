# Contract: ITokenTracker

**Layer**: Som3a.AI.Orchestration
**Purpose**: Token usage tracking — records consumption per provider and session, provides aggregated reports.

```csharp
namespace Som3a.AI.Orchestration;

public interface ITokenTracker
{
    Task RecordUsageAsync(string executionId, TokenUsage usage);
    
    Task<TokenUsage> GetSessionUsageAsync(string sessionId);
    
    Task<TokenUsage> GetProviderUsageAsync(string providerId);
    
    Task<TokenUsage> GetAggregateAsync();
}

public record TokenUsage(
    string ProviderId,
    string? SessionId,
    int TokensPrompt,
    int TokensCompletion,
    int TokensTotal,
    decimal? EstimatedCost);
```
