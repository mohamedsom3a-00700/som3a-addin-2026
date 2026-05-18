# Som3a Add-in 2026 — Spec Kit Execution Plan

## Sprint Name
`UI Foundation & Theme Engine`

## Goal
Transform the current custom-styled WPF UI into a production-grade Fluent Design System with:
- Runtime theme switching
- Enterprise-level architecture
- Reusable theme engine
- Modern Fluent UI styling
- Full DynamicResource support
- Professional UX consistency

---

# Phase 0 — Baseline Snapshot

## Goal
Freeze the current UI state before refactoring.

---

## Tasks

### 1. Create New Branch

```bash
git checkout -b feature/theme-engine-v2
```

---

### 2. Snapshot Current Architecture

Document and review current structure for:

```text
Theme/
Controls/
Styles/
Windows/
Views/
Resources/
```

---

### 3. Upload Important Files

Required files:
- App.xaml
- ModernWindow.xaml
- ModernWindow.cs
- Current ComboBox styles
- Theme dictionaries
- SettingsWindow
- Resource dictionaries

---

# Phase 1 — Spec Kit Foundation

## Goal
Generate the architecture specification for the new Fluent Theme System.

---

# Task 1 — Generate UI Architecture Spec

## Spec Kit Prompt

```text
Create a complete WPF Fluent UI architecture specification for a VSTO Add-in.

Requirements:
- Runtime theme switching
- Dark / Light / Custom themes
- Modular ResourceDictionary structure
- MVVM-friendly architecture
- Theme-aware controls
- Fluent UI inspired visuals
- Acrylic-like depth
- Shadow system
- Typography token system
- DynamicResource usage only
- High DPI support
- Reusable control styles

Generate:
1. Folder structure
2. ResourceDictionary hierarchy
3. Theme dependency graph
4. Runtime theme manager architecture
5. Recommended naming conventions
6. Shared control standards
7. Styling standards
8. Performance guidelines
```

---

# Phase 2 — Theme Engine Architecture

## Goal
Convert the project into a real runtime theme engine.

---

# Task 2 — Theme Dictionary Planning

## Required Structure

```text
Theme/
 ├── Base/
 │    ├── Colors.xaml
 │    ├── Typography.xaml
 │    ├── Spacing.xaml
 │    ├── Radius.xaml
 │    └── Elevation.xaml
 │
 ├── Dark/
 │    ├── DarkColors.xaml
 │    └── DarkTheme.xaml
 │
 ├── Light/
 │    ├── LightColors.xaml
 │    └── LightTheme.xaml
 │
 ├── Custom/
 │    ├── CustomColors.xaml
 │    └── CustomTheme.xaml
 │
 ├── Controls/
 │    ├── Button.xaml
 │    ├── ComboBox.xaml
 │    ├── TextBox.xaml
 │    ├── DataGrid.xaml
 │    ├── ScrollViewer.xaml
 │    ├── CheckBox.xaml
 │    ├── Window.xaml
 │    └── ProgressBar.xaml
 │
 ├── Effects/
 │    ├── Shadows.xaml
 │    ├── Animations.xaml
 │    └── Glow.xaml
 │
 └── ThemeManager.xaml
```

---

# Task 3 — DynamicResource Migration

## Goal

Replace all:

```xaml
StaticResource
```

with:

```xaml
DynamicResource
```

for every theme-aware visual resource.

---

# Task 4 — Create Token System

## Required Tokens

### Color Tokens
- Background
- Surface
- Border
- Accent
- Text

### Typography Tokens
- Title
- Subtitle
- Body
- Caption

### Radius Tokens
- Small
- Medium
- Large

### Elevation Tokens
- Shadows
- Glow
- Depth

### Animation Tokens
- Fade
- Hover
- Press
- Focus

---

# Phase 3 — Core UI Stabilization

## Goal
Fix all critical UI rendering and consistency issues.

---

# Task 5 — ComboBox Refactor

## Current Problems
- Popup not visible
- Popup layering issues
- Theme inconsistency
- Missing focus states

---

## OpenCode Prompt

