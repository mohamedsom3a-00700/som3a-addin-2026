# Quickstart: Full Platform Rebranding

**Phase 1 — Developer Onboarding Guide**
**Date**: 2026-05-29

## Prerequisites

- [ ] Brand assets delivered by external designer and placed in `WpfApp2/Assets/Branding/` (see [contracts/brand-token-interface.md](./contracts/brand-token-interface.md) for directory structure)
- [ ] Font files bundled in `Assets/Branding/Fonts/` or confirmed system-installed
- [ ] Existing solution builds cleanly before starting rebranding work

## Work Order

### Step 1: Brand Asset Infrastructure
Create `Assets/Branding/` directory structure per the [directory contract](./contracts/brand-token-interface.md#2-asset-directory-contract). Validate all prerequisite assets are in place.

### Step 2: Theme Token Updates
1. Update `WpfApp2/Theme/Base/Colors.xaml` — Add new brand Primitive tokens (see [contracts/brand-token-interface.md](./contracts/brand-token-interface.md#primitive-tokens-colorsxaml))
2. Update `WpfApp2/Theme/Base/Typography.xaml` — Add font family tokens for all presets (Inter, Segoe UI Variable, Cairo, IBM Plex Sans Arabic, Tajawal)
3. Update `WpfApp2/Theme/Dark/DarkColors.xaml` — Remap Semantic tokens to dark brand palette
4. Update `WpfApp2/Theme/Light/LightColors.xaml` — Remap Semantic tokens to light brand palette
5. Update `WpfApp2/Theme/Custom/CustomColors.xaml` — Add background image, blur, font selection support
6. Update `WpfApp2/Theme/Effects/Glow.xaml` — Add new brand glow effects

### Step 3: ThemeManager Updates
Extend `ThemeManager.cs` with:
- `ApplyFontPreset(string presetName)` — Switch shell fonts dynamically
- `ApplyBackgroundImage(string imagePath)` — Set custom background
- `ApplyBlurIntensity(double intensity)` — Adjust blur level

### Step 4: Shell Branding
1. Update `ShellSidebar.xaml` — Add Planova logo at top, branding footer at bottom
2. Update `HomePage.xaml` — Add product branding section, version info, release notes card

### Step 5: Splash Screen
Create `SplashWindow.xaml` with:
- Planova logo display
- Blueprint line animation
- Building formation animation
- Logo reveal with glow effects
- Static fallback mode for VSTO rendering limitations

### Step 6: Ribbon Icons
Replace all ribbon icons in `WpfApp2/Ribbon/` with new Planova engineering-style icons (provided as prerequisites).

### Step 7: WCAG Validation
Run automated color contrast audit against all new brand palette combinations. Document results.

### Step 8: Performance Validation
- Measure splash screen duration (target: ≤3s)
- Measure font switch time (target: ≤1s)
- Measure full theme switch time with all effects (target: ≤1s)

## Key Files

| File | Purpose |
|------|---------|
| `WpfApp2/Theme/Base/Colors.xaml` | Brand Primitive token definitions |
| `WpfApp2/Theme/Base/Typography.xaml` | Font family token definitions |
| `WpfApp2/Theme/Dark/DarkColors.xaml` | Dark theme Semantic token overrides |
| `WpfApp2/Theme/Light/LightColors.xaml` | Light theme Semantic token overrides |
| `WpfApp2/Theme/Custom/CustomColors.xaml` | Custom theme extensions |
| `WpfApp2/Services/ThemeManager.cs` | Font/background/blur switching API |
| `WpfApp2/Views/SplashWindow.xaml` | Animated splash screen |
| `WpfApp2/Pages/HomePage.xaml` | Home dashboard branding |
| `WpfApp2/Controls/ShellSidebar.xaml` | Sidebar logo and footer |
| `WpfApp2/Assets/Branding/` | All brand asset files (prerequisite) |
