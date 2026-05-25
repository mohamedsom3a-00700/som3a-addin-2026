# DPI & Multi-Monitor Audit Report

**Project**: Som3a Add-in 2026
**Phase**: 10 (Enterprise Polish) — User Story 3
**Date**: 2026-05-25
**Status**: Methodology defined, fixes pending

## Baseline (T003)

Each of the 14 windows must be tested at these DPI levels:

| DPI Level | Single Monitor | Multi-Monitor Same DPI | Multi-Monitor Mixed DPI |
|-----------|---------------|----------------------|------------------------|
| 100% | ⏳ | ⏳ | ⏳ |
| 125% | ⏳ | ⏳ | ⏳ |
| 150% | ⏳ | ⏳ | ⏳ |
| 200% | ⏳ | ⏳ | ⏳ |

## Issues to Check

| Issue Type | Description |
|------------|-------------|
| Clipping | Elements cut off or overflow containers |
| Misalignment | Controls not properly positioned |
| Text Rendering | Blurry or incorrectly sized text |
| Popup Positioning | ComboBox/ContextMenu at wrong location |
| Font Scaling | Text truncated or overlapping |

## Optimizations Applied

### Per-Monitor DPI Infrastructure
- `ModernWindow` includes `DpiScaleProperty` (DependencyProperty)
- `DpiHelper.GetCurrentDpiScale()` provides current DPI scale factor
- WindowChrome already handles per-monitor DPI in WPF 4.8

### ComboBox Popup (already configured)
- `AllowsTransparency="False"` — required for Excel VSTO
- `Placement="Bottom"` with `PlacementTarget` binding
- Popup position recalculated on DPI change

## Pending Fixes

### DPI Clipping/Overflow (T031)
- Adjust ControlTemplates in `Theme/Controls/*.xaml` for scaling
- Check popup position logic in `ComboBoxStyles.xaml`
- Verify window size constraints in all 14 windows

### Per-Monitor DPI Popup (T032)
- Handle `DpiChanged` event in `ModernWindow` for popup repositioning
- Ensure ComboBox popups render at correct size on target monitor

### Font Scaling (T033)
- Verify no text truncation or overlap at any DPI level
- Test all 14 windows at each DPI level

### Multi-Monitor Mixed DPI (T034)
- Move windows between 100% and 150% monitors
- Verify no visual artifacts or misalignment

## DPI Regression Checklist (T036)

When making future changes, re-test:
1. Set display to 100%, open all 14 windows
2. Set display to 125%, open all 14 windows
3. Set display to 150%, open all 14 windows
4. Set display to 200%, open all 14 windows
5. Connect second monitor at different DPI, move windows between monitors
6. Check ComboBox popup positioning at each level
7. Verify no text clipping at any level
