# Quickstart: WPF Fluent UI Theme Engine Migration

**Feature**: WPF Fluent UI Migration — Theme Engine & Runtime Switching
**Date**: 2026-05-18
**Prerequisites**: .NET Framework 4.8, Visual Studio, Excel 2016+

---

## Project Overview

This migration transforms the WPF UI layer of an Excel-hosted VSTO add-in into a production-grade Fluent Design system with runtime theme switching.

**Key deliverables**:
- 3 built-in themes (Dark, Light, Custom) with 8 accent color presets
- Theme card UI in SettingsWindow replacing the ComboBox dropdown
- Centralized shadow, glow, and animation library
- VisualStateManager for 4 high-priority controls (ComboBox, Button, ToggleButton, ThemeCards)
- Automatic fallback-safe window rendering for Excel hosting edge cases
- DPI-aware rendering at 100%, 125%, 150%, 200%

---

## Development Setup

### 1. Verify Project Structure

```
WpfApp2/
├── Theme/
│   ├── Base/Colors.xaml       ← Enhanced with primitive + semantic tokens
│   ├── Controls/             ← New + enhanced control styles
│   └── Effects/              ← NEW: Shadows, Glow, Animations
├── Services/ThemeManager.cs  ← NEW: Theme switching service
└── Properties/Settings.*     ← NEW: Persistence
```

### 2. Build and Verify Baseline

Build the existing WpfApp2 project before making any changes:

```powershell
# Using Visual Studio Developer Command Prompt with MSBuild
msbuild WpfApp2\WpfApp2.csproj /p:Configuration=Debug

# Or open in Visual Studio and verify the solution compiles
```

**Expected**: Build succeeds with existing code. All windows open with current dark theme.

### 3. Key Files to Create (Priority Order)

| Priority | File | Description |
|----------|------|-------------|
| P1 | `Theme/Effects/Shadows.xaml` | Centralized DropShadowEffect definitions |
| P1 | `Theme/Effects/Glow.xaml` | Centralized glow effects |
| P1 | `Theme/Effects/Animations.xaml` | Control state + popup animations |
| P1 | `Theme/Dark/DarkColors.xaml` | Dark theme semantic token overrides |
| P1 | `Theme/Dark/DarkTheme.xaml` | Dark merged dictionary |
| P1 | `Theme/Light/LightColors.xaml` | Light theme semantic token overrides |
| P1 | `Theme/Light/LightTheme.xaml` | Light merged dictionary |
| P1 | `Services/ThemeManager.cs` | Theme switching service |
| P2 | `Properties/Settings.settings` | Persistence configuration |
| P2 | `Theme/Custom/CustomColors.xaml` | Custom theme with accent swatches |
| P2 | `Theme/Custom/CustomTheme.xaml` | Custom merged dictionary |
| P2 | `Theme/Controls/ThemeCardStyles.xaml` | Theme card styles |
| P2 | `Views/SettingsWindow.xaml` | Settings with theme cards |
| P3 | `Theme/Controls/CheckBoxStyles.xaml` | Fluent CheckBox |
| P3 | `Theme/Controls/RadioButtonStyles.xaml` | Fluent RadioButton |
| P3 | `Theme/Controls/ToggleButtonStyles.xaml` | Fluent ToggleButton |
| P3 | `Theme/Controls/ScrollViewerStyles.xaml` | Fluent ScrollViewer |

### 4. Files to Modify (Priority Order)

| Priority | File | Modification |
|----------|------|-------------|
| P1 | `Theme/Base/Colors.xaml` | Already enhanced with primitive + semantic tokens ✅ |
| P1 | `Theme/Controls/ComboBoxStyles.xaml` | Fix AllowsTransparency, add shadow ref, add disabled state |
| P1 | `Views/SettingsWindow.xaml` | Replace ComboBox with theme cards, add accent swatches |
| P2 | `Theme/Controls/ButtonStyles.xaml` | Add VSM states (Normal, Hover, Pressed, Focused, Disabled) |
| P2 | `Theme/Controls/DataGridStyles.xaml` | Add virtualization, VSM states |
| P3 | `Theme/ThemeResources.xaml` | Ensure correct merged dictionary order |

---

