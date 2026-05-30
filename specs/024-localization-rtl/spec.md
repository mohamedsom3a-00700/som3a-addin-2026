# Feature Specification: Localization & RTL

**Feature Branch**: `feature/phase-24-localization-rtl`

**Created**: 2026-05-29

**Status**: Draft

**Input**: User description: "enterprise_planning_platform_plan.md phase 24"

## Clarifications

### Session 2026-05-29

- Q: Should RTL layout apply to Excel ribbon/VSTO interop controls or only the WPF Shell workspace? → A: Shell-only RTL — RTL applies to WPF Shell workspace only; Excel ribbon and VSTO interop controls remain LTR.
- Q: What is the translation completeness bar for shipping — must all UI be 100% translated or is English fallback acceptable? → A: Core Shell UI (Shell, Settings, Dashboard, error messages) must be 100% translated; plugin UI may use English fallback.
- Q: How should the system handle fonts when the configured Arabic font is not installed on the user's machine? → A: Rely on Windows system font fallback (Segoe UI Arabic on Win10+); bundle Cairo only if Segoe shaping proves insufficient for construction planning characters.
- Q: How should the system behave when the user switches language while a modal dialog is open? → A: Defer the language switch until all modal dialogs are dismissed.
- Q: How should the system handle mixed-language content (Arabic labels with English data values, or vice versa)? → A: UI chrome and labels follow the selected language; data values (BOQ items, quantities, activity names, codes) remain in their original language.
- Q: Which numeral system should the Arabic locale use for displayed numbers? → A: Western Arabic digits (0-9) for all numeric data values; only group/decimal separators, date formats, and currency follow Arabic locale conventions. This matches engineering software standards in the Gulf region.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Switch Application Language Between English and Arabic (Priority: P1)

An Arabic-speaking planning engineer opens the application in an English environment, navigates to Settings, selects "Arabic" from the language dropdown, and the shell UI instantly switches to Arabic — menus, sidebar, settings pages, dashboard widgets, and error messages all display in Arabic without restarting the application. Plugin UI that has not been localized displays English fallback text.

**Why this priority**: Language switching is the foundational capability of localization. Without it, no other localization feature is usable. All subsequent stories depend on this working correctly.

**Independent Test**: Can be fully tested by opening the app, going to Settings → Language, selecting "Arabic," and verifying all visible UI text switches to Arabic immediately. Switching back to English restores the original text.

**Acceptance Scenarios**:

1. **Given** the application is running in English, **When** the user selects "Arabic" from the language picker in Settings, **Then** all visible UI text switches to Arabic without requiring an application restart.
2. **Given** the application is now displaying in Arabic, **When** the user selects "English" from the language picker, **Then** all visible UI text switches back to English.
3. **Given** the user has switched to Arabic and closed the application, **When** the user reopens the application, **Then** the UI persists in Arabic (language preference is remembered).

---

### User Story 2 - Navigate the Application in Right-to-Left Layout (Priority: P1)

After switching to Arabic, the engineer sees the entire workspace layout mirrored for RTL — the sidebar moves to the right side of the screen, text is right-aligned in all controls, navigation order reverses, and all list/grid content reads right-to-left.

**Why this priority**: RTL layout is inseparable from Arabic language support. Without proper RTL rendering, Arabic text is unusable (left-aligned Arabic in a reversed sidebar context creates a broken experience). This is equally foundational.

**Independent Test**: Can be fully tested by switching to Arabic and verifying: sidebar is on the right, text in labels and inputs is right-aligned, Grid/DataGrid columns flow right-to-left, and all reading order is naturally RTL.

**Acceptance Scenarios**:

1. **Given** the application language is switched to Arabic, **When** the Shell workspace renders, **Then** the sidebar appears on the right side of the window.
2. **Given** the application is in Arabic RTL mode, **When** any text input, label, or data grid renders, **Then** text alignment is right-to-left.
3. **Given** the user switches back to English, **When** the Shell workspace renders, **Then** the sidebar returns to the left side and all text is left-aligned.

---

### User Story 3 - Use Culture-Aware Number and Date Formatting (Priority: P2)

An engineer enters quantity values and dates in Arabic mode — numbers display with Western Arabic digits (0-9) and Arabic locale separators (123,456.78), dates display in Arabic format (dd/mm/yyyy with Arabic month names), and currencies use Arabic locale conventions. When switching back to English, all formatting reverts to English conventions.

**Why this priority**: Construction planning engineers work extensively with quantities, dates, and costs. Incorrect or inconsistent number formatting undermines trust in the data and creates confusion in bilingual environments.

**Independent Test**: Can be tested by entering a number (e.g., 123456.78) and a date in a data entry field, switching between English and Arabic, and verifying that the display format changes appropriately for each locale.

**Acceptance Scenarios**:

1. **Given** the application is in Arabic mode, **When** a quantity value (e.g., 123456.78) is displayed, **Then** it appears with Western Arabic digits and Arabic locale separators.
2. **Given** the application is in Arabic mode, **When** a date is displayed, **Then** it uses Arabic date format conventions.
3. **Given** the user switches between English and Arabic, **When** numbers and dates are re-displayed, **Then** they immediately reflect the current culture's formatting.

---

### User Story 4 - Read and Edit Content with Arabic Typography (Priority: P2)

An engineer working in Arabic mode sees all text rendered in a legible Arabic-optimized font (e.g., Cairo or IBM Plex Sans Arabic) with proper Arabic script shaping, ligature support, and appropriate font size scaling for Arabic readability.

**Why this priority**: Arabic script has unique typographic requirements (contextual shaping, ligatures, different optimal sizes). Poor typography causes eye strain and reduces productivity for Arabic-speaking users.

