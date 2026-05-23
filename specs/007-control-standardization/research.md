# Research: Control Standardization — Design Decisions

**Date**: 2026-05-22
**Feature**: Control Standardization (Phase 4)
**Spec**: [spec.md](./spec.md)

## Overview

This document captures research findings and design decisions for the Control Standardization phase. All NEEDS CLARIFICATION markers from the spec have been resolved during the `/speckit.clarify` phase. This research focuses on best practices for WPF control standardization in an Excel VSTO hosting context.

---

## Decision 1: ComboBox Popup Architecture

**Decision**: Extend existing Phase 3 pattern (`AllowsTransparency="False"`, `Placement="Bottom"`, `PlacementTarget` binding) with smart direction detection and safe-mode shadow variants.

**Rationale**: The Phase 3 ComboBoxStyles.xaml already establishes the correct pattern. The remaining issues (width mismatch, clipping at window edges, inconsistent shadow in safe mode) are edge cases not fully addressed in the initial implementation. Extending, not replacing, the existing pattern minimizes regression risk.

**Alternatives considered**:
- Custom Popup control wrapper — rejected due to increased complexity and deviation from established pattern
- WinForms ComboBox interop — rejected due to visual inconsistency and Constitution §XIV (native WPF only)

---

## Decision 2: TextBox Standardization Approach

**Decision**: Extend existing TextBoxStyles.xaml with complete VSM states (Normal, MouseOver, Focused, Disabled) using accent border color transitions.

**Rationale**: Phase 3 created TextBoxStyles.xaml but may have incomplete VSM coverage. The standardized pattern from ButtonStyles.xaml (VSM states with DynamicResource brushes) serves as the template.

**Alternatives considered**:
- Replace all TextBox controls with a custom derived TextBox — rejected as over-engineering; WPF's built-in TextBox with proper styling meets all requirements
- Keep existing partial styling — rejected; inconsistency with other standardized controls

---

## Decision 3: Virtualization Strategy

**Decision**: Apply `VirtualizationMode="Recycling"` and `EnableRowVirtualization="True"` to all scrollable containers (DataGrid, ListView, TreeView, ScrollViewer). Remove any explicit `ScrollUnit="Pixel"` that conflicts with row virtualization.

**Rationale**: Container recycling is the standard WPF virtualization mode and is already partially applied to DataGrid (Phase 3). Extending to ListView and TreeView ensures consistent memory usage across all list controls. Row virtualization is essential for the 1000+ row DataGrid requirement (SC-003).

**Alternatives considered**:
- `VirtualizationMode="Standard"` — rejected; creates more UI elements than needed, impacting memory
- Custom virtualizing panel — rejected; WPF's built-in virtualization is sufficient for 1000-10000 rows

---

## Decision 4: Empty and Loading State Placeholder

**Decision**: Use a shared `ContentControl` style with a trigger on item count for "No data available" text, and a simple progress indicator for loading state, applied via implicit styles on DataGrid, ListView, and TreeView.

**Rationale**: The template placeholder approach (using WPF's `CollectionViewSource` or `HasItems` trigger) is the standard WPF pattern for empty state display without requiring code changes in each window. A single shared style ensures consistency.

**Alternatives considered**:
- Per-window empty state handling — rejected; inconsistent and duplicates effort
- Third-party placeholder library — rejected; Constitution §XIV prohibits third-party UI frameworks

---

## Decision 5: Style Audit and Deduplication

**Decision**: Manual grep-based audit of all `.xaml` files for `TargetType="..."` duplicates. Each duplicate flagged for manual review per clarification Q4. No automated removal.

**Rationale**: Automated removal risks breaking window-specific styling that may have intentional variations. Manual review ensures each duplicate is properly evaluated.

**Search patterns**:
- `TargetType="Button"`, `TargetType="TextBox"`, etc. — count occurrences
- Inline `<Style TargetType="...">` inside window XAML files — flag for migration to centralized styles
- `BasedOn="{StaticResource ...}"` — verify chain consistency

---

## Decision 6: Keyboard Navigation Pattern

**Decision**: Follow the Phase 3 pattern from CheckBox/RadioButton/ToggleButton styles. Apply `FocusVisualStyle="{DynamicResource Glow.Focus}"` universally. Ensure `IsTabStop` and `TabIndex` are correctly configured per control.

**Rationale**: Phase 3 already established the keyboard navigation pattern for simple controls (T045-T047). This phase extends it to TreeView, ListView, DataGrid, and dialogs. The Glow.Focus resource is already defined and themed.

**Alternatives considered**:
- Custom focus visual style per control type — rejected; Glow.Focus provides consistent appearance across all controls as required by User Story 4
- No focus visual changes — rejected; fails SC-004 and accessibility requirements

---

## Decision 7: Scrollbar Standardization

**Decision**: Apply the existing `ScrollViewerStyles.xaml` thin scrollbar style (4px track, 8px thumb) to all scrollable containers via implicit style targeting `ScrollViewer`.

**Rationale**: Phase 3 created the thin scrollbar style. Applying it via implicit `TargetType="ScrollViewer"` ensures all scrollable controls inherit it automatically without per-control configuration.

**Alternatives considered**:
- Per-control scrollbar styling — rejected; inconsistent, duplicates effort
- Default WPF scrollbar — rejected; too thick for modern Windows 11 aesthetic

---

## Decision 8: PasswordBox Standardization

**Decision**: Create `Theme/Controls/PasswordBoxStyles.xaml` following the same VSM pattern as TextBoxStyles.xaml, with the addition of a "Reveal password" toggle button.

**Rationale**: PasswordBox is listed as a control to standardize but currently lacks a dedicated style file. Using the TextBox VSM pattern ensures visual consistency while adding the reveal toggle as a usability improvement.

**Alternatives considered**:
- Style PasswordBox via implicit TextBox style — rejected; PasswordBox is a separate control type with different template structure
- Skip PasswordBox — rejected; it's explicitly listed in the spec scope

---

## Decision 9: Dialog Standardization

**Decision**: Use WPF's `Window` with `ShowDialog()` pattern, applying a consistent style via `Theme/Controls/WindowStyles.xaml` that includes modal overlay behavior, consistent title bar, and Escape-to-close binding.

**Rationale**: WPF dialogs are already modal by default. The standardization focuses on visual consistency (title bar styling, button placement) and keyboard behavior (Escape closes). No architectural change to the dialog system is needed.

**Alternatives considered**:
- Custom DialogService with ViewModel pattern — deferred; belongs in Phase 6 (MVVM cleanup) where DI and service infrastructure are introduced
- MessageBox replacement — out of scope; MessageBox is a system dialog not stylable via WPF theming

---

## Decision 10: Error Logging for Style Load Failures

**Decision**: Use `System.Diagnostics.Trace` or `log4net` (if already present) for logging style resolution failures. No custom logging infrastructure in this phase.

**Rationale**: Per clarification Q2 and Q3, detailed diagnostics are deferred to Phase 8. Minimal error logging uses whatever logging mechanism already exists in the project (check for existing logger).

**Alternatives considered**:
- Custom event source — rejected; over-engineering for this phase
- Silent failure — rejected; FR-013 requires logging
