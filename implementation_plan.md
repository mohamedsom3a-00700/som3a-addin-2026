# Som3a Add-in 2026 — Enterprise UI Transformation Master Plan

**Version**: 1.0.0  
**Date**: 2026-05-21  
**Status**: ✅ Implementation complete — validation tasks remaining  
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

# PHASE 3 — THEME ENGINE 2.0

**Status**: ✅ Implementation complete — validation tasks remaining

**Branch**: `002-fluent-theme-engine` (merged)

## Goal

Build a production-grade Fluent Runtime Theme Engine with Dark/Light/Custom theme switching, accent color system, centralized effects, DPI-aware controls, and Excel VSTO-safe rendering.

## Implementation

Phase 3 was delivered via two coordinated workstreams:

| Workstream | Spec | Focus |
|------------|------|-------|
| WS-A | `001-fluent-theme-engine` | Theme engine, effects library, control standardization, DPI, accessibility |
| WS-B | `002-themes-manager` | ThemeManager bug fixes, hardcoded color elimination, crash-risk removal |

---

### WS-A: Fluent Theme Engine (`specs/001-fluent-theme-engine/tasks.md`)

#### Phase A1: Setup — Effects Library & Persistence

- [x] T001 Create `Theme/Effects/Shadows.xaml` — centralized DropShadowEffect definitions (Shadow.Window, Shadow.Popup, Shadow.Card, 7 variants)
- [x] T002 Create `Theme/Effects/Glow.xaml` — centralized glow effects (Glow.Focus, Glow.ButtonHover, Glow.Primary, Glow.Selection, Glow.Accent, Glow.ThemeCard.Selected)
- [x] T003 Create `Theme/Effects/Animations.xaml` — HoverEnter/Exit, FocusEnter/Exit, PopupOpen/Close, FadeIn/Out, all ≤200ms with CubicEase
- [x] T004 Create `Properties/Settings.settings` — SelectedTheme + AccentColor user-scoped
- [x] T005 Create `Settings.Designer.cs` accessor

#### Phase A2: Foundational — Theme Dictionaries & ThemeManager

- [x] T006 Create `Theme/Dark/DarkColors.xaml` — Dark theme semantic overrides
- [x] T007 Create `Theme/Light/LightColors.xaml` — Light theme semantic overrides
- [x] T008 Create `Theme/Custom/CustomColors.xaml` — Custom theme overrides + 8 accent presets
- [x] T009 Create `Theme/Dark/DarkTheme.xaml` — merged dictionary
- [x] T010 Create `Theme/Light/LightTheme.xaml` — merged dictionary
- [x] T011 Create `Theme/Custom/CustomTheme.xaml` — merged dictionary with accent token
- [x] T012 Create `Services/ThemeManager.cs` — singleton with ApplyTheme(), LoadThemeFromSettings(), SaveCurrentTheme(), ThemeChanged event
- [x] T013 Integrate ThemeManager into `Controls/ModernWindow.cs` — LoadThemeFromSettings() on startup

#### Phase A3: US1 — Theme Selection via Settings Window (P1: MVP)

- [x] T014 Create `Theme/Controls/ThemeCardStyles.xaml` — glow + scale animations
- [x] T015 Create `Theme/Controls/AccentSwatchStyles.xaml` — clickable swatch circles
- [x] T016 Refactor `Views/SettingsWindow.xaml` — replace ComboBox with 3 theme cards
- [x] T017 Add 8 accent swatch circles to Custom theme card
- [x] T018 Wire card clicks → ThemeManager.ApplyTheme()
- [x] T019 Wire swatch clicks → ThemeManager.ApplyTheme("Custom", hex)
- [x] T020 Initial selection state on load
- [x] T021 Glow effect on selected card
- [ ] T022 Validate: theme switch < 1s, persistence across restarts, all windows update simultaneously

#### Phase A4: US2 — Visual Quality & Consistency (P1)

