# Phase 7: Runtime Theme Manager

**Branch**: `feature/runtime-theme-manager`
**Date**: 2026-05-18
**Status**: Saved for Implementation

---

## Summary

Implement the runtime theme switching infrastructure that allows themes to change without application restart, with persistent user preferences. The ThemeManager MUST handle theme dictionary swapping in the correct resource loading order and support semantic token swap for Dark/Light/Custom.

---

## Technical Context

- **Language/Version**: C# 8.0 / .NET Framework 4.8
- **Target Platform**: Windows Desktop (Excel VSTO Add-in)
- **Governed by**: `.specify/memory/constitution.md` v1.1.0
- **Files Affected**:
  - `WpfApp2/Services/ThemeManager.cs` (new)
  - `WpfApp2/Views/SettingsWindow.xaml` (integrate — theme cards)
  - `WpfApp2/Theme/ThemeManager.xaml` (already created in Phase 3)

---

## Tasks

### T029 Create ThemeManager service
**File**: `WpfApp2/Services/ThemeManager.cs`

```csharp
public class ThemeManager
{
    private static ThemeManager _instance;
    private static readonly object _lock = new object();

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ThemeManager();
                }
            }
            return _instance;
        }
    }

    public event EventHandler<string> ThemeChanged;

    private string _currentTheme = "Dark";

    public string CurrentTheme => _currentTheme;

    public void ApplyTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return;

        if (_currentTheme == themeName)
            return;

        var themeDict = GetThemeDictionary(themeName);
        if (themeDict == null)
            return;

        ReplaceThemeDictionary(themeDict);

        _currentTheme = themeName;
        SaveThemePreference(themeName);

        ThemeChanged?.Invoke(this, themeName);
    }

    public void LoadThemeFromSettings()
    {
        string savedTheme = LoadThemePreference();
        ApplyTheme(savedTheme ?? "Dark");
    }

    private ResourceDictionary GetThemeDictionary(string themeName)
    {
        try
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri(
                    $"pack://application:,,,/Som3a_WPF_UI;component/Theme/{themeName}/{themeName}Theme.xaml",
                    UriKind.Absolute)
            };
            return dict;
        }
        catch
        {
            return null;
        }
    }

    private void ReplaceThemeDictionary(ResourceDictionary newTheme)
    {
        var mergedDicts = Application.Current.Resources.MergedDictionaries;

        // Find and remove existing theme dictionaries
        // Theme dictionaries are at the end of the merged list
        var toRemove = mergedDicts
            .Where(d => d.Source?.ToString().Contains("/Theme/Dark/") == true ||
                        d.Source?.ToString().Contains("/Theme/Light/") == true ||
                        d.Source?.ToString().Contains("/Theme/Custom/") == true)
            .ToList();

        foreach (var dict in toRemove)
            mergedDicts.Remove(dict);

        // Add new theme dictionary at the end (per resource loading order)
        mergedDicts.Add(newTheme);
    }

    private void SaveThemePreference(string themeName)
    {
        // Save to app settings
        Properties.Settings.Default.SelectedTheme = themeName;
        Properties.Settings.Default.Save();
    }

    private string LoadThemePreference()
    {
        return Properties.Settings.Default.SelectedTheme;
    }
}
```

### T030 Add user preference persistence
**File**: Application settings

Store selected theme in:
- `Properties.Settings.Default.SelectedTheme` (or config file)

Settings structure:
```csharp
namespace Som3a_WPF_UI.Properties
{
    public sealed class Settings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("Dark")]
        public string SelectedTheme
        {
            get { return (string)this["SelectedTheme"]; }
            set { this["SelectedTheme"] = value; }
        }
    }
}
```

### T031 Wire SettingsWindow to ThemeManager
**File**: `WpfApp2/Views/SettingsWindow.xaml`

Connect theme cards to ThemeManager:
```csharp
public partial class SettingsWindow : ModernWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += SettingsWindow_Loaded;
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Set initial selection state based on current theme
        string currentTheme = ThemeManager.Instance.CurrentTheme;
        SetThemeCardSelection(currentTheme);
    }

    private void ThemeCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border card && card.Tag is string themeName)
        {
            UpdateThemeCardSelection(card);
            ThemeManager.Instance.ApplyTheme(themeName);
        }
    }

    private void SetThemeCardSelection(string themeName)
    {
        // Find the card with matching Tag and set its selected state
    }

    private void UpdateThemeCardSelection(Border selectedCard)
    {
        // Remove selected state from all cards
        // Add selected state to selectedCard (Glow.ThemeCard.Selected style)
    }
}
```

### T032 Test runtime switching
**File**: SettingsWindow (test scenario)

Verify:
- Theme switches without restart
- All open windows update
- Settings persist across app restarts
- No visual glitches during transition
- Semantic tokens swap correctly
- DPI remains correct after switch
- All windows pass theme validation checklist

---

## Resource Loading Order — Theme Swap

Theme swapping MUST maintain correct resource loading order:

```
1. Base/Colors.xaml              (primitive + semantic tokens — immutable)
2. Base/Typography.xaml
3. Base/Spacing.xaml
4. Base/Radius.xaml
5. Effects/Shadows.xaml
6. Effects/Animations.xaml
7. Effects/Glow.xaml
8. Controls/ (all control styles)
9. ModernWindow.xaml
10. WindowAnimations.xaml
11. [Theme Override — SWAPPED at runtime] Dark/Light/Custom
```

The ThemeManager replaces only the theme override dictionaries (step 11), not the entire merged list.

---

## Dependency Order

T029 → T030 → T031 → T032

---

## Acceptance Criteria

- [ ] ThemeManager singleton loads/replaces themes dynamically
- [ ] User preference persists across sessions
- [ ] Theme switches without application restart
- [ ] All windows reflect the new theme immediately
- [ ] Smooth transition (no flicker)
- [ ] Semantic token swap works for Dark/Light/Custom
- [ ] Resource loading order maintained after swap
- [ ] DPI scaling preserved after theme switch
- [ ] All windows pass Theme Validation Checklist after switch

---

## Constitution Check

Per constitution:
- **Principle IV (Runtime Theme Switching)**: Theme switching MUST update without restart; Merged dictionaries MUST be replaced dynamically; User preferences MUST be preserved across sessions ✅
- **Resource Loading Order**: Theme swap maintains correct merge order ✅
- **Primitive & Semantic Token Architecture**: Two-tier separation enables semantic token swap ✅
- **Incremental Migration Rules**: Validate in Excel before advancing ✅

(End of file — total 164 lines)