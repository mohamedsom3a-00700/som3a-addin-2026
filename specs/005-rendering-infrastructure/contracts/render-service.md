# RenderModeService Contract

**Date**: 2026-05-22

## Interface

```csharp
public enum RenderMode
{
    WindowChrome,   // Full effects, animations, shadows
    FallbackSafe    // Reduced effects, no animations, safe shadows
}

public class RenderModeService
{
    // --- Initialization ---
    // Called once at app startup. Determines and caches render mode.
    // On failure, defaults to FallbackSafe.
    void Initialize();

    // --- Query ---
    RenderMode GetCurrentMode();
    bool IsSafeModeRequired();          // Convenience: GetCurrentMode() == FallbackSafe
    bool IsGpuAvailable();              // GPU acceleration detected
    bool IsTransparencySupported();     // Transparency rendering supported

    // --- Events ---
    event EventHandler<RenderMode> RenderModeChanged;  // Diagnostics only

    // --- Lifecycle ---
    // Mode is determined once at startup and fixed for the session.
    // RenderModeChanged is reserved for future diagnostics use.
}
```

## Dependencies

- `WindowRenderModeDetector` (existing) — provides GPU detection, transparency detection, VSTO detection

## Usage in ModernWindow

```csharp
// In ModernWindow.InitializeWindow():
var renderService = RenderModeService.Instance; // singleton
if (renderService.IsSafeModeRequired())
{
    // Disable fade animations
    // Apply Shadow.*.Safe variants
    // Disable backdrop effects
    // Force AllowsTransparency="False"
}
else
{
    // Use full effects, animations, WindowChrome
}
```
