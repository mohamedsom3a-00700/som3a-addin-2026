# Implementation Plan: Localization & RTL

**Branch**: `025-localization-rtl` | **Date**: 2026-05-29 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/024-localization-rtl/spec.md`

## Summary

Implement full Arabic + English localization with dynamic RTL switching, Arabic typography presets, and culture-aware formatting for the Som3a WPF Shell workspace. Shell/Settings/Dashboard/error messages must be 100% translated before shipping; plugin UI may use English fallback.

## Technical Context

**Language/Version**: C# — .NET Framework 4.8 for WPF/VSTO host, .NET 8.0 for Som3a.Localization library

**Primary Dependencies**: Som3a.Localization (existing .resx-based project), LocalizationService, CultureManager, RTLHelper

**Storage**: .resx resource files (English default, Arabic translations), language preference persisted in Properties.Settings.Default

**Testing**: Manual visual verification (language switch, RTL mirroring, Arabic typography), automated UI tests for FlowDirection/culture formatting, Excel VSTO host validation in both languages

**Target Platform**: Windows x64 — Excel VSTO Add-in host (.NET Framework 4.8)

**Project Type**: Desktop application (WPF VSTO Add-in)

**Performance Goals**: Language switch completes in under 1s, RTL layout mirroring completes in under 500ms, font switch completes in under 200ms

**Constraints**: No restart required for language switch, DynamicResource-only for themeable properties, Excel rendering safety (Constitution §X), RTL applies to Shell only (Excel ribbon/interop remains LTR)

**Scale/Scope**: 2 languages (English + Arabic), 20+ static UI areas translated, all future new UI must include both English and Arabic resource entries

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Localization resources are organized in Som3a.Localization project with isolated .resx files per language. No monolithic dictionaries.
- [x] **III. DynamicResource-Only** — Localization uses string resources, not theme brushes. No StaticResource usage for themeable properties.
- [x] **IV. Runtime Theme Mutation Governance** — Localization does not mutate themes. Language switching flows through LocalizationService, not ThemeManager.
- [x] **IX. Animation Governance** — Language/RTL switch animations (if any) will be ≤200ms, GPU-safe, no layout thrashing.
- [x] **X. Excel Rendering Safety** — RTL scoped to Shell workspace only. Excel ribbon and VSTO interop controls remain LTR to prevent Excel stability issues. WindowRenderModeDetector fallback path preserved.
- [x] **XI. WindowChrome Enforcement** — No new windows introduced. All localization occurs within existing Shell pages and ModernWindow instances.
- [x] **XII. Centralized Effects** — No new effects introduced by this feature.
- [x] **XV. Resource Loading Order** — No new resource dictionaries introduced. .resx files loaded via standard .NET resource manager, not WPF MergedDictionaries.

## Project Structure

### Documentation (this feature)

```text
specs/024-localization-rtl/
├── plan.md              # This file
├── research.md          # Phase 0 — key decisions
├── data-model.md        # Phase 1 — entities and contracts
├── quickstart.md        # Phase 1 — developer guide
├── contracts/           # Phase 1 — interface definitions
├── checklists/          # Spec quality artifacts
└── tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
Som3a.Localization/                              # .NET 8.0 — existing project
├── Resources/
│   ├── Strings.en-US.resx                       # English resources
│   └── Strings.ar-SA.resx                       # Arabic resources
├── Services/
│   ├── LocalizationService.cs                   # Existing — extend for dynamic switching
│   └── CultureManager.cs                        # Existing — extend for RTL sync
└── RTL/
    └── RTLHelper.cs                             # Existing — extend for full Shell mirroring

WpfApp2/                                         # .NET Framework 4.8 — VSTO host
├── Services/
│   └── LocalizationBridgeService.cs             # NEW — bridges .NET 8.0 localization to WPF
├── Theme/
│   └── Fonts/
│       ├── ArabicFonts.xaml                     # NEW — Arabic font family resource dict
│       └── FontFallback.xaml                    # NEW — Arabic->Latin fallchain
├── Pages/
│   └── Settings/
│       └── LanguagePage.xaml                    # NEW — language picker + font selection
├── Converters/
│   └── CultureAwareFormattingConverter.cs       # NEW — number/date/currency formatting
└── Controls/
    └── Shell/
        └── ShellRTLManager.cs                  # NEW — manages FlowDirection per Shell window
```

**Structure Decision**: The existing WPF host projects remain unchanged for their core layout. New files are added to existing `Som3a.Localization` and `WpfApp2` directories without restructuring. This is the simplest approach — no new projects, no architectural disruption.

## Complexity Tracking

> No constitution violations to justify.
