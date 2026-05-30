using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Som3a.Bridge
{
    public static class ExcelCommandProtocol
    {
        public static PipeMessage CreateCommand(ExcelCommandPayload payload)
        {
            return new PipeMessage
            {
                Type = MessageType.excel_command,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = payload
            };
        }

        public static PipeMessage CreateResponse(string correlationId, object data)
        {
            return new PipeMessage
            {
                Type = MessageType.excel_response,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Payload = new ExcelResponsePayload
                {
                    Success = true,
                    Data = data
                }
            };
        }

        public static PipeMessage CreateErrorResponse(string correlationId, string code, string message)
        {
            return new PipeMessage
            {
                Type = MessageType.error,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Payload = new ErrorPayload
                {
                    Code = code,
                    Message = message,
                    CorrelationId = correlationId
                }
            };
        }

        public static PipeMessage CreatePerformanceTest(int sequenceNumber)
        {
            return new PipeMessage
            {
                Type = MessageType.performance_test,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = new { sequence = sequenceNumber }
            };
        }

        public static PipeMessage CreatePerformanceResult(Dictionary<string, object> metrics)
        {
            return new PipeMessage
            {
                Type = MessageType.performance_result,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = metrics
            };
        }
    }
}
