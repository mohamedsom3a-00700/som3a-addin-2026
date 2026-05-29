# Data Model: Localization & RTL

## Entities

### Language

| Field | Type | Description |
|-------|------|-------------|
| Code | string | Culture code, e.g., `en-US`, `ar-SA` |
| DisplayName | string | User-visible name, e.g., "English", "العربية" |
| NativeName | string | Name in the language itself |
| FlowDirection | enum (LTR/RTL) | Layout direction for the language |

**Validation rules**:
- Code must be a valid BCP-47 culture code
- Code must be unique across registered languages
- FlowDirection must be set explicitly (no default)

**State transitions**: Language is immutable once loaded from resources. Adding a new language requires a new .resx file and a Language entry.

---

### ResourceEntry

| Field | Type | Description |
|-------|------|-------------|
| Key | string | Unique identifier for the translated string |
| Value | string | Translated text in the target language |
| LanguageCode | string | Which language this entry belongs to |
| Area | string | UI area (Shell, Settings, Dashboard, Errors, Plugins) |
| FallbackKey | string | Optional key for English fallback when translation is missing |

**Validation rules**:
- Key must be unique within each language
- Value must not be empty (use English fallback instead)
- Area must be one of the defined UI areas
- Keys must follow naming convention: `{Area}.{Component}.{Identifier}`

---

### FontProfile

| Field | Type | Description |
|-------|------|-------------|
| LanguageCode | string | Associated language |
| PrimaryFont | string | Main font family name |
| FallbackFonts | string[] | Ordered fallback chain |
| SizeScale | float | Font size multiplier (1.0 = default) |

**Validation rules**:
- PrimaryFont must resolve to an installed or bundled font
- Fallback chain must include at least one Latin font for mixed content
- SizeScale must be between 0.8 and 1.5

---

### CultureProfile

| Field | Type | Description |
|-------|------|-------------|
| LanguageCode | string | Associated language |
| NumberFormat | CultureInfo.NumberFormat | Digit shapes, group/decimal separators |
| DateFormat | CultureInfo.DateTimeFormat | Date/time patterns, month/day names |
| CurrencyFormat | CultureInfo.NumberFormat | Currency symbol, placement |

**Validation rules**: Generated from .NET CultureInfo — no custom validation needed.

---

## Entity Relationships

```
Language (1) ──── has ────► ResourceEntry (many)
Language (1) ──── has ────► FontProfile (1)
Language (1) ──── has ────► CultureProfile (1)
ResourceEntry (many) ── belongs to ──► Language (1)
```

---

## State Transitions

### Language Switch Flow

```
Current Language (LTR/English)
  │
  ▼
User selects Arabic
  │
  ├──► LocalizationService.SetLanguage("ar-SA")
  │       │
  │       ├──► Load .resx resources for ar-SA
  │       ├──► Apply CultureProfile (number/date/currency formatting)
  │       ├──► Apply FontProfile (switch fonts)
  │       └──► Apply FlowDirection (RTL)
  │
  ▼
New Language (RTL/Arabic)
```

---

## Key Design Decisions

- **Culture profiles are NOT stored** — they are created on-the-fly from the .NET CultureInfo for the selected language. This ensures formatting always matches the OS locale definition.
- **Font fallback is mandatory** — even in Arabic mode, data values may contain Latin characters (measurement units, technical codes). The font fallback chain ensures these render correctly.
- **FlowDirection is per-language** — each Language entity has an explicit FlowDirection. Currently English is LTR and Arabic is RTL. This design enables future RTL languages (Hebrew, Persian, Urdu) without changes.
