# Tasks: Full Platform Rebranding & Visual Identity System

**Input**: Design documents from `/specs/025-platform-rebranding/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: No explicit test tasks requested in the spec. Validation tasks included per Success Criteria.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Main project**: `WpfApp2/` (VSTO/WPF host, .NET Framework 4.8)
- **Theme**: `WpfApp2/Theme/`
- **Brand assets**: `WpfApp2/Assets/Branding/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, brand asset prerequisite verification, folder structure creation

- [X] T001 Create Assets/Branding/ folder structure per contract — Logos/, Ribbon/, Splash/, Wallpapers/, Icons/, Theme/, Fonts/, Master/ subdirectories in WpfApp2/Assets/Branding/
- [X] T002 [P] Verify all prerequisite brand assets are present (logo SVG, PNG sizes, ICO, master reference) in WpfApp2/Assets/Branding/ per contracts/brand-token-interface.md
- [X] T003 [P] Verify prerequisite font files are bundled in WpfApp2/Assets/Branding/Fonts/ or confirmed system-installed
- [X] T004 [P] Confirm existing solution builds cleanly before rebranding modifications

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Brand palette token definitions, typography tokens, centralized effects — MUST be complete before any user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Define brand Primitive tokens in WpfApp2/Theme/Base/Colors.xaml — add Primitive.Brand.* entries for all 8 brand palette colors (Background, Surface, Border, AccentBlue, AccentCyan, AccentOrange, TextPrimary, TextSecondary)
- [X] T006 [P] Define additional Primitive tokens (BlueprintGlow, GlassBorder) in WpfApp2/Theme/Base/Colors.xaml
- [X] T007 [P] Add font family Primitive tokens in WpfApp2/Theme/Base/Typography.xaml — Inter, Segoe UI Variable, Cairo, IBM Plex Sans Arabic, Tajawal with fallback chains
- [X] T008 Remap dark theme Semantic tokens in WpfApp2/Theme/Dark/DarkColors.xaml to new brand palette values
- [X] T009 [P] Remap light theme Semantic tokens in WpfApp2/Theme/Light/LightColors.xaml to new brand palette values
- [X] T010 [P] Add custom theme extensions (background image DP, blur intensity DP, font selection) in WpfApp2/Theme/Custom/CustomTheme.xaml
- [X] T011 [P] Add new brand glow effects (cyan accent, orange accent) in WpfApp2/Theme/Effects/Glow.xaml
- [X] T012 [P] Add blueprint overlay effect resources in WpfApp2/Theme/Effects/Shadows.xaml
- [X] T013 Verify ThemeResources.xaml loading order includes all new dictionaries at correct positions per Constitution §XV

**Checkpoint**: Foundation ready — brand tokens, typography, effects configured. User story implementation can begin.

---

## Phase 3: User Story 1 - User Sees New Planova Branding on Startup (Priority: P1) 🎯 MVP

**Goal**: User sees animated Planova splash screen with blueprint animation, logo reveal, glow effects. Window title displays "Planova Platform".

**Independent Test**: Launch application — splash renders with Planova branding and animation, then main window opens with "Planova Platform" title bar, taskbar icon shows Planova.

### Implementation for User Story 1

- [X] T014 [US1] Create SplashWindow.xaml in WpfApp2/Views/ with Planova logo display, blueprint line animation canvas, and glow effects (Storyboard-based, ≤3s total)
- [X] T015 [US1] Implement splash animation storyboards in WpfApp2/Theme/Effects/Animations.xaml — blueprint line draw, building formation, logo reveal, glow pulse
- [X] T016 [P] [US1] Add static fallback mode to SplashWindow — display static Planova logo when GPU/DWM composition unavailable (test in VSTO host)
- [X] T017 [US1] Implement SplashWindow lifecycle — show on startup, transition to main shell after animation completion or max 3s timeout
- [X] T018 [US1] Update App.xaml.cs to launch SplashWindow before main Shell window
- [X] T019 [US1] Update all window title bars to display "Planova Platform" in WpfApp2/Controls/ModernWindow.cs
- [X] T020 [US1] Set Planova icon for EXE, taskbar, and system tray — update WpfApp2/app.manifest and project properties

