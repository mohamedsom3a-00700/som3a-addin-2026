using Som3a.Contracts;
using Som3a.Domain.Relationships;

namespace Som3a.Validation.Relationships
{
    public class LoopDetector
    {
        public ValidationResult DetectLoops(List<Relationship> relationships, List<Domain.Activities.Activity> activities)
        {
            var graph = new Dictionary<string, List<string>>();
            foreach (var r in relationships)
            {
                if (!graph.ContainsKey(r.Predecessor.ActivityId))
                    graph[r.Predecessor.ActivityId] = new List<string>();
                graph[r.Predecessor.ActivityId].Add(r.Successor.ActivityId);
            }

            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();

            foreach (var activity in activities)
            {
                if (HasCycle(activity.ActivityId, graph, visited, recStack))
                    return ValidationResult.Failure(
                        $"Cycle detected involving activity '{activity.ActivityId}'.");
            }

            return ValidationResult.Success();
        }

        private static bool HasCycle(string node, Dictionary<string, List<string>> graph,
            HashSet<string> visited, HashSet<string> recStack)
        {
            if (recStack.Contains(node)) return true;
            if (visited.Contains(node)) return false;

            visited.Add(node);
            recStack.Add(node);

            if (graph.TryGetValue(node, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (HasCycle(neighbor, graph, visited, recStack))
                        return true;
                }
            }

            recStack.Remove(node);
            return false;
        }
    }
}
