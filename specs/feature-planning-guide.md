# ChatGPT Feature Planning Guide — Som3a Add-in 2026

**Purpose**: This document is a full prompt/guide you can give to ChatGPT (or any AI) to plan new features for the Som3a Add-in 2026 project. It contains all the context, conventions, templates, and workflow steps needed to produce production-ready plans.

---

## How to Use This Guide

Copy the entire document (or relevant sections) into a ChatGPT conversation when starting a new feature. Fill in the feature-specific details at the top.

---

## 1. Feature Kickoff — Copy This Block

```text
I need to plan a new feature for the Som3a Add-in 2026 project. Here are the details:

**Feature Name**: [e.g., "Advanced Reporting Engine"]
**Feature Description**: [e.g., "Users can generate custom PDF reports from Excel data with selectable templates"]
**Priority**: [P1 (blocking), P2 (important), P3 (nice to have)]
**User Input/Requirements**: [Paste any user descriptions, tickets, or notes]
**Affected Components**: [e.g., WpfApp2, Som3a.Shared, Excel Ribbon]
```

---

## 2. Project Context (for AI Understanding)

### 2.1. What We Build

A professional Excel VSTO Add-in built with C#, .NET Framework 4.8, and WPF. It extends Microsoft Excel with custom ribbon UI elements and integrated WPF dialogs for enhanced user productivity. Includes database connectivity (Oracle, SQL Server), WebView2 integration, and MVVM architecture.

### 2.2. Solution Structure

```text
Som3a Addin 2026/
├── Som3a Addin 2026/          # Main VSTO Excel Add-in project
│   ├── Ui/                    # WPF UI components
│   ├── Properties/            # Assembly and project properties
│   ├── Resources/             # Icons and images
│   └── Ribbon1.cs             # Custom ribbon implementation
├── Som3a.Shared/              # Shared code and utilities
├── WpfApp2/                   # WPF UI library (main UI work happens here)
│   ├── Theme/                 # Fluent Design system (if theme engine is done)
│   ├── Controls/              # Custom controls (ModernWindow, LoadingOverlay, Toast)
│   ├── Services/              # Services (ThemeManager, DialogService, ToastService)
│   ├── ViewModels/            # ViewModels (MVVM)
│   ├── Views/                 # XAML windows
│   ├── Helpers/               # Utility helpers
│   ├── Converters/            # Value converters
│   ├── Behaviors/             # Attached behaviors
│   ├── Models/                # ViewModels (legacy naming)
│   └── Properties/            # Settings
├── WpfApp1/                   # Legacy WPF UI library (older project)
├── Som3a_WPF_UI/              # Another WPF UI project
├── Som3a_WPF_UId/             # Another WPF UI project
└── Docs/                      # Documentation
```

### 2.3. Technology Stack

| Technology | Detail |
|-----------|--------|
| Framework | .NET Framework 4.8 |
| Language | C# 8.0+ |
| UI | WPF (Windows Presentation Foundation) |
| Office Integration | VSTO (Visual Studio Tools for Office) |
| Database | Oracle Managed Data Access, System.Data.SqlClient |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.4.2 |
| Web | Microsoft.Web.WebView2 |
| UI Styling | WPF ResourceDictionaries with DynamicResource |
| Persistence | Properties.Settings.Default (ApplicationSettingsBase) |

### 2.4. Architecture Principles (The "Constitution")

| # | Principle | Description |
|---|-----------|-------------|
| I | Library-First Modular Architecture | UI components organized by function in folders; ResourceDictionaries independently testable |
| II | MVVM Architecture | Business logic in ViewModels; minimal code-behind; services are not ViewModels |
| III | DynamicResource Only | All theme colors use `{DynamicResource Brush.*}` — no StaticResource for themeable properties |
| IV | Runtime Theme Switching | ThemeManager replaces merged dictionaries dynamically; preferences persist across sessions |
| V | Feature Completeness Standard | All controls theme-aware, runtime switchable, keyboard accessible, DPI safe |
| VI | Performance & Efficiency | Brushes reused globally, no nested DropShadows, virtualized DataGrid rows, animations ≤200ms |
| VII | Resource Loading Order | Base tokens → Semantic tokens → Typography → Radius → Effects → Controls → Theme overrides |
| VIII | Primitive & Semantic Token Architecture | Two-tier separation; Semantic tokens alias primitives |
| IX | Popup Architecture Rules | AllowsTransparency=False, correct Placement/PlacementTarget, centralized shadow |
| X | WindowChrome Enforcement | WindowChrome preferred with automatic runtime fallback detection for Excel edge cases |
| XI | No Third-Party UI Frameworks | All UI work uses native WPF with custom styling |
| XII | No Inline Color Values | All color values defined as tokens; control templates reference via DynamicResource |

