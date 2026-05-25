using Som3a.Contracts;

namespace Som3a.AI.Tracking
{
    public class TokenTracker
    {
        private readonly List<TokenUsageRecord> _records = new();

        public int TotalPromptTokens => _records.Sum(r => r.Usage.PromptTokens);
        public int TotalCompletionTokens => _records.Sum(r => r.Usage.CompletionTokens);
        public int TotalTokens => TotalPromptTokens + TotalCompletionTokens;

        public void RecordUsage(string providerId, TokenUsage usage)
        {
            _records.Add(new TokenUsageRecord
            {
                Timestamp = DateTimeOffset.UtcNow,
                ProviderId = providerId,
                Usage = usage
            });
        }

        public IReadOnlyList<TokenUsageRecord> GetRecords() => _records.AsReadOnly();

        public Dictionary<string, int> GetUsageByProvider()
        {
            return _records
                .GroupBy(r => r.ProviderId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Usage.TotalTokens));
        }
    }

    public class TokenUsageRecord
    {
        public DateTimeOffset Timestamp { get; set; }
        public string ProviderId { get; set; } = string.Empty;
        public TokenUsage Usage { get; set; } = new();
    }
}
