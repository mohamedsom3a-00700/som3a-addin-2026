using Newtonsoft.Json;
using Som3a.Bridge;
using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public class RelationshipGenerationService : IRelationshipGenerationService
    {
        private readonly IServiceContainer _container;

        public RelationshipGenerationService(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public bool IsAIAvailable
        {
            get
            {
                try
                {
                    var bridge = _container.Resolve<IAIBridge>();
                    return bridge?.IsHostRunning ?? false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<IReadOnlyList<Relationship>> GenerateRelationshipsAsync(
            IReadOnlyList<ActivityItem> activities,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default)
        {
            if (activities == null || activities.Count == 0)
                throw new ArgumentException("Activity list must not be empty.", nameof(activities));

            progress?.Report(new GenerationProgress("BuildingContext", 10, "Building activity context for AI..."));

            try
            {
                var relationships = await CallAIGenerationAsync(activities, fullContext: true, progress, ct);
                if (relationships.Count > 0)
                    return relationships;
            }
            catch
            {
                // First attempt failed — retry with simplified prompt
            }

            progress?.Report(new GenerationProgress("Retrying", 50, "Retrying with simplified prompt..."));

            try
            {
                var retryRelationships = await CallAIGenerationAsync(activities, fullContext: false, progress, ct);
                if (retryRelationships.Count > 0)
                    return retryRelationships;
            }
            catch
            {
                // Both attempts failed — fall back to heuristic
            }

            progress?.Report(new GenerationProgress("Fallback", 70, "AI unavailable. Using trade-sequence heuristics..."));

            ct.ThrowIfCancellationRequested();

            var fallback = GenerateTradeSequenceRelationships(activities);
            if (fallback.Count > 0)
                return fallback;

            progress?.Report(new GenerationProgress("ManualMode", 100, "AI generation unavailable. Use manual editor."));
            return Array.Empty<Relationship>();
        }

        public async Task<IReadOnlyList<Relationship>> RegenerateRelationshipsAsync(
            IReadOnlyList<ActivityItem> activities,
            IReadOnlyList<Relationship> existingRelationships,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default)
        {
            var newRelationships = await GenerateRelationshipsAsync(activities, progress, ct);
            return PreserveUserEdits(newRelationships, existingRelationships);
        }

        private async Task<IReadOnlyList<Relationship>> CallAIGenerationAsync(
            IReadOnlyList<ActivityItem> activities,
            bool fullContext,
            IProgress<GenerationProgress>? progress,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            progress?.Report(new GenerationProgress("Analyzing", 30, fullContext
                ? $"Analyzing {activities.Count} activities for relationship generation..."
                : $"Re-analyzing with simplified context..."));

            var activityLines = string.Join(Environment.NewLine,
                activities.Select(a => $"{a.ActivityId}\t{a.Name}\t{a.TradeCategory}"));

            var systemPrompt = fullContext
                ? "You are a construction scheduling expert. Generate logical predecessor/successor relationships between construction activities based on trade sequence, spatial constraints, and resource flow."
                : "You are a construction scheduling expert. Generate simple Finish-to-Start predecessor/successor relationships based on trade sequence.";

            var userPrompt = fullContext
                ? $@"Given the following construction activities, generate logical relationships between them.

Activities:
{activityLines}

For each relationship determine: predecessorId, successorId, type (FS/SS/FF/SF), lagDays (number), rationale, confidence (High/Medium/Low).

Output ONLY a JSON array. No markdown, no code fences. Example:
[{{""predecessorId"":""A-001"",""successorId"":""A-002"",""type"":""FS"",""lagDays"":0,""rationale"":""Excavation must complete before foundation"",""confidence"":""High""}}]"
                : $@"Given the following construction activities, generate simple Finish-to-Start relationships.

Activities:
{activityLines}

Output ONLY a JSON array. No markdown, no code fences. Each object: predecessorId, successorId, type (FS), lagDays (0), rationale, confidence (Medium).";

            var (providerType, apiKey, model, endpoint) = AISettings.GetEffectiveProvider();
            var request = new AIBridgeRequest
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt,
                Temperature = fullContext ? 0.3f : 0.2f,
                MaxTokens = 4096,
                ProviderType = providerType,
                ApiKey = apiKey,
                Model = model,
                Endpoint = endpoint
            };

            var bridge = _container.Resolve<IAIBridge>();
            var response = await bridge.ExecutePromptAsync(request, ct);
            ct.ThrowIfCancellationRequested();

            if (!response.IsSuccess)
                throw new InvalidOperationException($"Relationship AI generation failed: {response.ErrorMessage}");

            return ParseRelationshipsResponse(response.Content, activities);
        }

        private static List<Relationship> ParseRelationshipsResponse(string json, IReadOnlyList<ActivityItem> activities)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<Relationship>();

            try
            {
                var parsed = JsonConvert.DeserializeObject<List<ParsedRelationship>>(json);
                if (parsed == null || parsed.Count == 0)
                    return new List<Relationship>();

                var activityLookup = activities.ToDictionary(a => a.ActivityId, a => a.Name, StringComparer.OrdinalIgnoreCase);

                return parsed.Select(r => new Relationship
                {
                    PredecessorId = r.PredecessorId ?? string.Empty,
                    PredecessorName = activityLookup.TryGetValue(r.PredecessorId ?? "", out var predName) ? predName : r.PredecessorId ?? "",
                    SuccessorId = r.SuccessorId ?? string.Empty,
                    SuccessorName = activityLookup.TryGetValue(r.SuccessorId ?? "", out var succName) ? succName : r.SuccessorId ?? "",
                    Type = ParseRelationshipType(r.Type),
                    LagDays = r.LagDays,
                    Rationale = r.Rationale ?? string.Empty,
                    Confidence = ParseConfidence(r.Confidence),
                    IsAccepted = false,
                    IsUserModified = false,
                    GeneratedAt = DateTime.UtcNow
                }).ToList();
            }
            catch (JsonException)
            {
                return new List<Relationship>();
            }
        }

        private static RelationshipType ParseRelationshipType(string? type)
        {
            return type?.ToUpperInvariant() switch
            {
                "SS" => RelationshipType.SS,
                "FF" => RelationshipType.FF,
                "SF" => RelationshipType.SF,
                _ => RelationshipType.FS
            };
        }

        private static RelationshipConfidence ParseConfidence(string? confidence)
        {
            return confidence?.ToLowerInvariant() switch
            {
                "high" => RelationshipConfidence.High,
                "low" => RelationshipConfidence.Low,
                _ => RelationshipConfidence.Medium
            };
        }

        private static IReadOnlyList<Relationship> PreserveUserEdits(
            IReadOnlyList<Relationship> newRelationships,
            IReadOnlyList<Relationship> existingRelationships)
        {
            if (existingRelationships == null || existingRelationships.Count == 0)
                return newRelationships;

            var existingByPair = existingRelationships
                .Where(r => !string.IsNullOrEmpty(r.PredecessorId) && !string.IsNullOrEmpty(r.SuccessorId))
                .ToDictionary(r => $"{r.PredecessorId}|{r.SuccessorId}|{r.Type}");

            var result = new List<Relationship>();

            foreach (var rel in newRelationships)
            {
                var pairKey = $"{rel.PredecessorId}|{rel.SuccessorId}|{rel.Type}";
                if (existingByPair.TryGetValue(pairKey, out var existing))
                {
                    rel.IsAccepted = existing.IsAccepted;
                    rel.IsUserModified = existing.IsUserModified;
                    if (existing.IsUserModified)
                    {
                        rel.LagDays = existing.LagDays;
                        rel.Type = existing.Type;
                    }
                }
                result.Add(rel);
            }

            return result;
        }

        private static List<Relationship> GenerateTradeSequenceRelationships(IReadOnlyList<ActivityItem> activities)
        {
            var relationships = new List<Relationship>();
            var ordered = activities.ToList();

            var tradeOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Site Preparation"] = 0,
                ["Excavation"] = 1,
                ["Foundation"] = 2,
                ["Structure"] = 3,
                ["Structural"] = 3,
                ["Concrete"] = 3,
                ["Steel"] = 3,
                ["Roof"] = 4,
                ["Roofing"] = 4,
                ["External Walls"] = 5,
                ["Masonry"] = 5,
                ["MEP"] = 6,
                ["MEP Rough-in"] = 6,
                ["Electrical"] = 6,
                ["Plumbing"] = 6,
                ["HVAC"] = 6,
                ["Insulation"] = 7,
                ["Drywall"] = 8,
                ["Finishes"] = 9,
                ["Painting"] = 9,
                ["Flooring"] = 9,
                ["Landscaping"] = 10,
            };

            for (int i = 0; i < ordered.Count; i++)
            {
                for (int j = i + 1; j < ordered.Count; j++)
                {
                    var a1 = ordered[i];
                    var a2 = ordered[j];

                    var order1 = tradeOrder.TryGetValue(a1.TradeCategory ?? "", out var o1) ? o1 : -1;
                    var order2 = tradeOrder.TryGetValue(a2.TradeCategory ?? "", out var o2) ? o2 : -1;

                    if (order1 >= 0 && order2 >= 0 && order1 < order2)
                    {
                        var confidence = (order2 - order1 <= 1)
                            ? RelationshipConfidence.High
                            : RelationshipConfidence.Medium;

                        relationships.Add(new Relationship
                        {
                            PredecessorId = a1.ActivityId,
                            PredecessorName = a1.Name,
                            SuccessorId = a2.ActivityId,
                            SuccessorName = a2.Name,
                            Type = RelationshipType.FS,
                            Rationale = $"{a1.TradeCategory} precedes {a2.TradeCategory} in trade sequence",
                            Confidence = confidence,
                            IsAccepted = false,
                            IsUserModified = false,
                            GeneratedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            return relationships;
        }

        private class ParsedRelationship
        {
            public string? PredecessorId { get; set; }
            public string? SuccessorId { get; set; }
            public string? Type { get; set; }
            public double LagDays { get; set; }
            public string? Rationale { get; set; }
            public string? Confidence { get; set; }
        }
    }
}
