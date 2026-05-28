using Som3a.Contracts;
using Som3a.Domain.Activities;
using Som3a.Domain.Relationships;

namespace Som3a.Validation.Relationships
{
    public class OpenEndDetector
    {
        public ValidationResult DetectOpenEnds(
            List<Relationship> relationships,
            List<Activity> activities)
        {
            var activityIds = new HashSet<string>(activities.Select(a => a.ActivityId));
            var hasPredecessor = new HashSet<string>();
            var hasSuccessor = new HashSet<string>();

            foreach (var rel in relationships)
            {
                hasPredecessor.Add(rel.Successor.ActivityId);
                hasSuccessor.Add(rel.Predecessor.ActivityId);
            }

            var openStarts = activityIds
                .Where(id => !hasPredecessor.Contains(id))
                .ToList();

            var openEnds = activityIds
                .Where(id => !hasSuccessor.Contains(id))
                .ToList();

            if (openStarts.Count == 0 && openEnds.Count == 0)
                return ValidationResult.Success();

            var messages = new List<string>();

            if (openStarts.Count > 0)
                messages.Add($"Open-start activities (no predecessors): {string.Join(", ", openStarts)}");

            if (openEnds.Count > 0)
                messages.Add($"Open-end activities (no successors): {string.Join(", ", openEnds)}");

            var result = ValidationResult.Success();
            result.Warnings = messages;
            return result;
        }
    }
}
