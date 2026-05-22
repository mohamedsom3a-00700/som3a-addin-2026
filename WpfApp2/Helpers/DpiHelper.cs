using System;
using System.Windows;
using System.Windows.Media;

namespace Som3a_WPF_UI.Helpers
{
    public static class DpiHelper
    {
        private static double? _cachedDpiScale;

        public static double PrimaryScreenDpi => 96.0;

        public static double GetCurrentDpiScale()
        {
            if (_cachedDpiScale.HasValue)
                return _cachedDpiScale.Value;

            try
            {
                var scale = GetScalingFactor(Application.Current?.MainWindow);
                _cachedDpiScale = scale;
                return scale;
            }
            catch
            {
                _cachedDpiScale = 1.0;
                return 1.0;
            }
        }

        public static double ScaleValue(double value)
        {
            return value * GetCurrentDpiScale();
        }

        public static bool IsHighDpi()
        {
            return GetCurrentDpiScale() >= 1.5;
        }

        public static double GetSystemDpi()
        {
            using (var source = new System.Windows.Interop.HwndSource(
                new System.Windows.Interop.HwndSourceParameters()))
            {
                return source.CompositionTarget?.TransformToDevice.M11 ?? 96.0;
            }
        }

        public static double GetWindowDpi(Window window)
        {
            if (window == null) return PrimaryScreenDpi;

            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(window);
                var dpiX = 0.0;
                var dpiY = 0.0;

                try
                {
                    var source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
                    if (source?.CompositionTarget != null)
                    {
                        var transform = source.CompositionTarget.TransformToDevice;
                        dpiX = transform.M11 * 96.0;
                        dpiY = transform.M22 * 96.0;
                    }
                }
                catch
                {
                }

                return dpiX > 0 ? dpiX : PrimaryScreenDpi;
            }
            catch
            {
                return PrimaryScreenDpi;
            }
        }

        public static double GetScalingFactor(Window window)
        {
            var dpi = GetWindowDpi(window);
            return dpi / PrimaryScreenDpi;
        }

        public static Thickness GetScaledThickness(Thickness thickness, Window window)
        {
            var scale = GetScalingFactor(window);
            return new Thickness(
                thickness.Left * scale,
                thickness.Top * scale,
                thickness.Right * scale,
                thickness.Bottom * scale);
        }

        public static double GetScaledValue(double value, Window window)
        {
            var scale = GetScalingFactor(window);
            return value * scale;
        }

        public static Size GetScaledSize(Size size, Window window)
        {
            var scale = GetScalingFactor(window);
            return new Size(size.Width * scale, size.Height * scale);
        }

        public static void InvalidateCache()
        {
            _cachedDpiScale = null;
        }

        public static bool IsSystemHighContrast()
        {
            return SystemParameters.HighContrast;
        }
    }
}