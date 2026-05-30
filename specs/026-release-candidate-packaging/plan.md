# Implementation Plan: Release Candidate & Production Packaging

**Branch**: `feature/phase-26-release-candidate` | **Date**: 2026-05-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/026-release-candidate-packaging/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Prepare the Planova Platform for production release by implementing a release pipeline (validation → optimization → packaging → installer → QA), final validation suites (UI, plugins, AI, Excel), performance optimization (DataGrid virtualization, COM cleanup, lazy loading), MSI installer packaging with code signing, documentation export, and crash diagnostics. All existing Phase 14–25 features are expected to be stable before this phase begins.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for VSTO host, .NET 8.0 for domain/AI libraries)

**Primary Dependencies**: MSBuild (solution build), WiX Toolset v4 (MSI packaging), signtool.exe / mage.exe (code signing), Som3a.Bridge (interop between .NET 4.8 and .NET 8.0), existing Theme Engine, EventBus, ServiceContainer, ModuleRegistry

**Storage**: File system for MSI artifacts, signed binaries, configuration files, and exported documentation. No database required.

**Testing**: Automated validation suites covering UI, plugins, AI, and Excel. Manual QA for installer and documentation. Performance benchmarking for export speed and memory.

**Target Platform**: Windows 10/11 x64 — Excel 2019 / Microsoft 365 — VSTO Add-in host

**Project Type**: Desktop Excel VSTO Add-in (existing) — release packaging and validation phase

**Performance Goals**: Release pipeline completes in under 30 minutes. Excel export of 10,000-row workbook completes in under 10 seconds. AI provider fallback activates within 5 seconds. UI startup within 1 second.

**Constraints**: No new feature development. No cloud deployment. No namespace migration (deferred post-stabilization per Phase 25). Must preserve all existing VSTO rendering stability. All performance optimizations must not break existing functionality.

**Scale/Scope**: 1 release candidate build. 4 validation suites (UI, plugins, AI, Excel). 6 documentation guides. 1 MSI installer. Production configuration for logging (error-level, 100MB cap) and AI provider endpoints.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — N/A. Phase 26 introduces no new resource dictionaries or UI libraries; it is a release packaging phase.
- [x] **III. DynamicResource-Only** — N/A. No new UI resources introduced.
- [x] **IV. Runtime Theme Mutation Governance** — N/A. No theme changes introduced.
- [x] **IX. Animation Governance** — N/A. No animations introduced.
- [x] **X. Excel Rendering Safety** — Phase 26 includes an Excel validation suite that explicitly verifies rendering safety and interop cleanup. No UI changes that could affect rendering.
- [x] **XI. WindowChrome Enforcement** — N/A. No new windows introduced.
- [x] **XII. Centralized Effects** — N/A. No new effects introduced.
- [x] **XV. Resource Loading Order** — N/A. No new resource dictionaries introduced.

**Gate evaluation**: All checks pass or are N/A. Phase 26 is a release packaging and validation phase, not a UI/feature development phase. No constitutional violations exist.

## Project Structure

### Documentation (this feature)

```text
specs/026-release-candidate-packaging/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Skipped — no external interfaces for this phase
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
WpfApp2/                              # Existing VSTO + WPF host (.NET Framework 4.8)
├── Services/                         # Existing services (add validation pipeline service)
├── Scripts/                          # NEW — build and validation scripts
│   ├── validate-ui.ps1
│   ├── validate-plugins.ps1
│   ├── validate-ai.ps1
│   └── validate-excel.ps1
├── Setup/                            # NEW — WiX installer project
│   ├── Product.wxs                   # MSI definition with VSTO registration
│   ├── Bundle.wxs                    # Burn bootstrapper for .NET 8 runtime
│   └── Resources/                    # Installer assets (icons, banners)

Som3a.Domain/                         # Existing — domain layer (unchanged)
Som3a.Contracts/                      # Existing — contract interfaces (unchanged)
Som3a.AI/                             # Existing — AI abstraction (validation scripts)
Som3a.Exporting/                      # Existing — export engine (performance optimization)
Som3a.Plugin.SDK/                     # Existing — plugin framework (validation scripts)
Som3a.Localization/                   # Existing — i18n (validation scripts)
Som3a.Validation/                     # Existing — validation engine (extend for export validation)
Som3a.Diagnostics/                    # Existing — diagnostics (crash recovery + safe logging)
Som3a.Infrastructure/                 # Existing — security/config (production configs)
```

**Structure Decision**: All Phase 26 work lives within the existing project structure. No new .NET projects required. New items are build scripts (`Scripts/`), WiX installer project (`Setup/`), and modifications to existing projects for performance optimization and crash diagnostics.

## Complexity Tracking

> No violations to justify. All constitution checks pass or are N/A.
