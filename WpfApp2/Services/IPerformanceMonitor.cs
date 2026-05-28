using System;

namespace Som3a_WPF_UI.Services
{
    public interface IPerformanceMonitor
    {
        double StartupTimeMs { get; }
        double LastNavigationTimeMs { get; }
        string LastNavigationTarget { get; }
        void RecordAppStart();
        void BeginNavigation(string targetKey);
        void EndNavigation();
        event EventHandler<PerformanceMetricsEventArgs> NavigationCompleted;
    }

    public class PerformanceMetricsEventArgs : EventArgs
    {
        public double NavigationTimeMs { get; }
        public string TargetKey { get; }

        public PerformanceMetricsEventArgs(double navigationTimeMs, string targetKey)
        {
            NavigationTimeMs = navigationTimeMs;
            TargetKey = targetKey;
        }
    }
}
