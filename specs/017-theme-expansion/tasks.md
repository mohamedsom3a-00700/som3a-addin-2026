# Tasks: Theme Expansion

**Input**: Design documents from `/specs/017-theme-expansion/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not explicitly requested — omitted. Manual verification per quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All source is in `WpfApp2/`. Theme resources at `WpfApp2/Theme/`, Services at `WpfApp2/Services/`, Controls at `WpfApp2/Controls/`, Views at `WpfApp2/Views/`, ViewModels at `WpfApp2/ViewModels/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install NuGet dependencies and register new resource dictionaries in loading order.

- [X] T001 Install MaterialDesignThemes NuGet package (v5.x) in WpfApp2/Som3a_WPF_UI.csproj
- [X] T002 [P] Add new settings keys (WindowBackdropStyle, BackgroundImagePath, BackgroundBlurIntensity, BackgroundBlurEnabled, SelectedFontFamily) to WpfApp2/Properties/Settings.settings
- [X] T003 [P] Create empty Theme/MaterialIntegration.xaml — will be populated in Phase 2

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core ThemeManager extensions, Material integration bridge, and persistence wiring that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Extend ThemeManager.ApplyTheme() in WpfApp2/Services/ThemeManager.cs to call new ApplyBackground(), ApplyFont(), GenerateAccentVariants(), ValidateContrast() subroutines (stubbed initially, filled by respective stories)
- [X] T005 Extend ThemeManager.SaveCurrentTheme() and LoadThemeFromSettings() in WpfApp2/Services/ThemeManager.cs to persist/restore new WindowBackdropStyle, BackgroundImagePath, BackgroundBlurIntensity, BackgroundBlurEnabled, SelectedFontFamily from Properties.Settings.Default
- [X] T006 Implement Theme/MaterialIntegration.xaml — merge MaterialDesignTheme.Defaults.xaml only (NOT Light/Dark), define bridging resources (MaterialDesignBody → {DynamicResource Brush.Text.Primary}, MaterialDesignPaper → {DynamicResource Brush.Surface.Primary}, MaterialDesignTextBoxBorder → {DynamicResource Brush.Border.Default})
- [X] T007 Register MaterialIntegration.xaml in WpfApp2/Theme/ThemeResources.xaml MergedDictionaries at position after Base/Opacity.xaml and before Control styles

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — Material Design Icons & Controls (Priority: P1) 🎯 MVP

**Goal**: Replace generic glyphs with Material PackIcon icons in sidebar, toolbar, ribbon; integrate Material DialogHost for dialogs; replace sliders/toggles/chips with Material equivalents.

**Independent Test**: Open Shell workspace in dark theme → verify all sidebar categories, navigation items, toolbar buttons, and ribbon groups display Material Design icons. Switch to light theme → icons adapt. Open any dialog → renders through DialogHost.

### Implementation for User Story 1

- [X] T008 [P] [US1] Create Theme/Controls/MaterialIcons.xaml — implicit PackIcon style with Foreground="{DynamicResource Brush.Accent.Primary}", define standard icon sizes (sidebar=24, toolbar=20, ribbon=16)
- [X] T009 [P] [US1] Create Theme/Controls/MaterialControls.xaml — control templates for MaterialDesign Slider (active track → Brush.Accent.Primary, inactive → Brush.Surface.CardSubtle), ToggleButton (Material ripple + Fluent accent), Chip (Material template + Fluent surface/text tokens)
- [X] T010 [US1] Register MaterialIcons.xaml and MaterialControls.xaml in WpfApp2/Theme/ThemeResources.xaml MergedDictionaries after existing Control styles, before ModernWindow.xaml
- [X] T011 [US1] Replace all static icon references in WpfApp2/Theme/ShellStyles.xaml (sidebar categories, navigation items) with PackIcon elements using the implicit style from MaterialIcons.xaml
- [X] T012 [P] [US1] Replace any remaining glyph/emoji icon usage in toolbar, ribbon, and page headers across WpfApp2/Views/ and WpfApp2/Pages/ with PackIcon elements
- [X] T013 [US1] Configure MaterialDesign DialogHost as root dialog container in ModernWindow content area in WpfApp2/Theme/ModernWindow.xaml — wrap ContentPresenter in `<materialDesign:DialogHost Identifier="RootDialog">`
- [X] T014 [US1] Verify all existing dialogs (message boxes, confirmations, errors) route through DialogHost by testing theme switching, settings save, and error scenarios

