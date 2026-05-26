using System.Text.Json;

namespace Som3a.AI.Parsers;

public class ReviewParser : BaseStructuredParser<ReviewResult>
{
    public override string ParserType => "review";
    public override JsonDocument ExpectedSchema { get; } = JsonDocument.Parse(@"
    {
        ""type"": ""object"",
        ""required"": [""isValid"", ""issues""],
        ""properties"": {
            ""isValid"": { ""type"": ""boolean"" },
            ""issues"": {
                ""type"": ""array"",
                ""items"": {
                    ""type"": ""object"",
                    ""required"": [""severity"", ""message""],
                    ""properties"": {
                        ""severity"": { ""type"": ""string"", ""enum"": [""error"", ""warning"", ""info""] },
                        ""message"": { ""type"": ""string"" },
                        ""location"": { ""type"": ""string"" }
                    }
                }
            },
            ""recommendations"": {
                ""type"": ""array"",
                ""items"": { ""type"": ""string"" }
            }
        }
    }");

    protected override Task<ReviewResult> ParseEntityAsync(JsonElement element, CancellationToken ct)
    {
        var result = new ReviewResult
        {
            IsValid = element.GetProperty("isValid").GetBoolean()
        };

        var issues = element.GetProperty("issues").EnumerateArray();
        foreach (var issue in issues)
        {
            result.Issues.Add(new ReviewIssue
            {
                Severity = issue.GetProperty("severity").GetString() ?? "error",
                Message = issue.GetProperty("message").GetString() ?? string.Empty,
                Location = issue.TryGetProperty("location", out var loc)
                    ? loc.GetString() ?? string.Empty
                    : string.Empty
            });
        }

        if (element.TryGetProperty("recommendations", out var recs))
        {
            result.Recommendations = recs.EnumerateArray()
                .Select(r => r.GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        return Task.FromResult(result);
    }
}

public class ReviewResult
{
    public bool IsValid { get; set; }
    public List<ReviewIssue> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class ReviewIssue
{
    public string Severity { get; set; } = "error";
    public string Message { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
