namespace Som3a.AI.Orchestration
{
    public class RetryHandler
    {
        private readonly int[] _backoffSeconds = { 1, 2, 4, 8 };
        private readonly int _maxConsecutiveFailures = 5;

        public int MaxConsecutiveFailures => _maxConsecutiveFailures;
        public int[] BackoffSeconds => _backoffSeconds;

        public async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt <= _backoffSeconds.Length; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < _backoffSeconds.Length)
                    {
                        var delay = TimeSpan.FromSeconds(_backoffSeconds[attempt]);
                        await Task.Delay(delay, ct);
                    }
                }
            }

            throw lastException ?? new InvalidOperationException("Retry failed with unknown error.");
        }
    }
}
