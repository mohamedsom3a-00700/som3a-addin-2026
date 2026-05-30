# Implementation Plan: Out-of-Process Architecture

**Branch**: `fluent/phase-1b` | **Date**: 2026-05-30 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/029-out-of-process-architecture/spec.md`

## Summary

Phase 1B converts the WPF UI from in-process (hosted by VSTO) to an out-of-process standalone .NET 8 application that communicates with the VSTO add-in via the existing Som3a.Bridge named pipe. The implementation covers: cold start protocol (VSTO launches WPF process, pipe handshake within 5s with 60s retry), Excel crash watchdog (heartbeat every 10s, shutdown after 3 missed + 10s grace), window ownership (WPF ShellWindow as child of Excel HWND), Excel command protocol (JSON pipe messages for WPF→VSTO commands), and a performance baseline of <5ms per message.

## Technical Context

**Language/Version**: C# (.NET 8.0 for WpfApp2, .NET Framework 4.8 for VSTO add-in in Som3a.Shared)

**Primary Dependencies**: Som3a.Bridge (.NET Standard 2.0, existing), System.IO.Pipes (built-in), System.Runtime.InteropServices (built-in), Newtonsoft.Json or System.Text.Json (existing in project)

**Storage**: N/A — no data persistence in this phase

**Testing**: `dotnet test Tests/Som3a_WPF_UI.Tests.csproj` for unit tests; manual VSTO smoke test (ribbon → shell opens → navigate pages → theme switch → Excel cell write → close Excel → verify WPF auto-shutdown and no orphan processes)

**Target Platform**: Windows (Excel VSTO host, x64)

**Project Type**: Desktop application — WPF .NET 8.0 frontend + VSTO COM add-in (.NET Framework 4.8)

**Performance Goals**: <5ms average per named pipe request-response cycle; 100 sequential messages in <500ms total

**Constraints**: WPF must operate as a standalone process; ShellWindow must be owned by Excel HWND for correct modal behavior; WPF must auto-shutdown within 15s of normal Excel close, within 45s of Excel crash; zero orphan processes after Excel exits

**Scale/Scope**: Single WPF process per Excel instance (1:1 relationship); single-user desktop scenario

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — N/A: Phase 1B introduces no resource dictionaries or UI libraries; the pipe bridge (Som3a.Bridge) is already modular.
- [x] **III. DynamicResource-Only** — N/A: No new themeable UI resources introduced.
- [x] **IV. Runtime Theme Mutation Governance** — N/A: No theme mutation changes.
- [x] **IX. Animation Governance** — N/A: No new animations.
- [x] **X. Excel Rendering Safety** — Window ownership via HWND (`WindowInteropHelper.Owner = excelHwnd`) is a standard WPF pattern compatible with WindowChrome. ShellWindow already inherits ModernWindow. No rendering pipeline changes — the same ShellWindow is used, only its launch context changes from in-process to out-of-process. No black window risk.
- [x] **XI. WindowChrome Enforcement** — ShellWindow already inherits ModernWindow. HWND-based ownership does not conflict with WindowChrome; `WindowInteropHelper` operates at the Win32 handle level, below WindowChrome. Verified compatible.
- [x] **XII. Centralized Effects** — N/A: No effects introduced.
- [x] **XV. Resource Loading Order** — N/A: No new resource dictionaries.

## Project Structure

### Documentation (this feature)

```text
specs/029-out-of-process-architecture/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — resolved decisions
├── data-model.md        # Phase 1 output — pipe message schema
├── quickstart.md        # Phase 1 output — developer guide
├── contracts/           # Phase 1 output — JSON message contracts
└── tasks.md             # (created by /speckit.tasks)
```

### Source Code (repository root)

```text
Som3a.Bridge/
├── PipeMessageSchema.cs        # NEW — base message types, enums
├── ExcelCommandProtocol.cs     # NEW — command constants, serialization

WpfApp2/
├── App.xaml.cs                 # MODIFY — standalone launch routing
├── Services/
│   ├── PipeClientService.cs    # NEW — named pipe client, connect/retry/disconnect
│   └── CrashWatchdogService.cs # NEW — heartbeat subscriber, shutdown trigger
├── Controls/Shell/
│   └── ShellWindow.xaml.cs     # MODIFY — accept Excel HWND, call WindowInteropHelper

Som3a.Shared/
├── ThisAddIn.cs                # MODIFY — cold start: Process.Start + pipe handshake
├── Som3aAddinBridge.cs         # MODIFY — Excel command handler routing
```

## Complexity Tracking

> No constitutional violations to justify. Phase 1B is a clean architectural change that does not conflict with any constitutional principles.
