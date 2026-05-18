# Modern Window System - Analyzer & Validation

## Analyzer (SWA001, SWA002)

The `ModernWindowAnalyzer.cs` provides build-time warnings when:
- A class inherits from `Window` instead of `ModernWindow`
- A window uses `AllowsTransparency="True"`

### Usage
The analyzer runs automatically during build in Visual Studio. To enable:

1. Build the solution in Visual Studio
2. Warnings will appear in the Error List

```
SWA001: Class 'MyWindow' inherits from Window. Use ModernWindow instead.
SWA002: Window 'MyWindow' uses AllowsTransparency='True' which causes GPU/rendering issues.
```

## Runtime Validation

Use `WindowValidationHelper` at application startup:

```csharp
// In your VSTO add-in startup
using Som3a_WPF_UI.Helpers;

// Validate all windows
WindowValidationHelper.ValidateAllWindows();

// Check VSTO mode
if (WindowValidationHelper.IsVstoMode())
{
    // Apply VSTO-safe settings
}

// Log DPI info for debugging
WindowValidationHelper.LogDpiInfo();
```

## Manual Validation Checklist

Before release, verify:

- [ ] All windows inherit from `ModernWindow`
- [ ] No windows use `AllowsTransparency="True"`
- [ ] No windows use `WindowStyle="None"` without ModernWindow
- [ ] Radius is 18px (`WindowRadius` resource)
- [ ] All windows use theme resources (`BackgroundBrush`, etc.)
- [ ] DPI scaling works on 4K monitors
- [ ] WindowChrome works in Excel hosting