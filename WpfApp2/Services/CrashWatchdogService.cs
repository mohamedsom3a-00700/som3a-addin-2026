using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Som3a.Bridge;
using Timer = System.Timers.Timer;

namespace Som3a_WPF_UI.Services
{
    public class CrashWatchdogService : IDisposable
    {
        private readonly PipeClientService _pipeClient;
        private int _missedHeartbeatCount;
        private int _lastSequence;
        private bool _inGraceWindow;
        private Timer _graceTimer;
        private readonly object _lock = new();
        private bool _disposed;

        public event EventHandler<string> ExcelDisconnected;
        public event EventHandler ShutdownTriggered;

        public CrashWatchdogService(PipeClientService pipeClient)
        {
            _pipeClient = pipeClient;
            _pipeClient.MessageReceived += OnMessageReceived;
            _pipeClient.StateChanged += OnStateChanged;
        }

        public void Start()
        {
            _missedHeartbeatCount = 0;
            _lastSequence = 0;
            _inGraceWindow = false;
        }

        private void OnMessageReceived(object sender, PipeMessage message)
        {
            if (message.Type == MessageType.heartbeat)
            {
                lock (_lock)
                {
                    _missedHeartbeatCount = 0;
                    _inGraceWindow = false;
                    CancelGraceTimer();

                    if (message.Payload is int seq)
                        _lastSequence = seq;
                }

                SendHeartbeatAck(message.CorrelationId);
            }
            else if (message.Type == MessageType.shutdown)
            {
                HandleGracefulShutdown();
            }
        }

        private void OnStateChanged(object sender, PipeConnectionState state)
        {
            if (state == PipeConnectionState.Disconnected && !_inGraceWindow)
            {
                StartMissedHeartbeatDetection();
            }
        }

        public void NotifyMissedHeartbeat()
        {
            lock (_lock)
            {
                _missedHeartbeatCount++;
                System.Diagnostics.Trace.WriteLine($"[CrashWatchdog] Missed heartbeat #{_missedHeartbeatCount}");

                if (_missedHeartbeatCount >= PipeConstants.MaxMissedHeartbeats && !_inGraceWindow)
                {
                    EnterGraceWindow();
                }
            }
        }

        private void StartMissedHeartbeatDetection()
        {
            _ = Task.Run(async () =>
            {
                for (int i = 0; i < PipeConstants.MaxMissedHeartbeats; i++)
                {
                    await Task.Delay(PipeConstants.HeartbeatIntervalMs);
                    if (_pipeClient.State != PipeConnectionState.Disconnected)
                        return;

                    NotifyMissedHeartbeat();
                }
            });
        }

        private void EnterGraceWindow()
        {
            _inGraceWindow = true;
            ExcelDisconnected?.Invoke(this, "Excel connection lost. Waiting for reconnect...");

            _graceTimer = new Timer(PipeConstants.GraceWindowMs);
            _graceTimer.Elapsed += OnGraceTimerElapsed;
            _graceTimer.AutoReset = false;
            _graceTimer.Start();
        }

        private void OnGraceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (!_inGraceWindow) return;

                System.Diagnostics.Trace.WriteLine("[CrashWatchdog] Grace window expired. Initiating shutdown.");
                ShutdownTriggered?.Invoke(this, EventArgs.Empty);
            }
        }

        private void HandleGracefulShutdown()
        {
            System.Diagnostics.Trace.WriteLine("[CrashWatchdog] Received graceful shutdown from Excel.");

            SendShutdownAck();

            EnterGraceWindow();
        }

        private void SendHeartbeatAck(string correlationId)
        {
            var ack = new PipeMessage
            {
                Type = MessageType.heartbeat_ack,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Payload = new { sequence = _lastSequence }
            };

            _ = _pipeClient.SendAsync(ack);
        }

        private void SendShutdownAck()
        {
            var ack = new PipeMessage
            {
                Type = MessageType.shutdown_ack,
                CorrelationId = Guid.NewGuid().ToString("D"),
                Timestamp = DateTime.UtcNow,
                Payload = null
            };

            _ = _pipeClient.SendAsync(ack);
        }

        private void CancelGraceTimer()
        {
            _graceTimer?.Stop();
            _graceTimer?.Dispose();
            _graceTimer = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            CancelGraceTimer();
            _pipeClient.MessageReceived -= OnMessageReceived;
            _pipeClient.StateChanged -= OnStateChanged;
        }
    }
}
