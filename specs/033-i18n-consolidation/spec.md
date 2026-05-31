# Feature Specification: i18n Consolidation & Language Support

**Feature Branch**: `033-i18n-consolidation`

**Created**: 2026-05-31

**Status**: Draft

**Input**: User description: "Consolidate localization systems into Som3a.Localization, complete Arabic translations, fix RTL/flow-direction, merge language and font settings into one page."

## User Scenarios & Testing

### User Story 1 - Switch Language and See Translated UI (Priority: P1)

An Arabic-speaking user opens the application and navigates to Settings to switch the interface language from English to Arabic. All UI text — menus, buttons, labels, tooltips, data grid headers, error messages — instantly displays in Arabic without requiring a restart.

**Why this priority**: Core value of the feature — bilingual users must be able to use the application in their preferred language. Without working language switching, the feature delivers no value.

**Independent Test**: Can be fully tested by switching language from Settings page and visually verifying text changes on all UI elements.

**Acceptance Scenarios**:

1. **Given** the application is running in English, **When** user selects Arabic from the language selector, **Then** all visible UI text changes to Arabic immediately
2. **Given** the user has switched to Arabic, **When** they navigate to any page, **Then** all text on that page displays in Arabic
3. **Given** the user switches language, **When** the switch completes, **Then** no crashes or blank UI elements occur
4. **Given** the user switches from Arabic back to English, **When** the language selector changes, **Then** all UI text returns to English

---

### User Story 2 - Correct RTL Layout in Arabic Mode (Priority: P1)

When the user switches to Arabic, the entire application layout mirrors for right-to-left reading: text aligns right, scrollbars appear on the left, DataGrid columns reverse order, and navigation sidebar flips direction. Mixed English-Arabic content (e.g., technical terms within Arabic sentences) renders with correct bidirectional alignment.

**Why this priority**: RTL rendering is essential for Arabic usability. Without proper RTL, Arabic mode is unusable despite translated text.

**Independent Test**: Can be tested by switching to Arabic and verifying layout reversal on each page.

**Acceptance Scenarios**:

1. **Given** the user is in Arabic mode, **When** viewing any page, **Then** text aligns to the right and scrollbars appear on the left
2. **Given** the user is in Arabic mode, **When** viewing a DataGrid, **Then** columns render in reverse order and text within cells aligns correctly
3. **Given** a cell contains mixed English and Arabic text, **When** displayed in Arabic mode, **Then** bidirectional text renders correctly

---

### User Story 3 - Automatic Font Switching per Locale (Priority: P2)

When switching between English and Arabic, font families automatically change to appropriate typefaces for each locale. English uses Segoe UI Variable (or system sans-serif), Arabic uses Cairo or Noto Naskh Arabic. The user can also customize fonts per locale from the settings page.

**Why this priority**: Proper font rendering is critical for readability but less urgent than language switching and RTL layout.

**Independent Test**: Switch languages and verify font family changes on text elements.

**Acceptance Scenarios**:

1. **Given** the user switches to Arabic, **When** any text renders, **Then** it uses an Arabic-appropriate font (Cairo or Noto Naskh Arabic)
2. **Given** the user switches to English, **When** any text renders, **Then** it uses an English-appropriate font
3. **Given** the user opens font settings, **When** they pick a different Arabic font, **Then** all Arabic text updates to the selected font

---

### User Story 4 - Manage Language & Font Settings (Priority: P2)

The user accesses a consolidated Language and Font settings page where they can: select language (EN/AR), pick fonts per locale, toggle an RTL preview to see how the UI will look, and adjust font size scaling for accessibility.

**Why this priority**: A unified settings page improves user experience and discoverability. Font scaling is important for accessibility.

**Independent Test**: Open Language & Font settings page and verify all controls function.

**Acceptance Scenarios**:

1. **Given** the user opens Settings → Language & Font, **When** the page loads, **Then** current language, fonts, and scaling values are displayed
2. **Given** the user changes font scaling to 1.2x, **When** they close settings, **Then** all text renders at 1.2x size
3. **Given** the user toggles RTL preview, **When** the preview activates, **Then** a sample UI card renders in RTL to demonstrate the effect

---

### User Story 5 - Quick Language Toggle from Shell (Priority: P3)

The user can switch languages directly from the shell window using a language toggle button (icon instead of 🌐 emoji) without navigating to Settings. Clicking the toggle switches language immediately and updates the icon to indicate the current locale.

**Why this priority**: Convenience feature; users can already switch from Settings. Lower priority than the core language and RTL functionality.

**Independent Test**: Click language toggle in shell window and verify language switches instantly.

**Acceptance Scenarios**:

1. **Given** the shell window is visible, **When** user clicks the language toggle, **Then** language switches immediately
2. **Given** the language toggle is visible, **When** in English mode, **Then** the icon indicates Arabic is available (and vice versa)

---

### Edge Cases

