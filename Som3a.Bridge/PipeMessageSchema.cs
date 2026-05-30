using System;

namespace Som3a.Bridge
{
    public enum MessageType
    {
        handshake,
        heartbeat,
        heartbeat_ack,
        excel_command,
        excel_response,
        shutdown,
        shutdown_ack,
        performance_test,
        performance_result,
        error
    }

    public class PipeMessage
    {
        public MessageType Type { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("D");
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object Payload { get; set; }
    }

    public class HandshakePayload
    {
        public int ExcelHwnd { get; set; }
        public int ProcessId { get; set; }
        public string Version { get; set; } = "1.0.0";
    }

    public class HeartbeatPayload
    {
        public int Sequence { get; set; }
        public string Status { get; set; }
    }

    public enum ExcelCommandType
    {
        WriteCell,
        ReadCell,
        WriteRange,
        ReadRange,
        GetSheetNames,
        ActivateSheet,
        RunFormula
    }

    public class ExcelCommandPayload
    {
        public ExcelCommandType Cmd { get; set; }
        public string Sheet { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string Value { get; set; }
        public string Range { get; set; }
    }

    public class ExcelResponsePayload
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }

    public class ErrorPayload
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
    }

    public static class PipeConstants
    {
        public const string PipeName = "Som3aBridge";
        public const string PipeFullPath = @"\\.\pipe\Som3aBridge";
        public const string ProtocolVersion = "1.0.0";
        public const int HandshakeTimeoutMs = 5000;
        public const int HandshakeRetryMs = 5000;
        public const int HandshakeMaxRetries = 12;
        public const int HeartbeatIntervalMs = 10000;
        public const int MaxMissedHeartbeats = 3;
        public const int GraceWindowMs = 10000;
        public const int CommandTimeoutMs = 10000;
        public const int MaxMessageSize = 1048576;
        public const int PerformanceTestMessageCount = 100;
        public const int PerformanceTestMaxMs = 500;
    }
}
