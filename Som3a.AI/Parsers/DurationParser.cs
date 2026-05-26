using System.Text.Json;

namespace Som3a.AI.Parsers;

public class DurationParser : BaseStructuredParser<DurationResult>
{
    public override string ParserType => "duration-estimate";
    public override JsonDocument ExpectedSchema { get; } = JsonDocument.Parse(@"
    {
        ""type"": ""object"",
        ""required"": [""durationDays"", ""productivityRate""],
        ""properties"": {
            ""durationDays"": { ""type"": ""number"" },
            ""productivityRate"": { ""type"": ""number"" },
            ""confidence"": { ""type"": ""string"", ""enum"": [""low"", ""medium"", ""high""] },
            ""factors"": {
                ""type"": ""array"",
                ""items"": { ""type"": ""string"" }
            }
        }
    }");

    protected override Task<DurationResult> ParseEntityAsync(JsonElement element, CancellationToken ct)
    {
        var result = new DurationResult
        {
            DurationDays = element.GetProperty("durationDays").GetDouble(),
            ProductivityRate = element.GetProperty("productivityRate").GetDouble(),
            Confidence = element.TryGetProperty("confidence", out var conf)
                ? conf.GetString() ?? "medium"
                : "medium"
        };

        if (element.TryGetProperty("factors", out var factors))
        {
            result.Factors = factors.EnumerateArray()
                .Select(f => f.GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        return Task.FromResult(result);
    }
}

public class DurationResult
{
    public double DurationDays { get; set; }
    public double ProductivityRate { get; set; }
    public string Confidence { get; set; } = "medium";
    public List<string> Factors { get; set; } = new();
}
