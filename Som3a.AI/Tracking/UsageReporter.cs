using Som3a.Contracts;

namespace Som3a.AI.Tracking
{
    public class UsageReporter
    {
        private readonly TokenTracker _tracker;

        public UsageReporter(TokenTracker tracker)
        {
            _tracker = tracker;
        }

        public string GenerateReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Token Usage Report ===");
            sb.AppendLine($"Total Prompt Tokens: {_tracker.TotalPromptTokens}");
            sb.AppendLine($"Total Completion Tokens: {_tracker.TotalCompletionTokens}");
            sb.AppendLine($"Total Tokens: {_tracker.TotalTokens}");
            sb.AppendLine();

            var byProvider = _tracker.GetUsageByProvider();
            foreach (var kvp in byProvider)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value} tokens");
            }

            return sb.ToString();
        }

        public Dictionary<string, decimal> GetCostEstimates()
        {
            var rates = new Dictionary<string, decimal>
            {
                ["openai"] = 0.03m,
                ["claude"] = 0.015m,
                ["deepseek"] = 0.001m,
                ["kimi"] = 0.002m,
                ["glm"] = 0.001m,
                ["codex"] = 0.03m
            };

            var byProvider = _tracker.GetUsageByProvider();
            return byProvider.ToDictionary(
                kvp => kvp.Key,
                kvp => (decimal)kvp.Value / 1000m * rates.GetValueOrDefault(kvp.Key, 0.01m)
            );
        }
    }
}