**Independent Test**: Can be tested by switching to Arabic and visually inspecting all text elements — headers, labels, input values, grid cells — for correct Arabic glyph shaping, no broken ligatures, and comfortable readability.

**Acceptance Scenarios**:

1. **Given** the application is in Arabic mode, **When** any Arabic text renders, **Then** it uses the configured Arabic font family with proper shaping and ligatures.
2. **Given** the user has configured an alternate Arabic font in typography settings, **When** the UI refreshes, **Then** the new font applies immediately.
3. **Given** the application switches between English and Arabic, **When** text renders, **Then** the font changes to the appropriate family for each language without text clipping or layout breaks.

### Edge Cases

- What happens when a plugin has not been localized and the application is in Arabic mode?
- How does the system handle mixed-language content (Arabic labels with English data values, or vice versa)?
  *Resolution: UI chrome and labels follow the selected language; data values (BOQ items, quantities, activity names, codes) remain in their original language to prevent data corruption.
- What happens if the configured Arabic font is not installed on the system?
  *Resolution: System falls back to Windows Segoe UI Arabic font family (available on Win10+). If Segoe shaping is insufficient, Cairo font will be bundled with the installer.
- How does RTL mode interact with Excel interop (VSTO) where Excel itself may be in a different language?
  *Resolution: RTL applies to WPF Shell workspace only. Excel ribbon and VSTO interop controls remain LTR to avoid Excel stability issues.
- What happens during a language switch while a modal dialog is open?
  *Resolution: Language switch is deferred until all modal dialogs are dismissed to prevent mid-dialog layout reflow and FlowDirection conflicts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support switching between English and Arabic without requiring an application restart.
- **FR-002**: System MUST persist the selected language preference across application sessions.
- **FR-003**: System MUST provide a language picker in the Settings - Appearance page.
- **FR-004**: System MUST dynamically switch all shell UI text (menus, sidebar, navigation, toolbar, status bar) when language changes.
- **FR-005**: System MUST dynamically switch all settings page text when language changes.
- **FR-006**: System MUST dynamically switch all dashboard widget text when language changes.
- **FR-007**: System MUST apply RTL layout (mirrored sidebar, right-aligned text, reversed navigation) when Arabic is selected.
- **FR-008**: System MUST revert to LTR layout when English is selected.
- **FR-009**: System MUST support culture-aware number formatting (digit shapes, group separators, decimal separators) per selected language.
- **FR-010**: System MUST support culture-aware date formatting per selected language.
- **FR-011**: System MUST support culture-aware currency formatting per selected language.
- **FR-012**: System MUST provide an Arabic-optimized font family selection in typography settings.
- **FR-013**: System MUST include at least two Arabic font options (e.g., Cairo, IBM Plex Sans Arabic) with proper fallback chains.
- **FR-014**: System MUST display Arabic script with correct glyph shaping and ligature support.
- **FR-015**: System MUST provide an English fallback for any plugin or UI element that has not been localized for Arabic.
- **FR-016**: System MUST support dynamic font switching between Arabic and Latin font families when language changes.
- **FR-017**: System MUST ensure that all DataGrid and ListView controls respect FlowDirection for RTL mode.
- **FR-018**: System MUST ensure that all ComboBox dropdowns, popups, and flyouts respect FlowDirection in RTL mode.
- **FR-019**: System MUST translate all static error messages and validation feedback strings.
- **FR-020**: System MUST allow future addition of more languages without architectural changes.

### Key Entities

- **Language**: Represents a supported language (English, Arabic) with associated culture code (en-US, ar-SA), display name, native name, and FlowDirection.
- **Resource Set**: A collection of translated strings for a language, organized by UI component/area. Each resource has a key and translated value.
- **Font Profile**: A named configuration of font families (primary, fallback chain) for a language, with optional size scaling.
- **Culture Profile**: A set of formatting rules (number format, date format, currency format, calendar) associated with a language.
- **FlowDirection Manager**: Controls the RTL/LTR state of each window and ensures consistent layout switching.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can switch between English and Arabic and see 100% of Shell/Settings/Dashboard/error text translated within 1 second; plugin UI shows English fallback where translations are missing.
- **SC-002**: RTL layout switch (sidebar mirroring + text alignment) completes in under 500ms.
- **SC-003**: All 20+ static UI areas (sidebar, menus, settings, dashboard, tooltips, error messages) display correctly in both languages.
- **SC-004**: Number, date, and currency formatting matches the selected culture's conventions with zero formatting errors.
- **SC-005**: Arabic text renders with correct shaping across all font sizes (10pt–24pt) with no broken glyphs or ligatures.
- **SC-006**: Plugins without Arabic translations display English fallback text rather than empty or broken UI.
- **SC-007**: Language preference persists correctly across application restarts with 100% reliability.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Arabic font rendering relies on Windows system font fallback (Segoe UI Arabic on Win10+). Cairo font will be bundled only if Segoe UI Arabic shaping proves insufficient for construction planning characters.
- Translation completeness requirement: Shell, Settings, Dashboard, and error messages must be 100% translated before shipping. Plugin UI may use English fallback and adopt translations per-plugin on their own schedule.
- The existing `Som3a.Localization` project structure will be used for .resx resource files and the `LocalizationService`.
- RTL mirroring applies to the WPF Shell workspace (sidebar, navigation, content area) only. Excel ribbon and VSTO interop controls remain LTR to avoid Excel stability issues.
- Individual plugin pages may require per-plugin RTL testing and adjustments.
- All new UI components going forward must include both English and Arabic resource entries as part of their development.
