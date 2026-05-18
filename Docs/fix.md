# 📘 Modern Fluent Window System Migration Guide
## Som3a Addin 2026

---

# 📌 Document Information

| Item | Value |
|---|---|
| Project | Som3a Addin 2026 |
| Target | Unified Modern Fluent Window Architecture |
| Frameworks | .NET Framework 4.8 + .NET 10 WPF |
| UI Stack | WPF + VSTO + WebView2 |
| Architecture | MVVM + Fluent Design |
| Status | Planning / Migration |
| Priority | Critical |
| Risk Level | Medium-High |
| Last Updated | May 2026 |

---

# 1. 🎯 PRIMARY OBJECTIVE

Build a complete enterprise-grade unified window system for the entire WPF layer of Som3a Addin 2026.

The migration must:

- Eliminate duplicated title bar implementations
- Unify ALL windows visually and behaviorally
- Improve rendering performance
- Improve DPI support
- Improve VSTO stability
- Create reusable architecture
- Reduce long-term maintenance cost
- Preserve existing MVVM architecture
- Preserve existing Theme system
- Preserve existing services and controls

---

# 2. 🧱 EXISTING ARCHITECTURE CONTEXT

The solution already contains:

```text
Som3a.Shared
WpfApp2
Som3a Addin 2026
```

---

# Existing Systems Already Implemented

## Theme System
- ThemeManager
- ThemeResources.xaml
- Dark Theme
- White Theme
- Accent Colors
- Fluent design tokens

## UI Systems
- ToastService
- WebView2 integration
- Shared converters
- MVVM ViewModels
- Modern buttons
- Existing Fluent resources

## Existing Window Types
- Forms
- DataGrid-heavy windows
- Graph windows
- WebView2 windows
- Tool dialogs
- Progress windows

---

# 3. ❌ CURRENT PROBLEMS

| Problem | Impact |
|---|---|
| Mixed Window styles | Inconsistent UX |
| Duplicate title bars | High maintenance |
| AllowsTransparency=True | GPU/rendering issues |
| Multiple resize implementations | Runtime bugs |
| No DPI strategy | 4K scaling issues |
| No VSTO-safe fallback | Excel hosting risk |
| No architectural enforcement | Future inconsistency |
| Duplicate chrome buttons | Technical debt |
| Different shadows/radius | Unprofessional appearance |

---

# 4. 🏗 TARGET ARCHITECTURE

---

# Unified Window System

All windows must inherit from:

```csharp
ModernWindow
```

---

# Required Shared Features

Every window must share:

- Same TitleBar
- Same shadows
- Same radius
- Same animations
- Same resize behavior
- Same snap behavior
- Same DPI handling
- Same theme integration
- Same Fluent styling

---

# Target UX Style

Inspired by:

- Windows 11 Fluent
- Notion
- Discord
- Modern Office
- Professional enterprise desktop apps

---

# 5. ⚠️ CRITICAL ENGINEERING DECISIONS

---

# 5.1 USE WindowChrome

## REQUIRED

Use:

```csharp
WindowChrome
```

---

# DO NOT USE

```xml
AllowsTransparency="True"
WindowStyle="None"
```

except as emergency fallback mode.

---

# WHY WindowChrome

| Feature | Benefit |
|---|---|
| Native resize | Better UX |
| GPU acceleration | Better performance |
| Native shadows | Cleaner rendering |
| Snap support | Windows 11 behavior |
| Better maximize | Proper monitor handling |
| Better DPI handling | 4K support |
| Better VSTO stability | Fewer runtime bugs |

---

# 5.2 Incremental Migration ONLY

## DO NOT

Migrate all windows immediately.

---

## REQUIRED STRATEGY

1. Build foundation
2. Validate on ONE window
3. Validate inside Excel
4. Validate DPI
5. Validate WebView2
6. Then migrate gradually

---

# 5.3 Fallback Strategy REQUIRED

WindowChrome can fail in:
- Excel hosting
- Multi-monitor
- WebView2 scenarios

A fallback strategy is mandatory.

---

# 6. 📂 REQUIRED FOLDER STRUCTURE

```text
Controls/
    ModernWindow.cs

Theme/
    ModernWindow.xaml
    WindowAnimations.xaml
    WindowButtonStyles.xaml

Helpers/
    WindowChromeHelper.cs
    WindowBehaviorHelper.cs
    DpiHelper.cs

Behaviors/
    DragMoveBehavior.cs
    EscapeCloseBehavior.cs

Analyzers/
    ModernWindowAnalyzer.cs
```

---

# 7. 🚀 PHASED IMPLEMENTATION PLAN

---

