namespace Som3a_WPF_UI.Models
{
    public class ParallelExecutionGroup
    {
        public string GroupId { get; set; } = string.Empty;
        public string[] ActivityIds { get; set; } = System.Array.Empty<string>();
        public string Reason { get; set; } = string.Empty;
        public int TopologicalLevel { get; set; }
    }

    public class CriticalPathResult
    {
        public string[] Path { get; set; } = System.Array.Empty<string>();
        public System.TimeSpan TotalDuration { get; set; }
        public int CriticalActivities { get; set; }
        public int TotalActivities { get; set; }
        public double CriticalityIndex { get; set; }
    }

    public class ResourceConflict
    {
        public string[] ActivityIds { get; set; } = System.Array.Empty<string>();
        public string ResourceType { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
    }
}
