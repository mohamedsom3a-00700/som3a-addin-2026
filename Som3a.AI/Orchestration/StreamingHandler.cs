using System.Runtime.CompilerServices;
using Som3a.Contracts;

namespace Som3a.AI.Orchestration;

public class StreamingHandler
{
    public async IAsyncEnumerable<OrchestrationStreamEvent> StreamAsync(
        IAIProvider provider,
        AIRequest request,
        string executionId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chunkCount = 0;

        await foreach (var chunk in provider.StreamPromptAsync(request, ct))
        {
            ct.ThrowIfCancellationRequested();

            yield return new OrchestrationStreamEvent
            {
                ExecutionId = executionId,
                ContentDelta = chunk.Delta,
                IsProgress = !chunk.IsFinal,
                IsFinal = chunk.IsFinal
            };

            chunkCount++;
        }
    }
}

public class OrchestrationStreamEvent
{
    public string ExecutionId { get; set; } = string.Empty;
    public string? ContentDelta { get; set; }
    public bool IsProgress { get; set; }
    public bool IsFinal { get; set; }
    public TokenUsage? Usage { get; set; }
}
