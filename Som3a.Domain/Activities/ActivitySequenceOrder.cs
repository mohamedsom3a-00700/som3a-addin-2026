using System;

namespace Som3a.Domain.Activities
{
    public class ActivitySequenceOrder
    {
        public string ActivityId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WbsPath { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? ParallelGroup { get; set; }
        public ActivityDependency[] Dependencies { get; set; } = Array.Empty<ActivityDependency>();
    }

    public class ActivityDependency
    {
        public string PredecessorId { get; set; } = string.Empty;
        public string SuccessorId { get; set; } = string.Empty;
        public DependencyType Type { get; set; } = DependencyType.FinishToStart;
        public string Confidence { get; set; } = "Medium";
        public string Rationale { get; set; } = string.Empty;
        public bool IsAccepted { get; set; }
        public bool IsUserModified { get; set; }
    }

    public enum DependencyType
    {
        FinishToStart
    }
}
