# Master Plan: Fluent UI & Modern Platform Migration

**Status**: Planned (not started)
**Target**: Post-Phase 26

## Overview

Full migration to modern .NET ecosystem: **26 requirements** consolidated into
**12 execution phases**. The key architectural change is upgrading WpfApp2 from
.NET Framework 4.8 to .NET 8.0 as an out-of-process WPF host communicating with
the VSTO add-in via the existing named pipe Bridge (Som3a.Bridge).

## Requirements Index

| # | Requirement | Phase |
|---|-------------|-------|
| 1 | Full solution integration with Fluent UI + WPF-UI + CommunityToolkit.Mvvm + Fluent 2 icons | 1A, 1B, 1C, 2, 3 |
| 2 | Sidebar toggle; move logo & "Planova" to titlebar | 9 |
| 3 | Setting to reorder sidebar groups (drag-and-drop) | 9 |
| 4 | Hide "Primavera Results" from sidebar | 9 |
| 5 | Dashboard status widgets (AI detect, DB, theme, license, Excel connection, memory, last sync) | 9 |
| 6 | Integrate all pages with Fluent UI + WPF-UI + CommunityToolkit.Mvvm + Fluent 2 icons | 3 |
| 7 | Remove "data privacy consent" from BOQ Generator → add to AI settings | 7 |
| 8 | Re-arrange/fix styles for theme integration across all pages | 3, 8 |
| 9 | Load BOQ into Duration Estimator → detect items → generate all durations | 11 |
| 10 | Resources dictionary (auto-collect from BOQ → database) | 10 |
| 11 | Full VSTO test after every phase | All phases |
| 12 | Add anything from any phase to shell/ribbon for user access | 12 |
| 13 | Add any missing pages to ribbon | 12 |
| 14 | WBS Editor: import/export table from Excel | 11 |
| 15 | Make AI provider settings more professional | 6 |
| 16 | Improve AI connection with all AI-using pages | 6 |
| 17 | Fix and test all AI-using pages for connection and output | 7 |
| 18 | Fix and improve language support (settings, fonts, EN↔AR transform); merge into one settings page | 4 |
| 19 | Redesign settings page — professional for every setting | 8 |
| 20 | All settings to DB (SQLite) for save/restore | 5 |
| 21 | Full dependency: Fluent UI + WPF-UI + CommunityToolkit.Mvvm + Fluent 2 only (remove MaterialDesign) | 1C, 3 |
| 22 | VSTO smoke test after every phase | All phases |
| 23 | Named pipe performance baseline (< 5ms avg per message) | 1B |
| 24 | Excel crash watchdog (WPF auto-shutdown on Excel crash) | 1B |
| 25 | WPF-UI compatibility pilot before full icon migration | 3 |
| 26 | Arabic translation pre-work (740 strings) before i18n phase | 4 |

---

## Phase 1A: Framework & Project Format Upgrade

**Requirements**: 1

**Goal**: Upgrade WpfApp2 to .NET 8.0 and convert to SDK-style project. Zero other changes.

| Step | Detail |
|------|--------|
| 1A.1 | Convert `Som3a_WPF_UI.csproj` from `net48` → `net8.0-windows`. Convert to SDK-style project format. |
| 1A.2 | Drop `packages.config` → `PackageReference`. Re-add existing NuGet packages only (no new ones yet). |
| 1A.3 | Fix any compile errors caused by .NET 4.8 → .NET 8 API changes. Do NOT change any logic. |

**Verification**: `MSBuild.exe WpfApp2/Som3a_WPF_UI.csproj` succeeds. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 1B: Out-of-Process Architecture

**Requirements**: 1

**Goal**: WPF UI runs as standalone .NET 8 process communicating with VSTO via `Som3a.Bridge` named pipe. Resolve all interop edge cases before any UI changes.

| Step | Detail |
|------|--------|
| 1B.1 | Refactor `App.xaml.cs` for standalone launch + pipe-based command relay. |
| 1B.2 | **Cold start protocol** — VSTO `ThisAddIn_Startup` calls `Process.Start()` for WPF process; waits up to 5s for pipe handshake before showing error toast. |
| 1B.3 | **Excel crash watchdog** — WPF process subscribes to VSTO heartbeat ping every 10s via pipe. If 3 consecutive pings fail → WPF calls `Application.Shutdown()`. |
| 1B.4 | **Window ownership** — WPF `ShellWindow` receives Excel HWND via first pipe message on startup; sets `new WindowInteropHelper(shellWindow).Owner = excelHwnd` for modal behavior. |
| 1B.5 | **Excel command protocol** — Define pipe message schema for WPF→VSTO Excel commands: `{ "cmd": "WriteCell", "sheet": "Sheet1", "row": 1, "col": 1, "value": "X" }`. Implement in `Som3aAddinBridge`. |
| 1B.6 | **Named Pipe performance baseline** — Send 100 sequential pipe messages, measure total time. Acceptable threshold: < 500ms for 100 messages (< 5ms avg). Log result to diagnostics. |

