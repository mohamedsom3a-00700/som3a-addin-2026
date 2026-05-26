using System;
using System.Runtime.InteropServices;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Services
{
    public static class DwmBlurService
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public uint AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
            ACCENT_INVALID_STATE = 6
        }

        private enum WindowCompositionAttribute
        {
            WCA_UNDEFINED = 0,
            WCA_NCRENDERING_ENABLED = 1,
            WCA_NCRENDERING_POLICY = 2,
            WCA_TRANSITIONS_FORCEDISABLED = 3,
            WCA_NONCLIENT_RTL_LAYOUT = 4,
            WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 5,
            WCA_EXTENDED_FRAME_BOUNDS = 6,
            WCA_HAS_ICONIC_BITMAP = 7,
            WCA_THEME_ATTRIBUTES = 8,
            WCA_NCRENDERING_EXILED = 9,
            WCA_NCADORNMENTINFO = 10,
            WCA_EXCLUDED_FROM_PEP = 11,
            WCA_ACTIVATE = 12,
            WCA_DRAWING_ATTRIBUTES = 13,
            WCA_CLOAK = 14,
            WCA_CLOAKED = 15,
            WCA_ACCENT_POLICY = 19
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private static bool? _isBlurSupported;

        public static bool IsBlurSupported
        {
            get
            {
                if (_isBlurSupported.HasValue)
                    return _isBlurSupported.Value;

                try
                {
                    var osVer = Environment.OSVersion.Version;
                    bool isWin10 = osVer.Major >= 10 && osVer.Build >= 16299;

                    if (!isWin10)
                    {
                        _isBlurSupported = false;
                        return false;
                    }

                    _isBlurSupported = true;
                    return true;
                }
                catch
                {
                    _isBlurSupported = false;
                    return false;
                }
            }
        }

        public static void EnableBlur(IntPtr hwnd, double intensity)
        {
            if (hwnd == IntPtr.Zero || !IsBlurSupported)
                return;

            intensity = Math.Max(0.0, Math.Min(1.0, intensity));

            try
            {
                var accent = new AccentPolicy();
                var accentStructSize = Marshal.SizeOf(accent);

                if (intensity > 0.0)
                {
                    accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

                    byte alpha = (byte)(intensity * 255);
                    byte r = 0, g = 0, b = 0;
                    accent.GradientColor = (uint)((alpha << 24) | (r << 16) | (g << 8) | b);
                }
                else
                {
                    accent.AccentState = AccentState.ACCENT_DISABLED;
                }

                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                try
                {
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentStructSize,
                        Data = accentPtr
                    };

                    if (!SetWindowCompositionAttribute(hwnd, ref data))
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        System.Diagnostics.Debug.WriteLine($"[DwmBlurService] SetWindowCompositionAttribute failed (hwnd={hwnd}, intensity={intensity:F2}, error={lastError})");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(accentPtr);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DwmBlurService] Failed to set blur: {ex.Message}");
            }
        }

        public static void DisableBlur(IntPtr hwnd)
        {
            EnableBlur(hwnd, 0.0);
        }
    }
}
