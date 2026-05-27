using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public class ActivitySequencingService : IActivitySequencingService
    {
        private static readonly Dictionary<string, int> TradeOrder = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Site Preparation"] = 1,
            ["Excavation"] = 2,
            ["Foundation"] = 3,
            ["Structure"] = 4,
            ["Roof"] = 5,
            ["External Walls"] = 6,
            ["MEP"] = 7,
            ["MEP Rough-in"] = 7,
            ["Insulation"] = 8,
            ["Drywall"] = 9,
            ["Finishes"] = 10,
            ["Landscaping"] = 11,
        };

        private static readonly (string Predecessor, string Successor, string Rationale)[] TradePrecedence = {
            ("Site Preparation", "Excavation", "Site must be prepared before excavation can begin."),
            ("Excavation", "Foundation", "Excavation must be complete before foundation work."),
            ("Foundation", "Structure", "Foundations must cure before structural work."),
            ("Structure", "Roof", "Structure must be complete before roofing."),
            ("Structure", "MEP Rough-in", "Structure must be framed before MEP rough-in."),
            ("MEP Rough-in", "Insulation", "MEP rough-in must be complete before insulation."),
            ("Insulation", "Drywall", "Insulation must be installed before drywall."),
            ("Drywall", "Finishes", "Drywall must be complete before finishing works."),
            ("Finishes", "Landscaping", "Interior finishes should be complete before final landscaping."),
        };

        public async Task<IReadOnlyList<ActivitySequenceOrder>> SequenceAsync(
            IReadOnlyList<GeneratedActivity> activities,
            CancellationToken ct = default)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            var ordered = activities
                .OrderBy(a => GetTradePriority(a.TradeCategory))
                .ThenBy(a => a.SortOrder)
                .Select((a, index) => new ActivitySequenceOrder
                {
                    ActivityId = a.ActivityId,
                    Name = a.Name,
                    WbsPath = a.WbsPath ?? string.Empty,
                    SortOrder = index + 1,
                    ParallelGroup = GetParallelGroup(a, activities),
                    Dependencies = Array.Empty<ActivityDependency>()
                })
                .ToList();

            return ordered;
        }

        public async Task<IReadOnlyList<ActivityDependency>> SuggestDependenciesAsync(
            IReadOnlyList<GeneratedActivity> activities,
            CancellationToken ct = default)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            var dependencies = new List<ActivityDependency>();

            foreach (var (predecessorTrade, successorTrade, rationale) in TradePrecedence)
            {
                ct.ThrowIfCancellationRequested();

                var predecessors = activities
                    .Where(a => string.Equals(a.TradeCategory, predecessorTrade, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(a => a.SortOrder)
                    .ToList();

                var successors = activities
                    .Where(a => string.Equals(a.TradeCategory, successorTrade, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(a => a.SortOrder)
                    .ToList();

                foreach (var successor in successors)
                {
                    var bestPredecessor = predecessors.LastOrDefault();
                    if (bestPredecessor != null)
                    {
                        dependencies.Add(new ActivityDependency
                        {
                            PredecessorId = bestPredecessor.ActivityId,
                            SuccessorId = successor.ActivityId,
                            Type = DependencyType.FinishToStart,
                            Confidence = "High",
                            Rationale = rationale,
                            IsAccepted = false
                        });
                    }
                }
            }

            return dependencies;
        }

        private static int GetTradePriority(string? tradeCategory)
        {
            if (tradeCategory == null)
                return 999;

            return TradeOrder.TryGetValue(tradeCategory, out var priority) ? priority : 100;
        }

        private static string? GetParallelGroup(GeneratedActivity activity, IReadOnlyList<GeneratedActivity> allActivities)
        {
            if (string.IsNullOrWhiteSpace(activity.WbsPath))
                return null;

            var sameWbs = allActivities
                .Where(a => string.Equals(a.WbsPath, activity.WbsPath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (sameWbs.Count > 1)
                return $"WBS:{activity.WbsPath}";

            return null;
        }
    }
}
