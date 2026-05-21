# Excel Rendering Rules

**Project**: Som3a Add-in 2026  
**Version**: 1.0.0  
**Date**: 2026-05-21  
**Target**: Excel VSTO Add-in hosting environment

---

## 1. The Excel Rendering Problem

Excel's rendering pipeline differs significantly from standalone WPF:

- Different GPU acceleration paths
- HWND parenting constraints
- Transparency compositing limitations
- DPI handling quirks
- Focus management conflicts

The UI system MUST remain stable inside this host.

---

## 2. Safe Mode Detection

### WindowRenderModeDetector

Already implemented in `Services/WindowRenderModeDetector.cs`.

**Detection criteria:**

| Check | Trigger for FallbackSafe |
|-------|-------------------------|
| `IsVstoHosted` | Always FallbackSafe when hosted in Excel |
| `IsExtremeDpi()` | DPI scale ≥ 3.0 |
| `IsRenderTestFailing()` | Cannot create test window or get composition target |

### Cached Result

Detection result is cached per session. First window triggers detection; subsequent windows reuse cached mode.

---

## 3. FallbackSafe Mode Behavior

When `WindowRenderMode.FallbackSafe` is active:

### Window Chrome

- Use standard window chrome instead of custom WindowChrome
- `WindowStyle="SingleBorderWindow"` instead of `None`
- No custom title bar (use system title bar)

### Transparency

- `AllowsTransparency="False"` enforced
- No semi-transparent backgrounds
- No glass/mica effects

### Animations

- Fade-in/out disabled
- Hover scale animations disabled
- Instant state changes only

### Shadows

- Use `Shadow.*.Safe` variants (reduced blur/opacity)
- No glow effects on non-essential elements

---

## 4. ModernWindow Integration

```csharp
var useSafeMode = WindowRenderModeDetector.DetectOptimalMode() == WindowRenderMode.FallbackSafe;
if (useSafeMode)
{
    WindowChromeHelper.ApplyWindowChrome(this, true); // safe mode
}
else
{
    WindowChromeHelper.ApplyWindowChrome(this, false); // full chrome
}
```

### ModernWindow v2 Extensions

New DPs for Phase 2:

| DP | Type | Purpose |
|----|------|---------|
| `RenderMode` | enum | Exposes current mode for diagnostics |
| `IsSafeMode` | bool (read-only) | Bound to detector result |
| `WindowBackdrop` | enum | solid/gradient/image (ignored in safe mode) |
| `DpiScale` | double | Current DPI scale for layout |

---

## 5. Focus Management

### Window Open

```csharp
// Ensure WPF window gets focus, not Excel
this.Activate();
Keyboard.Focus(firstFocusableElement);
```

### Window Close

```csharp
// Return interactivity to Excel
try { _xlApp.Interactive = true; } catch { }
```

### Escape-to-Close

Already implemented in `ModernWindow` via `CloseOnEscape` DP.

---

## 6. Black Window Prevention

Black window rendering indicates:

- `AllowsTransparency="True"` with incompatible GPU driver
- Missing or corrupted WindowChrome settings
- DWM compositor disabled

**Prevention:**

1. Always default to `AllowsTransparency="False"`
2. Only enable transparency after `WindowRenderModeDetector` confirms WindowChrome mode
3. Provide instant fallback if black window detected at runtime

---

## 7. DPI Handling

### DpiHelper Requirements

- `GetCurrentDpiScale()` — returns scale for current window
- `ScaleValue(double)` — scales value by DPI
- `IsHighDpi()` — true if scale ≥ 1.5

### Testing Required

- 100% (96 DPI)
- 125% (120 DPI)
- 150% (144 DPI)
- 200% (192 DPI) if available

---

## 8. Manual Test Checklist

Before each phase merge, test in Excel:

- [ ] Window opens without black rendering
- [ ] Window can be moved, resized, minimized, closed
- [ ] ComboBox popup opens and renders correctly
- [ ] No transparency artifacts
- [ ] Focus returns to Excel after close
- [ ] Excel remains interactive while window is open
- [ ] No crash on rapid open/close
- [ ] Correct appearance at 100%, 125%, 150% DPI

---

## Related Governance Documents

| Document | Description |
|----------|-------------|
| [AGENT_RULES.md](AGENT_RULES.md) | AI execution standards |
| [REVIEW_CHECKLIST.md](REVIEW_CHECKLIST.md) | Architecture review checklist |
| [POPUP_ARCHITECTURE.md](POPUP_ARCHITECTURE.md) | Popup rendering rules |
| [PERFORMANCE_RULES.md](PERFORMANCE_RULES.md) | Performance and rendering budget |
| [SHADOW_SYSTEM.md](SHADOW_SYSTEM.md) | Centralized effects architecture |

---

**Version**: 1.0.0 | **Last Updated**: 2026-05-21 | **Ratified by**: Architecture Review Team
