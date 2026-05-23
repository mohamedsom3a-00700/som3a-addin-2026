# Quickstart: Control Standardization (Phase 4)

**Date**: 2026-05-22
**Prerequisites**: Phase 3 (Theme Engine) complete, `msbuild` passes

---

## Before You Start

1. Read [spec.md](./spec.md) — understand the 5 user stories and 14 FRs
2. Read [research.md](./research.md) — understand the 10 design decisions
3. Read [data-model.md](./data-model.md) — understand the entities and relationships
4. Read [plan.md](./plan.md) — understand the overall approach

---

## Implementation Order

### Step 1: Audit all control templates (P4-T001)

**Files**: All `.xaml` in `Theme/Controls/` + all `.xaml` in `Views/`

**Commands**:
```powershell
# Count styles per TargetType
Select-String -Path "WpfApp2\Theme\Controls\*.xaml" -Pattern 'TargetType="(\w+)"' | Group-Object { $_.Matches[0].Groups[1].Value }

# Find inline styles in window files
Select-String -Path "WpfApp2\Views\*.xaml" -Pattern '<Style '

# Find hardcoded colors in controls
Select-String -Path "WpfApp2\Theme\Controls\*.xaml" -Pattern 'Color="#[0-9A-Fa-f]+"'

# Find inline DropShadowEffect
Select-String -Path "WpfApp2\Theme\Controls\*.xaml" -Pattern 'DropShadowEffect'
```

**Output**: Audit report listing duplicates, inline styles, hardcoded colors, and inline shadows.

---

### Step 2: Refactor ComboBox popup (P4-T002)

**File**: `WpfApp2\Theme\Controls\ComboBoxStyles.xaml`

**Changes**:
- Verify `AllowsTransparency="False"` on Popup
- Verify `Placement="Bottom"` with smart direction detection
- Verify `PlacementTarget` binding to parent ComboBox
- Add width synchronization with parent control
- Ensure safe-mode shadow variant: `{DynamicResource Shadow.Popup}` for normal, `{DynamicResource Shadow.Popup.Safe}` for safe mode
- Remove any inline width clipping

**Review**: `WpfApp2\Theme\Controls\ComboBoxItemStyles.xaml` for consistency

---

### Step 3: Standardize TextBox (P4-T003)

**File**: `WpfApp2\Theme\Controls\TextBoxStyles.xaml`

**Changes**:
- Add complete VSM states: Normal, MouseOver, Focused, Disabled
- Focus state: accent-colored border transition
- Disabled state: reduced opacity via `{DynamicResource Opacity.Disabled}`
- All border/background colors via `{DynamicResource Brush.*}`

---

### Step 4: Standardize DataGrid (P4-T004)

**File**: `WpfApp2\Theme\Controls\DataGridStyles.xaml`

**Changes**:
- Verify `EnableRowVirtualization="True"` and `VirtualizationMode="Recycling"`
- Verify `ScrollUnit="Item"` (required for row virtualization)
- Replace any remaining hardcoded colors with semantic tokens
- Add empty state placeholder trigger
- Ensure keyboard navigation: Arrow keys for cell navigation, Tab for cell exit

---

### Step 5: Standardize ListView (P4-T005)

**File**: `WpfApp2\Theme\Controls\ListViewStyles.xaml`

**Changes**:
- Add virtualization: `VirtualizingStackPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`
- Standardize selection colors via `{DynamicResource Brush.DataGrid.SelectedRow}` (or equivalent semantic token)
- Add empty state placeholder trigger
- Apply thin scrollbar style

---

### Step 6: Add virtualization (P4-T006)

**Files**: `DataGridStyles.xaml`, `ListViewStyles.xaml`, TreeView styles

**Changes**:
- Apply virtualization settings to all scrollable containers
- Remove any `ScrollUnit="Pixel"` that conflicts with virtualization
- Test with 1000+ rows to verify 30+ FPS scrolling

---

### Step 7: Add smooth scrolling (P4-T007)

**Files**: `ScrollViewerStyles.xaml`, all scrollable control styles

**Changes**:
- Ensure `ScrollViewer.CanContentScroll="True"` for virtualization compatibility
- Apply thin scrollbar style (4px track, 8px thumb) — already in `ScrollViewerStyles.xaml`
- Remove any `ScrollViewer.ManipulationMode` that may conflict

---

### Step 8: Add accessibility states (P4-T008)

**Files**: All control styles

**Changes**:
- Add `FocusVisualStyle="{DynamicResource Glow.Focus}"` to all interactive controls
- Verify `AutomationProperties.Name` on all interactive elements (T046 pattern from Phase 3)
- Verify `IsTabStop="True"` on all focusable controls
- Verify Enter/Space activation on all controls
- Add reduced-motion triggers for all animations (FR-010)

---

### Step 9: Remove duplicate styles (P4-T009)

**Action**: For each duplicate found in Step 1 audit:
1. Compare property setters between duplicate and canonical style
2. If identical: remove duplicate, update references
3. If divergent: flag for manual review per clarification decision
4. After resolution: verify build passes and no visual regressions

---

### Step 10: Validate popup rendering (P4-T010)

**Tests**:
- Open every window with ComboBox → verify popup width, shadow, no clipping
- Test near window bottom → verify popup opens upward
- Test in safe mode → verify AllowsTransparency=False variant
- Test at 100% and 150% DPI → verify correct sizing

---

## Build & Verify

```powershell
# Build
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Verify no hardcoded colors remain in controls
Select-String -Path "WpfApp2\Theme\Controls\*.xaml" -Pattern 'Color="#[0-9A-Fa-f]+"'
# Expected: zero results (colors only in Base/Colors.xaml and theme override files)

# Verify no inline DropShadowEffect
Select-String -Path "WpfApp2\Theme\Controls\*.xaml" -Pattern 'DropShadowEffect'
# Expected: zero results (effects only in Effects/Shadows.xaml and Effects/Glow.xaml)
```

---

## Success Criteria Checklist

- [ ] SC-001: Zero duplicate styles — passed audit
- [ ] SC-002: All ComboBox popups render correctly — validated all windows
- [ ] SC-003: DataGrid 1000+ rows at 30+ FPS — performance validated
- [ ] SC-004: All controls keyboard-reachable — full traversal test
- [ ] SC-005: Zero hardcoded colors/shadows in controls — grep audit
- [ ] SC-006: All animations ≤200ms; reduced motion disables them
- [ ] SC-007: Zero inline DropShadowEffect outside Effects/ — grep audit
