# Data Model: i18n Consolidation & Language Support

## Entities

### LocalizationResource

Represents a single translatable string key with values in each supported locale.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `Key` | string | Unique dot-separated identifier (e.g. `Shell.Title`, `Settings.Language.Label`) | Matches `{Area}.{SubArea}.{Element}` pattern; max 200 chars |
| `Area` | string | Top-level grouping (Shell, Settings, Dashboard, Error, Common, etc.) | Derived from Key prefix |
| `EnValue` | string | English (en-US) translation | Required; max 1000 chars |
| `ArValue` | string | Arabic (ar-SA) translation | Required; max 1500 chars (Arabic text is typically longer) |
| `HasFormatParameters` | bool | Whether value contains `{0}`, `{1}`, etc. placeholders | Default false |
| `ParameterCount` | int | Number of format parameters (0–5) | Required if `HasFormatParameters` true |

**Relationships**: `LocalizationResource` is a standalone entity — no foreign keys. Organized by `Area` for logical grouping.

**Uniqueness**: `Key` is unique across all areas. No two resources share the same Key.

**Lifecycle**: Resources are static (compiled into satellite assemblies). Updates require recompilation or runtime RESX reloading. New keys are added during development; missing keys are detected via diagnostic logging.

---

### LanguageSetting

Represents the user's language preference.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `CultureCode` | string | BCP-47 culture code (`en-US` or `ar-SA`) | Must be `en-US` or `ar-SA` |
| `DisplayName` | string | User-visible name ("English", "العربية") | Derived from CultureCode |
| `IsRTL` | bool | Whether the language is right-to-left | Derived: `ar-SA` → true, `en-US` → false |

**Relationships**: Used by `ILocalizationService` to load the correct resource set and set `CultureInfo.CurrentUICulture`.

**Persistence**: Stored in `Properties.Settings.Default.SelectedLanguage` (migrated from plain text file at `%APPDATA%/Som3a/language-preference.txt`).

**Lifecycle**: User selects language in Settings or via shell toggle → persisted immediately → restored at startup.

**State Transitions**:

```
Startup → Load from Properties.Settings
     ↓
User changes language → Persist new CultureCode
     ↓
Language switch: SetCultureInfo → Raise LanguageChanged → Update UI bindings
```

---

### FontMapping

Defines which font family to use for each locale.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `Locale` | string | Target locale (`en-US`, `ar-SA`) | Must match a supported CultureCode |
| `FontFamily` | string | Font family name (e.g., `Segoe UI`, `Cairo`, `Noto Naskh Arabic`) | Must be a valid installed or bundled font |
| `FallbackChain` | string[] | Ordered fallback fonts if primary lacks glyphs | At least 1 fallback; max 3 |

**Default mappings**:

| Locale | Primary Font | Fallback Chain |
|--------|-------------|----------------|
| en-US | Segoe UI | Arial, Microsoft Sans Serif |
| ar-SA | Cairo | Noto Naskh Arabic, Segoe UI, Arial |

**Relationships**: Referenced by `LanguagePageViewModel` for per-locale font picker. Managed by `ILocalizationService` (extended) or a dedicated `FontService`.

**Lifecycle**: Defaults defined at compile time; users can customize via Settings → Language & Font → Font picker.

---

### FontScalingSetting

Represents the user's preferred font size multiplier for accessibility.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| `ScaleFactor` | double | Multiplier applied to base font size (1.0 = 100%) | Range: 0.8–1.5; step 0.1 |
| `PreviewText` | string | Sample text shown in preview | Set by viewmodel; not persisted |

**Default**: 1.0 (no scaling).

**Persistence**: Stored in `Properties.Settings.Default.FontScalingFactor` (new setting).

---

### LanguageChangedEvent (Event Bus Event)

Event published on the `EventBus` when the user's language changes.

| Field | Type | Description |
|-------|------|-------------|
| `PreviousCultureCode` | string | Culture code before the change |
| `NewCultureCode` | string | Culture code after the change |
| `IsRTL` | bool | Whether the new locale is right-to-left |
| `Timestamp` | DateTime | When the change occurred |

**Subscribers**: `ShellRTLManager` (update layout), `CultureAwareFormattingService` (update formatting), `TranslationSource` replacement (refresh UI bindings), `ArabicFontManager` (switch fonts).

---

## Validation Rules

| Rule | Applies To | Description |
|------|-----------|-------------|
| R1 | LocalizationResource.Key | Must be unique across all resources |
| R2 | LocalizationResource.Key | Must match pattern `^[A-Za-z]+(\.[A-Za-z]+)+$` |
| R3 | LocalizationResource.EnValue | Cannot be null or empty |
| R4 | LocalizationResource.ArValue | Cannot be null or empty (must at minimum match EnValue as fallback) |
| R5 | LanguageSetting.CultureCode | Must be one of the supported codes from `GetSupportedLanguages()` |
| R6 | FontMapping.FontFamily | Must exist on the system or be bundled with the application |
| R7 | FontScalingSetting.ScaleFactor | Clamped to 0.8–1.5 range |

## State Diagrams

### Language Switch Flow

```
LanguageSelector (toggle/click)
    ↓
ILocalizationService.SetLanguage(cultureCode)
    ↓
CultureManager.SetCulture(CultureInfo)
    → Sets Thread.CurrentThread.CurrentUICulture
    ↓
Publish LanguageChangedEvent
    ↓
Subscribers:
├── UI Binding Manager → Refresh() all bound strings (~495 sites)
├── ShellRTLManager → ApplyLayout(isRTL) → FlowDirection, mirror sidebar
├── FontService → SwitchFonts(locale) → update FontFamily resources
├── CultureAwareFormattingService → RefreshCulture() → number/date format
└── LanguagePageViewModel → Update selection state
```

### Translation Fallback Flow

```
ILocalizationService.GetString(key)
    ↓
Lookup key in current culture resource
    ├── Found → return value
    └── Not found
        ├── Log missing key to diagnostics channel
        └── Fallback to English resource
            ├── Found → return English value
            └── Not found → return key name as last resort
```
