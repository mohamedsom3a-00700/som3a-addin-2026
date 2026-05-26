using System.Text.Json;
using Som3a.Domain.Activities;

namespace Som3a.AI.Parsers;

public class ActivityParser : BaseStructuredParser<List<Activity>>
{
    public override string ParserType => "activity-list";
    public override JsonDocument ExpectedSchema { get; } = JsonDocument.Parse(@"
    {
        ""type"": ""object"",
        ""required"": [""activities""],
        ""properties"": {
            ""activities"": {
                ""type"": ""array"",
                ""items"": {
                    ""type"": ""object"",
                    ""required"": [""activityId"", ""name""],
                    ""properties"": {
                        ""activityId"": { ""type"": ""string"" },
                        ""name"": { ""type"": ""string"" },
                        ""description"": { ""type"": ""string"" },
                        ""quantity"": { ""type"": ""number"" },
                        ""unit"": { ""type"": ""string"" },
                        ""durationDays"": { ""type"": ""number"" },
                        ""productivityRate"": { ""type"": ""number"" },
                        ""boqReferences"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } }
                    }
                }
            }
        }
    }");

    protected override Task<List<Activity>> ParseEntityAsync(JsonElement element, CancellationToken ct)
    {
        var activities = new List<Activity>();
        var items = element.GetProperty("activities").EnumerateArray();

        foreach (var item in items)
        {
            var activity = new Activity
            {
                ActivityId = item.GetProperty("activityId").GetString() ?? string.Empty,
                Name = item.GetProperty("name").GetString() ?? string.Empty,
                Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                Quantity = item.TryGetProperty("quantity", out var qty) ? qty.GetDecimal() : 0,
                Unit = item.TryGetProperty("unit", out var unit) ? unit.GetString() : null,
                ProductivityRate = item.TryGetProperty("productivityRate", out var rate) ? rate.GetDecimal() : 0
            };

            if (item.TryGetProperty("durationDays", out var dur))
                activity.Duration = TimeSpan.FromDays(dur.GetDouble());

            if (item.TryGetProperty("boqReferences", out var refs))
            {
                foreach (var r in refs.EnumerateArray())
                    activity.BOQReferences.Add(r.GetString() ?? string.Empty);
            }

            activities.Add(activity);
        }

        return Task.FromResult(activities);
    }
}
