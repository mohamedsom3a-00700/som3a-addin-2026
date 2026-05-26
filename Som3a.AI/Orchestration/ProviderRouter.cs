using Som3a.Contracts;

namespace Som3a.AI.Orchestration;

public class ProviderRouter
{
    private readonly IReadOnlyDictionary<string, IAIProvider> _providers;
    private readonly Dictionary<string, int> _consecutiveFailures = new();
    private readonly int _degradationThreshold;

    public ProviderRouter(IEnumerable<IAIProvider> providers, int degradationThreshold = 3)
    {
        _providers = providers.ToDictionary(p => p.ProviderId);
        _degradationThreshold = degradationThreshold;
    }

    public IReadOnlyList<IAIProvider> GetAvailableProviders()
    {
        return _providers.Values
            .Where(p => p.IsAvailable && !IsDegraded(p.ProviderId))
            .ToList();
    }

    public bool IsDegraded(string providerId)
    {
        return _consecutiveFailures.TryGetValue(providerId, out var failures)
            && failures >= _degradationThreshold;
    }

    public void RecordFailure(string providerId)
    {
        if (!_consecutiveFailures.ContainsKey(providerId))
            _consecutiveFailures[providerId] = 0;
        _consecutiveFailures[providerId]++;
    }

    public void RecordSuccess(string providerId)
    {
        _consecutiveFailures[providerId] = 0;
    }

    public void ResetDegradation(string providerId)
    {
        _consecutiveFailures.Remove(providerId);
    }
}
