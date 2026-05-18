# Phase 7: Runtime Theme Manager

**Branch**: `feature/runtime-theme-manager`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Implement the runtime theme switching infrastructure that allows themes to change without application restart, with persistent user preferences.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Files Affected**:
  - `WpfApp2/Services/ThemeManager.cs` (new)
  - `WpfApp2/Views/SettingsWindow.xaml` (integrate)

---

## Tasks

### T029 Create ThemeManager service
**File**: `WpfApp2/Services/ThemeManager.cs`

```csharp
public class ThemeManager
{
    void ApplyTheme(string themeName);
    void LoadThemeFromSettings();
    ResourceDictionary GetThemeDictionary(string themeName);
    event EventHandler ThemeChanged;
}
```

Responsibilities:
- Load theme dictionaries dynamically
- Replace merged Application resource dictionaries
- Apply themes globally across all windows
- Fire change event for UI updates

### T030 Add user preference persistence
**File**: Settings service / app.config

Store selected theme in:
- Application settings (Properties.Settings)
- Or config file

### T031 Wire SettingsWindow to ThemeManager
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Connect theme cards to ThemeManager:
```csharp
private void ThemeCard_Click(object sender, RoutedEventArgs e)
{
    ThemeManager.ApplyTheme("Dark"); // or Light, Custom
}
```

### T032 Test runtime switching
**File**: SettingsWindow (test scenario)

Verify:
- Theme switches without restart
- All open windows update
- Settings persist across app restarts
- No visual glitches during transition

---

## Dependency Order
T029 → T030 → T031 → T032

---

## Acceptance Criteria
- [ ] ThemeManager service loads/replaces themes dynamically
- [ ] User preference persists across sessions
- [ ] Theme switches without application restart
- [ ] All windows reflect the new theme
- [ ] Smooth transition (no flicker)