# Som3a Add-in 2026 — Enterprise UI Transformation Master Plan

**Version**: 1.0.0  
**Date**: 2026-05-21  
**Status**: Approved — awaiting execution  
**Governed by**: `.specify/memory/constitution-v2.md` (to be created in Phase 0)  
**Project**: Som3a Add-in 2026 - UI Foundation & Theme Engine  
**Platform**: Excel VSTO Add-in (.NET Framework 4.8, WPF)  

---

## Executive Summary

Transform the current Excel VSTO Add-in into a **Windows 11 Fluent Enterprise Platform** optimized for:

- Excel VSTO hosting
- Long-running enterprise workflows
- Modular future expansion
- Runtime theming
- Unified navigation
- Stability-first rendering
- Enterprise-grade UX consistency

**Priority**: Stability → Performance → Visual Polish

---

## Core Strategic Decisions

| Area | Decision |
|------|----------|
| Platform | Excel VSTO only |
| Framework | .NET Framework 4.8 |
| UI Direction | Windows 11 Native Feeling |
| Architecture | Full MVVM + Modular |
| Theme System | Full Token-Based Design System |
| Navigation | Unified Shell Architecture (new windows only) |
| Future Readiness | Plugin-ready |
| Settings Storage | Local only |
| Accessibility | Keyboard navigation + screen reader preparation |
| Localization | Architecture-ready for future localization |
| Rendering Strategy | Hybrid Excel-safe rendering |
| Governance | Mandatory review after every phase |
| Git Strategy | One branch per phase |
| QA Strategy | GitHub + CodeRabbit mandatory review gates |

---

## Git Workflow Strategy

### Branch Model

```text
main
  ↑
develop
  ↑
feature/phase-NN-name
```

### Merge Rules

**NO direct merge to main**

```text
feature branch
    ↓
GitHub PR
    ↓
CodeRabbit Review
    ↓
Manual Review
    ↓
develop
    ↓
integration validation
    ↓
main
```

### Branch Naming

```text
feature/phase-00-governance-foundation
feature/phase-01-design-system-core
feature/phase-02-rendering-infrastructure
feature/phase-03-theme-engine-v2
feature/phase-04-control-standardization
feature/phase-05-navigation-shell
feature/phase-06-mvvm-cleanup
feature/phase-07-settings-ux
feature/phase-08-diagnostics
feature/phase-09-plugin-platform
feature/phase-10-enterprise-polish
feature/phase-11-legacy-migration
```

---

## Mandatory Pre-Execution Validation

Before ANY phase implementation begins, AI agents MUST review:

1. All existing Theme files
2. Current WPF architecture
3. Existing Design Tokens
4. Current ThemeManager implementation
5. Current Popup/Shadow implementation
6. ResourceDictionary loading order
7. Existing Spec Kit structure
8. Existing OpenCode plans
9. Existing architecture constitution
10. Detect reusable components before creating new ones

**Status**: Phase 0 pre-review COMPLETED on 2026-05-21.

### Key Findings from Pre-Review

| Finding | Impact | Phase Addressed |
|---------|--------|---------------|
| Constitution has active Git merge conflicts | BLOCKING | P0-T000 |
| Inline colors in ButtonStyles, DataGridStyles, MainWindow | Violation | P1-T006 |
| Hardcoded radius in ComboBoxStyles | Violation | P1-T006 |
| Significant existing implementation overlap | Reuse opportunity | P0, P1, P2 |
| ModernWindow, ThemeManager, WindowRenderModeDetector functional | Extend, don't replace | P2 |
| All 3 themes (Dark/Light/Custom) exist | Refactor tokens | P1 |
| SettingsWindow has theme cards and accent swatches | Extend UX | P7 |
| Missing Elevation, Motion, ZIndex, Opacity tokens | Gap-fill | P1 |
| No Docs/Architecture/ folder | Create | P0 |
| No DI container, event bus, module registry | Create | P6 |

---

## Mandatory AI Execution Rules

### BEFORE creating ANY new token, control, dictionary, or architecture:

1. Inspect current implementation
2. Reuse existing structures
3. Extend existing systems
4. Avoid duplicate architecture

### NEVER:

- Inline colors
- Inline shadows
- Inline margins
- Inline font sizes
- Direct control styling
- Duplicate styles
- Hardcoded spacing
- Business logic in code-behind
- Direct service creation in views

### ALWAYS:

