using System.Text.Json;
using Som3a.Domain.WBS;

namespace Som3a.AI.Parsers;

public class WBSParser : BaseStructuredParser<WBSNode>
{
    public override string ParserType => "wbs-tree";
    public override JsonDocument ExpectedSchema { get; } = JsonDocument.Parse(@"
    {
        ""type"": ""object"",
        ""required"": [""wbs"", ""name"", ""code""],
        ""properties"": {
            ""wbs"": { ""type"": ""object"" },
            ""name"": { ""type"": ""string"" },
            ""code"": { ""type"": ""string"" },
            ""children"": { ""type"": ""array"", ""items"": { ""type"": ""object"" } }
        }
    }");

    protected override Task<WBSNode> ParseEntityAsync(JsonElement element, CancellationToken ct)
    {
        var wbsElement = element.TryGetProperty("wbs", out var wbs) ? wbs : element;
        var node = ParseNode(wbsElement);
        return Task.FromResult(node);
    }

    private WBSNode ParseNode(JsonElement element)
    {
        var node = new WBSNode
        {
            Code = element.GetProperty("code").GetString() ?? string.Empty,
            Name = element.GetProperty("name").GetString() ?? string.Empty
        };

        if (element.TryGetProperty("children", out var children))
        {
            foreach (var child in children.EnumerateArray())
            {
                var childNode = ParseNode(child);
                childNode.Parent = node;
                node.Children.Add(childNode);
            }
        }

        return node;
    }
}
