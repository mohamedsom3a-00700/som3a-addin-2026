using System;

namespace Som3a_WPF_UI.Models
{
    public enum RelationshipType
    {
        FS,
        SS,
        FF,
        SF
    }

    public enum RelationshipConfidence
    {
        High,
        Medium,
        Low
    }

    public class Relationship
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string PredecessorId { get; set; } = string.Empty;
        public string PredecessorName { get; set; } = string.Empty;
        public string SuccessorId { get; set; } = string.Empty;
        public string SuccessorName { get; set; } = string.Empty;
        public RelationshipType Type { get; set; } = RelationshipType.FS;
        public TimeSpan Lag { get; set; }
        public double LagDays
        {
            get => Lag.TotalDays;
            set => Lag = TimeSpan.FromDays(value);
        }
        public string? Rationale { get; set; }
        public RelationshipConfidence Confidence { get; set; } = RelationshipConfidence.Medium;
        public bool IsAccepted { get; set; }
        public bool IsUserModified { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public string ValidationStatus { get; set; } = "Pending";
        public string? ValidationMessage { get; set; }
    }
}
