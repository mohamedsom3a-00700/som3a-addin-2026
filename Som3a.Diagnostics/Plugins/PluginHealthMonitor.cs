using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Som3a.Contracts;
using Som3a.Plugin.SDK.Discovery;

namespace Som3a.Diagnostics.Plugins
{
    public class PluginHealthMonitor : IDiagnosticsProvider
    {
        private readonly List<PluginDescriptor> _plugins;
        private readonly List<DiagnosticEvent> _events = new();

        public string ProviderId => "plugin-health-monitor";
        public string ProviderName => "Plugin Health Monitor";

        public event EventHandler<DiagnosticEvent>? DiagnosticLogged;

        public PluginHealthMonitor(IEnumerable<PluginDescriptor> plugins)
        {
            _plugins = plugins.ToList();
        }

        public Task<DiagnosticsSnapshot> CollectSnapshotAsync(CancellationToken ct = default)
        {
            var loaded = _plugins.Count(p => p.Status == PluginStatus.Loaded);
            var errors = _plugins.Count(p => p.Status == PluginStatus.Error);

            return Task.FromResult(new DiagnosticsSnapshot
            {
                ProviderId = ProviderId,
                Version = "1.0.0",
                MemoryUsageBytes = GC.GetTotalMemory(false),
                ActiveOperationCount = loaded,
                Metrics = new Dictionary<string, object>
                {
                    ["plugins.total"] = _plugins.Count,
                    ["plugins.loaded"] = loaded,
                    ["plugins.errors"] = errors
                }
            });
        }

        public Task<HealthReport> ReportHealthAsync(CancellationToken ct = default)
        {
            var errorPlugins = _plugins.Where(p => p.Status == PluginStatus.Error).ToList();
            var status = errorPlugins.Count == 0 ? HealthStatus.Healthy
                : errorPlugins.Count < _plugins.Count / 2 ? HealthStatus.Degraded
                : HealthStatus.Unhealthy;

            return Task.FromResult(new HealthReport
            {
                Status = status,
                StatusMessage = status == HealthStatus.Healthy ? "All plugins healthy" : $"{errorPlugins.Count} plugin(s) in error state",
                Checks = errorPlugins.Select(p => new HealthCheck
                {
                    Name = p.Name,
                    Status = HealthStatus.Unhealthy,
                    Detail = $"Plugin {p.Id} is in {p.Status} state"
                }).ToList()
            });
        }

        public void LogDiagnostic(DiagnosticEvent evt)
        {
            _events.Add(evt);
            DiagnosticLogged?.Invoke(this, evt);
        }
    }
}
