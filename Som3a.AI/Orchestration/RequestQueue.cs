using System.Collections.Concurrent;

namespace Som3a.AI.Orchestration;

public class RequestQueue
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _providerThrottles = new();

    public async Task<bool> TryAcquireAsync(string providerId, CancellationToken ct = default)
    {
        var semaphore = _providerThrottles.GetOrAdd(providerId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            var bucket = _buckets.GetOrAdd(providerId, _ => new TokenBucket(30, TimeSpan.FromMinutes(1)));
            return bucket.TryConsume();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public void ConfigureRateLimit(string providerId, int rpm)
    {
        var bucket = new TokenBucket(rpm, TimeSpan.FromMinutes(1));
        _buckets.AddOrUpdate(providerId, bucket, (_, _) => bucket);
    }

    public void DisposeProvider(string providerId)
    {
        if (_providerThrottles.TryRemove(providerId, out var semaphore))
            semaphore.Dispose();
        _buckets.TryRemove(providerId, out _);
    }

    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly TimeSpan _window;
        private int _tokens;
        private DateTime _lastRefill;

        public TokenBucket(int capacity, TimeSpan window)
        {
            _capacity = capacity;
            _window = window;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryConsume()
        {
            Refill();
            if (_tokens <= 0) return false;
            Interlocked.Decrement(ref _tokens);
            return true;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefill;
            if (elapsed >= _window)
            {
                _tokens = _capacity;
                _lastRefill = now;
            }
        }
    }
}
