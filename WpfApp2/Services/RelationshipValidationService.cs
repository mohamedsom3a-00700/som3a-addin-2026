using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public class RelationshipValidationService : IRelationshipValidationService
    {
        public async Task<ValidationReport> ValidateNetworkAsync(
            RelationshipNetwork network,
            CancellationToken ct = default)
        {
            await Task.Yield();

            var issues = new List<NetworkValidationIssue>();

            DetectCycles(network, issues);
            DetectOpenEnds(network, issues);
            DetectDanglingActivities(network, issues);
            DetectRedundantRelationships(network, issues);
            ValidateReferences(network, issues);
            ValidateLagRanges(network, issues);

            return new ValidationReport
            {
                Issues = issues.ToArray(),
                HasErrors = issues.Any(i => i.Severity == "Error"),
                HasWarnings = issues.Any(i => i.Severity == "Warning"),
                TotalIssueCount = issues.Count,
                ValidatedAt = DateTime.UtcNow
            };
        }

        private static void DetectCycles(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            var graph = new Dictionary<string, List<string>>();
            foreach (var rel in network.Relationships)
            {
                if (!graph.ContainsKey(rel.PredecessorId))
                    graph[rel.PredecessorId] = new List<string>();
                graph[rel.PredecessorId].Add(rel.SuccessorId);
            }

            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();

            foreach (var activity in network.Activities)
            {
                if (HasCycle(activity.ActivityId, graph, visited, recStack))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.CircularDependency,
                        Severity = "Error",
                        Message = $"Circular dependency detected involving activity '{activity.ActivityId}'.",
                        AffectedActivityIds = new[] { activity.ActivityId }
                    });
                    return;
                }
            }
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

        private static void DetectOpenEnds(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            var hasPredecessor = new HashSet<string>();
            var hasSuccessor = new HashSet<string>();

            foreach (var rel in network.Relationships)
            {
                hasPredecessor.Add(rel.SuccessorId);
                hasSuccessor.Add(rel.PredecessorId);
            }

            foreach (var activity in network.Activities)
            {
                if (!hasPredecessor.Contains(activity.ActivityId))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.OpenStart,
                        Severity = "Warning",
                        Message = $"Activity '{activity.ActivityId}' ({activity.Name}) has no predecessors.",
                        AffectedActivityIds = new[] { activity.ActivityId }
                    });
                }
                if (!hasSuccessor.Contains(activity.ActivityId))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.OpenEnd,
                        Severity = "Warning",
                        Message = $"Activity '{activity.ActivityId}' ({activity.Name}) has no successors.",
                        AffectedActivityIds = new[] { activity.ActivityId }
                    });
                }
            }
        }

        private static void DetectDanglingActivities(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            if (network.Relationships.Count == 0)
            {
                foreach (var activity in network.Activities)
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.DanglingActivity,
                        Severity = "Warning",
                        Message = $"Activity '{activity.ActivityId}' ({activity.Name}) has no relationships at all.",
                        AffectedActivityIds = new[] { activity.ActivityId }
                    });
                }
                return;
            }

            var connectedIds = new HashSet<string>();
            foreach (var rel in network.Relationships)
            {
                connectedIds.Add(rel.PredecessorId);
                connectedIds.Add(rel.SuccessorId);
            }

            foreach (var activity in network.Activities)
            {
                if (!connectedIds.Contains(activity.ActivityId))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.DanglingActivity,
                        Severity = "Warning",
                        Message = $"Activity '{activity.ActivityId}' ({activity.Name}) is not connected to any relationship.",
                        AffectedActivityIds = new[] { activity.ActivityId }
                    });
                }
            }
        }

        private static void DetectRedundantRelationships(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            var seen = new HashSet<string>();
            foreach (var rel in network.Relationships)
            {
                var key = $"{rel.PredecessorId}|{rel.SuccessorId}|{rel.Type}|{rel.LagDays}";
                if (!seen.Add(key))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.RedundantRelationship,
                        Severity = "Error",
                        Message = $"Redundant relationship: {rel.PredecessorId} → {rel.SuccessorId}",
                        AffectedRelationshipIds = new[] { rel.Id }
                    });
                }
            }
        }

        private static void ValidateReferences(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            var activityIds = new HashSet<string>(network.Activities.Select(a => a.ActivityId));

            foreach (var rel in network.Relationships)
            {
                if (!activityIds.Contains(rel.PredecessorId))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.InvalidReference,
                        Severity = "Error",
                        Message = $"Predecessor '{rel.PredecessorId}' not found in activity list."
                    });
                }
                if (!activityIds.Contains(rel.SuccessorId))
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.InvalidReference,
                        Severity = "Error",
                        Message = $"Successor '{rel.SuccessorId}' not found in activity list."
                    });
                }
            }
        }

        private static void ValidateLagRanges(RelationshipNetwork network, List<NetworkValidationIssue> issues)
        {
            foreach (var rel in network.Relationships)
            {
                if (rel.LagDays < -365 || rel.LagDays > 365)
                {
                    issues.Add(new NetworkValidationIssue
                    {
                        IssueType = NetworkIssueType.LagOutOfRange,
                        Severity = "Warning",
                        Message = $"Relationship {rel.PredecessorId} → {rel.SuccessorId}: lag of {rel.LagDays} days is unusually large.",
                        AffectedRelationshipIds = new[] { rel.Id }
                    });
                }
            }
        }
    }
}
