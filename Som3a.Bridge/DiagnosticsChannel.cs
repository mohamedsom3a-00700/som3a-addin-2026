using System;
using System.Collections.Generic;

namespace Som3a.Bridge
{
    public interface IDiagnosticsChannel
    {
        void SendDiagnostic(DiagnosticEvent evt);
        event EventHandler<DiagnosticEvent>? DiagnosticReceived;
    }

    public class DiagnosticEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public int Level { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string>? Properties { get; set; }
        public string? ExceptionDetail { get; set; }
    }
}
