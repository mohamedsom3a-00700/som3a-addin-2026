# Implementation Plan: CommunityToolkit.Mvvm ViewModel Refactor

**Branch**: `[031-ctk-mvvm-refactor]` | **Date**: 2026-05-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/031-ctk-mvvm-refactor/spec.md`

## Summary

Replace all manual `INotifyPropertyChanged` implementations and custom `RelayCommand` classes with CommunityToolkit.Mvvm source-generated `[ObservableProperty]` and `[RelayCommand]` across all 20+ ViewModels. Perform migration in incremental batches of 5-7 ViewModels with build validation after each. Preserve custom setter side effects via `OnPropertyChanged` / `OnPropertyChanging` partial method hooks. Remove obsolete manual command helper classes (`Models/RelayCommand.cs`, `Helpers/AsyncRelayCommand.cs`). Enforce zero-regression via automated verification script that scans for forbidden manual patterns.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows

**Primary Dependencies**: CommunityToolkit.Mvvm (already installed)

**Storage**: N/A (no data persistence changes)

**Testing**: MSBuild compilation check + existing VSTO smoke test protocol

**Target Platform**: WPF desktop application hosted inside Excel VSTO

**Project Type**: Desktop application (WPF VSTO add-in host)

**Performance Goals**: Zero change to runtime performance; compile-time generation may marginally improve startup by reducing reflection

**Constraints**: Must not break existing XAML bindings; must preserve all existing ViewModel behavior; must compile successfully after each batch

**Scale/Scope**: ~20+ ViewModel classes, 2 obsolete helper files to delete, 1 verification script to create

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — No new dictionaries introduced; refactor is purely C# code. Pass.
- [x] **III. DynamicResource-Only** — No XAML changes in this phase. Pass.
- [x] **IV. Runtime Theme Mutation Governance** — No theme changes in this phase. Pass.
- [x] **IX. Animation Governance** — No animation changes in this phase. Pass.
- [x] **X. Excel Rendering Safety** — No window or rendering changes in this phase. Pass.
- [x] **XI. WindowChrome Enforcement** — No window changes in this phase. Pass.
- [x] **XII. Centralized Effects** — No effect changes in this phase. Pass.
- [x] **XV. Resource Loading Order** — No resource dictionary changes in this phase. Pass.

**Gate Result**: ALL CHECKS PASS. No complexity tracking required.

## Project Structure

### Documentation (this feature)

```text
specs/031-ctk-mvvm-refactor/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
WpfApp2/
├── ViewModels/
│   ├── ViewModelBase.cs                    # Change to inherit ObservableObject
│   ├── HomeViewModel.cs                    # [ObservableProperty] + [RelayCommand]
│   ├── SettingsViewModel.cs                # [ObservableProperty] + [RelayCommand]
│   ├── BOQActivityGeneratorViewModel.cs    # [ObservableProperty] + [RelayCommand]
│   ├── DurationEstimatorPageViewModel.cs   # [ObservableProperty] + [RelayCommand]
│   ├── WBSEditorViewModel.cs               # [ObservableProperty] + [RelayCommand]
│   ├── WBSGeneratorViewModel.cs            # [ObservableProperty] + [RelayCommand]
│   ├── RelationshipGeneratorViewModel.cs   # [ObservableProperty] + [RelayCommand]
│   ├── Dashboard/                          # All widget ViewModels
│   ├── Primavera/                          # All Primavera ViewModels
│   ├── DiagnosticsViewModel.cs             # [ObservableProperty] + [RelayCommand]
│   ├── LanguagePageViewModel.cs            # [ObservableProperty] + [RelayCommand]
│   ├── ShellViewModel.cs                   # [ObservableProperty] + [RelayCommand]
│   ├── CommandPaletteViewModel.cs          # [ObservableProperty] + [RelayCommand]
│   ├── ToastViewModel.cs                   # [ObservableProperty] + [RelayCommand]
│   └── ... (remaining ViewModels)
├── Models/
│   └── RelayCommand.cs                     # DELETE
├── Helpers/
│   └── AsyncRelayCommand.cs                # DELETE
└── Scripts/
    └── Verify-NoManualMvvmPatterns.ps1     # NEW: CI verification script
```

**Structure Decision**: This is a pure refactor within the existing `WpfApp2/ViewModels/` directory. No new project structure is introduced. The only new artifact is a PowerShell verification script under `Scripts/`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations. All constitutional checks pass.
