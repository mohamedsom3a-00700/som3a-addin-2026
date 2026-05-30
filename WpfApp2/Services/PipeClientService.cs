using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Som3a.Bridge;

namespace Som3a_WPF_UI.Services
{
    public enum PipeConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Active
    }

    public class PipeClientService : IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private NamedPipeClientStream _pipeStream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly CancellationTokenSource _cts = new();
        private PipeConnectionState _state = PipeConnectionState.Disconnected;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly ConcurrentDictionary<string, TaskCompletionSource<PipeMessage>> _pendingRequests = new();

        public event EventHandler<PipeConnectionState> StateChanged;
        public event EventHandler<PipeMessage> MessageReceived;
        public event EventHandler<Exception> ErrorOccurred;

        public PipeConnectionState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    StateChanged?.Invoke(this, value);
                }
            }
        }

        public int ExcelHwnd { get; private set; }
        public int VstoProcessId { get; private set; }

        public async Task<bool> ConnectAsync(int timeoutMs = PipeConstants.HandshakeTimeoutMs, CancellationToken cancellationToken = default)
        {
            State = PipeConnectionState.Connecting;

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
                linkedCts.CancelAfter(timeoutMs);

                _pipeStream = new NamedPipeClientStream(".", PipeConstants.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipeStream.ConnectAsync(timeoutMs, linkedCts.Token);

                _reader = new StreamReader(_pipeStream, Encoding.UTF8);
                _writer = new StreamWriter(_pipeStream, Encoding.UTF8) { AutoFlush = true };

                State = PipeConnectionState.Connected;
                _ = Task.Run(() => ReadLoopAsync(_cts.Token));

                return true;
            }
            catch (Exception ex)
            {
                State = PipeConnectionState.Disconnected;
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        public async Task<bool> PerformHandshakeAsync()
        {
            try
            {
                var handshake = new PipeMessage
                {
                    Type = MessageType.handshake,
                    CorrelationId = Guid.NewGuid().ToString("D"),
                    Timestamp = DateTime.UtcNow,
                    Payload = new { clientVersion = PipeConstants.ProtocolVersion }
                };

                var response = await SendAndWaitAsync(handshake, PipeConstants.HandshakeTimeoutMs);
                if (response?.Type == MessageType.handshake && response.Payload is JsonElement json)
                {
                    if (json.TryGetProperty("excelHwnd", out var hwnd))
                        ExcelHwnd = hwnd.GetInt32();
                    if (json.TryGetProperty("processId", out var pid))
                        VstoProcessId = pid.GetInt32();

                    State = PipeConnectionState.Active;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendAsync(PipeMessage message)
        {
            if (_writer == null) throw new InvalidOperationException("Not connected");

            await _writeLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(message, JsonOptions);
                await _writer.WriteLineAsync(json);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<PipeMessage> SendAndWaitAsync(PipeMessage message, int timeoutMs = PipeConstants.CommandTimeoutMs)
        {
            var tcs = new TaskCompletionSource<PipeMessage>();
            _pendingRequests[message.CorrelationId] = tcs;

            try
            {
                await SendAsync(message);
                using var cts = new CancellationTokenSource(timeoutMs);
                using var _ = cts.Token.Register(() => tcs.TrySetCanceled(), false);

                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            finally
            {
                _pendingRequests.TryRemove(message.CorrelationId, out _);
            }
        }

        private async Task ReadLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await _reader.ReadLineAsync().WaitAsync(cancellationToken);
                    if (line == null) break;

                    var message = JsonSerializer.Deserialize<PipeMessage>(line, JsonOptions);
                    if (message == null) continue;

                    if (_pendingRequests.TryRemove(message.CorrelationId, out var tcs))
                    {
                        tcs.TrySetResult(message);
                    }
                    else
                    {
                        MessageReceived?.Invoke(this, message);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                State = PipeConnectionState.Disconnected;
            }
        }

        public void Disconnect()
        {
            _cts.Cancel();
            _pipeStream?.Dispose();
            _pipeStream = null;
            State = PipeConnectionState.Disconnected;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _pipeStream?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
            _writeLock.Dispose();
        }
    }

}
