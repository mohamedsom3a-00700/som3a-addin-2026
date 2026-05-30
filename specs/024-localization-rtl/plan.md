# Implementation Plan: Localization & RTL

**Branch**: `025-localization-rtl` | **Date**: 2026-05-29 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/024-localization-rtl/spec.md`

## Summary

Implement full Arabic + English localization with dynamic RTL switching, Arabic typography presets, and culture-aware formatting for the Som3a WPF Shell workspace. Shell/Settings/Dashboard/error messages must be 100% translated before shipping; plugin UI may use English fallback.

## Technical Context

**Language/Version**: C# ΓÇö .NET Framework 4.8 for WPF/VSTO host, .NET 8.0 for Som3a.Localization library

**Primary Dependencies**: Som3a.Localization (existing .resx-based project), LocalizationService, CultureManager, RTLHelper

**Storage**: .resx resource files (English default, Arabic translations), language preference persisted in Properties.Settings.Default

**Testing**: Manual visual verification (language switch, RTL mirroring, Arabic typography), automated UI tests for FlowDirection/culture formatting, Excel VSTO host validation in both languages

**Target Platform**: Windows x64 ΓÇö Excel VSTO Add-in host (.NET Framework 4.8)

**Project Type**: Desktop application (WPF VSTO Add-in)

**Performance Goals**: Language switch completes in under 1s, RTL layout mirroring completes in under 500ms, font switch completes in under 200ms

**Constraints**: No restart required for language switch, DynamicResource-only for themeable properties, Excel rendering safety (Constitution ┬ºX), RTL applies to Shell only (Excel ribbon/interop remains LTR)

**Scale/Scope**: 2 languages (English + Arabic), 20+ static UI areas translated, all future new UI must include both English and Arabic resource entries

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** ΓÇö Localization resources are organized in Som3a.Localization project with isolated .resx files per language. No monolithic dictionaries.
- [x] **III. DynamicResource-Only** ΓÇö Localization uses string resources, not theme brushes. No StaticResource usage for themeable properties.
- [x] **IV. Runtime Theme Mutation Governance** ΓÇö Localization does not mutate themes. Language switching flows through LocalizationService, not ThemeManager.
- [x] **IX. Animation Governance** ΓÇö Language/RTL switch animations (if any) will be Γëñ200ms, GPU-safe, no layout thrashing.
- [x] **X. Excel Rendering Safety** ΓÇö RTL scoped to Shell workspace only. Excel ribbon and VSTO interop controls remain LTR to prevent Excel stability issues. WindowRenderModeDetector fallback path preserved.
- [x] **XI. WindowChrome Enforcement** ΓÇö No new windows introduced. All localization occurs within existing Shell pages and ModernWindow instances.
- [x] **XII. Centralized Effects** ΓÇö No new effects introduced by this feature.
- [x] **XV. Resource Loading Order** ΓÇö No new resource dictionaries introduced. .resx files loaded via standard .NET resource manager, not WPF MergedDictionaries.

## Project Structure

### Documentation (this feature)

```text
specs/024-localization-rtl/
Γö£ΓöÇΓöÇ plan.md              # This file
Γö£ΓöÇΓöÇ research.md          # Phase 0 ΓÇö key decisions
Γö£ΓöÇΓöÇ data-model.md        # Phase 1 ΓÇö entities and contracts
Γö£ΓöÇΓöÇ quickstart.md        # Phase 1 ΓÇö developer guide
Γö£ΓöÇΓöÇ contracts/           # Phase 1 ΓÇö interface definitions
Γö£ΓöÇΓöÇ checklists/          # Spec quality artifacts
ΓööΓöÇΓöÇ tasks.md             # Created by /speckit.tasks
```

### Source Code (repository root)

```text
Som3a.Localization/                              # .NET 8.0 ΓÇö existing project
Γö£ΓöÇΓöÇ Resources/
Γöé   Γö£ΓöÇΓöÇ Strings.en-US.resx                       # English resources
Γöé   ΓööΓöÇΓöÇ Strings.ar-SA.resx                       # Arabic resources
Γö£ΓöÇΓöÇ Services/
Γöé   Γö£ΓöÇΓöÇ LocalizationService.cs                   # Existing ΓÇö extend for dynamic switching
Γöé   ΓööΓöÇΓöÇ CultureManager.cs                        # Existing ΓÇö extend for RTL sync
ΓööΓöÇΓöÇ RTL/
    ΓööΓöÇΓöÇ RTLHelper.cs                             # Existing ΓÇö extend for full Shell mirroring

WpfApp2/                                         # .NET Framework 4.8 ΓÇö VSTO host
Γö£ΓöÇΓöÇ Services/
Γöé   ΓööΓöÇΓöÇ LocalizationBridgeService.cs             # NEW ΓÇö bridges .NET 8.0 localization to WPF
Γö£ΓöÇΓöÇ Theme/
Γöé   ΓööΓöÇΓöÇ Fonts/
Γöé       Γö£ΓöÇΓöÇ ArabicFonts.xaml                     # NEW ΓÇö Arabic font family resource dict
Γöé       ΓööΓöÇΓöÇ FontFallback.xaml                    # NEW ΓÇö Arabic->Latin fallchain
Γö£ΓöÇΓöÇ Pages/
Γöé   ΓööΓöÇΓöÇ Settings/
Γöé       ΓööΓöÇΓöÇ LanguagePage.xaml                    # NEW ΓÇö language picker + font selection
Γö£ΓöÇΓöÇ Converters/
Γöé   ΓööΓöÇΓöÇ CultureAwareFormattingConverter.cs       # NEW ΓÇö number/date/currency formatting
ΓööΓöÇΓöÇ Controls/
    ΓööΓöÇΓöÇ Shell/
        ΓööΓöÇΓöÇ ShellRTLManager.cs                  # NEW ΓÇö manages FlowDirection per Shell window
```

**Structure Decision**: The existing WPF host projects remain unchanged for their core layout. New files are added to existing `Som3a.Localization` and `WpfApp2` directories without restructuring. This is the simplest approach ΓÇö no new projects, no architectural disruption.

## Complexity Tracking

> No constitution violations to justify.
