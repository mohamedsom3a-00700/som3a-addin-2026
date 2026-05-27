using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public class ActivityValidationService : IActivityValidationService
    {
        public async Task<IReadOnlyList<ActivityValidationResult>> ValidateAsync(
            IReadOnlyList<GeneratedActivity> activities,
            BOQContext context,
            CancellationToken ct = default)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            var results = new List<ActivityValidationResult>();

            foreach (var activity in activities)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(ValidateSingle(activity, activities, context));
            }

            return results;
        }

        public ActivityValidationResult ValidateSingle(
            GeneratedActivity activity,
            IReadOnlyList<GeneratedActivity> allActivities,
            BOQContext context)
        {
            var errors = new List<ValidationIssue>();
            var warnings = new List<ValidationIssue>();

            var duplicateIssue = DetectDuplicate(activity, allActivities);
            if (duplicateIssue != null)
                warnings.Add(duplicateIssue);

            var namingIssue = ActivityNamingRule.Validate(activity.ActivityId, activity.Name);
            if (namingIssue != null)
                warnings.Add(namingIssue);

            var refIssue = CheckMissingReference(activity, context);
            if (refIssue != null)
                errors.Add(refIssue);

            var quantityIssue = CheckQuantityConsistency(activity, context);
            if (quantityIssue != null)
                warnings.Add(quantityIssue);

            var status = errors.Count > 0
                ? ValidationStatus.HasErrors
                : warnings.Count > 0
                    ? ValidationStatus.HasWarnings
                    : ValidationStatus.Passed;

            activity.ValidationStatus = status;

            return new ActivityValidationResult
            {
                ActivityId = activity.ActivityId,
                IsValid = errors.Count == 0,
                Errors = errors.ToArray(),
                Warnings = warnings.ToArray(),
                ValidatedAt = DateTime.UtcNow
            };
        }

        private static ValidationIssue? DetectDuplicate(
            GeneratedActivity activity,
            IReadOnlyList<GeneratedActivity> allActivities)
        {
            var duplicates = allActivities
                .Where(a => a.ActivityId != activity.ActivityId)
                .Where(a => string.Equals(a.Name, activity.Name, StringComparison.OrdinalIgnoreCase))
                .Where(a => a.BoqReferences != null && activity.BoqReferences != null &&
                            a.BoqReferences.Intersect(activity.BoqReferences, StringComparer.OrdinalIgnoreCase).Any())
                .ToList();

            if (duplicates.Count > 0)
            {
                var dupIds = string.Join(", ", duplicates.Select(d => d.ActivityId));
                return new ValidationIssue
                {
                    IssueType = IssueType.DuplicateDetection,
                    Severity = Severity.Warning,
                    Message = $"Similar activity found: {dupIds}. Consider merging.",
                    AffectedField = "Name"
                };
            }

            return null;
        }

        private static ValidationIssue? CheckMissingReference(
            GeneratedActivity activity,
            BOQContext context)
        {
            if (activity.BoqReferences == null || activity.BoqReferences.Count == 0)
            {
                return new ValidationIssue
                {
                    IssueType = IssueType.MissingReference,
                    Severity = Severity.Error,
                    Message = "Activity has no BOQ references.",
                    AffectedField = "BoqReferences"
                };
            }

            var validRefs = context.Items
                .Select(i => i.Identifier)
                .Where(id => id != null)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missing = activity.BoqReferences
                .Where(r => !validRefs.Contains(r))
                .ToList();

            if (missing.Count > 0)
            {
                return new ValidationIssue
                {
                    IssueType = IssueType.MissingReference,
                    Severity = Severity.Error,
                    Message = $"BOQ references not found: {string.Join(", ", missing)}",
                    AffectedField = "BoqReferences"
                };
            }

            return null;
        }

        private static ValidationIssue? CheckQuantityConsistency(
            GeneratedActivity activity,
            BOQContext context)
        {
            if (activity.BoqReferences == null || activity.BoqReferences.Count == 0)
                return null;

            var refItems = context.Items
                .Where(i => i.Identifier != null &&
                            activity.BoqReferences.Any(r => string.Equals(r, i.Identifier, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (refItems.Count == 0)
                return null;

            var boqTotal = refItems.Sum(i => i.Quantity);
            if (boqTotal == 0)
                return null;

            var diffPercent = Math.Abs((activity.Quantity - boqTotal) / boqTotal) * 100;
            if (diffPercent > 10)
            {
                return new ValidationIssue
                {
                    IssueType = IssueType.QuantityInconsistency,
                    Severity = Severity.Warning,
                    Message = $"Activity quantity ({activity.Quantity}) differs from BOQ total ({boqTotal}) by {diffPercent:F1}%.",
                    AffectedField = "Quantity"
                };
            }

            return null;
        }
    }
}
