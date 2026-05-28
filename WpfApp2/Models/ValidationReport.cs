using System;

namespace Som3a_WPF_UI.Models
{
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

    public class NetworkValidationIssue
    {
        public NetworkIssueType IssueType { get; set; }
        public string Severity { get; set; } = "Error";
        public string Message { get; set; } = string.Empty;
        public string[] AffectedActivityIds { get; set; } = Array.Empty<string>();
        public string[] AffectedRelationshipIds { get; set; } = Array.Empty<string>();
        public string? Details { get; set; }
    }

    public class ValidationReport
    {
        public NetworkValidationIssue[] Issues { get; set; } = Array.Empty<NetworkValidationIssue>();
        public bool HasErrors { get; set; }
        public bool HasWarnings { get; set; }
        public int TotalIssueCount { get; set; }
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    }
}
