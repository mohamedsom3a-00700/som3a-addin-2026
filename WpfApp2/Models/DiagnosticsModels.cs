using System;

namespace Som3a_WPF_UI.Models
{
    public class DiagnosticSnapshot
    {
        public string RenderMode { get; set; } = "Unknown";
        public string RenderModeSource { get; set; } = "N/A";
        public bool GpuAvailable { get; set; }
        public string GpuName { get; set; } = "N/A";
        public string ActiveTheme { get; set; } = "Dark";
        public string AccentColor { get; set; } = "#3A86FF";
        public bool IsFallbackMode { get; set; }
        public double? MemoryWorkingSetMB { get; set; }
        public double? MemoryManagedMB { get; set; }
        public string[] PopupStatus { get; set; } = Array.Empty<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ValidationResult
    {
        public string Id { get; set; } = string.Empty;
        public string Severity { get; set; } = "warning";
        public string Category { get; set; } = string.Empty;
        public string DictionaryName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SuggestedFix { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Severity { get; set; } = "INFO";
        public string Category { get; set; } = "System";
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Exception { get; set; } = string.Empty;
    }

    public class FallbackManifest
    {
        public bool IsActive { get; set; }
        public string[] FailedDictionaries { get; set; } = Array.Empty<string>();
        public string[] FailureReasons { get; set; } = Array.Empty<string>();
        public DateTime ActivatedAt { get; set; }
        public int HardcodedResourcesCount { get; set; }
    }
}