**Verification**: Full VSTO smoke test — ribbon works, Shell opens as out-of-process window, writes to Excel cell via pipe command, Excel close → WPF auto-shuts-down within 15s. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 1C: NuGet Restructure & MaterialDesign Removal

**Requirements**: 1, 21

**Goal**: Remove MaterialDesign, add new packages. Only after 1A and 1B are verified stable.

| Step | Detail |
|------|--------|
| 1C.1 | **Pre-removal audit** — Run `grep -rn "MaterialDesign\|materialDesign" --include="*.xaml" --include="*.cs" .` and produce a complete checklist of all references before touching any file. |
| 1C.2 | Uninstall `MaterialDesignThemes` and `MaterialDesignColors` from WpfApp2 and VSTO projects. Remove assembly references and binding redirects. |
| 1C.3 | Add NuGet packages: `FluentIcons.WPF`, `WPF-UI` (`Wpf.Ui`). CommunityToolkit.Mvvm is already installed. |
| 1C.4 | Remove `MaterialIntegration.xaml`, `MaterialIcons.xaml`, `MaterialControls.xaml` from `ThemeResources.xaml` aggregator. Remove `MaterialIconConverter.cs`. |
| 1C.5 | Remove `MaterialDesignThemes.Wpf` binding redirect from VSTO `app.config`. |
| 1C.6 | Fix all compile errors from MaterialDesign removal using the checklist from Step 1C.1. Replace broken references with direct `Brush.*` tokens as temporary stubs. |

**Verification**: `dotnet build` succeeds with zero MaterialDesign references. All pages render without crashes (placeholder icons acceptable at this stage). **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 2: CommunityToolkit.Mvvm ViewModel Refactor

**Requirements**: 1

**Goal**: Replace all manual `INotifyPropertyChanged` + `RelayCommand` with source-generated
`[ObservableProperty]` and `[RelayCommand]` across every ViewModel.

| Step | Detail |
|------|--------|
| 2.1 | **Base class** — Change `ViewModelBase` to inherit `ObservableObject` from `CommunityToolkit.Mvvm.ComponentModel`. |
| 2.2 | **Source-generate all ViewModels** — Apply `[ObservableProperty]` to all bindable properties and `[RelayCommand]` to all command methods across 20+ ViewModels: `HomeViewModel`, `SettingsViewModel`, `BOQActivityGeneratorViewModel`, `DurationEstimatorPageViewModel`, `WBSEditorViewModel`, `WBSGeneratorViewModel`, `RelationshipGeneratorViewModel`, all `ViewModels/Dashboard/*` widgets, all `ViewModels/Primavera/*`, `DiagnosticsViewModel`, `LanguagePageViewModel`, `ShellViewModel`, `CommandPaletteViewModel`, `ToastViewModel`, etc. |
| 2.3 | **Remove manual relay commands** — Delete `Models/RelayCommand.cs` and `Helpers/AsyncRelayCommand.cs`; use `CommunityToolkit.Mvvm.Input.AsyncRelayCommand`. |
| 2.4 | **Partial class enforcement** — All ViewModels must be `partial class` for source generator compatibility. |

**Files affected**: ~20+ `.cs` files in `ViewModels/`

**Verification**: Build succeeds; verify `*.g.cs` source-generated files appear in `obj/`. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 3: Fluent Icon & Control Migration

**Requirements**: 6, 21

**Goal**: Replace all `materialDesign:PackIcon` and `MaterialDesign*` style references
with Fluent 2 icons and WPF-UI native controls.