```text
Refactor the custom WPF ComboBox style into a production-ready Fluent UI ComboBox.

Requirements:
- Fix Popup rendering issue
- Popup must render above all containers
- Add drop shadow
- Add hover/focus/open states
- Use DynamicResource brushes only
- Support dark/light/custom themes
- Add smooth animations
- Use keyboard accessible states
- Preserve current fluent aesthetic
- Ensure compatibility with transparent borderless windows
```

---

# Task 6 — Window Shadow System

## Create

```text
Theme/Effects/Shadows.xaml
```

---

## Required Shadows

```text
Shadow.Window
Shadow.Small
Shadow.Medium
Shadow.Large
Shadow.Glow
```

---

# Task 7 — ModernWindow Refactor

## Goal
Convert ModernWindow into a production Fluent shell.

---

## Required Features

- Transparent borderless support
- Window shadows
- DPI awareness
- Acrylic depth
- Better title bar buttons
- Theme-aware chrome
- Runtime theme updates

---

# Phase 4 — Settings UX Rebuild

## Goal
Transform settings into a professional Fluent settings experience.

---

# Task 8 — Theme Cards

## Replace

```text
Theme: [Dark ▼]
```

## With

```text
[ Dark Theme Card ]
[ Light Theme Card ]
[ Custom Theme Card ]
```

---

## Requirements

Each card must include:
- Theme preview
- Accent visualization
- Selection glow
- Description
- Smooth animation

---

## Spec Kit Prompt

```text
Design a Fluent UI WPF Settings Appearance page using theme selection cards.

Requirements:
- Dark theme card
- Light theme card
- Custom theme card
- Active selection animation
- Modern Fluent styling
- Responsive layout
- MVVM friendly
- Runtime theme switching integration
```

---

# Phase 5 — Control Standardization

## Goal
Unify all controls under one design language.

---

## Controls To Refactor

### Inputs
- TextBox
- ComboBox
- PasswordBox

### Selection
- CheckBox
- RadioButton
- ToggleButton

### Containers
- GroupBox
- Expander
- Cards

### Data Controls
- DataGrid
- ListView
- TreeView

---

# Phase 6 — Animation & Motion

## Create

```text
Theme/Effects/Animations.xaml
```

---

## Required Motion System

- Hover transitions
- Fade animations
- Glow transitions
- Elevation transitions
- Press feedback
- Focus transitions

---

# Phase 7 — Runtime Theme Manager

## Goal
Enable real-time runtime theme switching.

---

## Create

```text
Services/ThemeManager.cs
```

---

## Responsibilities

- Load dictionaries
- Replace merged dictionaries
- Apply themes globally
- Save user preferences
- Support custom themes

---

# Phase 8 — Advanced Polish

## Goals

- Acrylic effects
- Glow system
- Fluent elevation
- Accessibility
- Keyboard navigation
- Responsive layouts
- UI consistency

---

# Tool Responsibilities

| Tool | Responsibility |
|---|---|
| Spec Kit | Architecture + Specifications |
| OpenCode | Refactor + Code Generation |
| ChatGPT | PM + UI Direction + Review |

---

# Critical Rules

## NEVER

```xaml
Background=\"#XXXXXX\"
```

inside controls.

---

## ALWAYS

```xaml
Background=\"{DynamicResource Brush.Background.Primary}\"
```

---

# Runtime Theme Rules

## MUST

- Support runtime switching
- Use DynamicResource
- Be globally applied
- Avoid hardcoded colors
- Be MVVM-friendly

---

# Current Active Sprint

## Sprint Name

```text
UI Foundation & Theme Engine
```

---

## Sprint Goals

1. Fix ComboBox popup rendering
2. Create shadow system
3. Build theme token system
4. Refactor ModernWindow
5. Create runtime theme architecture
6. Improve Settings UX
7. Standardize controls
8. Prepare Custom Theme system

---

# Definition of Done

A UI feature is NOT complete unless:

- Theme-aware
- Runtime switchable
- DPI safe
- Keyboard accessible
- Uses DynamicResource
- Uses centralized tokens
- Supports Dark/Light/Custom themes
- Matches Fluent UI direction
- Has proper hover/focus states
- Maintains visual consistency