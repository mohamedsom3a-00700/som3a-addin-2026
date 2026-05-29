# Research: Localization & RTL

## Decision Record

### 1. RTL Scope: Shell-Only

- **Decision**: RTL layout applies to WPF Shell workspace only. Excel ribbon and VSTO interop controls remain LTR.
- **Rationale**: Forcing RTL on Excel ribbon/interop risks breaking Excel's own layout and introduces VSTO stability issues. The WPF Shell is fully under our control. Constitution §X (Excel Rendering Safety) requires us to prevent Excel host instability.
- **Alternatives considered**:
  - Full RTL (Shell + Excel ribbon): Rejected due to Excel VSTO rendering safety concerns. Excel's own ribbon does not natively support per-add-in RTL mirroring.
  - Shell RTL + right-aligned Excel ribbon: Rejected as it creates a confusing mixed state where items are right-aligned but text is LTR.

### 2. Translation Completeness Bar

- **Decision**: Shell, Settings, Dashboard, and error messages must be 100% translated before shipping. Plugin UI may use English fallback.
- **Rationale**: Requiring 100% across all plugins creates hard dependency on every plugin owner and delays the feature. A two-tier approach delivers immediate value while plugins adopt translations on their own schedule.
- **Alternatives considered**:
  - 100% all-or-nothing: Rejected as it blocks shipping on plugin localization readiness.
  - Minimal coverage (language picker only): Rejected as insufficient for Arabic-speaking users.

### 3. Resource Format: .resx

- **Decision**: Use standard .NET .resx resource files organized by language code (Strings.en-US.resx, Strings.ar-SA.resx).
- **Rationale**: .resx is the native .NET localization format, supported by the existing Som3a.Localization project structure, and provides compile-time type safety via generated designer files. It integrates directly with the existing LocalizationService.
- **Alternatives considered**:
  - JSON resource files: Rejected — requires custom resource manager, no compile-time safety, and diverges from the established Som3a.Localization project pattern.
  - Database-stored translations: Rejected — adds deployment complexity for a desktop VSTO add-in. Better suited for Phase 27 (Persistence & Database).

### 4. Arabic Font Strategy

- **Decision**: Bundle Cairo font as the primary Arabic font with Segoe UI as the Latin fallback. IBM Plex Sans Arabic as an optional alternative in font settings.
- **Rationale**: Cairo is an open-source Arabic font with excellent glyph shaping, legibility at multiple sizes, and permissive licensing for redistribution. Segoe UI is pre-installed on Windows and covers the Latin character set.
- **Alternatives considered**:
  - System fonts only: Rejected — not all Windows installs include quality Arabic fonts.
  - Noto Naskh Arabic: Rejected — less suitable for UI (designed for long-form text, not labels/buttons).

### 5. Number Formatting Approach

- **Decision**: Use .NET CultureInfo formatting (CultureInfo.CreateSpecificContext("ar-SA")) for automatic number, date, and currency formatting. No custom digit conversion.
- **Rationale**: .NET's built-in culture support handles Arabic-Indic digits, date formats, and currency symbols correctly for ar-SA. This avoids reinventing locale formatting and ensures correctness.
- **Alternatives considered**:
  - Custom digit mapping: Rejected — error-prone and duplicates .NET framework capabilities.
  - Configurable digit preference (Arabic-Indic vs Western): Deferred — can be added later if users request it.

### 6. Missing Translation Diagnostics

- **Decision**: Log missing translation key lookups to the existing diagnostics system. No special UI indicator in v1.
- **Rationale**: Missing translations will display English fallback (via the resource manager's built-in fallback mechanism). Logging allows developers to identify gaps during testing without cluttering the UI.
- **Alternatives considered**:
  - Developer overlay highlighting untranslated strings: Deferred — useful but adds complexity for v1.