| Step | Detail |
|------|--------|
| 3.0 | **WPF-UI Pilot** — Before any other step in Phase 3, migrate a single low-risk panel (e.g. `DiagnosticsPage`) to WPF-UI controls and Fluent 2 icons. Verify: icons render correctly in Dark/Light themes, scrollbars behave correctly, no layout regressions. **Only proceed to steps 3.1–3.9 after the pilot page renders correctly in both themes.** If WPF-UI proves incompatible, fallback plan: keep custom theme engine, use `FluentIcons.WPF` only for icons (no WPF-UI controls). Document the fallback decision before proceeding. |
| 3.1 | **Replace `materialDesign:PackIcon` → `FluentIcon`** in `SidebarControl.xaml`, `ShellWindow.xaml`, `SettingsPage.xaml`, `ShellStyles.xaml`. Map each `PackIconKind` to equivalent Fluent 2 icon. |
| 3.2 | **Replace `MaterialDesignFlatButton` → Fluent styles** in `DiagnosticsPage.xaml` (3 refs), `DashboardWidgetDataTemplates.xaml` (5 refs), `WidgetCardStyles.xaml`. |
| 3.3 | **Replace `MaterialDesignScrollViewer` → WPF-UI ScrollViewer** in `HomePage.xaml`, `DiagnosticsPage.xaml`. |
| 3.4 | **Replace `MaterialDesignLinearProgressBar` → WPF-UI ProgressBar** in `WidgetCardStyles.xaml`. |
| 3.5 | **Replace `MaterialDesignFont` → Fluent theme font** in `WidgetCardStyles.xaml`. |
| 3.6 | **Replace `materialDesign:Chip` → WPF-UI or custom Chip** in `Theme/Controls/MaterialControls.xaml` content. |
| 3.7 | **Sidebar icon system** — Update `SidebarRegistrationService` to use Fluent 2 icon names; implement `FluentIconConverter` (replaces `MaterialIconConverter`) mapping string → `FluentIcon`. |
| 3.8 | **Remove MaterialDesign resource key references** — `MaterialDesignBody`, `MaterialDesignPaper`, `MaterialDesignCardBackground`, `MaterialDesignTextBoxBorder` → replace with direct `Brush.*` tokens. |
| 3.9 | **Remove `DefinitionColors` file** — If exists, remove Obsolete/empty theme definition files. |

**Files affected**: `Controls/Shell/SidebarControl.xaml`, `Controls/Shell/ShellWindow.xaml`, `Pages/SettingsPage.xaml`, `Theme/ShellStyles.xaml`, `Theme/Controls/MaterialControls.xaml`, `Theme/DashboardWidgetDataTemplates.xaml`, `Controls/WidgetCardStyles.xaml`, `Converters/MaterialIconConverter.cs`, `Theme/ThemeResources.xaml`, `Services/SidebarRegistrationService.cs`

**Verification**: All pages render with Fluent 2 icons; no `MaterialDesign*` references remain in XAML or C#. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 4: i18n Consolidation & Language Support

**Requirements**: 18

**Goal**: Consolidate two existing localization systems into `Som3a.Localization`,
complete missing Arabic translations, fix RTL/flow-direction, merge language + font
settings into one page.

| Step | Detail |
|------|--------|
| 4.0 | **Translation pre-work (must complete before Phase 4 starts)** — Export all 740+ missing string keys from `WpfApp2/Resources/Strings.resx` that have no corresponding entry in `StringsArabic.resx`. Use an AI model (e.g. Claude or DeepSeek) to produce a first-pass Arabic translation of the full list. Perform a manual review pass focusing on: technical terms (keep in English or use established Arabic equivalents), UI labels (must be concise), error messages (must be clear). Import reviewed translations into `StringsArabic.resx`. **Do not start steps 4.1–4.7 until this step is done.** |
| 4.1 | **Consolidate to `ILocalizationService`** — Replace all `LocalizationBridgeService.Instance` and `TranslationSource` usage with `ILocalizationService` from `Som3a.Localization`. |
| 4.2 | **Migrate WpfApp2 strings** — Move all 1800+ strings from `WpfApp2/Resources/Strings.resx` and `StringsArabic.resx` to `Som3a.Localization/Resources/Strings.en-US.resx` / `Strings.ar-SA.resx`. The Arabic file currently has ~1062 entries — add missing ~740 translations. |
| 4.3 | **Update XAML bindings** — Replace `{Binding Source={x:Static services:TranslationSource.Instance}, Path=[Key]}` pattern with `ILocalizationService` binding or WPF-UI `Locale` infrastructure. |
| 4.4 | **RTL/FlowDirection fix** — Ensure every Page and Window respects `FlowDirection` on language switch. Fix known issues: DataGrid column alignment, ScrollBar placement, text alignment in mixed EN/AR content. |
| 4.5 | **Font handling** — Fix `FontFamily` switching between Arabic fonts (Cairo, Noto Naskh Arabic) and English (Segoe UI Variable). Move font settings to the consolidated language page. |
| 4.6 | **Language + Font settings page** — Create `LanguagePage.xaml` (Settings sub-page) with: language selector (EN/AR), font picker per locale, RTL preview toggle, font size scaling. |
| 4.7 | **Shell language toggle** — Replace 🌐 emoji with Fluent 2 icon; wire to `ILocalizationService.SetLanguage()`. |

