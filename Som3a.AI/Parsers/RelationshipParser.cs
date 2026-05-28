using System.Text.Json;
using Som3a.Domain.Activities;
using Som3a.Domain.Relationships;

namespace Som3a.AI.Parsers;

public class RelationshipParser : BaseStructuredParser<List<Relationship>>
{
    private readonly Dictionary<string, Activity>? _activityLookup;

    public RelationshipParser() { }

    public RelationshipParser(IReadOnlyList<Activity> activities)
    {
        _activityLookup = activities?.ToDictionary(a => a.ActivityId) ?? new();
    }

    public override string ParserType => "relationship-list";
    public override JsonDocument ExpectedSchema { get; } = JsonDocument.Parse(@"
    {
        ""type"": ""object"",
        ""required"": [""relationships""],
        ""properties"": {
            ""relationships"": {
                ""type"": ""array"",
                ""items"": {
                    ""type"": ""object"",
                    ""required"": [""predecessorId"", ""successorId"", ""type""],
                    ""properties"": {
                        ""predecessorId"": { ""type"": ""string"" },
                        ""successorId"": { ""type"": ""string"" },
                        ""type"": { ""type"": ""string"", ""enum"": [""FS"", ""SS"", ""FF"", ""SF""] },
                        ""lagDays"": { ""type"": ""number"" },
                        ""rationale"": { ""type"": ""string"" },
                        ""confidence"": { ""type"": ""string"", ""enum"": [""High"", ""Medium"", ""Low""] }
                    }
                }
            }
        }
    }");

    protected override Task<List<Relationship>> ParseEntityAsync(JsonElement element, CancellationToken ct)
    {
        var relationships = new List<Relationship>();
        var items = element.GetProperty("relationships").EnumerateArray();

        foreach (var item in items)
        {
            var typeStr = item.GetProperty("type").GetString();
            var type = typeStr switch
            {
                "SS" => RelationshipType.SS,
                "FF" => RelationshipType.FF,
                "SF" => RelationshipType.SF,
                _ => RelationshipType.FS
            };

            var predecessorId = item.GetProperty("predecessorId").GetString() ?? string.Empty;
            var successorId = item.GetProperty("successorId").GetString() ?? string.Empty;

            var predecessor = _activityLookup?.GetValueOrDefault(predecessorId)
                ?? new Activity { ActivityId = predecessorId, Name = predecessorId };
            var successor = _activityLookup?.GetValueOrDefault(successorId)
                ?? new Activity { ActivityId = successorId, Name = successorId };

            var confidenceStr = item.TryGetProperty("confidence", out var confidenceEl)
                ? confidenceEl.GetString()
                : "Medium";

            var confidence = confidenceStr switch
            {
                "High" => RelationshipConfidence.High,
                "Low" => RelationshipConfidence.Low,
                _ => RelationshipConfidence.Medium
            };

            var rel = new Relationship
            {
                Predecessor = predecessor,
                Successor = successor,
                Type = type,
                Lag = item.TryGetProperty("lagDays", out var lag)
                    ? TimeSpan.FromDays(lag.GetDouble())
                    : TimeSpan.Zero,
                Rationale = item.TryGetProperty("rationale", out var rationale)
                    ? rationale.GetString()
                    : null,
                Confidence = confidence,
                GeneratedAt = DateTime.UtcNow
            };

            relationships.Add(rel);
        }

        return Task.FromResult(relationships);
    }
}
