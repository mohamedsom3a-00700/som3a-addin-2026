# Phase 3: Theme Engine Architecture

**Branch**: `feature/theme-engine`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Create the complete theme dictionary structure with Dark, Light, and Custom themes, establishing the foundation for runtime theme switching.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Theme/Dark/` (new folder)
  - `WpfApp2/Theme/Light/` (new folder)
  - `WpfApp2/Theme/Custom/` (new folder)
  - `WpfApp2/Theme/ThemeManager.xaml` (new)

---

## Tasks

### T011 Create Dark theme
**Files**: `WpfApp2/Theme/Dark/DarkColors.xaml`, `WpfApp2/Theme/Dark/DarkTheme.xaml`

DarkColors.xaml:
```xaml
<SolidColorBrush x:Key="BackgroundBrush" Color="#0E1720"/>
<SolidColorBrush x:Key="SurfaceBrush" Color="#1C2B3A"/>
<SolidColorBrush x:Key="CardBrush" Color="#15202B"/>
<SolidColorBrush x:Key="TextMainBrush" Color="#F2FFFFFF"/>
...
```

DarkTheme.xaml - Merged dictionary importing Base tokens + DarkColors

### T012 Create Light theme
**Files**: `WpfApp2/Theme/Light/LightColors.xaml`, `WpfApp2/Theme/Light/LightTheme.xaml`

Light palette (blue accent on white/light gray):
```xaml
<SolidColorBrush x:Key="BackgroundBrush" Color="#FAFAFA"/>
<SolidColorBrush x:Key="SurfaceBrush" Color="#FFFFFF"/>
<SolidColorBrush x:Key="CardBrush" Color="#FFFFFF"/>
<SolidColorBrush x:Key="TextMainBrush" Color="#1A1A1A"/>
...
```

### T013 Create Custom theme (placeholder)
**Files**: `WpfApp2/Theme/Custom/CustomColors.xaml`, `WpfApp2/Theme/Custom/CustomTheme.xaml`

Stub with accent color placeholders for future customization.

### T014 Create ThemeManager.xaml
**File**: `WpfApp2/Theme/ThemeManager.xaml`

ResourceDictionary containing all theme variants:
```xaml
<ResourceDictionary x:Key="DarkTheme" Source="Dark/DarkTheme.xaml"/>
<ResourceDictionary x:Key="LightTheme" Source="Light/LightTheme.xaml"/>
<ResourceDictionary x:Key="CustomTheme" Source="Custom/CustomTheme.xaml"/>
```

---

## Dependency Order
T011 → T012 → T013 → T014 (sequential for theme structure)

---

## Acceptance Criteria
- [ ] Dark theme complete with all color tokens
- [ ] Light theme complete with matching token structure
- [ ] Custom theme placeholder ready for future
- [ ] ThemeManager.xaml defines all theme variants
- [ ] Token names consistent across all themes