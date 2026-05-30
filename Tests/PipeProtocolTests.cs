using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Som3a.Bridge;

namespace Som3a_WPF_UI.Tests
{
    [TestClass]
    public class PipeProtocolTests
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        [TestMethod]
        public void PipeMessageSchema_HasAllRequiredMessageTypes()
        {
            Assert.AreEqual(10, Enum.GetValues(typeof(MessageType)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "handshake"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "heartbeat"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "heartbeat_ack"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "excel_command"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "excel_response"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "shutdown"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "shutdown_ack"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "performance_test"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "performance_result"));
            Assert.IsTrue(Enum.IsDefined(typeof(MessageType), "error"));
        }

        [TestMethod]
        public void PipeMessage_SerializationRoundtrip()
        {
            var original = new PipeMessage
            {
                Type = MessageType.handshake,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = new HandshakePayload { ExcelHwnd = 123456, ProcessId = 7890, Version = "1.0.0" }
            };

            var json = JsonSerializer.Serialize(original, JsonOptions);
            var deserialized = JsonSerializer.Deserialize<PipeMessage>(json, JsonOptions);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(original.Type, deserialized.Type);
            Assert.AreEqual(original.CorrelationId, deserialized.CorrelationId);
        }

        [TestMethod]
        public void ExcelCommandPayload_SerializationRoundtrip()
        {
            var cmd = ExcelCommandProtocol.CreateCommand(new ExcelCommandPayload
            {
                Cmd = ExcelCommandType.WriteCell,
                Sheet = "Sheet1",
                Row = 1,
                Col = 1,
                Value = "TestValue"
            });

            var json = JsonSerializer.Serialize(cmd, JsonOptions);
            var deserialized = JsonSerializer.Deserialize<PipeMessage>(json, JsonOptions);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(MessageType.excel_command, deserialized.Type);
        }

        [TestMethod]
        public void ExcelCommandType_AllValuesDefined()
        {
            Assert.AreEqual(7, Enum.GetValues(typeof(ExcelCommandType)).Length);
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "WriteCell"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "ReadCell"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "WriteRange"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "ReadRange"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "GetSheetNames"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "ActivateSheet"));
            Assert.IsTrue(Enum.IsDefined(typeof(ExcelCommandType), "RunFormula"));
        }

        [TestMethod]
        public void PipeConstants_AllValuesCorrect()
        {
            Assert.AreEqual("Som3aBridge", PipeConstants.PipeName);
            Assert.AreEqual(@"\\.\pipe\Som3aBridge", PipeConstants.PipeFullPath);
            Assert.AreEqual("1.0.0", PipeConstants.ProtocolVersion);
            Assert.AreEqual(5000, PipeConstants.HandshakeTimeoutMs);
            Assert.AreEqual(5000, PipeConstants.HandshakeRetryMs);
            Assert.AreEqual(12, PipeConstants.HandshakeMaxRetries);
            Assert.AreEqual(10000, PipeConstants.HeartbeatIntervalMs);
            Assert.AreEqual(3, PipeConstants.MaxMissedHeartbeats);
            Assert.AreEqual(10000, PipeConstants.GraceWindowMs);
            Assert.AreEqual(10000, PipeConstants.CommandTimeoutMs);
            Assert.AreEqual(1048576, PipeConstants.MaxMessageSize);
            Assert.AreEqual(100, PipeConstants.PerformanceTestMessageCount);
            Assert.AreEqual(500, PipeConstants.PerformanceTestMaxMs);
        }

        [TestMethod]
        public void ErrorPayload_SerializationRoundtrip()
        {
            var error = new PipeMessage
            {
                Type = MessageType.error,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = new ErrorPayload
                {
                    Code = "UNKNOWN_COMMAND",
                    Message = "Received unknown command type",
                    CorrelationId = Guid.NewGuid().ToString("D")
                }
            };

            var json = JsonSerializer.Serialize(error, JsonOptions);
            var deserialized = JsonSerializer.Deserialize<PipeMessage>(json, JsonOptions);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(MessageType.error, deserialized.Type);
        }
    }
}
