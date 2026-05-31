# Research: i18n Consolidation & Language Support

**Phase 0 output** — Existing-state analysis and migration pre-work assessment.

## Current Architecture

### Dual-System Problem

The application has **two independent localization systems** that have diverged:

| Aspect | Active System (WpfApp2) | Future System (Som3a.Localization) |
|--------|------------------------|-----------------------------------|
| Service | `LocalizationBridgeService` (singleton) | `ILocalizationService` / `LocalizationService` |
| XAML Binding | `TranslationSource.Instance[Key]` (indexer) | Not wired yet |
| Resources | `WpfApp2/Resources/Strings.resx` + `StringsArabic.resx` | `Som3a.Localization/Resources/Strings.en-US.resx` + `Strings.ar-SA.resx` |
| Brand | "Planova" | "Som3a" |
| Key Count | ~400+ (Strings) + ~1062 (Arabic) | ~130+ (en-US) + ~130+ (ar-SA) |
| Arabic Encoding | Mojibake (UTF-8 corruption) | Proper Arabic |
| Status | Active in production | Dormant library, never wired into WPF |

### Affected Services

| Service | File | Role | Disposition |
|---------|------|------|-------------|
| `LocalizationBridgeService` | `WpfApp2/Services/LocalizationBridgeService.cs` | Current active service — loads RESX, caches strings, raises LanguageChanged | REMOVE after migration |
| `TranslationSource` | `WpfApp2/Services/TranslationSource.cs` | XAML binding bridge — INotifyPropertyChanged indexer wrapping bridge | REMOVE after migration |
| `LocalizationService` | `Som3a.Localization/Services/LocalizationService.cs` | Future service — ResourceManager-based, sets CultureInfo | WIRE into WPF app |
| `ArabicFontManager` | `WpfApp2/Services/ArabicFontManager.cs` | Arabic font switching (Cairo, IBM Plex Sans Arabic, Segoe UI) | INTEGRATE into consolidated font model |
| `CultureAwareFormattingService` | `WpfApp2/Services/CultureAwareFormattingService.cs` | Number/date formatting per culture | RETAIN (separate concern) |
| `ShellRTLManager` | `WpfApp2/Controls/Shell/ShellRTLManager.cs` | RTL flow direction for shell elements | RETAIN with updates |
| `ThemeManager` | `WpfApp2/Services/ThemeManager.cs` | Font preset switching (also sets CustomFontFamily) | DECOUPLE font from theme |

### XAML Binding Pattern (~495 sites)

```xaml
{Binding Source={x:Static services:TranslationSource.Instance}, Path=[ResourceKeyName]}
```

Must be migrated to either:
- Direct `ILocalizationService` binding (C# ViewModels)
- A new WPF markup extension (`Loc`) for XAML

### Resource File Structure

**Current (to be removed):** `WpfApp2/Resources/`
```
Strings.resx                -> ~400+ English keys
StringsArabic.resx          -> ~1062 Arabic keys (CORRUPTED encoding)
```

**Target (to be completed):** `Som3a.Localization/Resources/`
```
Strings.en-US.resx          -> ~130+ English keys (needs merge from Strings.resx)
Strings.ar-SA.resx          -> ~130+ Arabic keys (needs merge + ~740 new translations)
```

Key naming convention: `{Area}.{SubArea}.{Element}` — dot-separated PascalCase.

### RTL Implementation Gaps

1. All RTL switching is **imperative code-behind** (`ShellRTLManager`, `WorkspaceHost`) — no XAML-declarative FlowDirection
2. Every page navigation calls `FlowDirection` assignment — fragile, easy to miss new pages
3. `StringsArabic.resx` has corrupted encoding — all existing Arabic values are garbled

### Font Handling Fragmentation

- `ArabicFontManager` sets Arabic fonts on language change
- `ThemeManager` also manages `CustomFontFamily` and `FontFamily.Active` resources
- No single authority for font-family-per-locale — dual management creates conflicts

## Migration Sequence

```
Step 1: Translation pre-work (4.0)
  ├── Export all 1800+ keys from WpfApp2/Resources/Strings.resx
  ├── Generate Arabic translations (AI + manual review)
  ├── Import into Som3a.Localization/Resources/Strings.ar-SA.resx
  └── Merge English keys into Som3a.Localization/Resources/Strings.en-US.resx

Step 2: Wire ILocalizationService (4.1)
  ├── Register LocalizationService in CompositionRoot
  ├── Replace LocalizationBridgeService.Instance calls in App.xaml.cs
  └── Route LanguageChanged event through EventBus

Step 3: Migrate bindings (4.2 + 4.3)
  ├── Create Loc markup extension (or use binding converter)
  ├── Bulk-replace TranslationSource.Instance bindings (~495 sites)
  └── Remove TranslationSource.cs + LocalizationBridgeService.cs

Step 4: Fix RTL/FlowDirection (4.4)
  ├── Add FlowDirection to XAML root elements (declarative)
  ├── Fix DataGrid column alignment in RTL
  ├── Fix ScrollBar placement in RTL
  └── Test mixed EN/AR content rendering

Step 5: Consolidate font handling (4.5)
  ├── Create FontService or extend ILocalizationService for font mapping
  ├── Decouple font switching from ThemeManager
  └── Define font-per-locale configuration

Step 6: Create LanguagePage (4.6)
  ├── Language selector (EN/AR)
  ├── Font picker per locale
  ├── RTL preview toggle
  └── Font size scaling (0.8x–1.5x)

Step 7: Update shell toggle (4.7)
  ├── Replace 🌐 emoji with Fluent 2 icon
  └── Wire to ILocalizationService.SetLanguage()
```

## Key Risks

1. **Arabic encoding corruption** — `StringsArabic.resx` values are mojibake. All ~1062 existing Arabic entries are unusable and must be re-translated
2. **495 binding sites** — Mechanical but high-risk; missed bindings will show English in Arabic mode
3. **RTL regression** — Every page must be manually verified for correct RTL layout; DataGrid virtualization adds complexity
4. **VSTO compatibility** — FlowDirection changes may cause rendering issues in Excel host; VSTO smoke test is critical