**Checkpoint**: Material icons render in sidebar and all navigation. Dialogs use Material DialogHost. Sliders/Toggles/Chips styled with Fluent tokens.

---

## Phase 4: User Story 6 — Window Controls Unification (Priority: P1)

**Goal**: All windows have consistent minimize/maximize/restore/close buttons with theme-aware hover/pressed states and correct drag regions.

**Independent Test**: Open Shell, Settings, and any dialog window → hover each button → verify red close hover, accent-colored min/max hover, correct 100ms transitions. Verify title bar drag works but buttons are excluded.

### Implementation for User Story 6

- [X] T015 [P] [US6] Refine WpfApp2/Theme/Controls/WindowButtonStyles.xaml — unify button sizing (Close=46x32, Min/Max=36x32), ensure CloseButton hover uses Brush.CloseButton.HoverBackground, Min/Max use Brush.TitleBar.ButtonHover, all with 100ms CubicEase transition
- [X] T016 [US6] Update WpfApp2/Theme/ModernWindow.xaml title bar template — confirm Grid column 1 (title text) has WindowChrome.IsHitTestVisibleInChrome="True", column 2 (button StackPanel) has WindowChrome.IsHitTestVisibleInChrome="False"
- [X] T017 [US6] Verify window control buttons render correctly at 100%, 150%, and 200% DPI by launching add-in at each scale
- [X] T018 [US6] Verify window control buttons render correctly in FallbackSafe mode (WindowRenderModeDetector safe mode) — buttons remain functional with standard WindowStyle if safe mode active

**Checkpoint**: Window controls uniform across all windows, drag region correct, DPI scaling verified.

---

## Phase 5: User Story 2 — Background Image & Blur (Priority: P2)

**Goal**: Users can select a background image for the Shell workspace, adjust blur intensity, and toggle blur on/off. Falls back gracefully when image is missing or DWM unsupported.

**Independent Test**: Open Custom theme settings → pick a JPG/PNG image → verify it appears as Shell workspace background within 1s. Adjust blur slider → blur changes in real-time. Switch to Dark theme → image preserved when returning to Custom.

### Implementation for User Story 2

- [X] T019 [P] [US2] Create WpfApp2/Services/DwmBlurService.cs — P/Invoke DwmSetWindowCompositionAttribute with ACCENT_ENABLE_BLURBEHIND, expose EnableBlur(IntPtr hwnd, double intensity) and IsBlurSupported (check OS >= Win10 1709, DWM composition, safe mode)
- [X] T020 [P] [US2] Create WpfApp2/Theme/Effects/Backdrop.xaml — define ImageBrush resources for background, solid/gradient fallback Brush resources, BlurEnabled and BlurIntensity binding hooks
- [X] T021 [US2] Register Backdrop.xaml in WpfApp2/Theme/ThemeResources.xaml MergedDictionaries after Effects/Glow.xaml
- [X] T022 [US2] Extend WpfApp2/Controls/ModernWindow.cs — wire WindowBackdrop DP to DWM blur service; add SetBackground(string imagePath) method that validates file (exists, ≤10MB, ≤4096px, .png/.jpg/.jpeg/.bmp), loads ImageBrush, applies via DwmBlurService
- [X] T023 [US2] Create WpfApp2/ViewModels/CustomThemeViewModel.cs — BackgroundType property (Solid/Gradient/Image), ImagePath, BlurIntensity (0.0–1.0), BlurEnabled; SelectImageCommand (OpenFileDialog with image filter), ClearImageCommand
- [X] T024 [US2] Create WpfApp2/Views/CustomThemeSettings.xaml — file picker button with selected filename label, blur intensity slider (0-100%), blur enable/disable ToggleButton, image size/dimension validation error display
- [X] T025 [US2] Wire CustomThemeSettings.xaml into SettingsPage.xaml navigation under Appearance category
- [X] T026 [US2] Extend ThemeManager with ApplyBackground(string imagePath, double blurIntensity) in WpfApp2/Services/ThemeManager.cs — calls ModernWindow.SetBackground, handles fallback (missing image → solid backdrop, corrupt image → error message, DWM unsupported → sharp background)
- [X] T027 [US2] Verify background image falls back to solid/gradient when: (a) image file deleted after selection, (b) file exceeds 10MB, (c) file exceeds 4096px, (d) corrupt image selected

