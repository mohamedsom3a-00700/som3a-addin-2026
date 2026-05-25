using Som3a.Contracts;
using Som3a.Domain.Activities;

namespace Som3a.Validation.Duration
{
    public class DurationLogicValidator
    {
        public ValidationResult ValidateDurations(List<Activity> activities)
        {
            var warnings = new List<string>();

            foreach (var activity in activities)
            {
                if (activity.Duration.HasValue && activity.Duration.Value <= TimeSpan.Zero)
                    return ValidationResult.Failure(
                        $"Activity '{activity.ActivityId}' has non-positive duration.");

                if (activity.Duration.HasValue && activity.ProductivityRate > 0)
                {
                    var expectedDuration = (double)activity.Quantity / (double)activity.ProductivityRate;
                    var actualDays = activity.Duration.Value.TotalDays;
                    var delta = Math.Abs(actualDays - expectedDuration);

                    if (delta > expectedDuration * 0.5 && expectedDuration > 0)
                    {
                        warnings.Add(
                            $"Activity '{activity.ActivityId}' duration ({actualDays:F1}d) deviates " +
                            $"from productivity-based estimate ({expectedDuration:F1}d).");
                    }
                }

                if (activity.Quantity > 0 && !activity.Duration.HasValue)
                    warnings.Add(
                        $"Activity '{activity.ActivityId}' has quantity but no duration set.");

                if (activity.ResourceAssignments.Count > 0 && !activity.Duration.HasValue)
                    warnings.Add(
                        $"Activity '{activity.ActivityId}' has resource assignments but no duration.");
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