- [x] T023 Refactor `ButtonStyles.xaml` — VSM states (Normal/Hover/Pressed/Focused/Disabled) with glow
- [x] T024 Refactor `ComboBoxStyles.xaml` — AllowsTransparency=False, PlacementTarget, Shadow.Popup
- [x] T025 Create `CheckBoxStyles.xaml` — VSM states + custom checkmark
- [x] T026 Create `RadioButtonStyles.xaml` — VSM states + dot indicator
- [x] T027 Create `ToggleButtonStyles.xaml` — VSM states + slide animation
- [x] T028 Refactor `TextBoxStyles.xaml` — VSM states with accent border transitions
- [x] T029 Refactor `DataGridStyles.xaml` — EnableRowVirtualization, hover/selection via DynamicResource
- [x] T030 Create `ScrollViewerStyles.xaml` — thin scrollbar (4px track, 8px thumb)
- [ ] T031 Verify all states render correctly in Dark + Light
- [ ] T032 Verify ComboBox popup renders above content with shadow, no clipping

#### Phase A5: US3 — DPI Scaling (P2)

- [x] T033 Audit XAML for hardcoded pixel values — all templates use DynamicResource
- [x] T034 Audit SnapsToDevicePixels/UseLayoutRounding — present on all controls
- [x] T035 DPI-aware ComboBox popup sizing
- [x] T036 DPI-aware theme card preview sizing
- [x] T037 DPI-aware accent swatch sizing
- [ ] T038 Validate DPI at 100%, 125%, 150%, 200% — no clipping/overflow

#### Phase A6: US4 — Performance Stability in Excel (P2)

- [x] T039 Audit nested DropShadowEffect — found 4 inline, documented
- [x] T040 Audit BlurEffect — zero found ✅
- [x] T041 Create `WindowRenderModeDetector.cs` — VSTO/DPI detection
- [x] T042 Integrate detector into ModernWindow startup
- [ ] T043 Validate DataGrid scrolling with 1000+ rows
- [ ] T044 Validate rapid theme switching (10x) inside Excel

#### Phase A7: US5 — Keyboard Navigation & Accessibility (P3)

- [x] T045 Audit Tab order in SettingsWindow — all elements Focusable/IsTabStop
- [x] T046 Add AutomationProperties.Name to cards + swatches
- [x] T047 Verify Enter/Space activation on all cards + swatches
- [ ] T048 Verify focus indicators (Glow.Focus) visible on all interactive elements
- [ ] T049 Validate WCAG 2.1 AA contrast (4.5:1) in Dark + Light
- [ ] T050 Validate ComboBox keyboard navigation (Tab → Arrows → Escape)

#### Phase A8: Polish & Cross-Cutting

- [x] T051 8-gate validation checklist — Dark theme
- [x] T052 8-gate validation checklist — Light theme
- [x] T053 8-gate validation checklist — Custom theme (2+ accent variations)
- [x] T054 Document resource loading order in `ThemeResources.xaml`
- [x] T055 Remove hardcoded colors from control templates
- [x] T056 Remove inline DropShadowEffect — 1 documented exception
- [x] T057 Update `AGENTS.md`
- [x] T058 Build validation — `msbuild` succeeds (warnings only)

---

### WS-B: Themes Manager (`specs/002-themes-manager/tasks.md`)

#### Phase B1: Setup

- [x] T001 Verify `msbuild` compiles before changes

#### Phase B2: Foundational — ThemeManager Fixes + New Tokens

- [x] T002 Fix accent persistence in `ThemeManager.cs` — preserve `_currentAccentColor` when null
- [x] T003 Fix early-return logic — reorder `_currentTheme` update
- [x] T004 Implement safe dictionary fallback — try/catch around removal/addition
- [x] T005 Add `ThemeChanged` thread safety — `Dispatcher.InvokeAsync()`
- [x] T006 Add theme switch debouncing — ~150ms coalesce window
- [x] T007 Define `Brush.Background.Root` (LinearGradientBrush) in Colors.xaml
- [x] T008 Define `Brush.Accent.ProgressFill` (LinearGradientBrush)
- [x] T009 Define `Brush.Stroke.Info` (SolidColorBrush)
- [x] T010 Define `Brush.Stroke.Status` (SolidColorBrush)
- [x] T011 Define `Brush.Fill.Info` (SolidColorBrush)
- [x] T012 Define `Brush.Fill.Status` (SolidColorBrush)
- [x] T013 Dark overrides for all new tokens
- [x] T014 Light overrides for all new tokens
- [x] T015 Custom overrides for all new tokens
- [x] T016 Add `ProgressGlow` DropShadowEffect to Shadows.xaml