**Files affected**: `Services/LocalizationBridgeService.cs`, `Services/TranslationSource.cs`, `Pages/Settings/LanguagePage.xaml*`, `ViewModels/Settings/LanguagePageViewModel.cs`, `Som3a.Localization/Resources/*.resx`, `Controls/Shell/ShellWindow.xaml`, every XAML page using `TranslationSource`.

**Verification**: Switch EN→AR→EN across all pages; verify labels, tooltips, DataGrids, navigation render correctly in both directions; fonts switch per locale. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 5: Settings DB Migration

**Requirements**: 20

**Goal**: All settings (theme, AI, WBS, plugin, encrypted secrets) persist exclusively
in SQLite via `SettingsRecord` table. Remove `Properties.Settings` and JSON file
dependencies.

| Step | Detail |
|------|--------|
| 5.1 | **Extend `SettingsRecord` schema** — Add `DataType` (string/int/bool/color/json), `IsEncrypted` flag, `EncryptedValue` column for DPAPI-encrypted secrets. |
| 5.2 | **Create `SettingsService`** — New service in `WpfApp2.Services` wrapping `ISettingsRepository`. Typed API: `Get<T>(category, name, default)`, `Set<T>(category, name, value)`, `GetAll(category)`, `ImportAsync()`, `ExportAsync()`. Handles encryption transparently. |
| 5.3 | **Migrate `Properties.Settings`** — On first launch after migration, read all 20+ values from `Properties.Settings.Default` and write to `SettingsRecord`. Remove `Properties.Settings` dependency. |
| 5.4 | **Migrate JSON plugin settings** — Move from `%APPDATA%/Som3a/Plugins/{id}/settings.json` to `SettingsRecord` with `PluginId` filter. |
| 5.5 | **Migrate encrypted secrets** — Move from `%APPDATA%/Som3a/Plugins/{id}/secrets.json` to `SettingsRecord` with `IsEncrypted=true` and DPAPI-encrypted `Value`. |
| 5.6 | **Rewrite `SettingsPersistenceService`** — Replace file-based I/O with `SettingsService` (SQLite-backed). Remove JSON serialization for individual settings. |
| 5.7 | **One-time migration in `CompositionRoot`** — Register one-time migration that reads legacy stores, writes to SQLite, then deletes legacy files. |
| 5.8 | **Backup/restore** — Implement `SettingsService.ExportToJsonAsync()`, `ImportFromJsonAsync()` for user-facing backup feature in Diagnostics panel. |

**Files affected**: `Som3a.Infrastructure/Persistence/Models/SettingsRecord.cs`, `Som3a.Infrastructure/Persistence/Repositories/SettingsRepository.cs`, `WpfApp2/Services/SettingsPersistenceService.cs`, `WpfApp2/Services/ThemeSettings.cs`, `WpfApp2/CompositionRoot.cs`

**Verification**: All settings persist across app restarts. Uninstall/reinstall preserves settings. Encrypted values remain encrypted at rest. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 6: AI Provider Overhaul

**Requirements**: 15, 16

**Goal**: Professional AI provider panel with health dashboard, connection test,
usage statistics, streaming toggle, advanced parameters, auto-retry, fallback chain,
and timeout control.

| Step | Detail |
|------|--------|
| 6.1 | **Redesign AI Panel** — Provider cards (OpenAI, Ollama, LM Studio, LocalAI) with connection status indicators (green/yellow/red). Provider priority ordering. Configurable per-provider. |
| 6.2 | **Connection test button** — Per-provider "Test Connection" button sending a ping/health prompt, measuring latency, displaying model info + response time. |
| 6.3 | **Provider health dashboard** — Real-time status cards: last heartbeat timestamp, latency (ms), requests today, success rate (%). Data stored in existing `AIExecutionRecord` table. |
| 6.4 | **Usage statistics** — Token usage counter (prompt + completion tokens per session), cost tracking (manual $/1K tokens entry per model), request count. Display in AI panel with reset option. |
| 6.5 | **Advanced parameters** — Per-model sliders: Temperature (0.0–2.0), MaxTokens (256–16384), TopP (0.0–1.0). |
| 6.6 | **Streaming toggle** — Per-page toggle for streaming responses (BOQ Generator, WBS Generator). |
| 6.7 | **Auto-retry with exponential backoff** — `AIBridgeClient.ExecutePromptAsync()`: 3 retry attempts with 1s/2s/4s delays on transient failures. |
| 6.8 | **Fallback provider chain** — Configure primary + fallback provider. On timeout/error from primary, auto-failover to fallback. Chain configurable in AI panel. |
| 6.9 | **Connection status indicator** — Global status bar in `ShellWindow` showing AI connection health. Per-page indicator in BOQ Generator and WBS Generator pages. |
| 6.10 | **Request timeout control** — Configurable timeout per operation (default: 30s generate, 10s health check). |

