# Localization Architecture

**Project**: Som3a Add-in 2026
**Phase**: 4 (i18n Consolidation & Language Support)
**Last Updated**: 2026-05-31

## Architecture Overview

### Localization Library

```
Som3a.Localization/
├── Contracts/
│   └── ILocalizationService.cs      # Interface: GetString, SetLanguage, FontScalingFactor
├── Services/
│   └── LocalizationService.cs       # Implementation: RESX loading, language switching
├── Resources/
│   ├── Strings.en-US.resx           # English (605 keys)
│   └── Strings.ar-SA.resx           # Arabic (605 keys, all translated)
└── Models/
    └── LanguageInfo.cs              # Code + DisplayName pair for UI binding
```

### Core Services (WpfApp2)

| Service | File | Role |
|---------|------|------|
| `FontService` | `Services/FontService.cs` | Per-locale font switching (Arabic/English), subscribes to `LanguageChanged` |
| `LocalizationBridge` | `Services/LocalizationBridge.cs` | Static facade for VSTO net48 project (returns string/bool only) |
| `ShellRTLManager` | `Controls/Shell/ShellRTLManager.cs` | Singleton managing FlowDirection, keyboard Tab mirroring, sidebar mirroring |

### XAML Converters

| Converter | Usage |
|-----------|-------|
| `BoolToFlowDirectionConverter` | Bind `IsRTL` boolean to `FlowDirection` on elements |
| `TextFlowDirectionConverter` | Per-cell bidirectional text detection (Arabic→RTL, Latin→LTR) |

### Markup Extension

`LocExtension` (`MarkupExtensions/LocExtension.cs`) — wraps `ILocalizationService.GetString()`, auto-refreshes on `LanguageChanged` event.

Usage: `Text="{l:Loc Settings.Language}"`

### Settings Persistence

- **Primary**: `Properties/Settings.settings` (ApplicationSettingsBase) — `SelectedLanguage` + `FontScalingFactor`
- **Legacy**: `Services/ThemeSettings.cs` (JSON at `%APPDATA%/Som3a/theme.json`) — still used for theme persistence

## Language Switching Flow

1. User triggers language change (shell toggle, LanguagePage ComboBox)
2. `ILocalizationService.SetLanguage(code)` → updates `CultureInfo.CurrentUICulture` → fires `LanguageChanged` event
3. `LocExtension` instances auto-refresh → XAML bindings update
4. `FontService` reacts → applies per-locale font
5. `ShellRTLManager` reacts → sets FlowDirection on root elements → all children inherit
6. ViewModels with `ILocalizationService` injection → refresh string properties

## RTL Layout (Phase 4)

FlowDirection inheritance is the primary mechanism:
- `ShellRTLManager` sets FlowDirection on `RootGrid` (ShellWindow)
- All child controls inherit via `FrameworkPropertyMetadataOptions.Inherits`
- DataGrid columns reverse order automatically in RTL
- ScrollBars move to left side automatically in RTL
- `BoolToFlowDirectionConverter` available for explicit XAML binding

### DataGrid RTL

- `DataGridColumnHeader` style: `HorizontalContentAlignment="Stretch"` (removed explicit `Left`)
- Header text alignment follows FlowDirection
- `TextFlowDirectionConverter` available for per-cell bidirectional text alignment

### Keyboard Tab Mirroring

- `ShellRTLManager.ApplyKeyboardTabMirroring()` reverses Tab navigation order in RTL mode
- Called on language change for content pages

## Font Switching (Phase 5)

- `FontService` manages per-locale font mappings
- Registers `ArabicFont` and `EnglishFont` resources at application level
- Subscribes to `ILocalizationService.LanguageChanged` for automatic switching
- Decoupled from `ThemeManager` (theme changes don't override per-locale fonts)
- Replaces deprecated `ArabicFontManager`

## Language Settings Page (Phase 6)

- Page: `Pages/Settings/LanguagePage.xaml`
- ViewModel: `ViewModels/Settings/LanguagePageViewModel.cs`
- Registered as Settings category "Language & Font" (Order=9, Globe icon)
- Controls:
  - Language selector (ComboBox with EN/AR options)
  - English font picker with system font list
  - Arabic font picker with preview area
  - Font size scaling slider (0.8x–1.5x)
  - RTL preview toggle (demonstrates layout mirroring without language switch)

## Shell Language Toggle (Phase 7)

- Located in ShellWindow toolbar
- FluentIcon.Globe icon (replaced emoji)
- ToolTip uses `{l:Loc}` for localized language name
- Click handler calls `ILocalizationService.SetLanguage()` then `SaveLanguagePreference()`

## Missing-Key Diagnostics

When a key is not found for the current culture, `LocalizationService.GetString()`:
1. Falls back to `Strings.resx` (neutral/default)
2. If still missing, returns `[key]` format for visual identification
3. Logs `Debug.WriteLine` warning with key name
4. Also logs if key exists in other cultures but not current (cross-culture diagnostic)

## RESX Keys

- **Total (en-US)**: 605 keys
- **Total (ar-SA)**: 605 keys (100% translated)
- **Migration**: Strings merged from `WpfApp2/Resources/Strings.resx` and `Som3a.Localization/Resources/Strings.ar-SA.resx` (previously `StringsArabic.resx`)
- **Brand resolution**: "Planova" → "Som3a" during merge

## VSTO Considerations

- VSTO project (net48) uses `LocalizationBridge` static facade
- Bridge wraps `App.Container.Resolve<ILocalizationService>()` behind string/bool-only API
- No net8.0 dependency required in VSTO project at compile time
- Language preference stored in `Properties.Settings.Default` (cross-process compatible)

## Performance Target

- Language switch: <500ms from toggle to full UI update
- Async loading of RESX resources on first access
- LocExtension instances refresh on UI thread via `LanguageChanged` event