### 2.5. Key Services (if already implemented)

- **ThemeManager**: Singleton service for runtime theme switching (ApplyTheme, LoadThemeFromSettings, SaveCurrentTheme)
- **WindowRenderModeDetector**: Auto-detects Excel hosting rendering issues, activates FallbackSafe mode
- **DialogService**: WPF dialog management
- **ToastService**: Toast notification system
- **ThemeSettings**: Legacy JSON persistence (secondary; primary is Properties.Settings.Default)

### 2.6. Build Command

```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

### 2.7. Testing Approach

- Visual/manual testing only (WPF UI, no automated UI testing framework)
- Test inside Excel VSTO host for VSTO-specific behaviors
- Test standalone for rapid iteration

---

## 3. Planning Workflow (6 Phases)

Each new feature goes through exactly 6 phases. The output documents live in `specs/<feature-id>/`.

### Phase 0: Research

**Goal**: Investigate unknowns, document decisions, and validate feasibility before writing specs.

**Output**: `specs/<feature-id>/research.md`

**Template**:

```markdown
# Research: [Feature Name]

**Feature**: [Feature Name]
**Date**: [YYYY-MM-DD]

## Research Topics

### RT-001: [Topic Title]

**Decision**: [What we decided]

**Rationale**: [Why we chose this approach]

**Alternatives considered**:
- [Alternative 1]: [Why rejected]
- [Alternative 2]: [Why rejected]
```

**Guidelines**:
- One section per research topic (RT-001, RT-002, etc.)
- Cover: technology choices, architecture approaches, library evaluation, performance considerations
- Lead with the decision, then justify with rationale, then briefly note alternatives
- Research at least 3-5 topics per feature
- If the feature is well-understood (no unknowns), this phase can be minimal or skipped

### Phase 1: Spec (Feature Specification)

**Goal**: Define what the feature does from a user/business perspective. No implementation details.

**Output**: `specs/<feature-id>/spec.md`

**Structure**:

```markdown
# Feature Specification: [Feature Name]

**Feature Branch**: `[feature-id]`
**Created**: [YYYY-MM-DD]
**Status**: Draft
**Input**: User description: "[quote]"

## Clarifications
Questions and answers from the user/stakeholder about the feature.

## User Stories & Testing
### User Story N - [Title] (Priority: P1/P2/P3)
User scenario description. Why this priority. Independent test description.

**Acceptance Scenarios**:
1. **Given** [context], **When** [action], **Then** [expected result].
2. ...

## Edge Cases
- [Edge case 1]
- [Edge case 2]

## Requirements
### Functional Requirements
- **FR-001**: The system MUST [requirement description].
- **FR-NNN**: ...

### Key Entities
- **Entity**: Description, attributes, behavior.

## Success Criteria
Measurable outcomes (SC-001, SC-002, ...).

## Assumptions
9-12 documented assumptions about platform, users, performance, constraints.

## Specification Quality Checklist
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous
- [ ] Success criteria are measurable
- [ ] All acceptance scenarios are defined
- [ ] Edge cases are identified
- [ ] Scope is clearly bounded
```

**Rules**:
- Use "MUST" language in requirements (FR-001: The system MUST ...)
- Use Given/When/Then format for acceptance scenarios
- No code, no XAML, no API names, no tech stack references in user-facing sections
- Every user story has: scenario, priority (P1/P2/P3), "why this priority", "independent test"
- Every requirement maps to at least one success criterion
- At least 3-5 user stories covering primary flows

### Phase 2: Plan (Implementation Plan)

**Goal**: Define how to build the feature — structure, phases, tasks, dependencies.

**Output**: `specs/<feature-id>/plan.md`

**Template**:

```markdown
# Implementation Plan: [Feature Name]