**Files affected**: `Views/AIPanel.xaml`, `ViewModels/SettingsViewModel.cs`, `Som3a.Bridge/AIBridgeClient.cs`, `Som3a.AI/Providers/OpenAIProvider.cs`, `Services/AISettings.cs`

**Verification**: All AI pages show connection status. Test button works per provider. Auto-fallback switches provider on failure. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 7: AI Page Fix & Verification

**Requirements**: 7, 17

**Goal**: Fix and verify all AI-using pages (BOQ Generator, WBS Generator).
Remove privacy consent from BOQ Generator → move to AI settings.

| Step | Detail |
|------|--------|
| 7.1 | **BOQ Activity Generator audit** — Verify `ActivityGenerationService` uses `SettingsService` for all AI configuration. Remove hardcoded model/provider references. Add `IAIBridge.HealthCheckAsync()` call before generation. |
| 7.2 | **WBS Generator audit** — Verify `WBSAIService` reads AI settings from `SettingsService`. Fix `IsAIAvailable` check. |
| 7.3 | **Remove privacy consent from BOQ Generator** — Delete `PrivacyOverlay` Grid from `BOQActivityGeneratorPage.xaml`. Remove `HasConsented`, `SetConsent()` from `BOQActivityGeneratorViewModel`. |
| 7.4 | **Add privacy consent to AI Settings** — Add toggle in redesigned AI Panel: "Enable AI Generation" with privacy info text. Persist via `SettingsService`. All AI pages check this setting. |
| 7.5 | **Standardize AI error handling** — Common error display across all AI pages: timeout → "Request timed out"; rate limit → "Too many requests, please wait"; auth failure → "Check API key"; no response → "AI did not return valid data". |
| 7.6 | **End-to-end AI test** — Enable AI → call each AI page → verify JSON response parsing → verify graceful failure on error. |

**Files affected**: `Pages/BOQActivityGeneratorPage.xaml`, `ViewModels/BOQActivityGeneratorViewModel.cs`, `Services/ActivityGenerationService.cs`, `ViewModels/WBS/WBSGeneratorViewModel.cs`, `Services/WBS/WBSAIService.cs`, `Views/AIPanel.xaml`

**Verification**: All AI-dependent pages produce correct output, handle errors gracefully, respect privacy consent toggle. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 8: Settings Page & Panel Redesign

**Requirements**: 19

**Goal**: Full Fluent design settings page using WPF-UI NavigationView. Professional
layout for all 8 panels with consistent card-based design.

| Step | Detail |
|------|--------|
| 8.1 | **SettingsPage.xaml redesign** — WPF-UI `NavigationView` layout. Left nav with Fluent 2 icons + category labels. Right content area with scrollable card sections. Responsive (collapses to icon-only on narrow). |
| 8.2 | **AppearancePanel refresh** — Fluent-style theme cards with border highlights, accent swatches with color picker, background with live preview, font selector. |
| 8.3 | **AIPanel integration** — Integrate Phase 6 redesign: provider health cards, connection test, usage stats, streaming toggle, advanced params, privacy consent toggle. |
| 8.4 | **Language+Font panel** — New combined panel replacing standalone `LanguagePage`. Language dropdown, font picker per locale, RTL preview toggle, font size slider. |
| 8.5 | **PerformancePanel** — Animation speed (Off/Reduced/Full), UI density (Compact/Normal/Spacious) with preview. |
| 8.6 | **AccessibilityPanel** — High contrast mode toggle, focus indicator toggle, with description text. |
| 8.7 | **ExcelPanel** — Render mode (Auto/Safe/Full), safe mode toggle, DPI info display. |
| 8.8 | **PluginsPanel** — Installed plugins DataGrid (Module, Version, State, Capabilities, Memory, Load Time, Last Error). |
| 8.9 | **WbsPanel** — WBS Code Mode (Numeric/Alpha), min alpha length, export color style selector. |
| 8.10 | **DiagnosticsPanel refresh** — Redesigned with Fluent cards: render mode, GPU info, memory usage, popup health, module status, settings export/import UI. |
| 8.11 | **Settings export/import UI** — Professional dialog: export as JSON file (download), import from file with validation preview and warning list. |

**Files affected**: `Pages/SettingsPage.xaml*`, `ViewModels/SettingsViewModel.cs`, `Views/AppearancePanel.xaml`, `Views/AIPanel.xaml`, `Views/PerformancePanel.xaml`, `Views/AccessibilityPanel.xaml`, `Views/ExcelPanel.xaml`, `Views/PluginsPanel.xaml`, `Views/WbsPanel.xaml`, `Views/DiagnosticsPanel.xaml`