## Theme Token Reference

### Primitive Tokens (Base/Colors.xaml)

| Key | Value | Usage |
|-----|-------|-------|
| `Blue500` | #3A86FF | Primary accent |
| `Slate900` | #0E1720 | Dark background |
| `Slate800` | #1C2B3A | Dark surface |
| `Slate700` | #15202B | Dark card |
| `WhiteAlpha95` | #F2FFFFFF | Primary text (dark theme) |
| `WhiteAlpha75` | #BFFFFFFF | Secondary text (dark theme) |
| `WhiteAlpha40` | #66FFFFFF | Disabled text |
| `WhiteAlpha20` | #33FFFFFF | Card stroke (dark) |
| `WhiteAlpha10` | #1AFFFFFF | Hover state |

### Semantic Tokens (override per theme)

| Key | Dark Value | Light Value | Usage |
|-----|-----------|-------------|-------|
| `Brush.Background.Primary` | #0E1720 | #FAFAFA | Main window background |
| `Brush.Background.Secondary` | #1C2B3A | #FFFFFF | Panels, sidebars |
| `Brush.Background.Card` | #15202B | #FFFFFF | Cards, dialogs |
| `Brush.Text.Primary` | #F2FFFFFF | #1A1A1A | Primary text |
| `Brush.Text.Secondary` | #BFFFFFFF | #666666 | Secondary text |
| `Brush.Text.Disabled` | #66FFFFFF | #AAAAAA | Disabled text |
| `Brush.Accent.Primary` | #3A86FF | #3A86FF | Accent elements |
| `Brush.Stroke.Card` | #33FFFFFF | #1A000000 | Card borders |
| `Brush.Control.Background` | #330E1720 | #F0F0F0 | Input backgrounds |
| `Brush.Control.Stroke` | #33FFFFFF | #1A000000 | Input borders |

---

## Accent Color Presets (Custom Theme)

| Swatch | Name | Hex Value |
|--------|------|----------|
| 🔵 | Blue | #3A86FF |
| 🟢 | Green | #2ED573 |
| 🟣 | Purple | #A855F7 |
| 🟠 | Orange | #FFA502 |
| 🩷 | Pink | #EC4899 |
| 🩵 | Teal | #14B8A6 |
| 🔴 | Red | #EF4444 |
| 🩵 | Cyan | #06B6D6 |

---

## Testing Checklist

Before declaring any phase complete, verify:

- [ ] Theme switches in under 1 second with no visual glitches
- [ ] Theme persists across app restarts (test all 3 themes + 2 accent colors)
- [ ] All 7 control types show all states in both Dark and Light themes
- [ ] ComboBox popup renders above all content with shadow
- [ ] DPI scaling correct at 100%, 125%, 150%, 200%
- [ ] All theme cards keyboard navigable (Tab + Enter)
- [ ] Fallback-safe mode activates on known Excel edge cases
- [ ] No nested DropShadows in visual tree
- [ ] DataGrid scrolls smoothly with 1000+ rows (virtualization enabled)

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Popup clips at window edges | Set Popup.Placement="Bottom" + PlacementTarget correctly |
| Theme flicker on switch | Ensure ThemeResources.xaml merges in correct order |
| AllowsTransparency rendering issues | Use WindowChrome default; auto-fallback detects issues |
| DataGrid scrolling stutters | Enable EnableRowVirtualization="True" |
| Accent color not updating | ThemeManager must call SaveCurrentTheme() after ApplyTheme |
| VSM states not triggering | Ensure VisualStateGroups defined on root element of template |

---

## Architecture Rules Reference

| Rule | Details |
|------|---------|
| DynamicResource only | All theme colors use `{DynamicResource Brush.*}` — no StaticResource for themeable properties |
| No inline DropShadowEffect | All shadows from `Effects/Shadows.xaml` via DynamicResource |
| Animation budget | All animations ≤ 200ms. No BlurEffect on scrollable containers |
| Resource loading order | Base tokens → Semantic tokens → Typography → Radius → Effects → Controls → Theme overrides |
| VSM scope | Only ComboBox, Button, ToggleButton, ThemeCards for this feature |
| Fallback-safe mode | Automatic runtime detection — no manual config needed |