#### Phase B3: US3 — No Crashes on Startup or Theme Change (P1)

- [x] T017 Remove duplicate converters from App.xaml
- [x] T018 Remove duplicate dictionary loads from SettingsWindow.xaml
- [x] T019 Remove orphaned FluentWhite.xaml from csproj
- [x] T020 Remove orphaned FluentEffects.xaml + references
- [x] T021 Remove commented-out legacy theme imports from App.xaml

#### Phase B4: US1 — Theme Switching Works Reliably (P1: MVP)

- [x] T022–T033 Replace root background gradient with `{DynamicResource Brush.Background.Root}` across all 12 windows

#### Phase B5: US2 — Accent Color Reflects Everywhere (P1)

- [x] T034–T038 Replace progress bar gradients with `{DynamicResource Brush.Accent.ProgressFill}` across 5 windows
- [x] T039 Fix ModernWindow.xaml hardcoded `Background="White"` / `Foreground="Black"`
- [x] T040 Replace inline DropShadowEffect on WindowBorder with `Shadow.Window`
- [x] T041 Fix TreeView hover/selected colors in Float_path.xaml

#### Phase B6: US4 — Progress Bars Display Correctly (P2)

- [x] T042–T045 Replace `Foreground="White"` with `TextOnAccentBrush` on progress percent text
- [x] T046–T047 Replace inline DropShadowEffect on progress bars with `ProgressGlow`

#### Phase B7: US5 — Window Background Matches Theme (P2)

- [x] T048 Add missing `Foreground` to PrimaveraResultsWindow TextBlock
- [x] T049 Add missing `Foreground` to SettingsWindow TextBlock
- [x] T050 Replace `Foreground="LimeGreen"` with `SuccessBrush`
- [x] T051 Replace close button hover `#22FF4757` with `DangerBrush` 13%
- [x] T052 Replace `#12FFFFFF` fills/strokes with `Brush.Fill.Info` / `Brush.Stroke.Info`
- [x] T053 Replace `#18FFFFFF` fills/strokes with `Brush.Fill.Status` / `Brush.Stroke.Status`
- [x] T054 Replace `#1FFFFFFF` fills/strokes with per-theme tokens
- [x] T055 Replace `#22FFFFFF` fills/strokes with per-theme tokens

#### Phase B8: Polish

- [x] T056 Build verification — zero errors
- [ ] T057 Runtime check: Excel VSTO — all windows update
- [ ] T058 Runtime check: accent → glow + progress bar
- [ ] T059 Runtime check: rapid switching (10x) — no crash
- [ ] T060 Regression: progress bar in all 7 windows
- [ ] T061 Regression: TreeView hover in Float_path
- [ ] T062 Regression: close button hover color
- [x] T063 Grep audit — zero `#HEX` in window .xaml files
- [x] T064 Constitution compliance review

---

## Phase 3 Deliverables Created

| File | Purpose |
|------|---------|
| `Theme/Effects/Shadows.xaml` | Centralized DropShadowEffect (7 variants + safe-mode variants) |
| `Theme/Effects/Glow.xaml` | Centralized glow effects (6 variants) |
| `Theme/Effects/Animations.xaml` | Control state + popup storyboards (all ≤200ms) |
| `Theme/Controls/CheckBoxStyles.xaml` | VSM states + custom checkmark |
| `Theme/Controls/RadioButtonStyles.xaml` | VSM states + dot indicator |
| `Theme/Controls/ToggleButtonStyles.xaml` | VSM states + slide animation |
| `Theme/Controls/ScrollViewerStyles.xaml` | Modern thin scrollbar |
| `Theme/Controls/ThemeCardStyles.xaml` | Theme card with glow + scale animations |
| `Theme/Controls/AccentSwatchStyles.xaml` | Accent swatch circles |
| `Services/RenderModeService.cs` | Render mode detection |
| `Docs/Architecture/EXCEL_TEST_CHECKLIST.md` | VSTO test scenarios |