**Verification**: All 8+ panels render with consistent Fluent design. Settings save/load correctly via `SettingsService`. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 9: Shell, Sidebar & Dashboard

**Requirements**: 2, 3, 4, 5

**Goal**: Restructure shell (logo→titlebar, hamburger toggle), enable sidebar
drag-and-drop reorder, hide Primavera Results, add 7 new dashboard status widgets.

| Step | Detail |
|------|--------|
| 9.1 | **Logo to titlebar** — Move Planova logo + "Planova" text from `SidebarControl` to `ShellWindow` titlebar (left side, before window buttons). Use WPF-UI `TitleBar` control if compatible. |
| 9.2 | **Hamburger toggle** — Add toggle button in titlebar to collapse/expand sidebar (icon-only ↔ full). Persist collapsed state in `SettingsService`. |
| 9.3 | **Drag-and-drop sidebar reorder** — Add drag handle per sidebar category. User drags category up/down to reorder. Persist order in `SettingsService` (category="Sidebar", name="CategoryOrder"). `NavigationService` reads persisted order on load. |
| 9.4 | **Hide Primavera Results** — Set `planning.primavera.results` to `IsVisible = false` in `SidebarRegistrationService.RegisterStaticPages()`. Still accessible programmatically via `NavigationService.Navigate()`. |
| 9.5 | **7 new dashboard widgets** — Implement in `ViewModels/Dashboard/` using `WidgetViewModel` base: |
| | — `AIDetectStatusWidgetViewModel` — AI provider connectivity, model loaded status, last heartbeat |
| | — `DatabaseStatusWidgetViewModel` — SQLite connection health, last backup, migration version |
| | — `ThemeStatusWidgetViewModel` — Current theme name, accent color hex, render mode |
| | — `AppInfoWidgetViewModel` — App version, build date, current phase/release tag, link to changelog. (Replaces `LicenseStatusWidgetViewModel`. A license system can be added as a future phase when licensing requirements are defined.) |
| | — `ExcelConnectionStatusWidgetViewModel` — VSTO/Excel interop health, workbook open status |
| | — `MemoryUsageWidgetViewModel` — Real-time memory pressure via `PerformanceMonitor` |
| | — `LastSyncWidgetViewModel` — Last Primavera DB sync / data refresh timestamp |
| 9.6 | **Expand dashboard grid** — From 3×3 to 4×4 (16 widgets max) with responsive layout in `HomePage.xaml`. |
| 9.7 | **Widget auto-refresh** — Status widgets refresh on 30s timer; statistical widgets on 5min timer. |

**Files affected**: `Controls/Shell/SidebarControl.xaml`, `Controls/Shell/ShellWindow.xaml*`, `Controls/Shell/ShellState.cs`, `Services/SidebarRegistrationService.cs`, `Services/NavigationService.cs`, `Pages/HomePage.xaml`, `ViewModels/HomeViewModel.cs`, `ViewModels/Dashboard/*` (7 new files)

**Verification**: Sidebar reorder persists across restarts. Dashboard shows all 7 new widgets with live data. Logo appears in titlebar. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 10: Resources Dictionary

**Requirements**: 10

**Goal**: New `ResourceDictionaryPage` — auto-collect resources from BOQ processing and persist in SQLite.

| Step | Detail |
|------|--------|
| 10.1 | **Data model** — Add `ResourceDictionaryRecord` entity to `Som3a.Infrastructure/Persistence/Models/`. Migration adds `ResourceDictionary` table: Id (GUID), SourceBOQId, ResourceName, ResourceType, Unit, UnitRate, CrewSize, ProductivityRate, Category, CreatedAt, UsageCount. |
| 10.2 | **Service** — `IResourceDictionaryService` with `AddFromBOQAsync()`, `SearchAsync()`, `UpdateRateAsync()`, `DeleteAsync()`. |
| 10.3 | **Auto-collect hook** — When user processes a BOQ (BOQ Generator or Duration Estimator), auto-extract resources and upsert to dictionary. |
| 10.4 | **Page** — `ResourceDictionaryPage.xaml` + `ResourceDictionaryViewModel.cs`. Browse/search/filter. Edit rate/crew size. Delete. Register in Planning category. |

**Files to create**: `Som3a.Infrastructure/Persistence/Models/ResourceDictionaryRecord.cs`, `WpfApp2/Services/IResourceDictionaryService.cs`, `WpfApp2/Services/ResourceDictionaryService.cs`, `WpfApp2/Pages/ResourceDictionaryPage.xaml`, `WpfApp2/Pages/ResourceDictionaryPage.xaml.cs`, `WpfApp2/ViewModels/ResourceDictionaryViewModel.cs`