**Branch**: `[feature-id]` | **Date**: [YYYY-MM-DD] | **Spec**: [spec.md](./spec.md)

## Summary
One paragraph describing the implementation approach.

## Technical Context
**Language/Version**: ...
**Primary Dependencies**: ...
**Storage**: ...
**Testing**: ...
**Target Platform**: ...
**Performance Goals**: ...
**Constraints**: ...

## Constitution Check
Table with each principle + ✅ PASS or ❌ FAIL + notes.

## Project Structure
Directories and files to create/modify, with descriptions.

## Complexity Tracking
Note any constitution violations requiring justification.

## Phases (as needed)
```

### Phase 3: Data Model & Contracts

**Goal**: Define entities, relationships, validation rules, and service contracts.

**Outputs**:
- `specs/<feature-id>/data-model.md` — Entities, fields, relationships, state machines
- `specs/<feature-id>/contracts/<contract-name>.md` — Service interfaces, method signatures, behaviors

### Phase 4: Tasks

**Goal**: Break work into individual, assignable tasks with dependencies.

**Output**: `specs/<feature-id>/tasks.md`

**Format**:

```text
- [ ] T001 [P] Description (file paths, notes)
- [ ] T002 [US1] Description
```

- `[P]` = parallelizable (different files, no dependencies)
- `[US1]`, `[US2]` = user story label
- Include exact file paths in descriptions
- Group by phases, with checkpoints between phases
- Add dependency graph at the end

### Phase 5: Quickstart

**Goal**: Enable a developer to start implementing immediately.

**Output**: `specs/<feature-id>/quickstart.md`

Contains: file structure, priority-ordered file list, token reference, testing checklist, common issues.

---

## 4. Document Naming Convention

All feature documents live under `specs/<feature-id>/`:

```text
specs/<feature-id>/
├── spec.md              # Phase 1: Feature specification
├── plan.md              # Phase 2: Implementation plan
├── research.md          # Phase 0: Research output
├── data-model.md        # Phase 3: Data model
├── quickstart.md        # Phase 5: Quickstart guide
├── contracts/           # Phase 3: Service contracts
│   └── theme-api.md     # Example contract file
├── tasks.md             # Phase 4: Task list
└── checklists/          # Quality checklists
    └── requirements.md  # Specification quality checklist
```

**Feature ID format**: `NNN-feature-name` (zero-padded 3-digit number + kebab-case name)
- Example: `001-fluent-theme-engine`
- Example: `002-advanced-reporting`

---

## 5. Writing Conventions

### General
- Use markdown throughout
- Use `[x]` for completed items, `[ ]` for pending
- Use tables for structured data (entities, fields, comparisons)
- Use code blocks (```) for file paths, contracts, examples
- Include line references to source code when relevant (filename.cs:123)

### Spec spec.md Rules
- Requirements use "The system MUST" language
- No implementation details in user-facing sections
- Given/When/Then for acceptance scenarios
- Every user story has: scenario, priority, "why this priority", "independent test"
- Success criteria are measurable (quantitative thresholds)

### Plan plan.md Rules
- Include a Constitution Check table (all principles must ✅ PASS)
- Document the proposed file structure (directories + files)
- Include Technical Context section (dependencies, constraints, performance goals)
- Add Complexity Tracking for any deviations

### Tasks tasks.md Rules
- Tasks use `TNNN` IDs (sequential, zero-padded)
- `[P]` tag = parallelizable (independent files)
- `[US#]` tag = user story mapping
- Group by implementation phases
- Include checkpoints at end of each phase
- Document dependency graph
- Show task summary table at end

---

## 6. Quality Checklists

### Specification Quality Checklist (spec.md)
- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous
- [ ] Success criteria are measurable
- [ ] Success criteria are technology-agnostic (no implementation details)
- [ ] All acceptance scenarios are defined
- [ ] Edge cases are identified
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified

### Constitution Check (plan.md)
Each principle must pass before proceeding:

