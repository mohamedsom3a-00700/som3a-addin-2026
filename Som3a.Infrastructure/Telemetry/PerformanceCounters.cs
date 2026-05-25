using System.Diagnostics;

namespace Som3a.Infrastructure.Telemetry
{
    public class PerformanceCounters
    {
        private readonly Stopwatch _uptimeStopwatch;
        private long _operationCount;
        private long _totalOperationTicks;

        public TimeSpan Uptime => _uptimeStopwatch.Elapsed;
        public long OperationCount => _operationCount;
        public double AverageOperationMs => _operationCount > 0
            ? (_totalOperationTicks / (double)Stopwatch.Frequency * 1000) / _operationCount
            : 0;
        public double OperationsPerSecond => Uptime.TotalSeconds > 0
            ? _operationCount / Uptime.TotalSeconds
            : 0;

        public PerformanceCounters()
        {
            _uptimeStopwatch = Stopwatch.StartNew();
        }

        public void RecordOperation(TimeSpan duration)
        {
            Interlocked.Increment(ref _operationCount);
            Interlocked.Add(ref _totalOperationTicks, duration.Ticks);
        }

        public IDisposable MeasureOperation()
        {
            return new OperationScope(this);
        }

        public PerformanceSnapshot GetSnapshot()
        {
            var proc = Process.GetCurrentProcess();
            return new PerformanceSnapshot
            {
                Uptime = Uptime,
                OperationCount = _operationCount,
                AverageOperationMs = AverageOperationMs,
                OperationsPerSecond = OperationsPerSecond,
                WorkingSetMB = proc.WorkingSet64 / (1024.0 * 1024.0),
                ManagedMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0),
                ThreadCount = proc.Threads.Count
            };
        }

        private class OperationScope : IDisposable
        {
            private readonly PerformanceCounters _counters;
            private readonly Stopwatch _sw;

            public OperationScope(PerformanceCounters counters)
            {
                _counters = counters;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                _counters.RecordOperation(_sw.Elapsed);
            }
        }
    }

    public class PerformanceSnapshot
    {
        public TimeSpan Uptime { get; set; }
        public long OperationCount { get; set; }
        public double AverageOperationMs { get; set; }
        public double OperationsPerSecond { get; set; }
        public double WorkingSetMB { get; set; }
        public double ManagedMemoryMB { get; set; }
        public int ThreadCount { get; set; }
    }
}
