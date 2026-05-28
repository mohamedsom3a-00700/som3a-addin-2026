using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public class RelationshipAnalysisService : IRelationshipAnalysisService
    {
        public async Task<IReadOnlyList<ParallelExecutionGroup>> AnalyzeParallelGroupsAsync(
            RelationshipNetwork network,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();

            var adjacency = BuildAdjacencyList(network, ct);
            var components = GetWeaklyConnectedComponents(network.Activities, adjacency, ct);

            var groups = new List<ParallelExecutionGroup>();
            int groupIndex = 0;

            foreach (var component in components)
            {
                ct.ThrowIfCancellationRequested();
                var levels = GetTopologicalLevels(component, adjacency, ct);

                foreach (var level in levels)
                {
                    ct.ThrowIfCancellationRequested();
                    groupIndex++;
                    groups.Add(new ParallelExecutionGroup
                    {
                        GroupId = $"G-{groupIndex:D3}",
                        ActivityIds = level.ToArray(),
                        Reason = level.Count > 1
                            ? "Activities at the same topological level can run in parallel"
                            : "Sequential activity in the dependency chain",
                        TopologicalLevel = groupIndex
                    });
                }
            }

            return groups;
        }

        public async Task<CriticalPathResult> AnalyzeCriticalPathAsync(
            RelationshipNetwork network,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();

            var adjacency = BuildAdjacencyList(network, ct);
            var topologicalOrder = TopologicalSort(network.Activities, adjacency, ct);

            if (topologicalOrder == null)
            {
                return new CriticalPathResult
                {
                    Path = Array.Empty<string>(),
                    CriticalActivities = 0,
                    TotalActivities = network.Activities.Count,
                    CriticalityIndex = 0
                };
            }

            var durations = network.Activities.ToDictionary(a => a.ActivityId, _ => 1.0);
            var dist = new Dictionary<string, double>();
            var prev = new Dictionary<string, string?>();

            foreach (var id in topologicalOrder)
            {
                dist[id] = 0;
                prev[id] = null;
            }

            foreach (var u in topologicalOrder)
            {
                ct.ThrowIfCancellationRequested();
                if (!adjacency.TryGetValue(u, out var neighbors)) continue;
                foreach (var v in neighbors)
                {
                    double edgeWeight;
                    if (!durations.TryGetValue(u, out edgeWeight)) edgeWeight = 1.0;
                    if (dist[u] + edgeWeight > dist[v])
                    {
                        dist[v] = dist[u] + edgeWeight;
                        prev[v] = u;
                    }
                }
            }

            var maxNode = topologicalOrder.OrderByDescending(n => { double v; return dist.TryGetValue(n, out v) ? v : 0.0; }).FirstOrDefault();
            var path = new List<string>();
            if (maxNode != null)
            {
                var current = maxNode;
                while (current != null)
                {
                    path.Add(current);
                    prev.TryGetValue(current, out current);
                }
                path.Reverse();
            }

            return new CriticalPathResult
            {
                Path = path.ToArray(),
                TotalDuration = TimeSpan.FromDays(maxNode != null && dist.TryGetValue(maxNode, out var maxDist) ? maxDist : 0),
                CriticalActivities = path.Count,
                TotalActivities = network.Activities.Count,
                CriticalityIndex = network.Activities.Count > 0
                    ? (double)path.Count / network.Activities.Count
                    : 0
            };
        }

        public async Task<IReadOnlyList<ResourceConflict>> DetectResourceConflictsAsync(
            RelationshipNetwork network,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();

            var conflicts = new List<ResourceConflict>();
            var adjacency = BuildAdjacencyList(network, ct);
            var components = GetWeaklyConnectedComponents(network.Activities, adjacency, ct);

            foreach (var component in components)
            {
                ct.ThrowIfCancellationRequested();
                var levels = GetTopologicalLevels(component, adjacency, ct);
                foreach (var level in levels)
                {
                    ct.ThrowIfCancellationRequested();
                    if (level.Count < 2) continue;

                    var activityById = network.Activities.ToDictionary(a => a.ActivityId);
                    var resourcesInLevel = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                    foreach (var id in level)
                    {
                        var resourceType = InferResourceType(id);
                        if (!resourcesInLevel.ContainsKey(resourceType))
                            resourcesInLevel[resourceType] = new List<string>();
                        resourcesInLevel[resourceType].Add(id);
                    }

                    foreach (var kvp in resourcesInLevel)
                    {
                        if (kvp.Value.Count > 1)
                        {
                            conflicts.Add(new ResourceConflict
                            {
                                ActivityIds = kvp.Value.ToArray(),
                                ResourceType = kvp.Key,
                                Severity = "Warning",
                                Message = $"Multiple activities in parallel group share '{kvp.Key}' resource"
                            });
                        }
                    }
                }
            }

            return conflicts;
        }

        private static Dictionary<string, List<string>> BuildAdjacencyList(RelationshipNetwork network, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var adj = new Dictionary<string, List<string>>();
            foreach (var a in network.Activities)
                adj[a.ActivityId] = new List<string>();

            foreach (var rel in network.Relationships)
            {
                ct.ThrowIfCancellationRequested();
                if (!adj.ContainsKey(rel.PredecessorId))
                    adj[rel.PredecessorId] = new List<string>();
                adj[rel.PredecessorId].Add(rel.SuccessorId);
            }

            return adj;
        }

        private static List<List<string>> GetWeaklyConnectedComponents(
            IReadOnlyList<ActivityItem> activities,
            Dictionary<string, List<string>> adjacency,
            CancellationToken ct = default)
        {
            var visited = new HashSet<string>();
            var components = new List<List<string>>();

            var reverseAdj = new Dictionary<string, List<string>>();
            foreach (var kvp in adjacency)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var neighbor in kvp.Value)
                {
                    if (!reverseAdj.ContainsKey(neighbor))
                        reverseAdj[neighbor] = new List<string>();
                    reverseAdj[neighbor].Add(kvp.Key);
                }
            }

            foreach (var activity in activities)
            {
                ct.ThrowIfCancellationRequested();
                if (visited.Contains(activity.ActivityId)) continue;

                var component = new List<string>();
                var stack = new Stack<string>();
                stack.Push(activity.ActivityId);

                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    if (!visited.Add(node)) continue;
                    component.Add(node);

                    if (adjacency.TryGetValue(node, out var neighbors))
                    {
                        foreach (var n in neighbors)
                        {
                            if (!visited.Contains(n)) stack.Push(n);
                        }
                    }
                    if (reverseAdj.TryGetValue(node, out var revNeighbors))
                    {
                        foreach (var n in revNeighbors)
                        {
                            if (!visited.Contains(n)) stack.Push(n);
                        }
                    }
                }

                components.Add(component);
            }

            return components;
        }

        private static List<List<string>> GetTopologicalLevels(
            List<string> component,
            Dictionary<string, List<string>> adjacency,
            CancellationToken ct = default)
        {
            var inDegree = new Dictionary<string, int>();
            foreach (var node in component)
                inDegree[node] = 0;

            foreach (var node in component)
            {
                ct.ThrowIfCancellationRequested();
                if (!adjacency.TryGetValue(node, out var neighbors)) continue;
                foreach (var neighbor in neighbors)
                {
                    if (inDegree.ContainsKey(neighbor))
                        inDegree[neighbor]++;
                }
            }

            var levels = new List<List<string>>();
            var queue = new Queue<string>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
            var processed = new HashSet<string>();

            while (queue.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var level = new List<string>();
                var count = queue.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = queue.Dequeue();
                    if (!processed.Add(node)) continue;
                    level.Add(node);

                    if (!adjacency.TryGetValue(node, out var neighbors)) continue;
                    foreach (var neighbor in neighbors)
                    {
                        if (!inDegree.ContainsKey(neighbor)) continue;
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }
                if (level.Count > 0)
                    levels.Add(level);
            }

            return levels;
        }

        private static List<string>? TopologicalSort(
            IReadOnlyList<ActivityItem> activities,
            Dictionary<string, List<string>> adjacency,
            CancellationToken ct = default)
        {
            var inDegree = new Dictionary<string, int>();
            foreach (var a in activities)
                inDegree[a.ActivityId] = 0;

            foreach (var kvp in adjacency)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var neighbor in kvp.Value)
                {
                    if (!inDegree.ContainsKey(neighbor)) continue;
                    inDegree[neighbor]++;
                }
            }

            var queue = new Queue<string>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));
            var result = new List<string>();

            while (queue.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var node = queue.Dequeue();
                result.Add(node);

                if (!adjacency.TryGetValue(node, out var neighbors)) continue;
                foreach (var neighbor in neighbors)
                {
                    if (!inDegree.ContainsKey(neighbor)) continue;
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            return result.Count == activities.Count ? result : null;
        }

        private static string InferResourceType(string activityId)
        {
            if (activityId.IndexOf("CONC", StringComparison.OrdinalIgnoreCase) >= 0 ||
                activityId.IndexOf("CONCRETE", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Concrete";
            if (activityId.IndexOf("REBAR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                activityId.IndexOf("STEEL", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Steel";
            if (activityId.IndexOf("CRANE", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Crane";
            if (activityId.IndexOf("MEP", StringComparison.OrdinalIgnoreCase) >= 0 ||
                activityId.IndexOf("ELEC", StringComparison.OrdinalIgnoreCase) >= 0 ||
                activityId.IndexOf("PLUMB", StringComparison.OrdinalIgnoreCase) >= 0)
                return "MEP";
            if (activityId.IndexOf("PAINT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                activityId.IndexOf("FINISH", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Finishes";

            return "General Labor";
        }
    }
}
