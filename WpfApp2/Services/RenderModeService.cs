using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Som3a_WPF_UI.Helpers;

namespace Som3a_WPF_UI.Services
{
    public enum RenderMode
    {
        WindowChrome,
        FallbackSafe
    }

    public class RenderModeEventArgs : EventArgs
    {
        public RenderMode Mode { get; }
        public RenderModeEventArgs(RenderMode mode) { Mode = mode; }
    }

    public sealed class RenderModeService
    {
        private static readonly Lazy<RenderModeService> _instance =
            new Lazy<RenderModeService>(() => new RenderModeService());

        public static RenderModeService Instance => _instance.Value;

        private RenderMode _currentMode = RenderMode.FallbackSafe;
        private bool _isInitialized;
        private bool _gpuAvailable;
        private bool _transparencySupported;
        private readonly object _initLock = new object();

        public event EventHandler<RenderModeEventArgs> RenderModeChanged;

        private RenderModeService()
        {
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            lock (_initLock)
            {
                if (_isInitialized) return;

                try
                {
                    var detectedMode = WindowRenderModeDetector.DetectOptimalMode();
                    _currentMode = detectedMode == WindowRenderMode.WindowChrome
                        ? RenderMode.WindowChrome
                        : RenderMode.FallbackSafe;

                    _gpuAvailable = detectedMode == WindowRenderMode.WindowChrome;
                    _transparencySupported = detectedMode == WindowRenderMode.WindowChrome;

                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[RenderModeService] Initialization failed, defaulting to FallbackSafe: {ex.Message}");
                    _currentMode = RenderMode.FallbackSafe;
                    _gpuAvailable = false;
                    _transparencySupported = false;
                    _isInitialized = true;
                    return;
                }

                try
                {
                    RenderModeChanged?.Invoke(this, new RenderModeEventArgs(_currentMode));
                }
                catch (Exception subscriberEx)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[RenderModeService] Subscriber exception in RenderModeChanged: {subscriberEx.Message}");
                }
            }
        }

        public RenderMode GetCurrentMode()
        {
            if (!_isInitialized) Initialize();
            return _currentMode;
        }

        public bool IsSafeModeRequired()
        {
            return GetCurrentMode() == RenderMode.FallbackSafe;
        }

        public bool IsGpuAvailable()
        {
            if (!_isInitialized) Initialize();
            return _gpuAvailable;
        }

        public bool IsTransparencySupported()
        {
            if (!_isInitialized) Initialize();
            return _transparencySupported;
        }

        public IReadOnlyList<string> GetPopupDiagnostics()
        {
            var results = new List<string>();

            try
            {
                if (Application.Current == null)
                {
                    results.Add("ComboBox: Application not available");
                    return results;
                }

                foreach (Window window in Application.Current.Windows)
                {
                    if (window == null) continue;
                    var comboBoxes = FindVisualChildren<ComboBox>(window);
                    foreach (var comboBox in comboBoxes)
                    {
                        var status = InspectComboBox(comboBox);
                        results.Add(status);
                    }
                }

                if (results.Count == 0)
                    results.Add("No ComboBox controls found");
            }
            catch (Exception ex)
            {
                results.Add($"Popup diagnostics error: {ex.Message}");
            }

            return results;
        }

        private static string InspectComboBox(ComboBox comboBox)
        {
            try
            {
                var name = string.IsNullOrEmpty(comboBox.Name) ? "Unnamed" : comboBox.Name;

                if (comboBox.Template == null)
                    return $"{name}: template not applied";

                var popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup;
                if (popup == null)
                    return $"{name}: PART_Popup not found in template";

                var issues = new List<string>();
                if (popup.AllowsTransparency)
                    issues.Add("AllowsTransparency=True (non-compliant)");
                else
                    issues.Add("AllowsTransparency=False");

                if (popup.Child != null)
                {
                    var parentWindow = Window.GetWindow(popup.Child);
                    if (parentWindow != null && parentWindow.AllowsTransparency)
                        issues.Add("parent Window AllowsTransparency=True");
                }

                return $"{name}: {string.Join(", ", issues)}";
            }
            catch (Exception ex)
            {
                return $"ComboBox: inspection failed - {ex.Message}";
            }
        }

        private static List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var children = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    children.Add(typedChild);

                children.AddRange(FindVisualChildren<T>(child));
            }

            return children;
        }
    }
}
