using System.Text.Json;
using Som3a.Domain.Activities;
using Som3a.Domain.Relationships;

namespace Som3a.AI.Parsers;

public class RelationshipParser : BaseStructuredParser<List<Relationship>>
{
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
                        ""lagDays"": { ""type"": ""number"" }
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

            var predecessor = new Activity { ActivityId = item.GetProperty("predecessorId").GetString() ?? string.Empty };
            var successor = new Activity { ActivityId = item.GetProperty("successorId").GetString() ?? string.Empty };

            var rel = new Relationship
            {
                Predecessor = predecessor,
                Successor = successor,
                Type = type,
                Lag = item.TryGetProperty("lagDays", out var lag)
                    ? TimeSpan.FromDays(lag.GetDouble())
                    : TimeSpan.Zero
            };

            relationships.Add(rel);
        }

        return Task.FromResult(relationships);
    }
}