- DynamicResource for themeable properties
- Semantic tokens
- MVVM separation
- Centralized effects (Effects/*.xaml)
- Excel-safe rendering
- Accessibility-ready controls
- Review before merge

---

## Merge Gate Requirements

A phase CANNOT continue unless:

| Requirement | Mandatory |
|-------------|-----------|
| Build passes | YES |
| Excel host test passes | YES |
| PR approved | YES |
| CodeRabbit clean | YES |
| Architecture review passed | YES |
| No rendering regressions | YES |
| No popup regressions | YES |
| No theme regressions | YES |

---

## Mandatory Review Gate After Every Phase

Each phase MUST STOP after completion. Then:

### 1. Local Manual Testing

- Excel host testing
- DPI testing (100%, 125%, 150%)
- Popup testing
- Rendering testing

### 2. GitHub Pull Request Review

- Architecture review
- Style review
- Performance review

### 3. CodeRabbit Review

- Code smells
- Performance issues
- MVVM violations
- Memory issues
- Resource leaks
- Duplicate logic
- Inline values

### 4. Manual Architectural Review

- Token usage consistency
- Naming consistency
- DynamicResource usage
- No inline colors
- No inline shadows
- No duplicate styles

---

# PHASE 0 — GOVERNANCE FOUNDATION

## Goal

Establish all architectural governance before implementation begins. Resolve blocking issues.

## Branch

`feature/phase-00-governance-foundation`

---

## P0-T000: Resolve Constitution Merge Conflicts (BLOCKING)

**Priority**: Critical  
**File**: `.specify/memory/constitution.md`

The constitution has active Git merge conflicts (`<<<<<<< HEAD`, `=======`, `>>>>>>>`) across ~30 lines. No governance document can reference a conflicted constitution.

**Steps:**
1. Identify conflicting sections (duplicated principle blocks with merge markers)
2. Reconcile into single coherent constitution v1.2.0
3. Remove duplicate principle blocks
4. Preserve all 16 principles (I–XVI) in final order
5. Update version header: `1.2.0 | Ratified: 2026-05-21`
6. Verify no merge markers remain

**Acceptance Criteria:**
- `grep -c "<<<<<<<" .specify/memory/constitution.md` returns 0
- All 16 principles present exactly once
- Version line reads `1.2.0`

---

## P0-T001: Review Existing Theme Architecture

**Priority**: High  
**Deliverable**: `Docs/Architecture/AUDIT_REPORT.md`

**Scope:**
- Map all existing theme files against Master Plan architecture
- Identify inline colors, inline shadows, hardcoded spacing, hardcoded radius
- Catalog DynamicResource vs StaticResource usage
- Identify duplicate or missing tokens

**Key findings from pre-review:**

| Violation | Location |
|-----------|----------|
| Inline color `#2FFFFFFF` | `ButtonStyles.xaml` BaseButton Background |
| Inline color `#3FFFFFFF` | `ButtonStyles.xaml` hover Background |
| Inline color `#22FFFFFF` | `ButtonStyles.xaml` pressed Background |
| Inline color `#0AFFFFFF` | `DataGridStyles.xaml` AlternatingRowBackground |
| Inline color `#15FFFFFF` | `DataGridStyles.xaml` row hover |
| Inline color `#333A86FF` | `DataGridStyles.xaml` row selected |
| Inline color `#22000000` | `MainWindow.xaml` progress background |
| Hardcoded `CornerRadius="10"` | `ComboBoxStyles.xaml` |
| Hardcoded margins/sizes | `ModernWindow.xaml`, `MainWindow.xaml` |

---

## P0-T002: Review Current ThemeManager

**Priority**: High  
**Deliverable**: Service audit report in `AUDIT_REPORT.md`

**Findings:**
- `ThemeManager.cs` already has singleton, debounce timer, accent application, persistence, events
- `ThemeSettings.cs` is a stub (no serialization implemented)
- `WindowRenderModeDetector.cs` has VSTO detection, DPI detection, render test
- All three services are functional and align with architecture goals

**Gaps:**
- No accent variant generation (hover, pressed, glow, border, subtle)
- No runtime validation of resource integrity
- No fallback recovery beyond try/catch

---

## P0-T003: Review Current Popup System

**Priority**: High  
**Deliverable**: Popup audit report in `AUDIT_REPORT.md`

**Findings:**
- `ComboBoxStyles.xaml` uses `AllowsTransparency="False"` (compliant)
- Uses `Placement="Bottom"` (compliant)
- `PlacementTarget` bound correctly (compliant)
- Uses centralized `Effect="{DynamicResource Shadow.Popup}"` (compliant)

**Gaps:**
- `ComboBoxItemStyles.xaml` needs full review for inline issues
- No popup diagnostics or clipping detection

---

## P0-T004: Review Current Shadows

**Priority**: High  
**Deliverable**: Effects audit report in `AUDIT_REPORT.md`

**Findings:**
- `Shadows.xaml` has 7 centralized DropShadowEffect definitions (compliant)
- `Glow.xaml` has 6 glow effects using `AccentColorValue` (compliant)
- No inline DropShadowEffect in reviewed files (compliant)

**Gaps:**
- No `Elevation.xaml` (elevation tokens per Master Plan)
- No safe-mode shadow variants for FallbackSafe rendering

---

## P0-T005: Create Constitution v2

**Priority**: High  
**File**: `.specify/memory/constitution-v2.md`

**Content:**
- Import reconciled v1.2.0 as baseline
- Add new sections from Master Plan:
  - AI Execution Rules (reuse-first mandate)
  - Branch Naming Standards
  - Token Naming Standards (formalize `Primitive.*`, `Brush.*`, `Spacing.*`, `Radius.*`, `Elevation.*`, `Motion.*`, `Component.*`)
  - Review Gate Requirements (build, Excel host, PR, CodeRabbit, architecture)
  - Phase 11: Legacy Window Migration

---

## P0-T006: Create Governance Documentation

**Priority**: High

| File | Purpose |
|------|---------|
| `Docs/Architecture/AGENT_RULES.md` | AI execution rules: inspect before creating, reuse before duplicating |
| `Docs/Architecture/UI_GUIDELINES.md` | Windows 11 Fluent design rules, token usage, DynamicResource mandate |
| `Docs/Architecture/PERFORMANCE_RULES.md` | ≤200ms animations, virtualization, no nested shadows, no BlurEffect on scrollables |
| `Docs/Architecture/TOKEN_RULES.md` | Naming conventions, layer rules, primitive vs semantic vs component |
| `Docs/Architecture/SHADOW_SYSTEM.md` | Centralized effects, no inline, glow dynamic behavior |
| `Docs/Architecture/POPUP_ARCHITECTURE.md` | AllowsTransparency rules, Placement rules, Excel-safe popup guidelines |
| `Docs/Architecture/EXCEL_RENDERING_RULES.md` | Safe mode detection, fallback behavior, DPI handling |
| `Docs/Architecture/MVVM_RULES.md` | Service/ViewModel separation, no code-behind business logic |
| `Docs/Architecture/ACCESSIBILITY_RULES.md` | Keyboard nav, focus visibility, screen reader prep |
| `Docs/Architecture/REVIEW_CHECKLIST.md` | Mandatory gates per phase (local test, GitHub PR, CodeRabbit, architecture review) |

**Note:** Markdown versions created now. Word/PDF conversion requires Pandoc or manual conversion after stabilization.

---

## P0-T007: Create Review Workflow

**Priority**: High  
**File**: `Docs/Architecture/REVIEW_CHECKLIST.md`

**Mandatory gates per phase:**
1. Local Manual Testing (Excel host, DPI, popup, rendering)
2. GitHub PR Review (architecture, style, performance)
3. CodeRabbit Review (smells, performance, MVVM, memory, leaks, duplicates)
4. Manual Architectural Review (tokens, DynamicResource, no inline colors/shadows)

---

## P0-T008: Create AI Execution Rules

**Priority**: High  
**File**: Included in `Docs/Architecture/AGENT_RULES.md`

**Key rules:**
- Before creating ANY new token/control/dictionary: inspect current implementation
- Reuse existing structures; extend existing systems
- Avoid duplicate architecture
- No inline colors, inline shadows, inline margins, inline font sizes
- MUST use DynamicResource, semantic tokens, centralized effects, shared control templates

---

## P0-T009: Create Token Naming Standards

**Priority**: High  
**File**: Included in `Docs/Architecture/TOKEN_RULES.md`

**Layers:**
```text
Primitive Tokens     → Primitive.<Color>.<Shade>     (e.g., Primitive.Blue.500)
Semantic Tokens      → Brush.<Category>.<State>      (e.g., Brush.Background.Primary)
Spacing Tokens       → Spacing.<Name>                (e.g., Spacing.Medium)
Radius Tokens        → Radius.<Size>                 (e.g., Radius.Medium)
Elevation Tokens     → Elevation.<Level>             (e.g., Elevation.Card)
Motion Tokens        → Motion.<Type>.<Property>      (e.g., Motion.Fade.Duration)
Component Tokens     → Component.<Control>.<Part>.<State>
```

---

## P0-T010: Create Branch Naming Standards

**Priority**: High  
**File**: Included in `Docs/Architecture/AGENT_RULES.md`

**Pattern:** `feature/phase-<NN>-<short-name>`

Examples:
- `feature/phase-00-governance-foundation`
- `feature/phase-01-design-system-core`
- `feature/phase-02-rendering-infrastructure`

---

## P0-T011: Update AGENTS.md

**Priority**: Medium  
**File**: `AGENTS.md`

**Update with:**
- Phase 0–2 file paths
- Build command (`msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug`)
- Spec Kit references
- Constitution v2 reference

---

## Phase 0 Acceptance Criteria

- [ ] Constitution merge conflicts resolved (P0-T000)
- [ ] Audit report created (P0-T001 through P0-T004)
- [ ] Constitution v2 created (P0-T005)
- [ ] All governance documents created (P0-T006)
- [ ] Review checklist created (P0-T007)
- [ ] AI execution rules documented (P0-T008)
- [ ] Token naming standards documented (P0-T009)
- [ ] Branch naming standards documented (P0-T010)
- [ ] AGENTS.md updated (P0-T011)
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean
- [ ] Manual architecture review passed

---

# PHASE 1 — DESIGN SYSTEM CORE

## Goal

Convert current styling into enterprise token architecture. Fix all inline violations found in P0-T001.

## Branch

`feature/phase-01-design-system-core`

## Architecture

```text
Primitive Tokens
    ↓
Semantic Tokens
    ↓
Component Tokens
    ↓
Control Templates
```

---

## P1-T001: Review All Existing Tokens

**Priority**: High  
**Deliverable**: Token inventory in `AUDIT_REPORT.md`

**Scope:**
- `Theme/Base/Colors.xaml` (primitive + semantic)
- `Theme/Base/Typography.xaml`
- `Theme/Base/Spacing.xaml`
- `Theme/Base/Radius.xaml`

**Inventory includes:**
- Current key names
- Type (primitive/semantic/legacy-flat)
- Used by which control styles
- Missing semantic equivalents for legacy flat keys

---

## P1-T002: Detect Duplicate Colors

**Priority**: High  
**Deliverable**: Deduplication report

**Method:**
- Regex search all `.xaml` for duplicate color hex values
- Identify semantic tokens that resolve to same color but have different names
- Identify legacy flat keys that shadow semantic tokens

**Key duplicates found in pre-review:**
- `#0E1720` appears in Colors.xaml, DarkColors.xaml, CustomColors.xaml, MainWindow.xaml
- `#33FFFFFF` appears in multiple stroke/fill tokens

---

## P1-T003: Create Primitive Token System

**Priority**: High  
**File**: `Theme/Base/Colors.xaml` — refactor primitive section

**Strategy:** Add new `Primitive.*` `Color` resources. Alias existing flat keys (`Blue500`, `Slate900`) to them. **No breaking changes.**

```xaml
<!-- New primitives -->
<Color x:Key="Primitive.Blue.500">#3A86FF</Color>
<Color x:Key="Primitive.Slate.900">#0E1720</Color>
<Color x:Key="Primitive.Slate.800">#1C2B3A</Color>
<Color x:Key="Primitive.Slate.700">#15202B</Color>
<Color x:Key="Primitive.White.95">#F2FFFFFF</Color>
<Color x:Key="Primitive.White.75">#BFFFFFFF</Color>
<Color x:Key="Primitive.White.40">#66FFFFFF</Color>
<Color x:Key="Primitive.White.20">#33FFFFFF</Color>
<Color x:Key="Primitive.White.10">#1AFFFFFF</Color>
<Color x:Key="Primitive.White.5">#0DFFFFFF</Color>
<Color x:Key="Primitive.Green.500">#2ED573</Color>
<Color x:Key="Primitive.Orange.500">#FFA502</Color>
<Color x:Key="Primitive.Red.500">#FF4757</Color>
<Color x:Key="Primitive.Blue.600">#1E90FF</Color>

<!-- Existing flat key preserved as alias -->
<SolidColorBrush x:Key="Blue500" Color="{StaticResource Primitive.Blue.500}"/>
<SolidColorBrush x:Key="Slate900" Color="{StaticResource Primitive.Slate.900}"/>
```

---

## P1-T004: Create Semantic Token System

**Priority**: High  
**File**: `Theme/Base/Colors.xaml` — refactor semantic section

**Strategy:**
1. Ensure all semantic `Brush.*` tokens reference primitive colors (not hardcoded hex)
2. Add missing semantic tokens for inline colors found in audit

```xaml
<!-- Background -->
<SolidColorBrush x:Key="Brush.Background.Primary" Color="{StaticResource Primitive.Slate.900}"/>
<SolidColorBrush x:Key="Brush.Background.Secondary" Color="{StaticResource Primitive.Slate.800}"/>
<SolidColorBrush x:Key="Brush.Background.Card" Color="{StaticResource Primitive.Slate.700}"/>

<!-- Text -->
<SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource Primitive.White.95}"/>
<SolidColorBrush x:Key="Brush.Text.Secondary" Color="{StaticResource Primitive.White.75}"/>
<SolidColorBrush x:Key="Brush.Text.Disabled" Color="{StaticResource Primitive.White.40}"/>

<!-- Accent -->
<SolidColorBrush x:Key="Brush.Accent.Primary" Color="{StaticResource Primitive.Blue.500}"/>

<!-- New tokens for inline color replacement -->
<SolidColorBrush x:Key="Brush.Button.Background" Color="#2FFFFFFF"/>
<SolidColorBrush x:Key="Brush.Button.HoverBackground" Color="#3FFFFFFF"/>
<SolidColorBrush x:Key="Brush.Button.PressedBackground" Color="#22FFFFFF"/>
<SolidColorBrush x:Key="Brush.DataGrid.AlternatingRow" Color="#0AFFFFFF"/>
<SolidColorBrush x:Key="Brush.DataGrid.HoverRow" Color="#15FFFFFF"/>
<SolidColorBrush x:Key="Brush.DataGrid.SelectedRow" Color="#333A86FF"/>
<SolidColorBrush x:Key="Brush.Progress.Background" Color="#22000000"/>
```

---

## P1-T005: Create Component Token System

**Priority**: High  
**New file**: `Theme/Base/ComponentTokens.xaml`

**Content:** Component-specific tokens that reference semantic/spacing tokens:

```xaml
<Thickness x:Key="Component.Button.Padding" Value="{DynamicResource ButtonPadding}"/>
<CornerRadius x:Key="Component.Button.Radius" Value="{DynamicResource MediumRadius}"/>
<sys:Double x:Key="Component.ComboBox.CornerRadius" Value="{DynamicResource MediumRadius}"/>
<Thickness x:Key="Component.Card.Padding" Value="{DynamicResource CardPadding}"/>
<CornerRadius x:Key="Component.Card.Radius" Value="{DynamicResource CardRadius}"/>
```

**Rationale:** Isolates component styling from direct semantic token consumption, enabling per-component theming.

---

## P1-T006: Refactor Existing Dictionaries

**Priority**: High

**Files to refactor:**

| File | Action |
|------|--------|
| `ButtonStyles.xaml` | Replace `#2FFFFFFF` with `Brush.Button.Background`; replace `#3FFFFFFF` hover with `Brush.Button.HoverBackground`; replace `#22FFFFFF` pressed with `Brush.Button.PressedBackground` |
| `DataGridStyles.xaml` | Replace `#0AFFFFFF` with `Brush.DataGrid.AlternatingRow`; replace `#15FFFFFF` with `Brush.DataGrid.HoverRow`; replace `#333A86FF` with `Brush.DataGrid.SelectedRow` |
| `ComboBoxStyles.xaml` | Replace `CornerRadius="10"` with `{DynamicResource MediumRadius}` |
| `MainWindow.xaml` | Replace `Background="#22000000"` with `{DynamicResource Brush.Progress.Background}` |

---

## P1-T007: Remove Duplicate Shadows

**Priority**: Medium  
**Deliverable**: Validation report

**Method:** Search all `.xaml` for inline `DropShadowEffect` definitions.

**Acceptance:** Zero inline shadows outside `Effects/Shadows.xaml` and `Effects/Glow.xaml`.

---

## P1-T008: Standardize Spacing

**Priority**: Medium  
**File**: `Theme/Base/Spacing.xaml` — extend

**Additions:**
```xaml
<sys:Double x:Key="Spacing.XSmall">4</sys:Double>
<sys:Double x:Key="Spacing.Small">8</sys:Double>
<sys:Double x:Key="Spacing.Medium">12</sys:Double>
<sys:Double x:Key="Spacing.Large">16</sys:Double>
<sys:Double x:Key="Spacing.XLarge">20</sys:Double>
<sys:Double x:Key="Spacing.XXLarge">24</sys:Double>

<Thickness x:Key="Padding.XSmall">4</Thickness>
<Thickness x:Key="Padding.Small">8,6</Thickness>
<Thickness x:Key="Padding.Medium">12,8</Thickness>
<Thickness x:Key="Padding.Large">16,12</Thickness>
<Thickness x:Key="Padding.XLarge">20,16</Thickness>
```

---

## P1-T009: Standardize Typography

**Priority**: Medium  
**File**: `Theme/Base/Typography.xaml` — extend

**Additions:**
```xaml
<Style x:Key="Style.Text.Title" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyPrimary}"/>
    <Setter Property="FontSize" Value="{DynamicResource TitleFontSize}"/>
    <Setter Property="FontWeight" Value="{DynamicResource FontWeightBold}"/>
    <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Primary}"/>
</Style>

<Style x:Key="Style.Text.Subtitle" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyPrimary}"/>
    <Setter Property="FontSize" Value="{DynamicResource HeaderFontSize}"/>
    <Setter Property="FontWeight" Value="{DynamicResource FontWeightMedium}"/>
    <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Primary}"/>
</Style>

<Style x:Key="Style.Text.Body" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyPrimary}"/>
    <Setter Property="FontSize" Value="{DynamicResource BodyFontSize}"/>
    <Setter Property="FontWeight" Value="{DynamicResource FontWeightNormal}"/>
    <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Primary}"/>
</Style>

<Style x:Key="Style.Text.Caption" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyPrimary}"/>
    <Setter Property="FontSize" Value="{DynamicResource CaptionFontSize}"/>
    <Setter Property="FontWeight" Value="{DynamicResource FontWeightNormal}"/>
    <Setter Property="Foreground" Value="{DynamicResource Brush.Text.Secondary}"/>
</Style>

<Style x:Key="Style.Text.Button" TargetType="TextBlock">
    <Setter Property="FontFamily" Value="{DynamicResource FontFamilyPrimary}"/>
    <Setter Property="FontSize" Value="{DynamicResource BodyFontSize}"/>
    <Setter Property="FontWeight" Value="{DynamicResource FontWeightMedium}"/>
    <Setter Property="Foreground" Value="{DynamicResource Brush.Text.OnAccent}"/>
</Style>
```

---

## P1-T010: Validate DynamicResource Usage

**Priority**: High  
**Deliverable**: Validation report

**Method:**
- Regex search all `.xaml` for hardcoded color values (`Color="#......"` or `Color="White"` outside primitive definitions)
- Regex search for `StaticResource` used on themeable properties (brushes, colors, borders)
- Ensure all control `Background`, `Foreground`, `BorderBrush` use `DynamicResource`

---

## P1-T011: Create New Token Files

**Priority**: Medium

| New File | Content |
|----------|---------|
| `Theme/Base/Elevation.xaml` | Elevation tokens: `Elevation.Card`, `Elevation.Popup`, `Elevation.Window`, `Elevation.Small`, `Elevation.Medium`, `Elevation.Large` — each mapping to shadow effect keys |
| `Theme/Base/Motion.xaml` | Motion tokens: `Motion.Fade.Duration`, `Motion.Hover.Duration`, `Motion.Press.Duration`, `Motion.ThemeSwitch.Duration`, `Motion.Easing.Default` |
| `Theme/Base/ZIndex.xaml` | Z-index constants for layering (popup, tooltip, overlay, dialog) |
| `Theme/Base/Opacity.xaml` | Standard opacity values: `Opacity.Disabled`, `Opacity.Hover`, `Opacity.Pressed`, `Opacity.Subtle` |

---

## P1-T012: Update ThemeResources.xaml Aggregator

**Priority**: High  
**File**: `Theme/ThemeResources.xaml`

**Add new base files to MergedDictionaries in correct order:**
```text
1. Base/Colors.xaml
2. Base/Typography.xaml
3. Base/Spacing.xaml
4. Base/Radius.xaml
5. Base/Elevation.xaml        (NEW)
6. Base/Motion.xaml           (NEW)
7. Base/ZIndex.xaml           (NEW)
8. Base/Opacity.xaml          (NEW)
9. Base/ComponentTokens.xaml  (NEW)
10. Effects/Shadows.xaml
11. Effects/Glow.xaml
12. Effects/Animations.xaml
13. Controls/... (existing)
14. ModernWindow.xaml
15. WindowAnimations.xaml
```

---

## Phase 1 Acceptance Criteria

- [ ] All existing tokens reviewed and inventoried (P1-T001)
- [ ] Duplicate colors detected and documented (P1-T002)
- [ ] Primitive token system created with aliases (P1-T003)
- [ ] Semantic token system created, all referencing primitives (P1-T004)
- [ ] Component token system created (P1-T005)
- [ ] All inline colors replaced with tokens (P1-T006)
- [ ] Zero inline shadows outside Effects/ folder (P1-T007)
- [ ] Spacing standardized (P1-T008)
- [ ] Typography standardized (P1-T009)
- [ ] DynamicResource validation passed (P1-T010)
- [ ] New token files created (P1-T011)
- [ ] ThemeResources.xaml updated with correct loading order (P1-T012)
- [ ] Build passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean
- [ ] Manual architecture review passed

---

# PHASE 2 — RENDERING & WINDOW INFRASTRUCTURE

## Goal

Create Excel-safe rendering architecture. Extend existing ModernWindow, WindowRenderModeDetector with new services.

## Branch

`feature/phase-02-rendering-infrastructure`

---

## P2-T001: Audit All Windows

**Priority**: High  
**Deliverable**: Window audit report

**Scope:** Review all 14 `.xaml` window files:
- `MainWindow.xaml`
- `SettingsWindow.xaml`
- `Float_path.xaml`
- `AssignTradeCodesWindow.xaml`
- `Fixpiecolors.xaml`
- `LinksManagerWindow.xaml`
- `StyleSelectorWindow.xaml`
- `SubDailyReportWindow.xaml`
- `UnmergeFillDownWindow.xaml`
- `XerEditorWindow.xaml`
- `ProjectAnalysisWindow.xaml`
- `PrimaveraCompareWindow.xaml`
- `PrimaveraResultsWindow.xaml`
- `ToastWindow.xaml`

**Check per window:**
- Inherits from `ModernWindow`? (If not, flag for Phase 11 migration)
- Uses `AllowsTransparency`? (Should be `False` for Excel-safe)
- Uses inline background/border/effects?
- Has `SnapsToDevicePixels` and `UseLayoutRounding`?

---

## P2-T002: Create ModernWindow v2

**Priority**: High  
**File**: `Controls/ModernWindow.cs` — extend

**Current state:** Already exists with:
- `WindowRenderModeDetector` integration
- `ThemeManager.LoadThemeFromSettings()` on init
- Fade-in/out animations
- Commands for min/max/close
- `CloseOnEscape` DP

**Extensions needed:**
1. Add `RenderMode` DP (exposes current mode for diagnostics)
2. Add `IsSafeMode` DP (read-only, bound to `WindowRenderModeDetector`)
3. Add `WindowBackdrop` DP (solid, gradient, image options for Phase 3)
4. Add `DpiScale` DP (for DPI-aware layout)
5. Refactor `InitializeWindow()` to support future backdrop injection
6. Ensure all new DPs use `DefaultValue` that matches current behavior

---

## P2-T003: Create Unified Shadow System

**Priority**: High  
**File**: `Theme/Effects/Shadows.xaml` — extend

**Current state:** 7 shadow effects + 1 progress glow

**Additions:**
- `Shadow.Window.Safe` — lower opacity/blur for FallbackSafe mode
- `Shadow.Card.Safe`
- `Shadow.Popup.Safe`
- Document safe-mode variants in `SHADOW_SYSTEM.md`

---

## P2-T004: Create Excel-Safe Render Detection

**Priority**: High  
**New file**: `Services/RenderModeService.cs`

**Responsibilities:**
- Wrap and extend `WindowRenderModeDetector`
- Cache render mode per session
- Provide `IsSafeModeRequired()` API
- Detect GPU availability
- Detect transparency support
- Expose `RenderModeChanged` event

---

## P2-T005: Implement Fallback Rendering

**Priority**: High  
**Files**: `Services/RenderModeService.cs` + `Controls/ModernWindow.cs`

**Behavior:**
- If `RenderModeService` returns `FallbackSafe`:
  - Disable fade animations on ModernWindow
  - Use `Shadow.*.Safe` variants
  - Disable backdrop effects
  - Force `AllowsTransparency="False"`
- If `WindowChrome` mode:
  - Use full shadows, animations, backdrops

---

## P2-T006: Refactor All Windows

**Priority**: High

**Scope:** Apply ModernWindow template and safe-mode settings to all audited windows.

**Per-window changes:**
- Ensure `WindowStyle="None"` + ModernWindow chrome (already done for some)
- Replace inline backgrounds with `{DynamicResource Brush.Background.Root}`
- Replace inline borders with `{DynamicResource CardStrokeBrush}`
- Add `SnapsToDevicePixels="True"` and `UseLayoutRounding="True"` where missing
- **Do NOT convert to Pages yet** (Phase 5 scope restriction)

---

## P2-T007: Add DPI Infrastructure

**Priority**: Medium  
**File**: `Helpers/DpiHelper.cs` — extend

**Current state:** Exists but needs review.

**Additions:**
- `GetCurrentDpiScale()` — returns scale factor for current window
- `ScaleValue(double value)` — scales a value by current DPI
- `IsHighDpi()` — returns true if scale ≥ 1.5
- Integrate with `ModernWindow` for automatic scaling awareness

---

## P2-T008: Add Accessibility Prep

**Priority**: Medium  
**Deliverable**: Accessibility checklist

**Per-control requirements:**
- `FocusVisualStyle` using `Glow.Focus` effect
- `AutomationProperties.Name` on all interactive elements
- `AutomationProperties.HelpText` on complex controls
- Keyboard navigation (Tab order, Enter/Space activation)
- High contrast awareness (use system colors when high contrast is active)

---

## P2-T009: Standardize Window Animations

**Priority**: Medium  
**File**: `Theme/WindowAnimations.xaml` — extend

**Current state:** Already exists but needs review.

**Requirements:**
- All window open animations ≤ 200ms
- All window close animations ≤ 150ms
- Use `Motion.*` tokens for durations
- Safe mode: skip animations entirely

---

## P2-T010: Validate Excel Stability

**Priority**: High  
**Deliverable**: `Docs/Architecture/EXCEL_TEST_CHECKLIST.md`

**Manual test checklist for you to execute:**

1. Open each window from Excel VSTO host
2. Verify no black window rendering
3. Verify ComboBox popup opens and renders correctly
4. Verify DPI scaling at 100%, 125%, 150%
5. Verify no transparency artifacts
6. Verify window can be moved, minimized, closed without Excel freezing
7. Verify theme switching works inside Excel host
8. Verify safe mode activates correctly on problematic systems

---

## Phase 2 Acceptance Criteria

- [ ] All windows audited (P2-T001)
- [ ] ModernWindow v2 with new DPs (P2-T002)
- [ ] Safe-mode shadow variants created (P2-T003)
- [ ] RenderModeService created (P2-T004)
- [ ] Fallback rendering implemented (P2-T005)
- [ ] All windows refactored for safe mode (P2-T006)
- [ ] DPI infrastructure extended (P2-T007)
- [ ] Accessibility checklist created (P2-T008)
- [ ] Window animations standardized (P2-T009)
- [ ] Excel stability test checklist created (P2-T010)
- [ ] Build passes
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean
- [ ] Manual architecture review passed

---

# PHASE 3 — THEME ENGINE 2.0 (Roadmap)

## Goal

Create runtime accent/background engine.

## Branch

`feature/phase-03-theme-engine-v2`

## Features

### Theme Base
- Dark
- Light

### Accent Engine
User selects one accent color. System generates:
- hover
- pressed
- glow
- border
- subtle variants

### Background Engine
- solid
- gradient
- image
- mica-style simulation

## Deliverables

| File | Purpose |
|------|---------|
| `Services/AccentGeneratorService.cs` | Generates hover/pressed/glow/border/subtle from single accent color |
| `Services/BackgroundManager.cs` | Manages solid/gradient/image/mica backgrounds |
| `Theme/Generated/` | Runtime-generated brush dictionaries |

## Key Tasks

| ID | Task |
|----|------|
| P3-T001 | Audit current custom theme |
| P3-T002 | Create runtime accent engine |
| P3-T003 | Create generated brushes |
| P3-T004 | Create background engine |
| P3-T005 | Create image background system |
| P3-T006 | Add runtime theme switching |
| P3-T007 | Add persistence |
| P3-T008 | Add safe fallback logic |
| P3-T009 | Add preview system |
| P3-T010 | Validate runtime performance |

---

# PHASE 4 — CONTROL STANDARDIZATION (Roadmap)

## Goal

Standardize ALL WPF controls.

## Branch

`feature/phase-04-control-standardization`

## Highest Priority

### ComboBox Popup Architecture
Must solve:
- width mismatch
- clipping
- shadow inconsistency
- Excel rendering issues

## Controls Included

### Inputs
- TextBox
- ComboBox
- PasswordBox

### Lists
- DataGrid
- ListView
- TreeView

### Navigation
- Tabs
- Sidebar

### Feedback
- Dialogs
- Toasts

## Mandatory Rules

All controls MUST:
- use semantic tokens
- use centralized shadows
- support keyboard navigation
- support reduced motion

## Key Tasks

| ID | Task |
|----|------|
| P4-T001 | Audit all control templates |
| P4-T002 | Refactor ComboBox popup |
| P4-T003 | Standardize TextBox |
| P4-T004 | Standardize DataGrid |
| P4-T005 | Standardize ListView |
| P4-T006 | Add virtualization |
| P4-T007 | Add smooth scrolling |
| P4-T008 | Add accessibility states |
| P4-T009 | Remove duplicate styles |
| P4-T010 | Validate popup rendering |

---

# PHASE 5 — NAVIGATION SHELL PLATFORM (Roadmap)

## Goal

Transform app into unified workspace shell.

## Branch

`feature/phase-05-navigation-shell`

## Scope Restriction

**Shell applies to NEW windows only. Existing windows remain standalone until Phase 11.**

## Architecture

```text
Shell
 ├── Sidebar
 ├── Workspace (Frame with Pages)
 ├── StatusBar
 ├── CommandPalette
 └── Navigation Pages
```

## Ribbon Integration

Ribbon becomes: launcher layer  
Shell becomes: workspace layer

## Convert New Windows Into Pages

Example:
```text
NewFeatureWindow.xaml
    ↓
Pages/NewFeaturePage.xaml
```

## Key Tasks

| ID | Task |
|----|------|
| P5-T001 | Create shell architecture |
| P5-T002 | Create navigation service |
| P5-T003 | Create sidebar |
| P5-T004 | Create page host |
| P5-T005 | Convert NEW feature windows to Pages |
| P5-T006 | Create command palette |
| P5-T007 | Create workspace restore |
| P5-T008 | Add navigation animations |
| P5-T009 | Integrate ribbon launching |
| P5-T010 | Validate shell performance |

---

# PHASE 6 — MVVM & ARCHITECTURE CLEANUP (Roadmap)

## Goal

Clean architecture completely.

## Branch

`feature/phase-06-mvvm-cleanup`

## Deliverables

### Dependency Injection

**Package:** `Microsoft.Extensions.DependencyInjection` (NuGet)

```powershell
Install-Package Microsoft.Extensions.DependencyInjection
```

### Module Infrastructure
- Event bus
- Command registry
- Module registry

## Required Cleanup

- remove logic from code-behind
- relocate misplaced ViewModels
- isolate services
- reduce coupling

## Key Tasks

| ID | Task |
|----|------|
| P6-T001 | Audit MVVM violations |
| P6-T002 | Introduce DI container |
| P6-T003 | Create module registry |
| P6-T004 | Create event bus |
| P6-T005 | Refactor services |
| P6-T006 | Move ViewModels |
| P6-T007 | Remove business logic from views |
| P6-T008 | Standardize commands |
| P6-T009 | Reduce coupling |
| P6-T010 | Validate architecture |

---

# PHASE 7 — SETTINGS & PERSONALIZATION UX (Roadmap)

## Goal

Build Windows 11 personalization experience.

## Branch

`feature/phase-07-settings-ux`

## Features

### Settings Categories
- Appearance
- Performance
- Accessibility
- Diagnostics
- Excel
- Plugins

### Personalization
- live preview
- accent picker
- background picker
- animation controls
- density controls

## Key Tasks

| ID | Task |
|----|------|
| P7-T001 | Audit current settings UI |
| P7-T002 | Create settings navigation |
| P7-T003 | Create accent picker |
| P7-T004 | Create background picker |
| P7-T005 | Create live preview |
| P7-T006 | Create performance settings |
| P7-T007 | Create accessibility settings |
| P7-T008 | Create diagnostics settings |
| P7-T009 | Create import/export settings |
| P7-T010 | Validate UX consistency |

---

# PHASE 8 — DIAGNOSTICS & STABILITY PLATFORM (Roadmap)

## Goal

Prevent WPF/Excel rendering failures.

## Branch

`feature/phase-08-diagnostics`

## Features

### Diagnostics Panel
- render mode
- GPU mode
- active theme
- memory usage
- popup diagnostics

### Validation System

Detect:
- missing tokens
- inline colors
- invalid resources
- duplicate styles

## Key Tasks

| ID | Task |
|----|------|
| P8-T001 | Create diagnostics service |
| P8-T002 | Create logging infrastructure |
| P8-T003 | Create validation engine |
| P8-T004 | Create render diagnostics |
| P8-T005 | Create popup diagnostics |
| P8-T006 | Create memory diagnostics |
| P8-T007 | Create theme validator |
| P8-T008 | Create fallback recovery |
| P8-T009 | Add crash-safe loading |
| P8-T010 | Validate stability |

---

# PHASE 9 — PLUGIN & FEATURE PLATFORM (Roadmap)

## Goal

Prepare future extensibility.

## Branch

`feature/phase-09-plugin-platform`

## Features

### Module System

Features register:
- pages
- commands
- ribbon actions

### Lazy Loading

Load modules only when needed.

## Key Tasks

| ID | Task |
|----|------|
| P9-T001 | Create module contracts |
| P9-T002 | Create plugin loader |
| P9-T003 | Create module registration |
| P9-T004 | Create lazy loading |
| P9-T005 | Create feature isolation |
| P9-T006 | Create navigation registration |
| P9-T007 | Create ribbon registration |
| P9-T008 | Create plugin diagnostics |
| P9-T009 | Create plugin validation |
| P9-T010 | Validate module stability |

---

# PHASE 10 — ENTERPRISE POLISH (Roadmap)

## Goal

Production-grade optimization.

## Branch

`feature/phase-10-enterprise-polish`

## Required Work

### Performance
- memory optimization
- rendering optimization
- virtualization tuning

### Accessibility
- keyboard audit
- focus audit
- screen reader prep

### DPI
- scaling validation
- multi-monitor validation

## Key Tasks

| ID | Task |
|----|------|
| P10-T001 | Run performance audit |
| P10-T002 | Optimize rendering |
| P10-T003 | Optimize memory |
| P10-T004 | Optimize animations |
| P10-T005 | Run accessibility audit |
| P10-T006 | Run DPI audit |
| P10-T007 | Run Excel host audit |
| P10-T008 | Finalize diagnostics |
| P10-T009 | Finalize documentation |
| P10-T010 | Production validation |

---

# PHASE 11 — LEGACY WINDOW MIGRATION (New)

## Goal

Migrate existing standalone windows to Shell/Page architecture gradually.

## Branch

`feature/phase-11-legacy-migration`

## Approach

- Each existing window becomes a candidate for migration
- Create `Pages/` folder for migrated windows
- Original `.xaml` preserved until migration is validated
- Ribbon launcher updated to open Page in Shell instead of standalone window
- Migrate most-used windows first

## Key Tasks

| ID | Task |
|----|------|
| P11-T001 | Identify migration priority (most-used windows first) |
| P11-T002 | Create `Pages/` folder structure |
| P11-T003 | Migrate MainWindow to Page |
| P11-T004 | Migrate SettingsWindow to Page |
| P11-T005 | Migrate remaining feature windows |
| P11-T006 | Update ribbon launchers |
| P11-T007 | Validate each migrated window in Excel |
| P11-T008 | Remove original standalone XAML after validation |
| P11-T009 | Document migration patterns |
| P11-T010 | Final migration audit |

---

# New Architecture Docs

Created in Phase 0:

```text
Docs/Architecture/
├── UI_GUIDELINES.md
├── TOKEN_RULES.md
├── POPUP_ARCHITECTURE.md
├── SHADOW_SYSTEM.md
├── ACCESSIBILITY_RULES.md
├── EXCEL_RENDERING_RULES.md
├── MVVM_RULES.md
├── REVIEW_CHECKLIST.md
├── AGENT_RULES.md
├── PERFORMANCE_RULES.md
├── AUDIT_REPORT.md
└── EXCEL_TEST_CHECKLIST.md
```

---

# Absolute Non-Negotiable Rules

## NEVER:

- inline colors
- inline shadows
- duplicate styles
- hardcoded spacing
- direct service creation in views
- business logic in code-behind

## ALWAYS:

- DynamicResource
- semantic tokens
- MVVM
- centralized effects
- Excel-safe rendering
- accessibility-ready controls
- review before merge

---

# Recommended Execution Order

```text
Phase 0
    ↓
Phase 1
    ↓
Phase 2
    ↓
Phase 3
    ↓
Phase 4
    ↓
Phase 5
    ↓
Phase 6
    ↓
Phase 7
    ↓
Phase 8
    ↓
Phase 9
    ↓
Phase 10
    ↓
Phase 11
```

**DO NOT start Shell before:**
- Design System (Phase 1)
- Rendering Infrastructure (Phase 2)
- Theme Engine (Phase 3)
- Control Standardization (Phase 4)

are stable.

---

# Spec Kit Integration

User will create Spec Kit folders using `speckit` commands. Each phase maps to:

```text
specs/
├── 003-design-system-core/
│   ├── plan.md
│   ├── spec.md
│   ├── tasks.md
│   ├── checklists/
│   │   └── requirements.md
│   └── contracts/
│       └── token-api.md
├── 004-rendering-infrastructure/
│   ├── plan.md
│   ├── spec.md
│   ├── tasks.md
│   ├── checklists/
│   │   └── requirements.md
│   └── contracts/
│       └── window-api.md
└── ... (one folder per phase)
```

---

# Appendices

## Appendix A: Existing File Inventory

### Theme Engine
```text
WpfApp2/Theme/
├── Base/
│   ├── Colors.xaml
│   ├── Typography.xaml
│   ├── Spacing.xaml
│   └── Radius.xaml
├── Dark/
│   ├── DarkColors.xaml
│   └── DarkTheme.xaml
├── Light/
│   ├── LightColors.xaml
│   └── LightTheme.xaml
├── Custom/
│   ├── CustomColors.xaml
│   └── CustomTheme.xaml
├── Controls/
│   ├── ButtonStyles.xaml
│   ├── ComboBoxStyles.xaml
│   ├── ComboBoxItemStyles.xaml
│   ├── DataGridStyles.xaml
│   ├── TextBoxStyles.xaml
│   ├── ListViewStyles.xaml
│   ├── ListViewItemStyles.xaml
│   ├── ScrollBarStyles.xaml
│   ├── CheckBoxStyles.xaml
│   ├── RadioButtonStyles.xaml
│   ├── ToggleButtonStyles.xaml
│   ├── ScrollViewerStyles.xaml
│   ├── WindowStyles.xaml
│   ├── WindowButtonStyles.xaml
│   ├── GroupBoxStyles.xaml
│   ├── LabelStyles.xaml
│   ├── ProgressBarStyles.xaml
│   ├── ThemeCardStyles.xaml
│   └── AccentSwatchStyles.xaml
├── Effects/
│   ├── Shadows.xaml
│   ├── Glow.xaml
│   └── Animations.xaml
├── ModernWindow.xaml
├── ThemeResources.xaml
└── WindowAnimations.xaml
```

### Services
```text
WpfApp2/Services/
├── ThemeManager.cs
├── ThemeSettings.cs
├── WindowRenderModeDetector.cs
├── DialogService.cs
├── ToastService.cs
└── ExcelProjectAnalysisService.cs
```

### Controls
```text
WpfApp2/Controls/
├── ModernWindow.cs
├── LoadingOverlay.xaml
├── LoadingOverlay.xaml.cs
└── Toast/
    ├── ToastWindow.xaml
    ├── ToastWindow.xaml.cs
    └── ToastModel.cs
```

### Windows
```text
WpfApp2/
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Float_path.xaml
├── Float_path.xaml.cs
├── AssignTradeCodesWindow.xaml
├── AssignTradeCodesWindow.xaml.cs
├── Fixpiecolors.xaml
├── Fixpiecolors.xaml.cs
├── LinksManagerWindow.xaml
├── LinksManagerWindow.xaml.cs
├── StyleSelectorWindow.xaml
├── StyleSelectorWindow.xaml.cs
├── SubDailyReportWindow.xaml
├── SubDailyReportWindow.xaml.cs
├── UnmergeFillDownWindow.xaml
├── UnmergeFillDownWindow.xaml.cs
├── XerEditorWindow.xaml
├── XerEditorWindow.xaml.cs
├── Views/
│   ├── SettingsWindow.xaml
│   └── SettingsWindow.xaml.cs
└── Windows/
    └── PrimaveraComparison/
        ├── PrimaveraCompareWindow.xaml
        ├── PrimaveraCompareWindow.xaml.cs
        ├── PrimaveraResultsWindow.xaml
        └── PrimaveraResultsWindow.xaml.cs
```

## Appendix B: Token Layers Reference

```text
Primitive Tokens
    Primitive.Blue.500      → #3A86FF
    Primitive.Slate.900     → #0E1720
    Primitive.White.95      → #F2FFFFFF
    ...

    ↓ (referenced by)

Semantic Tokens
    Brush.Background.Primary    → Primitive.Slate.900
    Brush.Text.Primary          → Primitive.White.95
    Brush.Accent.Primary        → Primitive.Blue.500
    ...

    ↓ (referenced by)

Component Tokens
    Component.Button.Padding    → Spacing.Medium
    Component.Button.Radius     → Radius.Medium
    ...

    ↓ (used by)

Control Templates
    ButtonStyles.xaml
    ComboBoxStyles.xaml
    ...
```

## Appendix C: Resource Loading Order

```text
1. Primitive Tokens       → Base/Colors.xaml
2. Semantic Tokens        → Base/Colors.xaml (continued)
3. Typography             → Base/Typography.xaml
4. Spacing                → Base/Spacing.xaml
5. Radius                 → Base/Radius.xaml
6. Elevation              → Base/Elevation.xaml
7. Motion                 → Base/Motion.xaml
8. ZIndex                 → Base/ZIndex.xaml
9. Opacity                → Base/Opacity.xaml
10. Component Tokens      → Base/ComponentTokens.xaml
11. Effects               → Effects/Shadows.xaml, Glow.xaml, Animations.xaml
12. Controls              → All control styles
13. Window Styles         → ModernWindow.xaml, WindowAnimations.xaml
14. Theme Overrides       → Dark/Light/Custom (swapped at runtime)
15. Runtime Overrides     → Generated brushes, accent variants
```

---

**Plan Version**: 1.0.0  
**Approved by**: User (2026-05-21)  
**Next Step**: User creates Spec Kit branches and folders, then signals to begin Phase 0 execution.
