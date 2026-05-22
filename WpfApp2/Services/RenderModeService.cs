using System;
using Som3a_WPF_UI.Helpers;

namespace Som3a_WPF_UI.Services
{
    public enum RenderMode
    {
        WindowChrome,
        FallbackSafe
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

        public event EventHandler<RenderMode> RenderModeChanged;

        private RenderModeService()
        {
        }

        public void Initialize()
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

                RenderModeChanged?.Invoke(this, _currentMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RenderModeService] Initialization failed, defaulting to FallbackSafe: {ex.Message}");
                _currentMode = RenderMode.FallbackSafe;
                _gpuAvailable = false;
                _transparencySupported = false;
                _isInitialized = true;
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
    }
}