# Implementation Plan: Full Platform Rebranding & Visual Identity System

**Branch**: `026-platform-rebranding` | **Date**: 2026-05-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/025-platform-rebranding/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Transform the Som3a Addin 2026 visual identity into Planova Platform with a complete enterprise rebranding: new brand color palettes (dark engineering + light engineering-white), animated splash screen, shell sidebar logo/branding, ribbon iconography, typography system (English + Arabic), custom theme enhancements (background images, blur, font selection), brand asset folder structure, and WCAG 2.1 AA accessibility validation. Namespace migration (Som3a.* → Planova.*) is deferred to post-stabilization.

## Technical Context

**Language/Version**: C# (.NET Framework 4.8 for VSTO/WPF host, .NET 8.0 for domain libraries)

**Primary Dependencies**: Existing Theme Engine, ThemeManager, ModernWindow, Shell workspace, MaterialDesignThemes (icons), WPF (.NET Framework 4.8)

**Storage**: File-based — Assets/Branding/ folder structure for logo files, font files, master brand reference; existing Properties.Settings.Default for theme/font preferences

**Testing**: Automated WCAG 2.1 AA color contrast audit, automated color sampling, visual audit against master brand reference, VSTO host splash screen test, theme switch performance measurement, regression test of existing UI functionality

**Target Platform**: Windows x64 — Excel VSTO Add-in host (.NET Framework 4.8)

**Project Type**: Desktop application (WPF) — VSTO Add-in

**Performance Goals**: Splash screen ≤3s, font switch ≤1s, full theme switch ≤1s

**Constraints**: DynamicResource-only for themeable properties (Constitution §III), ThemeManager-governed mutations (§IV), centralized effects (§XII), Excel rendering safety (§X), WindowChrome enforcement (§XI), animation governance (§IX with splash exemption)

**Scale/Scope**: Single desktop application; rebranding touches all shell surfaces, theme dictionaries, ribbon assets, and startup flow

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Per the [Project Constitution](../.specify/memory/constitution.md), every implementation plan MUST verify:

- [x] **I. Library-First Modular Architecture** — Feature adds new theme token dictionaries and brand asset files; no monolithic dictionaries introduced. Resources remain isolated and testable.
- [x] **III. DynamicResource-Only** — All new brand palette colors and theme properties use DynamicResource. No StaticResource introduced.
- [x] **III. DynamicResource-Only** — All new brand palette colors and theme properties use DynamicResource. No StaticResource introduced.
- [x] **IV. Runtime Theme Mutation Governance** — Theme mutation path continues through ThemeManager exclusively. New font switching and background image toggles integrate via ThemeManager.
- [x] **IX. Animation Governance** — Splash screen startup animation exempt (clarified); runs up to 3s. All runtime UI animations (logo glow, icon transitions) ≤200ms, GPU-safe.
- [x] **X. Excel Rendering Safety** — Splash screen tested in VSTO host; fallback to static logo if rendering limitations detected. WindowRenderModeDetector respected.
- [x] **XI. WindowChrome Enforcement** — No new standalone windows. Splash is temporary startup window; shell remains primary workspace via ModernWindow.
- [x] **XII. Centralized Effects** — New glass effects, glow accents, and blueprint overlays sourced from Effects/Shadows.xaml and Effects/Glow.xaml. No inline effects.
- [x] **XV. Resource Loading Order** — New brand palette token dictionaries follow prescribed loading sequence (Primitive → Semantic → Typography → ... → Theme Overrides).

**Re-check (post-design):** All items confirmed. No constitution violations detected in the generated artifacts.

## Project Structure

### Documentation (this feature)

```text
specs/025-platform-rebranding/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
WpfApp2/
├── Theme/
│   ├── Base/
│   │   ├── Colors.xaml              # Update with brand palette Primitive tokens
│   │   ├── Typography.xaml          # Add font family tokens (Inter, Segoe UI, Cairo, etc.)
│   │   └── Spacing.xaml             # No changes expected
│   ├── Dark/
│   │   ├── DarkColors.xaml          # Update Semantic tokens to dark brand palette
│   │   └── DarkTheme.xaml           # Add blueprint overlay resources
│   ├── Light/
│   │   ├── LightColors.xaml         # Update Semantic tokens to light brand palette
│   │   └── LightTheme.xaml          # Add engineering-white refinements
│   ├── Custom/
│   │   ├── CustomColors.xaml        # Add accent override, background image support
│   │   └── CustomTheme.xaml         # Add blur intensity, font selection resources
│   ├── Effects/
│   │   ├── Glow.xaml                # Add new brand glow effects (cyan, orange)
│   │   └── Animations.xaml          # Add splash animation storyboards
│   └── ThemeResources.xaml          # Ensure loading order compliance
│
├── Services/
│   ├── ThemeManager.cs              # Add font switching, background image, blur APIs
│   └── ThemeSettings.cs             # Add font selection persistence
│
├── Views/
│   └── SplashWindow.xaml            # NEW — Animated splash screen with Planova branding
│
├── Pages/
│   └── HomePage.xaml                # Update with product branding section + version info
│
├── Controls/
│   └── ShellSidebar.xaml            # Update with Planova logo, animated icon, branding footer
│
└── Assets/
    └── Branding/                    # NEW — Brand asset folder (prerequisite from designer)
        ├── Master/
        │   └── planova-master-brand-reference.png
        ├── Logos/
        │   ├── SVG/
        │   ├── PNG/
        │   └── ICO/
        ├── Ribbon/
        ├── Splash/
        ├── Wallpapers/
        ├── Icons/
        ├── Theme/
        └── Fonts/
```

**Structure Decision**: Single WPF application project with existing Theme/ directory extended. No new projects required for this phase. Brand assets stored in WpfApp2/Assets/Branding/ alongside the existing codebase.

## Complexity Tracking

> No constitution violations detected — Complexity Tracking section not required.

## Generated Artifacts

- `research.md` — Phase 0: No NEEDS CLARIFICATION items remained after `/speckit.clarify`; research confirms all decisions documented in spec clarifications.
- `data-model.md` — Phase 1: Data model for brand tokens, typography presets, brand assets.
- `quickstart.md` — Phase 1: Developer onboarding for rebranding work.
