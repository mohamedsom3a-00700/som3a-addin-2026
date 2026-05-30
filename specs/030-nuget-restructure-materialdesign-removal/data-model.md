# Data Model: NuGet Restructure & MaterialDesign Removal

**Date**: 2026-05-30
**Feature**: NuGet Restructure & MaterialDesign Removal (Phase 1C)

## Overview

No new data entities are introduced in this phase. The feature is strictly a dependency cleanup and compilation fix. All existing data models remain unchanged.

## Existing Entities (Unchanged)

The following entities are referenced during migration for token substitution but are not modified:

- **Theme Resource Catalog** (`WpfApp2/Theme/`)
  - Primitive tokens (colors, spacing, radius, typography)
  - Semantic tokens (brushes, borders, backgrounds)
  - Effects (shadows, glow, animations)
  - Control styles (buttons, combo boxes, data grids, etc.)

- **SettingsRecord** (`Som3a.Infrastructure/Persistence/Models/`)
  - Theme mode, accent color, and other persisted settings remain stored in SQLite.

- **Dependency Registry** (NuGet package manifests in `.csproj` files)
  - Captures the before/after state of third-party library references.

## Data Flow (Unchanged)

- Theme switching via `ThemeManager` → `MergedDictionaries` update → `{DynamicResource}` reevaluation
- Settings persistence via `SettingsService` → SQLite `SettingsRecord` table
- Excel interop via `Som3a.Bridge` named pipe → VSTO add-in command execution

## Validation Rules (Unchanged)

- All themeable properties MUST resolve to a valid `{DynamicResource}` key.
- Missing resource keys MUST fall back to neutral theme-safe colors (Constitution §XVI).
- No hardcoded brushes or inline colors (Constitution §III).

## State Transitions (N/A)

No entity state transitions occur in this phase.
