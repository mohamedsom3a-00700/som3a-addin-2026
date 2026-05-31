# Implementation Plan: i18n Consolidation & Language Support

**Branch**: `033-i18n-consolidation` | **Date**: 2026-05-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/033-i18n-consolidation/spec.md`

## Summary

Consolidate two parallel localization systems into `Som3a.Localization` (`ILocalizationService`), complete all Arabic translations (1800+ keys), fix RTL/FlowDirection rendering, migrate font handling to a unified model, and create a consolidated Language & Font settings page. The shell language toggle moves from a 🌐 emoji to a Fluent 2 icon wired to `ILocalizationService`.

## Technical Context

**Language/Version**: .NET 8.0 (WpfApp2), .NET Framework 4.8 (Som3a.Shared — COM interop only, no localization work)

**Primary Dependencies**: WPF native, `Som3a.Localization` library (Ph14), `Som3a.Bridge` (.NET Standard 2.0 interop). No third-party localization frameworks.

**Storage**: RESX embedded resources in `Som3a.Localization/Resources/` (migrated from `WpfApp2/Resources/`). Language preference persisted via `Properties.Settings.Default` (migrated from plain text file at `%APPDATA%/Som3a/language-preference.txt`).

**Testing**: `dotnet test Tests/Som3a_WPF_UI.Tests.csproj` (`Som3a_WPF_UI.Tests` project). Existing tests are in MSTest. New localization service tests should target `Som3a.Localization` library directly.

**Target Platform**: Windows 10/11 — Excel VSTO Add-in host (WPF in win32 parent). All rendering changes must pass VSTO smoke test.

**Project Type**: Desktop application (WPF VSTO Add-in)

**Performance Goals**: Language switch <500ms from toggle to full UI update (per spec clarification). No loading spinner; async update.

**Constraints**: 
- Excel VSTO rendering safety (WindowRenderModeDetector)
- DynamicResource-only for all themeable properties (Constitution §III)
- Animations ≤200ms (Constitution §IX)
- 495+ XAML binding sites must be migrated from `TranslationSource.Instance` pattern
- `StringsArabic.resx` has corrupted UTF-8 encoding — must be re-generated

**Scale/Scope**: 1800+ string keys, 2 locales (en-US, ar-SA), ~495 XAML binding sites, 2 services to consolidate (`LocalizationBridgeService` + `Som3a.Localization`), 1 settings page to create, 1 shell toggle to update.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Localization resources consolidate into `Som3a.Localization` library; resources remain modular by locale (en-US, ar-SA .resx files). No monolithic dictionary.
- [x] **III. DynamicResource-Only** — Feature deals with localization bindings, not theme bindings. Any new XAML (LanguagePage, shell toggle) will use DynamicResource for themeable properties. StaticResource not introduced.
- [x] **IV. Runtime Theme Mutation Governance** — Language/RTL switching is orthogonal to theme switching. Theme mutation remains exclusively through ThemeManager.
- [x] **IX. Animation Governance** — No new animations introduced. Language switch is an async data refresh, not an animated transition.
- [x] **X. Excel Rendering Safety** — RTL/FlowDirection changes affect window layout; WindowRenderModeDetector fallback mode must be verified after RTL changes. Spec assumptions document VSTO compatibility requirement.
- [x] **XI. WindowChrome Enforcement** — No new windows. LanguagePage is a sub-page within existing Settings flow; inherits ModernWindow via parent.
- [x] **xII. Centralized Effects** — No new effects introduced. Existing effects sourced from centralized dictionaries.
- [x] **XV. Resource Loading Order** — New RESX files loaded through existing `Som3a.Localization` infrastructure; no theme dictionary loading order impact.

**All gates pass — no violations.**

## Project Structure

### Documentation (this feature)

```text
specs/033-i18n-consolidation/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 — existing-state analysis
├── data-model.md        # Phase 1 — entity definitions
├── quickstart.md        # Phase 1 — developer onboarding
├── contracts/           # Phase 1 — ILocalizationService contract
└── tasks.md             # Phase 2 — (created by /speckit.tasks)
```

### Source Code (repository root)

```text
Som3a.Localization/
├── Contracts/
│   └── ILocalizationService.cs        # Existing — extend if needed
├── Services/
│   └── LocalizationService.cs         # Existing — wire into WPF app
├── Resources/
│   ├── Strings.en-US.resx             # Primary English strings (migrate from WpfApp2/Resources/Strings.resx)
│   └── Strings.ar-SA.resx             # Arabic strings (migrate + complete ~740 missing translations)

WpfApp2/
├── Services/
│   ├── LocalizationBridgeService.cs   # REMOVE after migration
│   ├── TranslationSource.cs           # REMOVE after migration
│   ├── ArabicFontManager.cs           # INTEGRATE into consolidated font service
│   ├── CultureAwareFormattingService.cs # RETAIN (culture formatting is separate concern)
│   └── ShellRTLManager.cs             # RETAIN — consolidate RTL logic
├── Controls/Shell/
│   ├── ShellWindow.xaml               # Update language toggle (🌐 → Fluent 2 icon)
│   └── ShellRTLManager.cs             # RETAIN with updates
├── Pages/Settings/
│   └── LanguagePage.xaml              # NEW — consolidated language + font settings page
├── ViewModels/Settings/
│   └── LanguagePageViewModel.cs       # NEW — viewmodel for language page
├── Resources/
│   ├── Strings.resx                   # REMOVE after migration to Som3a.Localization
│   └── StringsArabic.resx             # REMOVE after migration
└── App.xaml.cs                        # Update language init/event handlers

Som3a.Bridge/
└── InteropContracts.cs                # No changes (existing bridge API stable)

Tests/
├── Som3a_WPF_UI.Tests/
│   └── Services/
│       └── LocalizationServiceTests.cs  # NEW — integration tests
└── Som3a.Localization.Tests/           # NEW test project if needed
    └── LocalizationServiceTests.cs
```

## Phase 0 — Outline & Research

**No NEEDS CLARIFICATION markers exist** — all spec ambiguities resolved during `/speckit.clarify`. Research phase documents existing-state analysis.

**Research tasks** (output in `research.md`):
1. Document current dual-system architecture (active `LocalizationBridgeService` + dormant `Som3a.Localization`)
2. Map all affected files (495 XAML bindings, 2 RESX sets, 4+ services)
3. Identify migration sequence (pre-work → service swap → binding migration → page creation)
4. Note encoding corruption in `StringsArabic.resx`
5. Document RTL implementation gaps (imperative only, no XAML-declarative FlowDirection)

## Phase 1 — Design & Contracts

### Data Model

Entities defined in `data-model.md`. Core entities:
- `LocalizationResource` (key, en-US value, ar-SA value, area grouping)
- `LanguageSetting` (selected culture code, persistence via Properties.Settings)
- `FontMapping` (per-locale font family, font size scaling)
- `LanguageChangedEvent` (event bus event for cross-service coordination)

### Contracts

Interface contracts documented in `contracts/`:
- `ILocalizationService` — the single service contract for all localization consumers
- XAML binding contract — how views consume localization (direct `ILocalizationService` binding, not `TranslationSource`)

### Agent Context

Updated `AGENTS.md` to reference `specs/033-i18n-consolidation/plan.md` as the active plan.

## Complexity Tracking

*No Constitution Check violations — no complexity justification needed.*
