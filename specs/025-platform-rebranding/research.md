# Research: Full Platform Rebranding & Visual Identity System

**Phase 0 — Research & Decision Record**
**Date**: 2026-05-29

## Introduction

All ambiguities were resolved during `/speckit.clarify`. This document consolidates the design decisions and their rationale.

## Decisions

### D1: Splash Animation Duration

- **Decision**: Splash screen animation is exempt from the Constitution §IX 200ms limit.
- **Rationale**: Splash is a startup/loading sequence, not a runtime UI animation. The constitution targets runtime micro-interactions and theme transitions. Splash may run up to 3s (matching SC-001).
- **Alternatives considered**: A) Constrain to 200ms (would reduce splash to a static logo fade-in, losing brand impact). C) Sequence multiple ≤200ms animations (adds complexity without meaningful benefit).

### D2: Accessibility Compliance

- **Decision**: WCAG 2.1 AA with formal contrast validation included in this phase.
- **Rationale**: Industry standard for enterprise software. Prevents expensive rework if accessibility issues surface post-release. New brand colors (neon cyan, blueprint overlays) need verification against 4.5:1 ratio.
- **Alternatives considered**: A) No validation (highest risk). C) AAA (too restrictive for accent colors). D) Defer to future phase (would delay issue detection).

### D3: Font Fallback Strategy

- **Decision**: Font stack with predefined fallback chain (e.g., Cairo → Arial → System UI for Arabic; Inter → Segoe UI → System UI for English).
- **Rationale**: Provides resilience without administrator intervention. Silent fallback with logging prevents user confusion.
- **Alternatives considered**: A) Hard fail on missing font (disruptive). C) Silent fallback to system default (users may not notice failed selection). D) Download on demand (adds network dependency).

### D4: Theme Switch Performance Target

- **Decision**: Full theme switch completes in under 1 second.
- **Rationale**: Matches existing font-switching target (SC-003) and master plan performance goal. Consistent user experience across all branding elements.
- **Alternatives considered**: A) 500ms (aggressive; glass/glow effects may need lazy loading). C) 2s (feels sluggish for a UI action).

### D5: Brand Asset Creation & Timing

- **Decision**: Brand assets (logos, master reference) are prerequisites delivered by an external designer before this phase begins.
- **Rationale**: Decouples design work from engineering. Allows parallel progress. This phase implements the infrastructure that consumes the assets.
- **Alternatives considered**: A) Create assets within this phase (blocks engineering until designs are ready). C) Hybrid (core logos as prerequisite, ribbon icons as deliverable — adds coordination complexity).

## Technology Best Practices

### WPF Splash Screen Implementation
- Use a separate WPF window displayed before the main shell window loads
- Ensure compatibility with VSTO hosting (test in-process)
- Provide static fallback if DWM composition or GPU acceleration is unavailable
- Follow existing pattern from ModernWindow/WindowChrome for consistency

### Theme Dictionary Pattern
- All new color tokens added as Primitive tokens in Colors.xaml
- Semantic tokens reference Primitive tokens (e.g., `WindowBackgroundBrush` → `{StaticResource Primitive.Surface.Dark}`)
- Theme override dictionaries (Dark/DarkColors.xaml) remap Semantic tokens per theme
- Font family tokens added to Typography.xaml

### WCAG 2.1 AA Contrast Validation
- Use automated tools (e.g., Colour Contrast Analyser, WCAG Contrast Checker) to verify all text/background combinations
- Normal text (<18pt): minimum 4.5:1 contrast ratio
- Large text (≥18pt or ≥14pt bold): minimum 3:1 contrast ratio
- Non-text elements (icons, UI components): minimum 3:1 against adjacent colors
- Document all tested combinations and results