**Checkpoint**: Background image applies to Shell workspace, blur works on supported systems, graceful fallback on unsupported.

---

## Phase 6: User Story 5 — Accent Customization (Priority: P2)

**Goal**: Users pick accent via color wheel, hex input, or auto-generated variant swatches. All accent changes propagate across windows in real-time.

**Independent Test**: Open Settings → Appearance → use color wheel to select custom accent → verify real-time preview. Enter hex color → exact color applied. Click variant swatches → correct HSL-derived colors. All windows update within 500ms.

### Implementation for User Story 5

- [X] T028 [P] [US5] Create WpfApp2/Controls/ColorWheel.cs — custom UserControl with WriteableBitmap (256x256) HSV wheel rendering; MouseDown/MouseMove → polar→HSV→RGB; SelectedColor DependencyProperty; preview circle overlay
- [X] T029 [P] [US5] Extend ThemeManager.GenerateAccentVariants(Color baseColor) in WpfApp2/Services/ThemeManager.cs — compute 5 variants (Hover/Pressed/Glow/Border/Subtle) via HSL transformations per research.md R5; set Accent.Color.Hover/Pressed/Glow/Border/Subtle and Accent.Brush.* resources; enforce minimum ΔE ≥ 5 perceptual distance
- [X] T030 [P] [US5] Add helper methods RgbToHsl(Color) and HslToRgb(double,double,double) to WpfApp2/Services/ThemeManager.cs (or shared ColorHelper utility)
- [X] T031 [US5] Update WpfApp2/Views/AppearancePanel.xaml — add ColorWheel control inside Custom theme section, add hex color TextBox with regex validation (^#[0-9A-Fa-f]{6}$), add 5 auto-generated variant swatches (labeled Hover, Pressed, Glow, Border, Subtle) below existing 8 preset swatches
- [X] T032 [US5] Update WpfApp2/ViewModels/SettingsViewModel.cs — add SelectedCustomColor property, HexColorText property (two-way binding with validation), GeneratedVariants ObservableCollection, wire ColorWheel drag and hex input to call ThemeManager.ApplyAccentColor() with 100ms debounce via DispatcherTimer
- [X] T033 [US5] Verify accent changes propagate to all open windows (Shell, Settings, dialogs) within 500ms; verify Glow.xaml effects update with new accent color

**Checkpoint**: Color wheel functional, hex input works, 5 variant swatches generated correctly, all windows update on accent change.

---

## Phase 7: User Story 3 — Font System (Priority: P2)

**Goal**: Users browse installed fonts with preview thumbnails, select a font, and see it applied dynamically to all windows without restart.

**Independent Test**: Open font selection in Custom theme settings → browse fonts with preview thumbnails → select a font → verify all window text updates within 500ms without restart. Switch themes → font selection preserved.

### Implementation for User Story 3

- [X] T034 [P] [US3] Create WpfApp2/Services/FontEnumerator.cs — GetSystemFonts() returns List<FontFamilyInfo> (Name, FamilyName, IsArabicCompatible via GlyphTypeface.CharacterToGlyphMap for U+0600–U+06FF range), GeneratePreview(FontFamily, string sampleText) returns BitmapSource thumbnail (200x40px)
- [X] T035 [P] [US3] Create WpfApp2/Controls/FontPreview.cs — UserControl displaying font thumbnail + family name label, click selection with highlight border, SelectedFont DependencyProperty
- [X] T036 [US3] Add FontFamily="{DynamicResource CustomFontFamily}" fallback to "Segoe UI" on root-level implicit TextBlock, Label, Button styles in existing WpfApp2/Theme/Controls/LabelStyles.xaml and WpfApp2/Theme/Controls/ButtonStyles.xaml
- [X] T037 [US3] Extend ThemeManager with ApplyFont(string fontFamilyName) in WpfApp2/Services/ThemeManager.cs — calls SetResource("CustomFontFamily", new FontFamily(fontFamilyName)); handles fallback (font unavailable → Segoe UI + notification)
- [X] T038 [US3] Update WpfApp2/Views/AppearancePanel.xaml (or CustomThemeSettings.xaml) — add FontFamily items list with FontPreview controls, Arabic-compatible font section (Phase 24 readiness), font change applies on selection
- [X] T039 [US3] Update WpfApp2/ViewModels/CustomThemeViewModel.cs — add AvailableFonts ObservableCollection, SelectedFont property, ApplyFontCommand; call FontEnumerator.GetSystemFonts() on load
- [X] T040 [US3] Verify font change propagates to all open windows (Shell, Settings, dialogs, popups, context menus) within 500ms without visual flicker; verify Arabic fonts render correctly when an Arabic-compatible font is selected

**Checkpoint**: Font browsing with previews works, font changes propagate to all windows, Arabic font detection functional, fallback to Segoe UI on missing font works.

---

## Phase 8: User Story 4 — Light Theme WCAG Compliance (Priority: P3)

**Goal**: Both dark and light themes meet WCAG 2.1 AA contrast ratios (4.5:1 normal text, 3:1 large text). All semantic token pairs pass programmatic validation.

**Independent Test**: Switch to light theme → run contrast checker → all token pairs meet 4.5:1/3:1 minimum. Switch to dark theme → same validation passes. Disabled controls clearly distinguishable from enabled.

### Implementation for User Story 4

- [X] T041 [US4] Implement ThemeManager.ValidateContrast() in WpfApp2/Services/ThemeManager.cs — iterate defined semantic token pairs, compute relative luminance, calculate contrast ratio (L1+0.05)/(L2+0.05), log warnings for non-compliant pairs (below 4.5:1 normal or 3:1 large text)
- [X] T042 [US4] Define token pair validation list as static readonly array in ThemeManager.cs: Brush.Text.Primary vs Brush.Surface.Primary, Brush.Text.Secondary vs Brush.Surface.Primary, Brush.Text.Primary vs Brush.Surface.Card, Brush.Accent.Primary vs Brush.Surface.Primary, Brush.Text.OnAccent vs Brush.Accent.Primary
- [X] T043 [P] [US4] Fix non-compliant tokens in WpfApp2/Theme/Light/LightColors.xaml — darken TextSecondary (target: ≥ #5B7186 on #FFFFFF surface), ensure disabled states are distinguishable (opacity 0.4 on disabled controls)
- [X] T044 [P] [US4] Fix non-compliant tokens in WpfApp2/Theme/Dark/DarkColors.xaml — lighten TextSecondary (target: ≥ #94A3B8 on #0E1720 surface), verify text-on-accent contrast for all swatch accent colors
- [X] T045 [US4] Call ValidateContrast() from ThemeManager after each theme switch (add to end of ApplyThemeInternal) and log results via existing ILoggingService
- [X] T046 [US4] Manually verify contrast with external tool (e.g., Colour Contrast Analyser) for top 10 most-used token pairs in both themes

**Checkpoint**: Both themes pass WCAG 2.1 AA. Contrast validation runs on every theme switch. Non-compliant tokens flagged in logs.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, safe mode validation, Excel VSTO testing, and constitution compliance review.

- [X] T047 [P] Verify all new resource dictionaries follow DynamicResource-only convention (no StaticResource for themeable brushes/colors/borders/effects) — audit MaterialIcons.xaml, MaterialControls.xaml, Backdrop.xaml, MaterialIntegration.xaml
- [X] T048 [P] Verify WindowRenderModeDetector safe mode: DWM blur disabled, solid backdrop fallback active, window controls still functional, no black-window rendering
- [X] T049 Run full build: MSBuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug — must pass with zero errors
- [X] T050 Launch in Excel VSTO host — verify theme switching (Dark↔Light↔Custom) with all customizations preserved (background image, blur, font, accent), Material icons render in sidebar, no rendering glitches
- [X] T051 Verify rapid theme switching debounce — switch theme 5 times in 2 seconds, verify no visual flicker and final state is correct
- [X] T052 Verify customizations preserved across application restart (open, set background+font+accent, close, reopen — all settings restore)
- [X] T053 Constitution compliance review: verify DynamicResource-only (III), ThemeManager exclusivity (IV), animations ≤200ms (IX), Excel VSTO safety (X), WindowChrome (XI), centralized effects (XII), loading order (XV), fallback recovery (XVI)
- [X] T054 Run quickstart.md validation checklist end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational — P1 MVP
- **User Story 6 (Phase 4)**: Depends on Foundational — P1, independent of US1 (different files)
- **User Story 2 (Phase 5)**: Depends on Foundational — P2, independent of US1/US6
- **User Story 5 (Phase 6)**: Depends on Foundational — P2, independent of US1/US2/US6
- **User Story 3 (Phase 7)**: Depends on Foundational — P2, independent of US1/US2/US5/US6
- **User Story 4 (Phase 8)**: Depends on Foundational + US5 (needs ThemeManager.ApplyTheme path for contrast validation) — P3
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational — No dependencies on other stories
- **US6 (P1)**: Can start after Foundational — No dependencies on other stories
- **US2 (P2)**: Can start after Foundational — No dependencies on other stories
- **US5 (P2)**: Can start after Foundational — No dependencies on other stories
- **US3 (P2)**: Can start after Foundational — No dependencies on other stories
- **US4 (P3)**: Can start after US5 (indirect — needs ThemeManager's post-switch hook for ValidateContrast)

### Within Each User Story

- Services before Views (ThemeManager methods before UI binding)
- XAML resources before code-behind wiring
- Core implementation before integration with SettingsPage

### Parallel Opportunities

- All Setup tasks T001-T003 can run in parallel
- T004 and T005 are sequential within Foundational
- Once Foundational completes, US1, US6, US2, US5, US3 can ALL start in parallel (5 streams)
- Within US1: T008, T009 parallel → T010 → T011, T012 parallel → T013 → T014
- Within US2: T019, T020 parallel → T021 → T022 → T023, T024 parallel → T025 → T026 → T027
- Within US5: T028, T029, T030 parallel → T031, T032 parallel → T033
- Within US3: T034, T035 parallel → T036 → T037 → T038, T039 parallel → T040
- Within US4: T043, T044 parallel → T041 → T042 → T045 → T046

---

## Parallel Example: User Story 1

```bash
# Launch parallel tasks:
Task: "T008 Create MaterialIcons.xaml with implicit PackIcon style"
Task: "T009 Create MaterialControls.xaml with Slider/Toggle/Chip templates"

# After T008+T009 complete:
Task: "T010 Register both in ThemeResources.xaml"

# Then parallel:
Task: "T011 Replace icons in ShellStyles.xaml"
Task: "T012 Replace icons in Views/ and Pages/"
```

## Parallel Example: User Story 2

```bash
# Launch parallel tasks:
Task: "T019 Create DwmBlurService.cs"
Task: "T020 Create Backdrop.xaml"

# Then sequential:
Task: "T021 Register Backdrop.xaml in ThemeResources.xaml"
Task: "T022 Extend ModernWindow.cs with SetBackground()"

# Then parallel:
Task: "T023 Create CustomThemeViewModel.cs"
Task: "T024 Create CustomThemeSettings.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 6)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 — Material Design Icons & Controls
4. Complete Phase 4: US6 — Window Controls
5. **STOP and VALIDATE**: Test US1+US6 independently — verify Material icons everywhere, window controls unified
6. Deploy/demo if ready (visual upgrade alone is impactful)

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Material icons/controls) + US6 (window controls) → Test → Demo (visual transformation!)
3. Add US2 (Background/blur) → Test → Demo (personalization)
4. Add US5 (Accent customization) → Test → Demo (color expression)
5. Add US3 (Font system) → Test → Demo (typography)
6. Add US4 (WCAG) → Test → Demo (accessibility compliance)
7. Polish → Full validation

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Material icons) + US6 (Window controls)
   - Developer B: US2 (Background/blur) + US5 (Accent)
   - Developer C: US3 (Fonts)
3. After US5 completes → Developer B picks up US4 (WCAG)
4. All converge on Polish phase together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- ThemeManager.cs is the only shared file — additive changes (new methods), no conflicting rewrites
- ThemeResources.xaml MergedDictionaries additions are append-only in loading order
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Build verification after each phase before proceeding
