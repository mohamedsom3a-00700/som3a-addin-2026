using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Som3a.Bridge;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public class ActivityGenerationService : IActivityGenerationService
    {
        private readonly IServiceContainer _container;
        private readonly IAIBridge _aiBridge;
        private DateTime _lastGenerationTime = DateTime.MinValue;
        private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(10);

        public bool IsAIAvailable => AISettings.IsAIEnabled;

        public ActivityGenerationService(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _aiBridge = container.Resolve<IAIBridge>();
        }

        public async Task<IReadOnlyList<GeneratedActivity>> GenerateActivitiesAsync(
            BOQContext context,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default)
        {
            await EnforceCooldown(ct);
            ct.ThrowIfCancellationRequested();

            progress?.Report(new GenerationProgress("BuildingContext", 10, "Building AI context from BOQ data..."));

            var boqItemsText = string.Join(Environment.NewLine,
                context.Items.Select(i => $"{i.ItemNumber}\t{i.Description}\t{i.Quantity}\t{i.Unit}\t{i.Classification}"));

            var systemPrompt = "You are a construction planning assistant. Generate structured construction activities from BOQ items. Each activity must have a verb-noun name, description, BOQ reference, quantity, and unit.";

            var userPrompt = $@"Given the following BOQ data, generate construction activities.

BOQ Context:
- Workbook: {context.WorkbookName}
- Sheet: {context.SheetName}
- Total Items: {context.ItemCount}
- Total Quantity: {context.TotalQuantity:F2}
- Truncated: {(context.IsTruncated ? "Yes" : "No")}

BOQ Items:
{boqItemsText}

Instructions:
1. Group similar BOQ items into single activities where appropriate
2. Use verb-noun naming convention (e.g., ""Pour Concrete Foundation"", ""Install Rebar"")
3. Each activity must reference its source BOQ items
4. Assign a trade category to each activity
5. Provide quantities and units for each activity
6. Output as a JSON array of objects with properties: name, description, boqReferences (array), quantity, unit, tradeCategory, wbsPath";

            progress?.Report(new GenerationProgress("CallingAI", 40, "Sending BOQ data to AI provider..."));

            var activities = await CallAIProviderAsync(systemPrompt, userPrompt, ct);

            ct.ThrowIfCancellationRequested();

            progress?.Report(new GenerationProgress("Parsing", 70, "Parsing AI response into activities..."));

            var parsed = ParseActivitiesResponse(activities);

            progress?.Report(new GenerationProgress("Mapping", 90, "Mapping activities to BOQ references..."));

            var result = MapToGeneratedActivities(parsed, context);
            _lastGenerationTime = DateTime.UtcNow;

            progress?.Report(new GenerationProgress("Complete", 100, $"Generated {result.Count} activities."));

            return result;
        }

        public async Task<IReadOnlyList<GeneratedActivity>> RegenerateActivitiesAsync(
            BOQContext context,
            IReadOnlyList<GeneratedActivity> previousActivities,
            IProgress<GenerationProgress>? progress = null,
            CancellationToken ct = default)
        {
            var newActivities = await GenerateActivitiesAsync(context, progress, ct);
            ct.ThrowIfCancellationRequested();

            return PreserveUserEdits(newActivities, previousActivities);
        }

        private async Task EnforceCooldown(CancellationToken ct)
        {
            var elapsed = DateTime.UtcNow - _lastGenerationTime;
            if (elapsed < CooldownPeriod && _lastGenerationTime != DateTime.MinValue)
            {
                var delay = CooldownPeriod - elapsed;
                await Task.Delay(delay, ct);
            }
        }

        private async Task<string> CallAIProviderAsync(string systemPrompt, string userPrompt, CancellationToken ct)
        {
            var request = new AIBridgeRequest
            {
                SystemPrompt = systemPrompt + " Output your response as a JSON array of objects with properties: name, description, boqReferences (array of strings), quantity (number), unit, tradeCategory, wbsPath. Return ONLY valid JSON, no markdown.",
                UserPrompt = userPrompt,
                Temperature = 0.3f,
                MaxTokens = 4096,
                ProviderType = AISettings.ProviderType == Services.AIProviderType.Ollama ? "ollama" : "cloud",
                ApiKey = AISettings.CloudApiKey,
                Model = AISettings.CloudMainModel,
                Endpoint = AISettings.ProviderType == Services.AIProviderType.Ollama ? AISettings.OllamaEndpoint : null
            };

            var response = await _aiBridge.ExecutePromptAsync(request, ct);
            ct.ThrowIfCancellationRequested();

            if (!response.IsSuccess)
                throw new InvalidOperationException(
                    $"AI generation failed: {response.ErrorMessage}");

            return response.Content;
        }

        private static List<ParsedActivity> ParseActivitiesResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return new List<ParsedActivity>();

            try
            {
                var activities = JsonConvert.DeserializeObject<List<ParsedActivity>>(response);
                return activities ?? new List<ParsedActivity>();
            }
            catch
            {
                return new List<ParsedActivity>();
            }
        }

        private static List<GeneratedActivity> MapToGeneratedActivities(
            List<ParsedActivity> parsed,
            BOQContext context)
        {
            var results = new List<GeneratedActivity>();
            int seq = 0;

            foreach (var p in parsed)
            {
                seq++;
                results.Add(new GeneratedActivity
                {
                    ActivityId = p.ActivityId ?? $"A-{seq:D3}",
                    Name = p.Name ?? string.Empty,
                    Description = p.Description ?? string.Empty,
                    BoqReferences = p.BoqReferences?.ToList() ?? new List<string>(),
                    Quantity = p.Quantity,
                    Unit = p.Unit ?? string.Empty,
                    TradeCategory = p.TradeCategory ?? string.Empty,
                    WbsPath = p.WbsPath ?? string.Empty,
                    SortOrder = seq,
                    OriginalName = p.Name ?? string.Empty,
                    ValidationStatus = ValidationStatus.Pending
                });
            }

            return results;
        }

        private static IReadOnlyList<GeneratedActivity> PreserveUserEdits(
            IReadOnlyList<GeneratedActivity> newActivities,
            IReadOnlyList<GeneratedActivity> previousActivities)
        {
            var previousByRefs = previousActivities
                .Where(a => a.BoqReferences != null && a.BoqReferences.Count > 0 && a.IsUserModified)
                .SelectMany(a => a.BoqReferences.Select(r => new { Ref = r, Activity = a }))
                .GroupBy(x => x.Ref, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Activity, StringComparer.OrdinalIgnoreCase);

            var dependencyMap = previousActivities
                .Where(a => a.Dependencies != null && a.Dependencies.Count > 0)
                .SelectMany(a => a.Dependencies.Select(d => new { Dep = d, FromActivity = a }))
                .GroupBy(x => x.FromActivity.ActivityId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Dep).ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var activity in newActivities)
            {
                if (activity.BoqReferences == null) continue;

                foreach (var boqRef in activity.BoqReferences)
                {
                    if (previousByRefs.TryGetValue(boqRef, out var previous))
                    {
                        activity.Name = previous.Name;
                        activity.Description = previous.Description;
                        activity.IsUserModified = true;
                        break;
                    }
                }

                if (dependencyMap.TryGetValue(activity.ActivityId, out var prevDeps))
                {
                    activity.Dependencies ??= new List<string>();
                    foreach (var dep in prevDeps)
                    {
                        if (!activity.Dependencies.Any(d => string.Equals(d, dep, StringComparison.OrdinalIgnoreCase)))
                            activity.Dependencies.Add(dep);
                    }
                }
            }

            return newActivities.ToList();
        }
    }

    internal class ParsedActivity
    {
        public string? ActivityId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string[]? BoqReferences { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? TradeCategory { get; set; }
        public string? WbsPath { get; set; }
    }
}