**Verification**: Add resources via BOQ Generator. View them in ResourceDictionaryPage. Edit rate. Verify persistence across restart. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 11: Duration Estimator BOQ Load & WBS Excel I/O

**Requirements**: 9, 14

**Goal**: Load BOQ into Duration Estimator for batch duration generation. Import/export WBS from/to Excel.

| Step | Detail |
|------|--------|
| 11.1 | **Duration Estimator BOQ load** — Add "Load BOQ from Excel" button in `DurationEstimatorPage`. Read active sheet BOQ items (description, quantity, unit) via pipe→Excel interop. Display in DataGrid. Add "Generate All Durations" button iterating each item through `IDurationEstimatorBridge.CalculateAsync()`. |
| 11.2 | **WBS Editor Excel import** — "Import from Excel" button in `WBSEditorViewModel`. Auto-detect WBS columns (scan headers for WBS/Level/Code/Name). Parse indentation → build `WBSNode` tree → populate TreeView. |
| 11.3 | **WBS Editor Excel export** — Enhanced export: WBS hierarchy with Excel outline grouping, indentation, color coding by level. |

**Verification**: Load BOQ from Excel → generate durations → verify output. Import WBS from Excel → verify tree structure. Export WBS → open in Excel → verify outline groups. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Phase 12: Ribbon Completeness & VSTO Testing Protocol

**Requirements**: 11, 12, 13

**Goal**: All new pages accessible from ribbon. Formal VSTO test protocol documented and executed.

| Step | Detail |
|------|--------|
| 12.1 | **Ribbon buttons** — Add buttons in `Ribbon1.cs` for: BOQ Activity Generator, Duration Estimator, WBS Templates, WBS Generator, WBS Editor, Relationship Generator, Diagnostics, Resource Dictionary. |
| 12.2 | **Missing pages to ribbon** — Audit all registered pages vs ribbon buttons. Add any page not yet accessible from ribbon. |
| 12.3 | **VSTO Test Protocol doc** — Create `Docs/VSTO_TEST_PROTOCOL.md` with full per-phase checklist: ribbon buttons visible, navigation from ribbon, theme consistency, sidebar render + reorder, page-specific functionality, AI generation (all providers), settings persistence (SQLite), language switch (EN↔AR), WindowChrome/FallbackSafe rendering, no crashes on load/navigate. |
| 12.4 | **Full protocol execution** — Run complete `VSTO_TEST_PROTOCOL.md` checklist. All items must pass before this phase is marked complete. |

**Files to create**: `Docs/VSTO_TEST_PROTOCOL.md`

**Verification**: All ribbon buttons work. All pages load. AI generates output. Settings persist. Language switches correctly. Full protocol checklist passes with zero failures. **VSTO Smoke Test**: Ribbon buttons visible → ShellWindow opens → sidebar renders → navigate to 3 pages → theme switch (Dark/Light) → Excel cell write via interop → no crashes.

---

## Dependency Graph

```
Phase 1A (Framework Upgrade — .NET 8, SDK-style)
    │
    ▼
Phase 1B (Out-of-Process Architecture — Named Pipe, cold start, watchdog)
    │
    ▼
Phase 1C (NuGet Restructure — remove MaterialDesign, add FluentIcons/WPF-UI)
    │
    ├──► Phase 2 (MVVM Refactor — CommunityToolkit.Mvvm source generators)
    │         │
    │         ▼
    ├──► Phase 3 (Fluent Icons & Controls — pilot first, then all pages)
    │         │
    │         ├──────────────► Phase 8 (Settings Redesign)
    │         │
    ├──► Phase 4 (i18n Consolidation — pre-work: 740 translations done first)
    │         │
    │         └──────────────► Phase 8 (Settings Redesign — language panel)
    │
    ├──► Phase 5 (Settings DB Migration — SQLite, DPAPI)
    │         │
    │         └──────────────► Phase 8 (Settings Redesign — data layer ready)
    │
    ├──► Phase 6 (AI Provider Overhaul — health, test, retry, fallback, local AI)
    │         │
    │         ▼
    │    Phase 7 (AI Page Fix & Verify — BOQ/WBS fix, privacy consent move)
    │
    └──► Phase 9 (Shell/Sidebar/Dashboard — titlebar, reorder, widgets)
              │
              ▼
         Phase 10 (Resources Dictionary — new page, auto-collect from BOQ)
              │
              ▼
         Phase 11 (Duration BOQ Load + WBS Excel I/O)
              │
              ▼
         Phase 12 (Ribbon Completeness + Full VSTO Test Protocol)
```

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| .NET 8.0 WPF out-of-process breaks VSTO interop | Critical | Bridge already exists (Som3a.Bridge). Extend pipe protocol for shell commands (navigation, ribbon actions). |
| WPF-UI incompatible with existing theme engine | Medium | Pilot on single panel first. Fallback: use only FluentIcons.WPF + CommunityToolkit.Mvvm, keep custom theme engine. |
| 740 missing Arabic translations in i18n consolidation | Medium | Flag missing strings during migration; auto-fallback to English; complete translations incrementally. |
| SQLite settings migration data loss | High | Test migration on backup of legacy files first. Keep legacy files until migration verified. |
| AI provider chain complexity increases bug surface | Medium | Comprehensive error handling and logging per provider. Unit tests for fallback logic. |