**Checkpoint**: At this point, User Story 1 should be fully functional — splash screen plays on startup, Planova name/icon shown everywhere.

---

## Phase 4: User Story 2 - User Navigates Rebranded Shell with New Visual Identity (Priority: P1)

**Goal**: User sees Planova logo in sidebar, new brand palette across all themes, rebranded ribbon icons with unified engineering style.

**Independent Test**: Open shell — sidebar shows Planova logo with animated icon, dark/light themes render correct brand colors, all ribbon icons show new engineering-style design.

### Implementation for User Story 2

- [X] T021 [P] [US2] Add Planova logo image element at top of WpfApp2/Controls/Shell/SidebarControl.xaml with mini animated icon
- [X] T022 [P] [US2] Add branding footer element at bottom of WpfApp2/Controls/Shell/SidebarControl.xaml
- [X] T023 [US2] Verify dark theme surfaces render with new palette (Background #0E1720, Surface #13202B, Accent Blue #2D9CFF, Accent Cyan #00D1FF, Accent Orange #FF8A3D) — update references in WpfApp2/Theme/Dark/DarkTheme.xaml
- [X] T024 [US2] Verify light theme surfaces render with new palette (Background #F5F7FA, Surface #FFFFFF, Accent Blue #2D9CFF, Accent Cyan #00B8E6) — update references in WpfApp2/Theme/Light/LightTheme.xaml
- [ ] T025 [P] [US2] Replace all ribbon icons in WpfApp2/Ribbon/ with new Planova engineering-style icons (SVG/PNG prerequisite from designer) — match dark and light theme compatibility
- [X] T026 [US2] Update HomePage.xaml product branding section with Planova logo, version information, and release notes card
- [X] T027 [US2] Add blueprint-inspired overlay visuals to shell workspace background in ShellSidebar.xaml or Shell workspace container

**Checkpoint**: User Stories 1 AND 2 functional — full shell rebranding visible, all themes use new palette.

---

## Phase 5: User Story 3 - User Benefits from New Typography System (Priority: P2)

**Goal**: User can choose English or Arabic font presets from settings, fonts switch dynamically, RTL shaping works correctly.

**Independent Test**: Open appearance settings — select Inter font → shell UI updates immediately. Select Cairo font → Arabic text renders with correct shaping.

### Implementation for User Story 3

- [ ] T028 [US3] Extend ThemeManager.cs — add ApplyFontPreset(string presetName) method that swaps Typography.xaml font family resources
- [ ] T029 [P] [US3] Add font selection UI to appearance settings page in WpfApp2/Pages/SettingsPage.xaml — dropdown or card selector for all 5 presets
- [ ] T030 [US3] Implement font file loading — load bundled TTF/WOFF from WpfApp2/Assets/Branding/Fonts/ or use System.Windows.Interop for system fonts
- [ ] T031 [US3] Test font fallback chain — simulate missing font, verify fallback to next family in chain (e.g., Cairo → Arial → System UI)
- [ ] T032 [P] [US3] Verify Arabic font presets (Cairo, IBM Plex Sans Arabic, Tajawal) render with correct shaping and ligatures in RTL mode
- [ ] T033 [US3] Add font preview thumbnails in appearance settings for each preset
- [ ] T034 [US3] Persist selected font preset to Properties.Settings.Default so it restores on next launch

**Checkpoint**: User Stories 1–3 functional — full rebranding plus dynamic typography system working.

---

## Phase 6: User Story 4 - Administrator Accesses Brand Assets for Enterprise Deployment (Priority: P3)

**Goal**: Administrator finds all brand assets organized in Assets/Branding/ folder structure with required formats and documentation.

**Independent Test**: Navigate to Assets/Branding/ — Logos/ contains SVG, PNG (5 sizes), ICO. All required subdirectories present with at least one file each.

### Implementation for User Story 4

- [X] T035 [P] [US4] Finalize WpfApp2/Assets/Branding/Logos/ with all format variants — SVG source, PNG 64/128/256/512/1024, ICO, Transparent, Dark, Light, Monochrome sub-variants
- [X] T036 [P] [US4] Place master brand reference file at WpfApp2/Assets/Branding/Master/planova-master-brand-reference.png
- [ ] T037 [P] [US4] Organize ribbon icons in WpfApp2/Assets/Branding/Ribbon/ for documentation/reference (deferred to Fluent UI migration — see future-plan.md)
- [X] T038 [P] [US4] Place splash screen assets in WpfApp2/Assets/Branding/Splash/
- [X] T039 [US4] Add wallpapers, theme decorative assets (blueprint overlays, background textures) to WpfApp2/Assets/Branding/Wallpapers/ and WpfApp2/Assets/Branding/Theme/

**Checkpoint**: All 4 user stories functional — full platform rebranding complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: WCAG validation, performance measurement, compliance review

- [X] T040 [P] Run automated WCAG 2.1 AA color contrast audit across all brand palette text/background combinations — document results in WpfApp2/Docs/Branding/contrast-audit.md
- [ ] T041 [P] Measure splash screen duration — verify ≤3s on target hardware, log results (requires runtime execution)
- [ ] T042 [P] Measure font switch time — verify ≤1s across all shell UI elements (requires runtime execution)
- [ ] T043 [P] Measure full theme switch time (dark→light, light→dark, custom→any) — verify ≤1s (requires runtime execution)
- [ ] T044 [P] Verify 100% existing UI functionality — run existing test suite, confirm zero regressions in theme switching, navigation, VSTO interop (requires runtime execution)
- [X] T045 [P] Constitution compliance review — verify DynamicResource-only usage, ThemeManager integration for font/background/blur APIs, Excel rendering safety with WindowRenderModeDetector, WindowChrome inheritance in SplashWindow
- [X] T046 Run quickstart.md validation — verify all steps produce expected results

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3–6)**: All depend on Foundational phase completion
  - US1 and US2 (both P1) can proceed in parallel or sequentially
  - US3 (P2) depends on US1/US2 for shell UI integration
  - US4 (P3) can proceed independently
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational — shares theme tokens with US1 but works independently
- **User Story 3 (P2)**: Can start after Foundational — font selection UI integrates into appearance settings, independently testable
- **User Story 4 (P3)**: Can start after Foundational — purely asset organization, independently testable

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- Once Foundational phase completes, US1, US2, US3, and US4 can all start in parallel (if team capacity allows)
- Models/assets within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all parallel tasks for User Story 1 together:
Task: "T016 [P] [US1] Add static fallback mode to SplashWindow"
Task: "T019 [US1] Update all window title bars"
Task: "T020 [US1] Set Planova icon for EXE"

# Sequential after parallel tasks complete:
Task: "T014 [US1] Create SplashWindow.xaml" (depends on T012 effects)
Task: "T015 [US1] Implement splash animation storyboards"
Task: "T017 [US1] Implement SplashWindow lifecycle"
Task: "T018 [US1] Update App.xaml.cs"
```

## Parallel Example: User Story 2

```bash
# Launch all parallel tasks together:
Task: "T021 [P] [US2] Add Planova logo to sidebar"
Task: "T022 [P] [US2] Add branding footer to sidebar"
Task: "T025 [P] [US2] Replace ribbon icons"

# Sequential after parallel:
Task: "T023 [US2] Verify dark theme rendering"
Task: "T024 [US2] Verify light theme rendering"
Task: "T026 [US2] Update HomePage branding section"
Task: "T027 [US2] Add blueprint overlay visuals"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 — Animated splash screen + Planova window title/icon
4. **STOP and VALIDATE**: Launch app — verify splash screen plays, title shows "Planova Platform"
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Brand token infrastructure ready
2. Add User Story 1 → Splash screen and window identity → MVP
3. Add User Story 2 → Full shell rebranding with theme colors, ribbon icons
4. Add User Story 3 → Dynamic typography system added
5. Add User Story 4 → Brand asset organization finalized
6. Final Phase: Polish → WCAG validation, performance tuning, compliance review

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (splash screen)
   - Developer B: User Story 2 (shell branding + ribbon icons)
   - Developer C: User Story 3 (typography system)
3. All stories integrate independently
4. Developer A or B handles User Story 4 and Polish Phase after their story completes

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Brand assets (logos, master reference, ribbon icons, font files) are prerequisites from external designer
- Namespace migration (Som3a.* → Planova.*) is explicitly deferred to post-stabilization per FR-017
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
