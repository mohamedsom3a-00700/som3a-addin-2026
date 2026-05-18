using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Som3a_WPF_UI.Services
{
    public enum WindowRenderMode
    {
        WindowChrome,
        FallbackSafe
    }

    public static class WindowRenderModeDetector
    {
        private static WindowRenderMode? _cachedMode;

        public static WindowRenderMode DetectOptimalMode()
        {
            if (_cachedMode.HasValue)
                return _cachedMode.Value;

            var mode = DetectOptimalModeInternal();
            _cachedMode = mode;
            return mode;
        }

        private static WindowRenderMode DetectOptimalModeInternal()
        {
            if (Helpers.WindowChromeHelper.IsVstoHosted)
                return WindowRenderMode.FallbackSafe;

            if (IsExtremeDpi())
                return WindowRenderMode.FallbackSafe;

            if (IsRenderTestFailing())
                return WindowRenderMode.FallbackSafe;

            return WindowRenderMode.WindowChrome;
        }

        private static bool IsExtremeDpi()
        {
            try
            {
                var source = PresentationSource.FromVisual(Application.Current?.MainWindow);
                if (source?.CompositionTarget == null)
                    return false;

                var matrix = source.CompositionTarget.TransformToDevice;
                var dpiScale = Math.Max(matrix.M11, matrix.M22);
                return dpiScale >= 3.0;
            }
            catch { }

            return false;
        }

        private static bool IsRenderTestFailing()
        {
            try
            {
                var testWindow = new Window
                {
                    Width = 100,
                    Height = 100,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = false,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Left = -10000,
                    Top = -10000
                };

                testWindow.Show();
                testWindow.Activate();

                var helper = new WindowInteropHelper(testWindow);
                var source = HwndSource.FromHwnd(helper.Handle);
                if (source?.CompositionTarget == null)
                {
                    testWindow.Close();
                    return true;
                }

                var matrix = source.CompositionTarget.TransformToDevice;
                testWindow.Close();
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}