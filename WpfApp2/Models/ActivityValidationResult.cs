using System;

namespace Som3a_WPF_UI.Models
{
    public class ActivityValidationResult
    {
        public string ActivityId { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public ValidationIssue[] Errors { get; set; } = Array.Empty<ValidationIssue>();
        public ValidationIssue[] Warnings { get; set; } = Array.Empty<ValidationIssue>();
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ValidationIssue
    {
        public IssueType IssueType { get; set; }
        public Severity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AffectedField { get; set; } = string.Empty;
    }

    public enum IssueType
    {
        DuplicateDetection,
        NamingConvention,
        MissingReference,
        QuantityInconsistency
    }

    public enum Severity
    {
        Error,
        Warning
    }

    public enum ValidationStatus
    {
        Pending,
        Passed,
        HasWarnings,
        HasErrors
    }
}
