# Quickstart: Theme Engine 2.0

**Phase**: 1 — Design & Contracts

## Overview

The Theme Engine 2.0 enables runtime switching between Dark, Light, and Custom themes with accent color personalization across all 14 application windows.

## Build & Run

```powershell
# Build the solution
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Build should succeed with zero XAML parse errors
```

## Manual Test Sequence

### 1. Theme Switching (MVP)

1. Launch the add-in inside Excel
2. Open Settings window (ribbon → Settings)
3. Click each theme card: Dark → Light → Custom
4. Verify ALL open windows update instantly
5. Close and reopen Excel — verify theme persists

### 2. Accent Color

1. Select Custom theme
2. Click each of the 8 accent swatches
3. Verify accent changes in: buttons (hover glow), progress bars (fill), selection highlights, glow effects
4. Switch to Dark → verify accent color is remembered when switching back to Custom

### 3. Control States

1. Open any window with interactive controls
2. Hover over buttons, text boxes, checkboxes, radio buttons, toggles, dropdowns
3. Tab through controls to verify focus indicators
4. Click and hold to verify pressed states
5. Verify disabled controls look distinct

### 4. DPI Scaling

1. Set display scaling to 125%, 150%, and 200%
2. Open Settings window — verify theme cards and swatches scale correctly
3. Open a dropdown — verify popup is correctly sized and positioned
4. Verify no text clipping or element overlap

### 5. Excel Stability

1. Open a window with a DataGrid containing 500+ rows
2. Scroll continuously — verify smooth scrolling with no frame drops
3. Switch themes 10 times rapidly — verify no freezing, only final theme applied
4. Minimize, move, and close windows — verify Excel does not freeze

### 6. Keyboard Navigation

1. Open Settings window
2. Navigate using Tab only — verify all elements reachable
3. Press Enter/Space on theme cards and swatches — verify activation
4. Press Escape — verify window closes

## Key Files

| File | Purpose |
|------|---------|
| `Services/ThemeManager.cs` | Theme switching singleton |
| `Services/WindowRenderModeDetector.cs` | Excel-safe mode detection |
| `Theme/ThemeResources.xaml` | Resource aggregator |
| `Theme/Effects/Shadows.xaml` | Centralized shadow effects |
| `Theme/Controls/*.xaml` | Standardized control styles |
| `Views/SettingsWindow.xaml` | Theme card + swatch UI |

## Validation Checklist

- [ ] Build passes with zero errors
- [ ] All 3 themes switch correctly
- [ ] 8 accent swatches work
- [ ] Theme persists across restarts
- [ ] All controls show hover/focus/pressed/disabled states
- [ ] Dropdowns render with shadow, no clipping
- [ ] DPI scaling at 100%, 125%, 150%, 200%
- [ ] DataGrid scrolls smoothly with 1000+ rows
- [ ] Rapid theme switching (10x) — no crash
- [ ] Keyboard navigation — all elements reachable
- [ ] Focus indicators visible in both themes