| Principle | Status |
|-----------|--------|
| Library-First Modular Architecture | ✅ PASS / ❌ FAIL |
| MVVM Architecture | ✅ PASS / ❌ FAIL |
| DynamicResource Only | ✅ PASS / ❌ FAIL |
| Runtime Theme Switching | ✅ PASS / ❌ FAIL |
| Feature Completeness Standard | ✅ PASS / ❌ FAIL |
| Performance & Efficiency | ✅ PASS / ❌ FAIL |
| Resource Loading Order | ✅ PASS / ❌ FAIL |
| Primitive & Semantic Token Architecture | ✅ PASS / ❌ FAIL |
| Popup Architecture Rules | ✅ PASS / ❌ FAIL |
| WindowChrome Enforcement | ✅ PASS / ❌ FAIL |
| No Third-Party UI Frameworks | ✅ PASS / ❌ FAIL |
| No Inline Color Values | ✅ PASS / ❌ FAIL |

### Task Quality Checklist
- [ ] Tasks grouped by user story
- [ ] Dependencies documented
- [ ] Parallel tasks identified with [P] tag
- [ ] File paths included in descriptions
- [ ] Checkpoints between phases
- [ ] No missing tasks (every spec requirement mapped to at least one task)

---

## 7. Example: Complete Feature Plan Structure

Here's what a complete feature planning session produces:

```text
specs/002-new-feature/
├── research.md           # Phase 0: Tech research & decisions (3-5 topics)
├── spec.md               # Phase 1: User-facing spec (5 user stories, 10+ FRs)
├── plan.md               # Phase 2: Implementation plan (structure, phases, constitution check)
├── data-model.md         # Phase 3: Entities, validation, state machines
├── contracts/
│   └── service-api.md    # Phase 3: Service contract (public interface, behaviors, events)
├── tasks.md              # Phase 4: Task list (grouped by phase, dependencies)
└── quickstart.md         # Phase 5: Developer onboarding (files, tokens, checklist)
```

Each phase builds on the previous. Don't skip phases.

---

## 8. Example Pattern: User Story Writing

```markdown
### User Story 1 - Theme Selection via Settings Window (Priority: P1)

A user opens the Settings window from within Excel and switches the application theme from Dark to Light.
The UI updates instantly across all open windows without restarting Excel.

**Why this priority**: Theme selection is the primary user-facing feature. It directly demonstrates the
runtime theme switching capability and delivers immediate visual customization value.

**Independent Test**: Can be fully tested by opening SettingsWindow, clicking a theme card, observing
instant UI updates across all windows, restarting the app, and confirming the theme persisted.

**Acceptance Scenarios**:
1. **Given** the app is running with Dark theme, **When** the user clicks the "Light" theme card,
   **Then** all open windows immediately switch to Light theme colors without visual glitches.
2. **Given** the user has selected Light theme and closed the app, **When** the user reopens the app,
   **Then** Light theme is automatically applied on startup.
```

## 9. Example Pattern: Requirement Writing

```markdown
### Functional Requirements
- **FR-001**: The system MUST provide three built-in themes: Dark, Light, and Custom.
- **FR-002**: The system MUST allow users to switch themes at runtime without restarting the application.
- **FR-003**: The system MUST persist the selected theme preference across application restarts.
```

## 10. Example Pattern: Success Criteria

```markdown
### Measurable Outcomes
- **SC-001**: Users can switch between themes in under 1 second with no visible flicker.
- **SC-002**: Theme preference persists across application restarts with 100% reliability.
- **SC-003**: All open windows update simultaneously to the new theme within 500ms.
```

---

## 11. Common Pitfalls to Avoid

| Pitfall | How to Avoid |
|---------|-------------|
| Leaking implementation into spec.md | Keep spec.md purely user-facing. No C#, no XAML, no API names. |
| Writing tasks without user story context | Tag every task with its user story: `[US1]`, `[US2]` |
| Skipping research for "obvious" choices | Always document at least the decision + rationale. |
| Having too few user stories | Every major feature needs 3-5 user stories covering primary flows. |
| Requirements too vague | Use "MUST" + measurable terms. Avoid "should", "could", "might". |
| No edge cases | Always list 3-5 edge cases. Consider: error states, boundary conditions, rapid user actions. |
| Missing success criteria | Every requirement should map to at least one measurable success criterion. |
| Assuming testing framework | Document testing approach explicitly (visual/manual, no automated UI tests). |

---
