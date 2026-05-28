using Som3a.Contracts;
using Som3a.Domain.Activities;
using Som3a.Domain.Relationships;

namespace Som3a.Validation.Relationships
{
    public class NetworkValidator
    {
        private readonly DependencyValidator _dependencyValidator;
        private readonly LoopDetector _loopDetector;
        private readonly OpenEndDetector _openEndDetector;

        public NetworkValidator()
        {
            _dependencyValidator = new DependencyValidator();
            _loopDetector = new LoopDetector();
            _openEndDetector = new OpenEndDetector();
        }

        public NetworkValidationReport Validate(
            List<Relationship> relationships,
            List<Activity> activities)
        {
            var issues = new List<NetworkValidationIssue>();

            var depResult = _dependencyValidator.ValidateDependencies(relationships, activities);
            if (depResult.Warnings.Count > 0)
            {
                foreach (var warning in depResult.Warnings)
                {
                    issues.Add(new NetworkValidationIssue(
                        NetworkIssueType.LagOutOfRange,
                        "Warning",
                        warning));
                }
            }
            if (!depResult.IsSuccess)
                issues.Add(new NetworkValidationIssue(
                    NetworkIssueType.InvalidReference,
                    "Error",
                    depResult.ErrorMessage ?? "Invalid dependency reference."));

            var loopResult = _loopDetector.DetectLoops(relationships, activities);
            if (!loopResult.IsSuccess)
                issues.Add(new NetworkValidationIssue(
                    NetworkIssueType.CircularDependency,
                    "Error",
                    loopResult.ErrorMessage ?? "Circular dependency detected."));

            var openEndResult = _openEndDetector.DetectOpenEnds(relationships, activities);
            if (openEndResult.Warnings.Count > 0)
            {
                foreach (var warning in openEndResult.Warnings)
                {
                    var type = warning.Contains("no predecessors")
                        ? NetworkIssueType.OpenStart
                        : NetworkIssueType.OpenEnd;
                    issues.Add(new NetworkValidationIssue(type, "Warning", warning));
                }
            }

            DetectDanglingActivities(relationships, activities, issues);
            DetectRedundantRelationships(relationships, issues);

            return new NetworkValidationReport(issues);
        }

        private static void DetectDanglingActivities(
            List<Relationship> relationships,
            List<Activity> activities,
            List<NetworkValidationIssue> issues)
        {
            var connectedIds = new HashSet<string>();
            foreach (var rel in relationships)
            {
                connectedIds.Add(rel.Predecessor.ActivityId);
                connectedIds.Add(rel.Successor.ActivityId);
            }

            foreach (var activity in activities)
            {
                if (!connectedIds.Contains(activity.ActivityId))
                {
                    issues.Add(new NetworkValidationIssue(
                        NetworkIssueType.DanglingActivity,
                        "Warning",
                        $"Activity '{activity.ActivityId}' ({activity.Name}) has no relationships."));
                }
            }
        }

        private static void DetectRedundantRelationships(
            List<Relationship> relationships,
            List<NetworkValidationIssue> issues)
        {
            var seen = new HashSet<string>();

            foreach (var rel in relationships)
            {
                var key = $"{rel.Predecessor.ActivityId}|{rel.Successor.ActivityId}|{rel.Type}|{rel.Lag.TotalDays}";
                if (!seen.Add(key))
                {
                    issues.Add(new NetworkValidationIssue(
                        NetworkIssueType.RedundantRelationship,
                        "Error",
                        $"Redundant relationship: {rel.Predecessor.ActivityId} → {rel.Successor.ActivityId} ({rel.Type}, lag={rel.Lag.TotalDays}d)"));
                }
            }
        }
    }

    public enum NetworkIssueType
    {
        CircularDependency,
        OpenStart,
        OpenEnd,
        DanglingActivity,
        RedundantRelationship,
        InvalidReference,
        LagOutOfRange
    }

    public record NetworkValidationIssue(
        NetworkIssueType IssueType,
        string Severity,
        string Message);

    public class NetworkValidationReport
    {
        public List<NetworkValidationIssue> Issues { get; }
        public bool HasErrors => Issues.Any(i => i.Severity == "Error");
        public bool HasWarnings => Issues.Any(i => i.Severity == "Warning");
        public int TotalIssueCount => Issues.Count;

        public NetworkValidationReport(List<NetworkValidationIssue> issues)
        {
            Issues = issues ?? new List<NetworkValidationIssue>();
        }
    }
}
