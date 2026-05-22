<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at: specs/004-design-system-core/plan.md

Also refer to the master implementation plan:
- implementation_plan.md — Full execution plan for Phases 0-11
<!-- SPECKIT END -->

# Som3a Add-in 2026 — Implementation Notes

## Theme Engine (Feature: 001-fluent-theme-engine)

### Architecture

```
WpfApp2/Theme/
├── Base/
│   ├── Colors.xaml              # Primitive + Semantic tokens (default dark)
│   ├── Typography.xaml
│   ├── Spacing.xaml             # ThemeCardWidth/Height, AccentSwatchSize tokens
│   └── Radius.xaml
├── Dark/
│   ├── DarkColors.xaml          # Semantic token overrides
│   └── DarkTheme.xaml           # Merged dictionary
├── Light/
│   ├── LightColors.xaml
│   └── LightTheme.xaml
├── Custom/
│   ├── CustomColors.xaml         # 8 accent swatch presets
│   └── CustomTheme.xaml
├── Controls/
│   ├── ButtonStyles.xaml
│   ├── ComboBoxStyles.xaml       # Popup: AllowsTransparency=False + PlacementTarget
│   ├── DataGridStyles.xaml       # EnableRowVirtualization=True
│   ├── CheckBoxStyles.xaml        # New
│   ├── RadioButtonStyles.xaml    # New
│   ├── ToggleButtonStyles.xaml   # New
│   ├── ScrollViewerStyles.xaml   # New
│   ├── ThemeCardStyles.xaml      # New
│   ├── AccentSwatchStyles.xaml   # New
│   └── ... (existing)
├── Effects/
│   ├── Shadows.xaml              # Centralized DropShadowEffect definitions
│   ├── Glow.xaml                # DynamicResource AccentColorValue
│   └── Animations.xaml           # Storyboards (all ≤200ms)
└── ThemeResources.xaml          # Aggregator with documented loading order
```

### Key Conventions

- **DynamicResource only** — all theme colors use `{DynamicResource Brush.*}`, no StaticResource for themeable properties
- **No inline DropShadowEffect** — use `{DynamicResource Shadow.*}` from Effects/Shadows.xaml
- **Glow color is dynamic** — Glow.xaml uses `Color="{DynamicResource AccentColorValue}"`, updated by ThemeManager
- **AccentColorValue Color resource** — set in CustomColors.xaml, updated at runtime by ThemeManager.ApplyAccentColor()
- **WindowRenderModeDetector** — auto-detects Excel VSTO hosting and activates FallbackSafe mode

### Services

- `Services/ThemeManager.cs` — singleton, ApplyTheme(string, string), LoadThemeFromSettings(), SaveCurrentTheme(), ThemeChanged event
- `Services/WindowRenderModeDetector.cs` — detects VSTO/DPI/rendering issues, returns FallbackSafe or WindowChrome mode
- `Services/ThemeSettings.cs` — legacy persistence (JSON file), still used by some parts
- `Controls/ModernWindow.cs` — calls LoadThemeFromSettings() on init, uses WindowRenderModeDetector

### Settings Persistence

- Primary: `Properties/Settings.settings` (ApplicationSettingsBase) — SelectedTheme + AccentColor
- Secondary: `Services/ThemeSettings.cs` (JSON file at AppData/Som3a/theme.json)

### Theme Switching Flow

1. User clicks theme card → `ThemeManager.Instance.ApplyTheme("Dark")`
2. ThemeManager finds + removes existing theme dictionary from MergedDictionaries
3. Creates new theme dictionary from DarkTheme.xaml (loads in try/catch first)
4. Adds new theme dictionary to MergedDictionaries
5. If accentColor provided → `ApplyAccentColor()` → updates AccentColorValue + AccentColorBrush + glow effects
6. Fires `ThemeChanged` event
7. SettingsWindow listens → updates card/swatches selection

### Control Templates

- ComboBox popup: `AllowsTransparency="False"`, `Placement="Bottom"`, `PlacementTarget` binding
- DataGrid: `EnableRowVirtualization="True"`, `VirtualizationMode="Recycling"`
- All controls: `SnapsToDevicePixels="True"`, `UseLayoutRounding="True"`
- No BlurEffect on any scrollable container

### Governance Documents

```text
Docs/Architecture/
├── UI_GUIDELINES.md
├── TOKEN_RULES.md
├── POPUP_ARCHITECTURE.md
├── SHADOW_SYSTEM.md
├── ACCESSIBILITY_RULES.md
├── EXCEL_RENDERING_RULES.md
├── MVVM_RULES.md
├── REVIEW_CHECKLIST.md
├── AGENT_RULES.md
├── PERFORMANCE_RULES.md
├── AUDIT_REPORT.md
└── EXCEL_TEST_CHECKLIST.md
```

### Constitution

- `.specify/memory/constitution.md` — Current active constitution (v1.2.0)
- `.specify/memory/constitution-v2.md` — Next-generation constitution (v2.0.0)

### Build

```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```
