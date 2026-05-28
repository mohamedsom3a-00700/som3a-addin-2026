using Som3a.AI.Orchestration;
using Som3a.Contracts;
using Som3a.DurationEstimator.Benchmarks;

namespace Som3a.DurationEstimator.AI;

public interface IAIProductivitySuggestor
{
    Task<ProductivitySuggestion?> SuggestRateAsync(string activityDescription, string tradeCategory, decimal quantity, CancellationToken ct = default);
    Task<IReadOnlyList<AnomalyResult>> AnalyzeDurationsAsync(IReadOnlyList<(string ActivityId, decimal Duration, string TradeCategory)> estimates, CancellationToken ct = default);
}

public record ProductivitySuggestion
{
    public decimal SuggestedRate { get; init; }
    public decimal ConfidenceScore { get; init; }
    public List<string> SourceReferences { get; init; } = new();
    public string? Reasoning { get; init; }
}

public record AnomalyResult
{
    public string ActivityId { get; init; } = string.Empty;
    public bool IsAnomaly { get; init; }
    public AnomalyType Type { get; init; }
    public string? Explanation { get; init; }
    public decimal DeviationFactor { get; init; }
}

public enum AnomalyType
{
    None,
    TooLong,
    TooShort,
    Impossible
}

public class AIProductivitySuggestor : IAIProductivitySuggestor
{
    private readonly AIOrchestrator? _orchestrator;
    private readonly IBenchmarkLibrary _benchmarkLibrary;

    public AIProductivitySuggestor(AIOrchestrator? orchestrator, IBenchmarkLibrary benchmarkLibrary)
    {
        _orchestrator = orchestrator;
        _benchmarkLibrary = benchmarkLibrary;
    }

    public async Task<ProductivitySuggestion?> SuggestRateAsync(string activityDescription, string tradeCategory, decimal quantity, CancellationToken ct = default)
    {
        var benchmarks = _benchmarkLibrary.GetByTradeCategory(tradeCategory).ToList();

        if (benchmarks.Any())
        {
            var bestMatch = benchmarks
                .OrderByDescending(b => StringSimilarity(b.ActivityDescription, activityDescription))
                .FirstOrDefault();

            if (bestMatch != null)
            {
                return new ProductivitySuggestion
                {
                    SuggestedRate = bestMatch.ProductivityValue,
                    ConfidenceScore = 0.7m,
                    SourceReferences = new List<string> { $"Built-in benchmark: {bestMatch.ActivityDescription}" },
                    Reasoning = $"Matched {tradeCategory} benchmark by description similarity."
                };
            }
        }

        if (_orchestrator != null)
        {
            try
            {
                var request = new AIRequest
                {
                    SystemPrompt = "You are a construction productivity expert. Suggest realistic productivity rates (quantity per crew-day) for construction activities based on trade category and activity description.",
                    UserPrompt = $"Suggest a productivity rate for: Activity=\"{activityDescription}\", Trade={tradeCategory}, Quantity={quantity}. Respond with a JSON object: {{\"rate\": <number>, \"reasoning\": \"<string>\"}}.",
                    MaxTokens = 512,
                    Temperature = 0.3f
                };

                var response = await _orchestrator.ExecuteAsync(request, ct);
                if (response.IsSuccess)
                {
                    return new ProductivitySuggestion
                    {
                        SuggestedRate = ExtractRate(response.Content, fallback: GetFallbackRate(tradeCategory)),
                        ConfidenceScore = 0.5m,
                        SourceReferences = new List<string> { "AI Orchestrator" },
                        Reasoning = response.Content
                    };
                }
            }
            catch { }
        }

        return null;
    }

    public async Task<IReadOnlyList<AnomalyResult>> AnalyzeDurationsAsync(IReadOnlyList<(string ActivityId, decimal Duration, string TradeCategory)> estimates, CancellationToken ct = default)
    {
        var results = new List<AnomalyResult>();

        if (estimates.Count == 0)
            return results;

        foreach (var group in estimates.GroupBy(e => e.TradeCategory))
        {
            var durations = group.Select(e => e.Duration).ToList();
            var avg = durations.Average();
            var stdDev = StandardDeviation(durations);

            if (avg == 0) continue;

            foreach (var est in group)
            {
                var deviation = stdDev > 0 ? Math.Abs(est.Duration - avg) / stdDev : 0;

                AnomalyType type = AnomalyType.None;
                string? explanation = null;

                if (est.Duration > avg * 3m)
                {
                    type = AnomalyType.TooLong;
                    explanation = $"Duration ({est.Duration:F1}d) is {est.Duration / avg:F1}x the peer average ({avg:F1}d).";
                }
                else if (est.Duration < avg * 0.3m && avg > 0.1m)
                {
                    type = AnomalyType.TooShort;
                    explanation = $"Duration ({est.Duration:F1}d) is significantly lower than peers ({avg:F1}d).";
                }
                else if (est.Duration > 365 * 5)
                {
                    type = AnomalyType.Impossible;
                    explanation = $"Duration exceeds 5 years — likely data entry error.";
                }

                results.Add(new AnomalyResult
                {
                    ActivityId = est.ActivityId,
                    IsAnomaly = type != AnomalyType.None,
                    Type = type,
                    Explanation = explanation,
                    DeviationFactor = deviation
                });
            }
        }

        return results;
    }

    private static decimal ExtractRate(string content, decimal fallback)
    {
        var words = content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            var cleaned = word.TrimEnd('.', ',', ':', ';', 'd', 'D');
            if (decimal.TryParse(cleaned, out var rate) && rate > 0 && rate < 1000)
                return rate;
        }
        return fallback;
    }

    private static decimal GetFallbackRate(string tradeCategory) => tradeCategory switch
    {
        "concrete" => 25m,
        "steel" => 5m,
        "masonry" => 80m,
        "mep" => 20m,
        "finishes" => 30m,
        "earthwork" => 100m,
        _ => 10m
    };

    private static double StringSimilarity(string a, string b)
    {
        a = a.ToLowerInvariant();
        b = b.ToLowerInvariant();
        var common = a.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Count(w => b.Contains(w));
        return (double)common / Math.Max(a.Split(' ').Length, 1);
    }

    private static decimal StandardDeviation(List<decimal> values)
    {
        if (values.Count <= 1) return 0;
        var avg = values.Average();
        var sumSq = values.Sum(v => (double)((v - avg) * (v - avg)));
        return (decimal)Math.Sqrt(sumSq / (values.Count - 1));
    }
}