## Phase 3 Remaining Tasks

- [ ] **WS-A**: Validate theme switch (T022), control states (T031), popup rendering (T032), DPI (T038), DataGrid performance (T043), rapid switching (T044), focus indicators (T048), contrast ratios (T049), keyboard nav (T050)
- [ ] **WS-B**: Excel VSTO runtime checks (T057–T062)

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

Production-grade optimization — validate, harden, and polish every aspect of the application for enterprise deployment. Address all remaining validation gaps from prior phases, eliminate technical debt, and produce auditable proof of quality across performance, accessibility, DPI, stability, and resource management.

## Branch

`feature/phase-10-enterprise-polish`

## Prerequisites

Before Phase 10 work begins, the following **must** be confirmed complete:

| Dependency | Why |
|------------|-----|
| Phase 4 (Control Standardization) | All control templates must be standardized before final polish |
| Phase 3 remaining validation (15 tasks) | Unchecked WS-A tasks (T022, T031, T032, T038, T043, T044, T048, T049, T050) and WS-B tasks (T057–T062) must be resolved |
| Phase 5 (Navigation Shell) | Shell must be stable for Page-level polish |
| Phase 6 (MVVM Cleanup) | Architecture must be clean before final audit |
| Phase 9 (Plugin Platform) | Module system must be stable before final diagnostics |

## Workstreams

### WS-A: Performance Hardening

- Memory profiling and leak detection
- Rendering path optimization (layout passes, visual tree depth)
- Virtualization tuning (DataGrid, ListView, TreeView)
- Animation budget compliance (all ≤200ms, no jank)
- Startup time optimization (cold start, background initialization)
- Resource dictionary merging optimization (eliminate duplicate loads)
- Freezable optimization (Freeze() on all frozen brushes, pens, transforms)
- BitmapScalingMode and RenderOptions optimization for all images/icons
- ClearType/font rendering consistency across all DPIs

### WS-B: Accessibility Compliance

- Full keyboard navigation audit (Tab order, arrow keys, Enter/Space activation, Escape close)
- Focus visual audit (Glow.Focus visible on every interactive element)
- Screen reader preparation (AutomationProperties.Name, HelpText, LabeledBy on all controls)
- WCAG 2.1 AA contrast validation (4.5:1 minimum) in Dark, Light, and all Custom themes
- Reduced motion support (respects system "Reduce motion" setting)
- High contrast mode awareness (fall back to system colors when active)
- Focus trap detection and prevention
- Accessibility compliance report generation

### WS-C: DPI & Multi-Monitor Validation

- DPI scaling validation at 100%, 125%, 150%, 200% — no clipping, overflow, or misalignment
- Multi-monitor validation across mixed DPI configurations
- Per-monitor DPI awareness (WPF PerMonitorDPI scaling)
- DPI-aware popup positioning (ComboBox, context menus)
- DPI-aware window sizing and layout
- DPI-aware font scaling consistency
- DPI regression checklist documented

### WS-D: Excel Host Stability

- All 15 remaining Phase 3 validation tasks closed
- Full Excel VSTO host test pass across all 14 windows
- Rapid theme switching validation (10+ switches, no crash, no leak)
- DataGrid virtualization stress test (10,000+ rows)
- Popup rendering validation in Excel host (no clipping, no black rectangles)
- Safe mode activation/deactivation validation
- Memory stability during extended Excel session (2+ hours)
- Crash-safe loading under low-memory conditions

### WS-E: Technical Debt Cleanup

