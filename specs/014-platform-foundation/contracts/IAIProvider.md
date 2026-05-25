# Contract: IAIProvider

**Namespace**: `Som3a.Contracts`
**Assembly**: `Som3a.Contracts.dll`

## Purpose

Defines the contract for AI provider interaction. All AI calls flow through this interface, enabling provider-agnostic orchestration. Providers include OpenAI, Claude, DeepSeek, GLM, Kimi, and Codex.

## Interface

```csharp
public interface IAIProvider
{
    string ProviderId { get; }
    string ProviderName { get; }
    bool IsAvailable { get; }

    Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken ct = default);
    IAsyncEnumerable<AIStreamChunk> StreamPromptAsync(AIRequest request, CancellationToken ct = default);
    Task<bool> HealthCheckAsync(CancellationToken ct = default);
}
```

## Request/Response Types

```csharp
public class AIRequest
{
    public string SystemPrompt { get; set; }
    public string UserPrompt { get; set; }
    public string? JsonSchema { get; set; }        // Optional JSON Schema for structured output
    public Dictionary<string, string>? Parameters { get; set; }
    public int MaxTokens { get; set; } = 4096;
    public float Temperature { get; set; } = 0.3f; // Low temp for structured planning output
}

public class AIResponse
{
    public string Content { get; set; }
    public string? ParsedJson { get; set; }        // Parsed if JsonSchema was provided
    public TokenUsage Usage { get; set; }
    public string ProviderId { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AIStreamChunk
{
    public string Delta { get; set; }
    public int Index { get; set; }
    public bool IsFinal { get; set; }
}

public class TokenUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens => PromptTokens + CompletionTokens;
}
```

## Orchestration Rules

1. **Failover**: If primary provider returns error or `IsAvailable == false`, orchestrator routes to next provider in priority order.
2. **Retry**: Transient errors (HTTP 429, 5xx) retried with exponential backoff (1s, 2s, 4s, 8s).
3. **Timeout**: Total failover + retry time ≤ 30 seconds (SC-003).
4. **Streaming**: `StreamPromptAsync` delivers chunks as they arrive; caller handles partial display.
5. **Schema validation**: When `JsonSchema` is provided, response is validated against it post-parse.
