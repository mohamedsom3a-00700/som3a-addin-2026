# Theme API Contract

**Feature**: WPF Fluent UI Migration — Theme Engine & Runtime Switching
**Date**: 2026-05-18

## Service Contract: ThemeManager

The ThemeManager is a singleton service responsible for runtime theme switching, persistence, and theme state management. It is the single point of entry for all theme operations.

---

### Public Interface

```csharp
public sealed class ThemeManager
{
    // Singleton access
    ThemeManager Instance { get; }

    // Current state
    string CurrentTheme { get; }           // "Dark", "Light", or "Custom"
    string CurrentAccentColor { get; }     // Hex string, e.g., "#3A86FF"

    // Events
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    // Theme operations
    void ApplyTheme(string themeName);
    void ApplyTheme(string themeName, string accentColor);
    void LoadThemeFromSettings();
    void ResetToDefault();

    // Persistence
    void SaveCurrentTheme();
}

public class ThemeChangedEventArgs : EventArgs
{
    string PreviousTheme { get; }
    string NewTheme { get; }
    string PreviousAccent { get; }
    string NewAccent { get; }
}
```

---

### Method Behaviors

| Method | Precondition | Postcondition | Error Behavior |
|--------|-------------|-------------|---------------|
| `ApplyTheme("Dark")` | themeName is valid | Application resources updated, ThemeChanged fires | Logs warning, no-op if already Dark |
| `ApplyTheme("Light")` | themeName is valid | Application resources updated, ThemeChanged fires | Logs warning, no-op if already Light |
| `ApplyTheme("Custom")` | themeName is valid | Custom base theme loaded, AccentColor unchanged | Logs warning |
| `ApplyTheme("Custom", "#14B8A6")` | Both valid | Custom loaded + accent token updated, ThemeChanged fires | Falls back to default accent, logs |
| `ApplyTheme("invalid")` | — | Throws `ArgumentException` | — |
| `LoadThemeFromSettings()` | Settings file exists | Last saved theme + accent applied | Falls back to Dark if load fails |
| `SaveCurrentTheme()` | — | Persists CurrentTheme + CurrentAccentColor to Settings | No-op on failure, logs error |

---

### Event Contract: ThemeChanged

| Condition | Behavior |
|-----------|---------|
| Theme changes | Firing order: Replace merged ResourceDictionaries → Fire ThemeChanged event |
| Only accent changes within Custom | ThemeChanged fires with PreviousTheme=NewTheme, Accent fields differ |
| Popup open during theme switch | Calling code must close popups before ApplyTheme (FR edge case) |
| Multiple rapid ApplyTheme calls | Each call replaces the previous; ThemeChanged fires once per call |

**Subscribers**: Views (SettingsWindow) respond to ThemeChanged to update UI (e.g., ThemeCard selection states).

---

### Settings Persistence Contract

```csharp
// Stored in Properties.Settings.Default
namespace Som3a_WPF_UI.Properties
{
    public sealed class Settings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("Dark")]
        public string SelectedTheme { get; set; }

        [UserScopedSetting()]
        [DefaultSettingValue("#3A86FF")]
        public string AccentColor { get; set; }
    }
}
```