- ToastWindow.xaml migration from plain `<Window>` to `controls:ModernWindow`
- Final hardcoded-value grep sweep — zero `#HEX`, zero inline effects, zero hardcoded margins in all .xaml files outside Theme/Base/
- ValidationEngine TODO resolved — implement proper XAML/dictionary resource resolver
- MVVM_COMPLIANCE.md per-file audit table filled (all 10 files)
- Dispose pattern audit — ensure IDisposable implementations on all services with unmanaged resources
- Event handler unsubscription audit — prevent memory leaks from lingering subscriptions
- Static event cleanup — identify and remove any static event handlers on services
- ResourceDictionary cleanup — remove orphaned/unreferenced dictionaries from MergedDictionaries

### WS-F: Diagnostics & Monitoring Finalization

- DiagnosticsService snapshot coverage audit (all windows, all render modes)
- LoggingService rotation validation (5MB rollover, 3-file rotation, AppData path)
- ValidationEngine inline color scan across ALL .xaml files (not just known locations)
- Missing token detection coverage (validate all 70+ tokens expected by control templates)
- Diagnostics panel UX polish (loading states, error states, empty states)
- Plugin diagnostics integration (module state, version, memory in diagnostics panel)
- Performance counters or lightweight telemetry for long-running sessions
- Crash recovery validation (corrupt theme settings, missing resource dictionaries)

### WS-G: Documentation & Audit Deliverables

- `Docs/Architecture/PERFORMANCE_AUDIT_REPORT.md` — perf findings, measurements, optimizations applied
- `Docs/Architecture/ACCESSIBILITY_AUDIT_REPORT.md` — keyboard nav, focus, contrast, screen reader results
- `Docs/Architecture/DPI_AUDIT_REPORT.md` — scaling validation at all DPIs, multi-monitor results
- `Docs/Architecture/EXCEL_STABILITY_REPORT.md` — host test pass, stress test, memory stability results
- `Docs/Architecture/LOCALIZATION_READINESS.md` — document architecture readiness for future i18n
- `Docs/Architecture/ENTERPRISE_POLISH_CHECKLIST.md` — Phase 10 validation checklist
- Update `AGENTS.md` with Phase 10 paths, build commands, audit references

## Key Tasks

