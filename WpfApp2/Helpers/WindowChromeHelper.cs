using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace Som3a_WPF_UI.Helpers
{
    public static class WindowChromeHelper
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        public static bool IsVstoHosted =>
            Application.Current?.MainWindow == null ||
            System.Diagnostics.Process.GetCurrentProcess()
                .ProcessName
                .IndexOf("EXCEL", StringComparison.OrdinalIgnoreCase) >= 0;

        public static void ApplyWindowChrome(Window window, bool useSafeMode = false)
        {
            if (window == null) return;

            if (useSafeMode || IsVstoHosted)
            {
                ApplySafeWindowChrome(window);
                return;
            }

            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = false;

            var chrome = new System.Windows.Shell.WindowChrome
            {
                ResizeBorderThickness = new Thickness(6),
                CaptionHeight = 40,
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = System.Windows.Shell.NonClientFrameEdges.None,
                CornerRadius = new CornerRadius(8)
            };

            window.SetValue(System.Windows.Shell.WindowChrome.WindowChromeProperty, chrome);
        }

        private static void ApplySafeWindowChrome(Window window)
        {
            window.WindowStyle = WindowStyle.None;
            window.AllowsTransparency = false;

            var chrome = new System.Windows.Shell.WindowChrome
            {
                ResizeBorderThickness = new Thickness(4),
                CaptionHeight = 36,
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = System.Windows.Shell.NonClientFrameEdges.None,
                CornerRadius = new CornerRadius(0)
            };

            window.SetValue(System.Windows.Shell.WindowChrome.WindowChromeProperty, chrome);
        }

        public static void HandleDpiChanged(Window window)
        {
            if (window == null) return;

            window.InvalidateMeasure();
            window.InvalidateArrange();
        }

        public static Rect GetMonitorBounds(Window window)
        {
            var helper = new WindowInteropHelper(window);
            var monitor = NativeMethods.MonitorFromWindow(helper.Handle, 2);
            if (monitor == IntPtr.Zero)
                return new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (NativeMethods.GetMonitorInfo(monitor, ref info))
            {
                return new Rect(
                    info.rcMonitor.Left,
                    info.rcMonitor.Top,
                    info.rcMonitor.Right - info.rcMonitor.Left,
                    info.rcMonitor.Bottom - info.rcMonitor.Top);
            }

            return new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }

        public static void SnapToMonitor(Window window)
        {
            var bounds = GetMonitorBounds(window);
            var rect = new Rect(window.Left, window.Top, window.Width, window.Height);

            if (rect.Left < bounds.Left) window.Left = bounds.Left;
            if (rect.Top < bounds.Top) window.Top = bounds.Top;
            if (rect.Right > bounds.Right) window.Left = bounds.Right - window.Width;
            if (rect.Bottom > bounds.Bottom) window.Top = bounds.Bottom - window.Height;
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}