# ✅ PHASE 1 — FOUNDATION SYSTEM

## Priority
CRITICAL

## Risk
LOW

## Goal
Build reusable architecture BEFORE migration.

---

# Deliverables

## Create Base Window

```csharp
Controls/ModernWindow.cs
```

Responsibilities:
- Shared window behavior
- Drag support
- Resize handling
- Maximize/restore
- ESC close support
- Window state persistence
- Theme integration

---

## Create Shared Template

```xml
Theme/ModernWindow.xaml
```

Must include:
- Unified title bar
- Shared buttons
- Shared animations
- Shared shadows
- Shared radius
- Shared content presenter

---

## Create WindowChrome Helper

```csharp
Helpers/WindowChromeHelper.cs
```

Responsibilities:
- Configure WindowChrome
- Handle resize borders
- Handle maximize behavior
- Handle monitor changes
- Handle DPI scaling

---

## Create Shared Animation Resources

```xml
Theme/WindowAnimations.xaml
```

Animations:
- Fade in
- Hover transitions
- Pressed states
- Smooth button transitions

---

# Validation Window

ONLY migrate:

```text
StyleSelectorWindow
```

This becomes:
- Prototype window
- Validation target
- Regression baseline

---

# REQUIRED VALIDATION

Before Phase 2:
- Resize works
- Snap works
- Maximize works
- Theme switching works
- Excel hosting works
- DPI works
- Multi-monitor works

---

# ✅ PHASE 2 — VSTO VALIDATION

## Priority
CRITICAL

## Risk
HIGH

## Goal
Validate WindowChrome safely inside Excel.

---

# REQUIRED HOSTING CHECK

```csharp
public static bool IsVstoHosted =>
    Application.Current?.MainWindow == null ||
    Process.GetCurrentProcess()
        .ProcessName
        .Contains("EXCEL", StringComparison.OrdinalIgnoreCase);
```

---

# REQUIRED BEHAVIOR

If:

```csharp
IsVstoHosted == true
```

then optionally disable:
- Aggressive shadows
- NonClientFrameEdges
- Experimental acrylic
- Certain resize optimizations

---

# REQUIRED FALLBACK STRATEGY

---

# OPTION A — Safe Window Mode

Fallback to lightweight custom chrome.

```xml
WindowStyle=None
```

ONLY if WindowChrome becomes unstable.

---

# OPTION B — Isolated WPF Host

Use:
```csharp
HwndSource
```

or isolated WPF process.

---

# OPTION C — Runtime Compatibility Switch

```csharp
UseSafeWindowMode
```

loaded from config/settings.

---

# REQUIRED VALIDATION MATRIX

| Scenario | Required |
|---|---|
| Excel Hosted | YES |
| Standalone WPF | YES |
| 1080p | YES |
| 4K | YES |
| Multi-monitor | YES |
| WebView2 | YES |
| Maximized | YES |
| DPI switching | YES |

---

# ✅ PHASE 3 — CORE WINDOW MIGRATION

## Priority
MEDIUM

## Risk
MEDIUM

---

# Migration Order

## 1. SettingsWindow
Low risk validation.

## 2. AssignTradeCodesWindow
Tests forms + progress + ListView.

## 3. PrimaveraCompareWindow
Tests DataGrids + bindings.

## 4. PrimaveraResultsWindow
Tests large DataGrids + tabs.

---

# REQUIRED CLEANUP

Remove from all migrated windows:

```xml
AllowsTransparency="True"
WindowStyle="None"
Background="Transparent"
```

Also remove:
- Old title bars
- Duplicate close buttons
- Duplicate borders
- Manual drag logic

---

# ✅ PHASE 4 — ADVANCED WINDOW MIGRATION

## Priority
MEDIUM

## Risk
HIGH

---

# Windows

- Float_path
- XerEditorWindow
- LinksManagerWindow
- SubDailyReportWindow
- UnmergeFillDownWindow
- Fixpiecolors

---

# WHY HIGH RISK

These contain:
- WebView2
- Graph rendering
- Heavy layouts
- Large visual trees
- Complex bindings

---

# REQUIRED TESTING

- GPU rendering
- Resize performance
- DPI switching
- WebView2 stability
- Memory usage
- Excel interaction

---

# ✅ PHASE 5 — HARDENING & ENFORCEMENT

## Priority
HIGH

## Risk
LOW

---

# Roslyn Analyzer Guard

Create:

```text
Analyzers/ModernWindowAnalyzer.cs
```

---

# Purpose

Prevent future developers from creating raw WPF windows.

---

# Warning Rule

Warn if:

```csharp
public partial class MyWindow : Window
```

instead of:

```csharp
public partial class MyWindow : ModernWindow
```