| ID | Task | Workstream |
|----|------|------------|
| P10-T001 | Run comprehensive performance audit — profile memory, rendering, layout passes, visual tree depth | WS-A |
| P10-T002 | Optimize rendering — reduce layout passes, simplify visual trees, optimize bindings | WS-A |
| P10-T003 | Optimize memory — implement Freeze() on all freezable resources, fix leaks, add disposal patterns | WS-A |
| P10-T004 | Optimize animations — validate all storyboards ≤200ms, no jank, reduced-motion support | WS-A |
| P10-T005 | Optimize startup — cold-start timing, background initialization, lazy resource loading | WS-A |
| P10-T006 | Optimize resource dictionaries — deduplicate MergedDictionaries, remove orphans | WS-A |
| P10-T007 | Run full accessibility audit — keyboard nav, focus visuals, automation properties, contrast | WS-B |
| P10-T008 | Fix all accessibility violations found in audit — focus traps, missing labels, contrast failures | WS-B |
| P10-T009 | Implement screen reader preparation — AutomationProperties on all interactive elements | WS-B |
| P10-T010 | Validate reduced motion and high contrast mode across all themes | WS-B |
| P10-T011 | Run DPI audit — 100%, 125%, 150%, 200% — clipping, overflow, alignment | WS-C |
| P10-T012 | Implement per-monitor DPI awareness — popup positioning, window sizing, font scaling | WS-C |
| P10-T013 | Validate multi-monitor mixed DPI configurations | WS-C |
| P10-T014 | Document DPI regression checklist | WS-C |
| P10-T015 | Close all 15 remaining Phase 3 validation tasks (WS-A T022/T031/T032/T038/T043/T044/T048/T049/T050; WS-B T057–T062) | WS-D |
| P10-T016 | Run full Excel VSTO host test pass — all 14 windows, theme switching, popups, safe mode | WS-D |
| P10-T017 | Run DataGrid virtualization stress test (10,000+ rows) in Excel host | WS-D |
| P10-T018 | Validate memory stability during extended Excel session (2+ hours) | WS-D |
| P10-T019 | Migrate ToastWindow.xaml from plain &lt;Window&gt; to controls:ModernWindow | WS-E |
| P10-T020 | Run final hardcoded-value grep sweep — zero #HEX, zero inline effects, zero hardcoded margins outside Theme/Base/ | WS-E |
| P10-T021 | Fix ValidationEngine TODO — implement proper XAML/dictionary resource resolver | WS-E |
| P10-T022 | Fill MVVM_COMPLIANCE.md per-file audit table with pass/fail for all 10 files | WS-E |
| P10-T023 | Run dispose pattern audit — IDisposable on all services with unmanaged resources | WS-E |
| P10-T024 | Run event handler unsubscription audit — prevent memory leaks from lingering subscriptions | WS-E |
| P10-T025 | Validate DiagnosticsService snapshot coverage — all windows, all render modes | WS-F |
| P10-T026 | Validate LoggingService — rotation, path, rollover behavior | WS-F |
| P10-T027 | Extend ValidationEngine — inline color scan across ALL .xaml files, 70+ token coverage | WS-F |
| P10-T028 | Polish diagnostics panel UX — loading, error, and empty states | WS-F |
| P10-T029 | Validate crash recovery — corrupt theme settings, missing dictionaries | WS-F |
| P10-T030 | Create PERFORMANCE_AUDIT_REPORT.md | WS-G |
| P10-T031 | Create ACCESSIBILITY_AUDIT_REPORT.md | WS-G |
| P10-T032 | Create DPI_AUDIT_REPORT.md | WS-G |
| P10-T033 | Create EXCEL_STABILITY_REPORT.md | WS-G |
| P10-T034 | Create LOCALIZATION_READINESS.md | WS-G |
| P10-T035 | Create ENTERPRISE_POLISH_CHECKLIST.md | WS-G |
| P10-T036 | Update AGENTS.md with Phase 10 paths and audit references | WS-G |
| P10-T037 | Production validation — build passes, Excel host test, all checklists green | ALL |

## Phase 10 Acceptance Criteria

- [ ] Performance audit complete with report (P10-T001 through P10-T006)
- [ ] All animations within 200ms budget, reduced motion supported (P10-T004)
- [ ] All freezable resources frozen, no resource leaks detected (P10-T003)
- [ ] Startup time measured and optimized (P10-T005)
- [ ] Accessibility audit complete with report (P10-T007 through P10-T010)
- [ ] WCAG 2.1 AA contrast (4.5:1) met in Dark, Light, all Custom themes
- [ ] All interactive elements have AutomationProperties.Name, keyboard navigable, visible focus indicators
- [ ] DPI audit complete, multi-monitor mixed DPI validated (P10-T011 through P10-T014)
- [ ] All 15 Phase 3 validation tasks closed (P10-T015)
- [ ] Excel VSTO test pass green across all 14 windows (P10-T016, P10-T017, P10-T018)
- [ ] ToastWindow.xaml migrated to ModernWindow (P10-T019)
- [ ] Zero hardcoded #HEX, inline effects, or hardcoded margins outside Theme/Base/ (P10-T020)
- [ ] ValidationEngine TODO resolved (P10-T021)
- [ ] MVVM_COMPLIANCE.md fully populated (P10-T022)
- [ ] Dispose pattern and event handler audits clean (P10-T023, P10-T024)
- [ ] All diagnostic services validated, panels polished (P10-T025 through P10-T029)
- [ ] All 6 audit/report documents created (P10-T030 through P10-T035)
- [ ] AGENTS.md updated (P10-T036)
- [ ] Production validation green — build, host, all checklists (P10-T037)
- [ ] GitHub PR created and approved
- [ ] CodeRabbit review clean
- [ ] Manual architecture review passed

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