| Field | Default | Format | Valid Values |
|-------|---------|--------|-------------|
| SelectedTheme | "Dark" | String | "Dark", "Light", "Custom" |
| AccentColor | "#3A86FF" | Hex string (#RRGGBB) | Any 6-digit hex color |

---

### Fallback-Safe Mode Contract

```csharp
public enum WindowRenderMode
{
    WindowChrome,      // Default: modern borderless with WindowChrome
    FallbackSafe       // Edge case: WindowStyle=None + AllowsTransparency=True
}

public static class WindowRenderModeDetector
{
    WindowRenderMode DetectOptimalMode();
    // Called once at application startup
    // Uses try/catch rendering test + Excel version detection
    // Returns FallbackSafe on known problematic configurations
}
```

| Detection Method | Trigger for Fallback |
|-----------------|---------------------|
| Rendering test | WindowChrome throws during initial render |
| Excel version check | Known problematic Office versions identified |
| DPI check | Extreme DPI values (300%+) that cause WindowChrome issues |

---

### Theme ResourceDictionary Structure

Each theme dictionary (DarkTheme.xaml, LightTheme.xaml, CustomTheme.xaml) is a merged ResourceDictionary following this structure:

```
ResourceDictionary.MergedDictionaries:
  1. Base/Colors.xaml             (Primitive + Semantic tokens — base definitions)
  2. [ThemeName]Colors.xaml       (Semantic token overrides for this theme)

ResourceDictionary entries:
  - All Semantic tokens (Brush.*) with theme-specific color values
  - Custom theme: Brush.Accent.Primary = swatch hex value
```

**Dark theme overrides**: Background → Slate900 family, Text → WhiteAlpha family, Accent → Blue500
**Light theme overrides**: Background → #FAFAFA family, Text → #1A1A1A family, Accent → Blue500
**Custom theme overrides**: Background → Slate900 family, Accent → swatch value, others → Dark defaults

---

### Theme Card UI Contract (SettingsWindow)

```xaml
<!-- Theme Cards Panel Structure -->
<ItemsControl x:Name="ThemeCardsPanel">
    <Border x:Name="CardDark" Tag="Dark" Cursor="Hand">
        <!-- Preview gradient -->
        <!-- Accent strip -->
        <!-- Name label -->
        <!-- Description label -->
    </Border>
    <Border x:Name="CardLight" Tag="Light" Cursor="Hand">
        <!-- ... -->
    </Border>
    <Border x:Name="CardCustom" Tag="Custom" Cursor="Hand">
        <!-- ... -->
        <!-- Accent Swatches Panel (visible only when Custom selected) -->
        <ItemsControl x:Name="AccentSwatchesPanel">
            <!-- 8 AccentSwatch circles -->
        </ItemsControl>
    </Border>
</ItemsControl>
```

| UI Element | Behavior |
|-----------|---------|
| Card click | Calls ThemeManager.ApplyTheme(Tag) |
| Swatch click | Calls ThemeManager.ApplyTheme("Custom", SwatchHex) |
| Selected card | Has Glow.ThemeCard.Selected effect + scale 1.02 |
| Selected swatch | Has accent-colored fill ring |

---

### Animation Library Contract

```xaml
<!-- Animations.xaml — available Storyboards -->
<Storyboard x:Key="HoverEnter">
    <!-- Target: Opacity, Duration: 0:0:0.15 -->
</Storyboard>
<Storyboard x:Key="HoverExit">
    <!-- Target: Opacity, Duration: 0:0:0.15 -->
</Storyboard>
<Storyboard x:Key="FocusEnter">
    <!-- Target: Opacity, Duration: 0:0:0.1 -->
</Storyboard>
<Storyboard x:Key="FocusExit">
    <!-- Target: Opacity, Duration: 0:0:0.1 -->
</Storyboard>
<Storyboard x:Key="PopupOpen">
    <!-- Target: Opacity, Duration: 0:0:0.15, CubicEase EaseOut -->
</Storyboard>
<Storyboard x:Key="PopupClose">
    <!-- Target: Opacity, Duration: 0:0:0.1 -->
</Storyboard>
<Storyboard x:Key="FadeIn">
    <!-- Target: Opacity, Duration: 0:0:0.2 -->
</Storyboard>
<Storyboard x:Key="FadeOut">
    <!-- Target: Opacity, Duration: 0:0:0.15 -->
</Storyboard>

<!-- Easing Functions -->
<CubicEase x:Key="AnimEase" EasingMode="EaseOut"/>
```

All durations ≤ 200ms. No bounce or elastic easing. No BlurEffect animations.

---

### Shadow & Glow Library Contract

```xaml
<!-- Shadows.xaml -->
<DropShadowEffect x:Key="Shadow.Window"    BlurRadius="30" ShadowDepth="8" Opacity="0.4"/>
<DropShadowEffect x:Key="Shadow.Popup"     BlurRadius="15" ShadowDepth="3" Opacity="0.3"/>
<DropShadowEffect x:Key="Shadow.Popup.Small" BlurRadius="12" ShadowDepth="2" Opacity="0.25"/>
<DropShadowEffect x:Key="Shadow.Card"      BlurRadius="16" ShadowDepth="4" Opacity="0.25"/>
<DropShadowEffect x:Key="Shadow.Small"     BlurRadius="8" ShadowDepth="2" Opacity="0.2"/>
<DropShadowEffect x:Key="Shadow.Medium"    BlurRadius="12" ShadowDepth="3" Opacity="0.25"/>
<DropShadowEffect x:Key="Shadow.Large"    BlurRadius="20" ShadowDepth="5" Opacity="0.3"/>

<!-- Glow.xaml -->
<DropShadowEffect x:Key="Glow.Focus"              Color="#3A86FF" BlurRadius="10" ShadowDepth="0" Opacity="0.35"/>
<DropShadowEffect x:Key="Glow.ButtonHover"        Color="#3A86FF" BlurRadius="8" ShadowDepth="0" Opacity="0.3"/>
<DropShadowEffect x:Key="Glow.Primary"            Color="#3A86FF" BlurRadius="12" ShadowDepth="0" Opacity="0.4"/>
<DropShadowEffect x:Key="Glow.Selection"           Color="#3A86FF" BlurRadius="6" ShadowDepth="0" Opacity="0.5"/>
<DropShadowEffect x:Key="Glow.Accent"              Color="#3A86FF" BlurRadius="8" ShadowDepth="0" Opacity="0.4"/>
<DropShadowEffect x:Key="Glow.ThemeCard.Selected"  Color="#3A86FF" BlurRadius="14" ShadowDepth="0" Opacity="0.45"/>
```

All effects use DynamicResource in control templates. No inline DropShadowEffect definitions in any control template.