---

# Additional Runtime Validation

Optional startup checks:
- Missing resources
- DPI mode
- WindowChrome support
- Invalid themes
- Missing DynamicResources

---

# 8. 🖥 DPI STRATEGY (CRITICAL)

---

# REQUIRED MANIFEST UPDATE

## app.manifest

Must contain:

```xml
<dpiAwareness>PerMonitorV2</dpiAwareness>
```

---

# WHY THIS IS REQUIRED

Without PerMonitorV2:
- Blur on 4K monitors
- Broken scaling
- Incorrect resize zones
- Wrong snap calculations
- WindowChrome artifacts

---

# REQUIRED DPI FEATURES

Must support:
- Monitor switching
- Dynamic DPI changes
- Mixed DPI environments
- Maximized calculations
- Resize grip scaling

---

# REQUIRED WINDOWCHROME DPI SUPPORT

Special care required for:

```csharp
WindowChrome.NonClientFrameEdges
```

especially:
- Inside Excel
- On secondary monitors
- During monitor drag

---

# 9. ⚡ PERFORMANCE REQUIREMENTS

---

# MUST OPTIMIZE FOR

- Excel VSTO hosting
- GPU rendering
- Large DataGrids
- WebView2
- Multi-window scenarios

---

# AVOID

- Heavy transparency
- Software rendering
- Deep visual trees
- Multiple nested shadows
- Expensive blur effects

---

# USE

- Shared templates
- DynamicResource
- Virtualization
- Lightweight visual trees
- Shared animations

---

# 10. 🎨 VISUAL DESIGN REQUIREMENTS

---

# Radius

```text
18
```

---

# Shadows

Soft Fluent shadows only.

---

# Typography

Use existing Fluent typography system.

---

# Theme Support

Must support:
- Dark Blue
- White
- Accent colors

---

# REQUIRED RESOURCE USAGE

Use existing resources ONLY:

```text
WindowBackgroundBrush
CardBrush
CardStrokeBrush
TextMainBrush
TextSubBrush
PrimaryButton
BaseButton
GhostButton
```

DO NOT replace ThemeResources.xaml architecture.

---

# 11. 🧩 REQUIRED TITLE BAR FEATURES

Every window must include:

- App title
- Optional icon
- Minimize
- Maximize/Restore
- Close button
- Hover states
- Pressed states
- Keyboard accessibility
- Drag region

---

# REQUIRED BUTTON FEATURES

Buttons must:
- Use DynamicResources
- Animate smoothly
- Support keyboard focus
- Have Fluent hover states
- Have pressed states

---

# 12. 🔒 ACCESSIBILITY REQUIREMENTS

Must support:
- Keyboard navigation
- Focus visuals
- Screen scaling
- High DPI
- Proper tab order
- ESC close optional
- Accessibility-friendly hit targets

---

# 13. 🧪 REQUIRED TESTING CHECKLIST

---

# Rendering Tests

- Window resize
- Maximize
- Restore
- Snap
- Minimize
- Multi-monitor drag

---

# Theme Tests

- Dark theme
- White theme
- Accent switching

---

# Performance Tests

- WebView2 rendering
- Large DataGrid performance
- Excel responsiveness

---

# Stability Tests

- Open/close repeatedly
- Monitor switching
- DPI switching
- VSTO startup
- Window activation

---

# 14. 📘 REQUIRED OUTPUT FROM IMPLEMENTATION

Implementation must generate FULL production-ready code:

- Full XAML
- Full C#
- Full resource dictionaries
- Full WindowChrome setup
- Behaviors
- Helpers
- Manifest updates
- Migration examples

---

# 15. 📈 SUCCESS CRITERIA

Migration succeeds ONLY if:

- All windows share one visual system
- VSTO remains stable
- WebView2 remains stable
- DPI issues are eliminated
- Performance improves
- Duplicate chrome code is removed
- Future windows are forced into consistency

---

# 16. 🏁 FINAL TARGET EXPERIENCE

The final application should feel like:

- Premium enterprise software
- Modern Fluent desktop app
- Commercial-grade product
- High-end Windows application

Users should immediately notice:
- Smooth interaction
- Unified UX
- Better rendering
- Professional polish
- Consistent architecture

---

# 17. 📌 FINAL NOTES

This migration is:
- Architectural
- Performance-sensitive
- VSTO-sensitive
- DPI-sensitive

The migration MUST prioritize:
1. Stability
2. Incremental rollout
3. Fallback safety
4. Maintainability
5. Long-term consistency

DO NOT optimize for speed of migration.
Optimize for correctness and architectural quality.

---

# ✅ END OF DOCUMENT