using System;
using System.Diagnostics;

namespace Som3a_WPF_UI.Services
{
    public sealed class PerformanceMonitor : IPerformanceMonitor
    {
        private DateTime _appStartTime;
        private readonly Stopwatch _navigationStopwatch = new Stopwatch();
        private string _currentNavigationTarget;

        public double StartupTimeMs { get; private set; }
        public double LastNavigationTimeMs { get; private set; }
        public string LastNavigationTarget { get; private set; }

        public event EventHandler<PerformanceMetricsEventArgs> NavigationCompleted;

        public void RecordAppStart()
        {
            _appStartTime = DateTime.UtcNow;
            StartupTimeMs = 0;
        }

        public void CalculateStartupTime()
        {
            StartupTimeMs = (DateTime.UtcNow - _appStartTime).TotalMilliseconds;
        }

        public void BeginNavigation(string targetKey)
        {
            _currentNavigationTarget = targetKey;
            _navigationStopwatch.Restart();
        }

        public void EndNavigation()
        {
            _navigationStopwatch.Stop();
            LastNavigationTimeMs = _navigationStopwatch.Elapsed.TotalMilliseconds;
            LastNavigationTarget = _currentNavigationTarget;
            NavigationCompleted?.Invoke(this, new PerformanceMetricsEventArgs(LastNavigationTimeMs, LastNavigationTarget));
        }
    }
}
