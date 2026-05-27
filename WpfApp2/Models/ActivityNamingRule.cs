using System.Text.RegularExpressions;

namespace Som3a_WPF_UI.Models
{
    public static class ActivityNamingRule
    {
        private static readonly Regex VerbNounPattern = new(
            @"^(Install|Pour|Erect|Excavate|Form|Fix|Lay|Weld|Paint|Test|Commission|Demolish|Backfill|Compact|Grout|Seal|Protect|Supply|Fabricate|Deliver|Remove|Cut|Drill|Bolt)\s+.+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsVerbNounFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return VerbNounPattern.IsMatch(name.Trim());
        }

        public static ValidationIssue? Validate(string activityId, string name)
        {
            if (!IsVerbNounFormat(name))
            {
                return new ValidationIssue
                {
                    IssueType = IssueType.NamingConvention,
                    Severity = Severity.Warning,
                    Message = "Activity name should follow verb-noun format (e.g., \"Pour Concrete\")",
                    AffectedField = "Name"
                };
            }

            return null;
        }
    }
}
