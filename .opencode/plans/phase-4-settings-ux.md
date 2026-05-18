# Phase 4: Settings UX Rebuild

**Branch**: `feature/settings-ux`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Transform the theme selection from a simple dropdown to beautiful theme cards with preview thumbnails, selection glow, and smooth animations.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Views/SettingsWindow.xaml`
  - `WpfApp2/Theme/Controls/ThemeCardStyles.xaml` (new)

---

## Tasks

### T015 Replace dropdown with theme cards
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Replace:
```xaml
<ComboBox Style="{DynamicResource RoundComboBox}">
    <ComboBoxItem Content="Dark"/>
    <ComboBoxItem Content="Light"/>
    <ComboBoxItem Content="Blue"/>
</ComboBox>
```

With three theme cards (Dark, Light, Custom) in a horizontal panel.

### T016 Add theme preview thumbnails
**Files**: `WpfApp2/Views/SettingsWindow.xaml`, `WpfApp2/Theme/Controls/ThemeCardStyles.xaml`

Each card shows a mini preview of the theme colors:
- Dark card: Dark gradient preview
- Light card: Light gradient preview
- Custom card: Custom accent color preview

### T017 Add selection glow animation
**File**: `WpfApp2/Theme/Controls/ThemeCardStyles.xaml`

When a theme card is selected:
- Add Glow.Focus effect around the card
- Animate scale to 1.02
- Add border highlight with AccentBrush

### T018 Integrate runtime theme switching
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Wire up the cards to trigger theme switch on click (future: via ThemeManager service).

---

## Dependency Order
T015 → T016 → T017 → T018 (sequential)

---

## Acceptance Criteria
- [ ] Theme dropdown replaced with 3 theme cards
- [ ] Each card has preview thumbnail
- [ ] Selected card has visible glow effect
- [ ] Cards animate on hover
- [ ] Theme selection is click-driven
- [ ] UI is responsive and polished