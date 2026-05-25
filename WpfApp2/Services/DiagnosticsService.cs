using Som3a_WPF_UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public interface IDiagnosticsService
    {
        DiagnosticSnapshot CaptureSnapshot();
        event EventHandler<DiagnosticSnapshot> SnapshotUpdated;
    }

    public sealed class DiagnosticsService : IDiagnosticsService
    {
        private readonly RenderModeService _renderModeService;
        private readonly ThemeManager _themeManager;
        private readonly ILoggingService _loggingService;

        public event EventHandler<DiagnosticSnapshot> SnapshotUpdated;

        public DiagnosticsService(ILoggingService loggingService)
        {
            _renderModeService = RenderModeService.Instance;
            _themeManager = ThemeManager.Instance;
            _loggingService = loggingService;
        }

        public DiagnosticSnapshot CaptureSnapshot()
        {
            var process = Process.GetCurrentProcess();
            double? workingSetMB = null;
            double? managedMB = null;

            try
            {
                workingSetMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 1);
            }
            catch
            {
            }

            try
            {
                managedMB = Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 1);
            }
            catch
            {
            }

            var snapshot = new DiagnosticSnapshot
            {
                RenderMode = _renderModeService.GetCurrentMode().ToString(),
                RenderModeSource = DetectRenderModeSource(),
                GpuAvailable = _renderModeService.IsGpuAvailable(),
                GpuName = GetGpuName(),
                ActiveTheme = _themeManager.CurrentTheme,
                AccentColor = _themeManager.CurrentAccentColor,
                IsFallbackMode = _themeManager.IsFallbackActive,
                MemoryWorkingSetMB = workingSetMB,
                MemoryManagedMB = managedMB,
                PopupStatus = GetPopupDiagnosticsSafe(),
                Timestamp = DateTime.UtcNow
            };

            SnapshotUpdated?.Invoke(this, snapshot);
            return snapshot;
        }

        private string DetectRenderModeSource()
        {
            try
            {
                if (!_renderModeService.IsTransparencySupported())
                    return "Detected by: transparency test failed";
                if (!_renderModeService.IsGpuAvailable())
                    return "Detected by: GPU unavailable";
                return "Detected by: host type";
            }
            catch
            {
                return "Detection failed";
            }
        }

        private string GetGpuName()
        {
            try
            {
                if (_renderModeService.IsGpuAvailable())
                    return "WPF GPU Renderer";
                return "N/A (GPU not available)";
            }
            catch
            {
                return "N/A";
            }
        }

        private string[] GetPopupDiagnosticsSafe()
        {
            try
            {
                var popupDiag = _renderModeService.GetPopupDiagnostics();
                return popupDiag?.ToArray() ?? Array.Empty<string>();
            }
            catch
            {
                return new[] { "Popup diagnostics unavailable" };
            }
        }
    }
}