## Rollback Strategy

Each phase must be developed on a dedicated feature branch (`fluent/phase-1a`, `fluent/phase-1b`, etc.). Do not merge to `develop` until the phase verification passes. If a phase verification fails:

| Scenario | Rollback Action |
|----------|----------------|
| Build fails after framework upgrade (Phase 1A) | `git revert` all changes in the branch; re-examine API incompatibilities one by one |
| Out-of-process breaks Excel interop (Phase 1B) | Keep WPF in-process for current release; defer out-of-process to next major version |
| WPF-UI pilot causes theme regression (Phase 3.0) | Activate fallback: use `FluentIcons.WPF` only, keep existing custom theme engine, skip WPF-UI controls |
| Settings migration corrupts user data (Phase 5) | Legacy `Properties.Settings` files are NOT deleted until 72h after successful migration verification; restore from legacy files if SQLite data is invalid |
| Any phase causes VSTO smoke test failure | Do not merge; fix on branch; re-run full smoke test before re-attempting merge |

## Files to Create

| File | Phase |
|------|-------|
| `ViewModels/Dashboard/AIDetectStatusWidgetViewModel.cs` | 9 |
| `ViewModels/Dashboard/DatabaseStatusWidgetViewModel.cs` | 9 |
| `ViewModels/Dashboard/ThemeStatusWidgetViewModel.cs` | 9 |
| `Docs/VSTO_SMOKE_TEST.md` | 1A |
| `Som3a.Bridge/PipeMessageSchema.cs` | 1B |
| `Som3a.Bridge/ExcelCommandProtocol.cs` | 1B |
| `ViewModels/Dashboard/AppInfoWidgetViewModel.cs` | 9 |
| `ViewModels/Dashboard/ExcelConnectionStatusWidgetViewModel.cs` | 9 |
| `ViewModels/Dashboard/MemoryUsageWidgetViewModel.cs` | 9 |
| `ViewModels/Dashboard/LastSyncWidgetViewModel.cs` | 9 |
| `Som3a.Infrastructure/Persistence/Models/ResourceDictionaryRecord.cs` | 10 |
| `WpfApp2/Services/IResourceDictionaryService.cs` | 10 |
| `WpfApp2/Services/ResourceDictionaryService.cs` | 10 |
| `WpfApp2/Pages/ResourceDictionaryPage.xaml` | 10 |
| `WpfApp2/Pages/ResourceDictionaryPage.xaml.cs` | 10 |
| `WpfApp2/ViewModels/ResourceDictionaryViewModel.cs` | 10 |
| `WpfApp2/Services/SettingsService.cs` | 5 |
| `Docs/VSTO_TEST_PROTOCOL.md` | 12 |

## Files to Delete

| File | Phase | Reason |
|------|-------|--------|
| `Theme/MaterialIntegration.xaml` | 1C | MaterialDesign bridge no longer needed |
| `Theme/Controls/MaterialIcons.xaml` | 1C | Replaced by FluentIcons styles |
| `Theme/Controls/MaterialControls.xaml` | 1C | Replaced by WPF-UI controls |
| `Converters/MaterialIconConverter.cs` | 3.x | Replaced by FluentIconConverter |
| `Models/RelayCommand.cs` | 2 | Replaced by CommunityToolkit.Mvvm |
| `Helpers/AsyncRelayCommand.cs` | 2 | Replaced by CommunityToolkit.Mvvm |
| `Services/LocalizationBridgeService.cs` | 4 | Consolidated into Som3a.Localization |
| `Services/TranslationSource.cs` | 4 | Consolidated into Som3a.Localization |
| `Services/ThemeSettings.cs` | 5.x | Settings migrated to SQLite |
| `Services/AISettings.cs` | 5.x | Settings migrated to SQLite |
