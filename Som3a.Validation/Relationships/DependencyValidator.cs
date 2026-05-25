using Som3a.Contracts;
using Som3a.Domain.Relationships;

namespace Som3a.Validation.Relationships
{
    public class DependencyValidator
    {
        public ValidationResult ValidateDependencies(
            List<Relationship> relationships,
            List<Domain.Activities.Activity> activities)
        {
            var activityIds = new HashSet<string>(activities.Select(a => a.ActivityId));
            var warnings = new List<string>();

            foreach (var rel in relationships)
            {
                if (!activityIds.Contains(rel.Predecessor.ActivityId))
                    return ValidationResult.Failure(
                        $"Predecessor '{rel.Predecessor.ActivityId}' not found in activity list.");

                if (!activityIds.Contains(rel.Successor.ActivityId))
                    return ValidationResult.Failure(
                        $"Successor '{rel.Successor.ActivityId}' not found in activity list.");

                if (rel.Predecessor.ActivityId == rel.Successor.ActivityId)
                    return ValidationResult.Failure(
                        $"Activity '{rel.Predecessor.ActivityId}' cannot depend on itself.");

                if (rel.Lag.TotalDays < -365 || rel.Lag.TotalDays > 365)
                    warnings.Add($"Relationship lag of {rel.Lag.TotalDays} days is unusually large.");
            }

            if (warnings.Count > 0)
            {
                var result = ValidationResult.Success();
                result.Warnings = warnings;
                return result;
            }

            return ValidationResult.Success();
        }
    }
}