- **Missing translation**: If a string key lacks an Arabic translation, the system shows the English text as fallback — no crash or blank UI
- **Dynamic format strings**: Strings containing placeholders (e.g., "Welcome, {0}!") are correctly substituted after translation
- **Language switch mid-operation**: If user switches language while a modal or dialog is open, the dialog content updates immediately
- **Mixed content in DataGrid**: Cells containing both English numbers and Arabic text render with correct bidirectional alignment
- **Font glyph fallback**: If a selected Arabic font lacks certain glyphs, the system falls back gracefully without rendering boxes
- **RTL + DataGrid virtualization**: Virtualized DataGrid rows maintain correct column order and alignment during scroll
- **Language persistence after restart**: Selected language and font preferences survive application restart

## Requirements

### Functional Requirements

- **FR-001**: Users MUST be able to switch between English and Arabic from a single consolidated Settings page
- **FR-002**: All UI text MUST update to the selected language immediately without requiring application restart
- **FR-003**: The system MUST display Arabic text with correct right-to-left flow direction across all pages, windows, and dialogs
- **FR-004**: DataGrid column order, scrollbar placement, and text alignment MUST mirror correctly in RTL mode
- **FR-005**: Font families MUST switch automatically per locale (Arabic fonts for Arabic, English fonts for English)
- **FR-006**: Users MUST be able to customize fonts per locale from the Settings page
- **FR-007**: Users MUST be able to adjust font size scaling (accessibility) from the Settings page
- **FR-008**: Language and font settings MUST persist across application restarts
- **FR-009**: Missing translations MUST gracefully fall back to English without causing errors or blank UI; missing key lookups MUST be logged to the diagnostics channel for developer visibility
- **FR-010**: The shell window MUST include a language toggle button with an appropriate icon that switches language instantly
- **FR-011**: All translation string resources MUST be consolidated into a single centralized localization system
- **FR-012**: All existing string binding patterns MUST be migrated to the new centralized localization system
- **FR-013**: The system MUST support 1800+ string keys with complete translations available in both English and Arabic
- **FR-014**: The Settings page MUST include an RTL preview toggle that shows sample UI rendering in RTL before applying

### Key Entities

- **LocalizationResource**: Key-value pairs of translation strings organized by locale (en-US, ar-SA). Each key has a unique identifier and optional format parameters.
- **LanguageSetting**: User's selected language preference (English or Arabic). Persisted across sessions.
- **FontMapping**: Per-locale font family configuration defining which font to use for each supported language.
- **FontScalingSetting**: User's preferred font size multiplier (default 1.0x, range 0.8x–1.5x) for accessibility.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can switch the interface language in 2 clicks or fewer from the Settings page
- **SC-002**: 100% of visible UI strings render correctly after language switch — no untranslated text in the target language for keys that have translations
- **SC-003**: RTL layout renders correctly across all pages with no misaligned elements, incorrect scrollbar placement, or broken DataGrid columns; Tab order, arrow key navigation, and focus direction mirror for RTL in Arabic mode
- **SC-004**: Fonts switch automatically per locale; user can verify correct font family is applied without manual inspection of settings
- **SC-005**: Language and font preferences persist and restore correctly after application restart in 100% of cases
- **SC-006**: Missing translations degrade gracefully — user sees English fallback text with zero crashes or errors
- **SC-007**: All 1800+ string keys have corresponding translations in both English and Arabic before the feature is considered complete
- **SC-008**: Language switch (EN→AR→EN) across all pages completes in under 500ms without crashes, freezes, or visual artifacts; UI remains responsive during update with no loading spinner
- **SC-009**: Application starts and functions correctly in either language without additional configuration

## Clarifications

### Session 2026-05-31

- Q: What is the acceptable UI update time for language switch, and what transition behavior should users see? → A: <500ms target with no loading state; async update without loading spinner; UI remains responsive during switch.
- Q: Should missing translation fallback events be logged/reported for diagnostics? → A: Log to diagnostics channel for developer visibility; no UI-level indicator.
- Q: Should keyboard navigation order and screen reader focus direction mirror for RTL? → A: Mirror Tab order for RTL — Tab flow, arrow key navigation, and focus order follow RTL direction in Arabic mode.

## Constitutional Constraints

The following constraints are mandated by the project constitution and MUST be respected:

- All UI resources MUST use `{DynamicResource}` for themeable properties (Constitution §III).
- Theme mutations MUST be routed through ThemeManager exclusively (Constitution §IV).
- Animations MUST complete within 200ms and remain GPU-safe (Constitution §IX).
- Excel VSTO rendering safety MUST be considered for all window changes (Constitution §X).
- WindowChrome MUST remain the primary window rendering strategy (Constitution §XI).
- Effects MUST be centralized; inline DropShadowEffect is prohibited (Constitution §XII).

## Assumptions

- Translation pre-work (export of 740+ missing string keys, AI generation of Arabic translations, manual review) is completed before feature implementation begins
- The existing `Som3a.Localization` project is the target for all consolidated resources
- English is the default language; Arabic is the only secondary language for v1
- The `TranslationSource` pattern exists in XAML and will be systematically replaced
- Users have access to standard Arabic fonts (Cairo, Noto Naskh Arabic) and English fonts (Segoe UI Variable) on Windows
- Font size scaling range of 0.8x–1.5x in 0.1x increments covers accessibility needs
- The application runs as a VSTO add-in within Excel; all RTL changes must be compatible with the host window
