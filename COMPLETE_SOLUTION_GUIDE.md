# Som3a Addin 2026 - Complete Solution Guide

**Status**: Production Ready / Validation Phase
**Framework**: .NET Framework 4.8 + WPF
**Last Updated**: May 25, 2026
**Build Status**: Successfully Compiles (3 projects + 1 test project + 1 sample module)
**Solution Root**: `C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\`

---

## Table of Contents

1. [Solution Overview](#solution-overview)
2. [Project Structure](#project-structure)
3. [Architecture & Design](#architecture--design)
4. [Installation & Setup](#installation--setup)
5. [Configuration](#configuration)
6. [Implementation Phases (0-11)](#implementation-phases)
7. [Core Components](#core-components)
8. [Usage Guide](#usage-guide)
9. [API Reference](#api-reference)
10. [Troubleshooting](#troubleshooting)
11. [Development Workflow](#development-workflow)
12. [Testing Strategy](#testing-strategy)
13. [Deployment Guide](#deployment-guide)
14. [Future Roadmap](#future-roadmap)
15. [Specification & Governance](#specification--governance)

---

## 1. Solution Overview

### Purpose
The Som3a Addin 2026 is a comprehensive Excel VSTO Add-in for Primavera P6 project management data. It enables:
- Connect to Primavera databases (Oracle/SQL Server)
- Load and compare multiple project versions
- Identify differences in activities, relationships, and resources
- Export comparison results to XER format
- Visualize results in a unified Windows-11-Fluent WPF Shell interface
- Runtime theming (Dark/Light/Custom with accent colors)
- Plugin/module architecture for future extensibility
- Enterprise diagnostics, logging, and validation

### Key Capabilities
- Multi-database support (SQL Server, Oracle-ready)
- Async/await throughout for responsiveness
- Parallel data loading with progress tracking
- O(1) lookup-based comparison engine
- Full XML documentation
- Production-ready code quality
- XER file parsing and editing
- Excel comparison and analysis tools
- WBS styling and coloring
- Float path and critical path analysis
- Trade code management
- Workbook links management
- Toast notifications system
- Full Fluent Design token-based theme engine (Dark/Light/Custom + 8 accent swatches)
- Unified Shell navigation (Sidebar + Workspace with Pages)
- DI container (ServiceContainer), EventBus, Module Registry
- Plugin/module system with lazy loading
- Diagnostics & logging platform
- Validation engine (token integrity, hardcoded color detection)

### Technology Stack
```
Language:        C# (LangVersion 14.0, nullable enabled)
Framework:       .NET Framework 4.8
UI Framework:    WPF (Windows Presentation Foundation)
MVVM Toolkit:    CommunityToolkit.Mvvm 8.4.2
Database:        SQL Server (Oracle.ManagedDataAccess ready)
Build System:    MSBuild / Visual Studio Community 2026 (18.5.1)
Testing:         MSTest (Tests/ project)
NuGet Packages:  Oracle.ManagedDataAccess, System.Data.SqlClient,
                 Newtonsoft.Json 13.0.4, Microsoft.Web.WebView2 1.0.3912.50,
                 CommunityToolkit.Mvvm 8.4.2
```

---

## 2. Project Structure

### Solution Layout (3 main projects + 3 ancillary)
```
Som3a Addin 2026/
├── Som3a Addin 2026.slnx                # Solution file (3 projects, ARM64 + Any CPU)
│
├── Som3a Addin 2026/                    # [VSTO Excel Add-in - .NET Framework 4.8]
│   ├── ThisAddIn.cs                      # VSTO entry point (creates WPF App, registers DI)
│   ├── Ribbon1.cs                        # Excel Ribbon - 14 buttons + dynamic modules group
│   ├── Ribbon1.Designer.cs               # Ribbon designer code
│   ├── Ribbon1.resx                      # Ribbon resources
│   ├── Som3aAddinBridge.cs               # Bridge between Excel VSTO and WPF
│   ├── ExcelWindowHandle.cs              # Get Excel HWND for WPF owner
│   ├── WpfDialogHost.cs                  # Host WPF dialogs in Excel
│   ├── AddInAutomation.cs                # IAddInAutomation interface
│   ├── IAddInAutomation.cs               # Add-in automation contract
│   ├── Properties/
│   │   ├── AssemblyInfo.cs
│   │   ├── Resources.Designer.cs / Resources.resx
│   │   ├── Settings.Designer.cs
│   │   └── Settings.settings
│   ├── Resources/                        # 25+ PNG/JPG icons for ribbon
│   ├── Ui/
│   │   ├── WpfDialogHost.cs
│   │   └── LinksManagerLauncher.cs
│   ├── app.config                        # Connection strings + app settings
│   ├── packages.config
│   ├── Som3a Addin 2026.csproj
│   ├── Som3a Addin 2026.csproj.user
│   └── Som3a Addin 2026_TemporaryKey.pfx # ClickOnce signing key
│
├── Som3a.Shared/                         # [Shared Business Logic - .NET Framework 4.8]
│   ├── Models/
│   │   ├── Primavera/
│   │   │   ├── ProjectDto.cs             # P6 project data transfer object
│   │   │   ├── ActivityDto.cs            # P6 activity DTO
│   │   │   ├── RelationshipDto.cs        # P6 relationship DTO
│   │   │   └── ResourceDto.cs            # P6 resource DTO
│   │   ├── AssignTradeCodesViewModel.cs
│   │   ├── MainViewModel.cs
│   │   ├── Float_path.cs
│   │   ├── WbsItem.cs / WbsLevelStyle.cs
│   │   ├── ExcelService.cs / ExcelColorHelper.cs
│   │   ├── TableItemVM.cs
│   │   ├── XerMapper.cs
│   │   ├── LinkItem.cs / LinkTypeItem.cs
│   │   ├── WorkbookItem.cs
│   │   ├── SheetCheckRow.cs
│   │   ├── ProjectAnalysisLogRow.cs
│   │   ├── ProjectAnalysisSummaryRow.cs
│   │   ├── CompareResultItem.cs
│   │   └── RelayCommand.cs
│   ├── Core/
│   │   ├── Primavera/
│   │   │   ├── IPrimaveraDbService.cs / PrimaveraDbService.cs
│   │   │   ├── IPrimaveraDataLoaderService.cs / PrimaveraDataLoaderService.cs
│   │   │   ├── IPrimaveraComparisonService.cs / PrimaveraComparisonService.cs
│   │   │   ├── ComparisonModels.cs
│   │   │   └── ProjectFullData.cs
│   │   ├── AssignTradeCodesService.cs
│   │   ├── ExcelCompareService.cs
│   │   ├── ExcelLinkTextHelper.cs
│   │   ├── FixPieColorsService.cs / FixPieColorsResult.cs
│   │   ├── FloatPathService.cs
│   │   ├── GraphBuilder.cs / GraphService.cs
│   │   ├── LinksManagerService.cs
│   │   ├── PathFinder.cs
│   │   ├── SubDlyReportService.cs
│   │   ├── UnmergeFillDownService.cs
│   │   ├── WorkbookCloneService.cs
│   │   ├── WbsBuilder.cs / WbsColoringService.cs / WbsStyleFactory.cs
│   │   ├── XER Parser.cs / XerExportService.cs
│   ├── Controllers/
│   │   └── CompareController.cs
│   ├── Interop/
│   │   └── ComRelease.cs
│   ├── Utils/
│   │   └── RelUtils.cs
│   ├── Properties/AssemblyInfo.cs
│   ├── packages.config
│   └── Som3a.Shared.csproj
│
├── WpfApp2/                              # [WPF UI Library - .NET Framework 4.8]
│   ├── App.xaml / App.xaml.cs             # Application resources (ThemeResources + DarkTheme)
│   ├── CompositionRoot.cs                # DI registration (ServiceContainer, EventBus, ModuleRegistry)
│   │
│   ├── Controls/
│   │   ├── ModernWindow.cs               # Custom Window subclass (VSTO-safe, DPI-aware, theme integration)
│   │   ├── LoadingOverlay.xaml/.cs       # Loading spinner overlay
│   │   ├── ModuleLoadingOverlay.xaml/.cs  # Module lazy-load overlay
│   │   ├── Shell/
│   │   │   ├── ShellWindow.xaml/.cs      # Main Shell container (Sidebar + Workspace)
│   │   │   ├── ShellState.cs             # Shell navigation state
│   │   │   ├── SidebarControl.xaml/.cs   # Sidebar navigation
│   │   │   ├── WorkspaceHost.cs           # Page host in Shell
│   │   │   ├── CommandPalette.xaml/.cs    # Ctrl+K command palette
│   │   │   ├── NavigationDestination.cs   # Navigation target definition
│   │   │   ├── NavigationEventArgs.cs     # Navigation event args
│   │   │   └── NavigationPage.cs          # Page base class
│   │   └── Toast/
│   │       ├── ToastModel.cs
│   │       ├── ToastWindow.xaml/.cs
│   │
│   ├── Pages/                             # Shell-hosted Page variants (Phase 11 Migration)
│   │   ├── PageBase.cs                    # Base Page with lifecycle hooks
│   │   ├── WelcomePage.xaml/.cs
│   │   ├── MainPage.xaml/.cs
│   │   ├── SettingsPage.xaml/.cs
│   │   ├── ProjectAnalysisPage.xaml/.cs
│   │   ├── FloatPathPage.xaml/.cs
│   │   ├── AssignTradeCodesPage.xaml/.cs
│   │   ├── FixPieColorsPage.xaml/.cs
│   │   ├── LinksManagerPage.xaml/.cs
│   │   ├── StyleSelectorPage.xaml/.cs
│   │   ├── SubDailyReportPage.xaml/.cs
│   │   ├── UnmergeFillDownPage.xaml/.cs
│   │   ├── XerEditorPage.xaml/.cs
│   │   ├── PrimaveraComparePage.xaml/.cs
│   │   └── PrimaveraResultsPage.xaml/.cs
│   │
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs               # INotifyPropertyChanged base
│   │   ├── Primavera/
│   │   │   ├── PrimaveraCompareViewModel.cs
│   │   │   └── PrimaveraResultsViewModel.cs
│   │   ├── FixPieColorsViewModel.cs
│   │   ├── FloatPathViewModel.cs
│   │   ├── LinksManagerViewModel.cs
│   │   ├── ProjectAnalysisViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   ├── SubDailyReportViewModel.cs
│   │   ├── ToastViewModel.cs
│   │   ├── UnmergeFillDownViewModel.cs
│   │   ├── WbsStyleSelectorViewModel.cs
│   │   ├── XerEditorViewModel.cs
│   │   ├── CommandPaletteViewModel.cs
│   │   ├── DiagnosticsViewModel.cs
│   │   └── PluginDiagnosticsViewModel.cs
│   │
│   ├── Views/
│   │   ├── SettingsWindow.xaml/.cs        # Theme + accent settings (ModernWindow)
│   │   ├── AccessibilityPanel.xaml/.cs    # Accessibility settings tab
│   │   ├── AppearancePanel.xaml/.cs       # Appearance settings tab
│   │   ├── DiagnosticsPanel.xaml/.cs      # Diagnostics settings tab
│   │   ├── ExcelPanel.xaml/.cs            # Excel settings tab
│   │   ├── PerformancePanel.xaml/.cs      # Performance settings tab
│   │   └── PluginsPanel.xaml/.cs          # Plugin settings tab
│   │
│   ├── Windows/
│   │   └── PrimaveraComparison/
│   │       ├── PrimaveraCompareWindow.xaml/.cs
│   │       └── PrimaveraResultsWindow.xaml/.cs
│   │
│   ├── UI/
│   │   ├── ProjectAnalysisWindow.xaml/.cs
│   │
│   ├── Services/
│   │   ├── ThemeManager.cs                # Singleton runtime theme switcher
│   │   ├── ThemeSettings.cs               # Legacy JSON theme persistence
│   │   ├── WindowRenderModeDetector.cs    # Auto-detect Excel VSTO rendering mode
│   │   ├── RenderModeService.cs           # Render mode cache + events
│   │   ├── DialogService.cs               # Show MessageBox dialogs from VMs
│   │   ├── ToastService.cs                # Toast notifications
│   │   ├── ServiceContainer.cs            # DI container (Singleton/Transient/Scoped)
│   │   ├── EventBus.cs                    # Typed event pub/sub with weak references
│   │   ├── ModuleRegistry.cs              # Module registration + priority-init
│   │   ├── ModuleLoadOrchestrator.cs      # Module lifecycle management
│   │   ├── ModuleInitializationContext.cs  # Per-module init context
│   │   ├── ModuleRibbonActionsAvailableEvent.cs
│   │   ├── NavigationService.cs           # Shell navigation singleton
│   │   ├── NavigationRegistrar.cs         # Register pages with nav service
│   │   ├── ShellNavigationHelper.cs       # Shell nav utilities
│   │   ├── CommandRegistrar.cs            # Register commands for palette
│   │   ├── RibbonRegistrar.cs             # Register ribbon actions
│   │   ├── PluginLoader.cs                # Discover + load plugin assemblies
│   │   ├── PluginRegistry.cs              # Plugin metadata registry
│   │   ├── LazyModuleCommand.cs           # Deferred module command
│   │   ├── DiagnosticsService.cs          # Render/theme/memory diagnostics
│   │   ├── LoggingService.cs              # File logging (5MB rollover, 3-file rotation)
│   │   ├── ValidationEngine.cs            # Token integrity + hardcoded color detection
│   │   ├── ModuleDiagnosticsService.cs    # Plugin stability diagnostics
│   │   ├── SettingsPersistenceService.cs  # Settings serialization
│   │   └── ExcelProjectAnalysisService.cs
│   │
│   ├── Contracts/
│   │   ├── ICommandRegistrar.cs
│   │   ├── IModule.cs
│   │   ├── IModuleInitializationContext.cs
│   │   ├── IModuleRegistry.cs
│   │   ├── INavigationRegistrar.cs
│   │   ├── IPluginLoader.cs
│   │   ├── IRibbonRegistrar.cs
│   │   ├── ModuleInfo.cs
│   │   └── ModuleManifest.cs
│   │
│   ├── Theme/                             # Fluent Design token system
│   │   ├── ThemeResources.xaml            # Aggregator with documented loading order
│   │   ├── ModernWindow.xaml              # ModernWindow control template
│   │   ├── ShellStyles.xaml               # Shell-specific styles
│   │   ├── WindowAnimations.xaml          # Window open/close fade animations
│   │   ├── Base/
│   │   │   ├── Colors.xaml                # Primitive + Semantic color tokens
│   │   │   ├── Typography.xaml            # Font size/weight tokens
│   │   │   ├── Spacing.xaml               # Margin/padding/height tokens
│   │   │   ├── Radius.xaml                # Corner radius tokens
│   │   │   ├── Elevation.xaml             # Elevation level tokens
│   │   │   ├── Motion.xaml                # Animation duration/easing tokens
│   │   │   ├── Opacity.xaml               # Standard opacity values
│   │   │   ├── ZIndex.xaml                # Z-index layering constants
│   │   │   └── ComponentTokens.xaml       # Component-specific tokens
│   │   ├── Controls/                      # 22 control style XAML files
│   │   │   ├── ButtonStyles.xaml
│   │   │   ├── ComboBoxStyles.xaml / ComboBoxItemStyles.xaml
│   │   │   ├── TextBoxStyles.xaml / PasswordBoxStyles.xaml
│   │   │   ├── CheckBoxStyles.xaml / RadioButtonStyles.xaml
│   │   │   ├── ToggleButtonStyles.xaml
│   │   │   ├── DataGridStyles.xaml
│   │   │   ├── ListViewStyles.xaml / ListViewItemStyles.xaml
│   │   │   ├── TreeViewStyles.xaml
│   │   │   ├── ScrollViewerStyles.xaml / ScrollBarStyles.xaml
│   │   │   ├── ProgressBarStyles.xaml
│   │   │   ├── GroupBoxStyles.xaml / LabelStyles.xaml
│   │   │   ├── ThemeCardStyles.xaml / AccentSwatchStyles.xaml
│   │   │   ├── WindowStyles.xaml / WindowButtonStyles.xaml
│   │   │   └── SettingsPanelStyles.xaml
│   │   ├── Dark/
│   │   │   ├── DarkColors.xaml / DarkTheme.xaml
│   │   ├── Light/
│   │   │   ├── LightColors.xaml / LightTheme.xaml
│   │   ├── Custom/
│   │   │   ├── CustomColors.xaml / CustomTheme.xaml
│   │   ├── Effects/
│   │   │   ├── Shadows.xaml               # Centralized DropShadowEffect (7 + safe variants)
│   │   │   ├── Glow.xaml                  # Centralized glow effects (6 + accent dynamic)
│   │   │   └── Animations.xaml            # Storyboards (all ≤200ms)
│   │   └── Fluent/
│   │       ├── FluentEffects.xaml          # Legacy effects (excluded from build)
│   │       └── FluentWhite.xaml            # Legacy light theme (excluded from build)
│   │
│   ├── Helpers/
│   │   ├── NotifyBase.cs                  # INotifyPropertyChanged base
│   │   ├── RelayCommand.cs / AsyncRelayCommand.cs
│   │   ├── DpiHelper.cs                   # Monitor DPI detection + scaling
│   │   ├── ExcelOwnerHelper.cs            # Parent WPF to Excel window
│   │   ├── WindowChromeHelper.cs          # Apply WindowChrome
│   │   ├── WindowBehaviorHelper.cs        # Window property reflection
│   │   └── WindowValidationHelper.cs      # Validate window properties
│   │
│   ├── Converters/
│   │   ├── SharedConverters.cs            # BoolToVisibility, NullToVisibility
│   │   ├── DifferenceTypeToColorConverter.cs
│   │   └── WindowConverters.cs
│   │
│   ├── Behaviors/
│   │   ├── DragMoveBehavior.cs
│   │   └── EscapeCloseBehavior.cs
│   │
│   ├── Models/
│   │   ├── RelayCommand.cs / DiagnosticsModels.cs
│   │   ├── SettingsCategory.cs / SettingsExport.cs / UserSettings.cs
│   │   ├── SubDailyReportViewModel.cs
│   │   ├── UnmergeFillDownViewModel.cs
│   │   └── XerEditorVM.cs
│   │
│   ├── Analyzers/
│   │   ├── ModernWindowAnalyzer.cs        # Roslyn analyzer (excluded from build)
│   │   └── README.md
│   │
│   ├── UIHost/
│   │   └── WpfWindowManager.cs            # Manage WPF windows in Excel host
│   │
│   ├── scripts/
│   │   └── Validate-Tokens.ps1
│   │
│   ├── Properties/
│   │   ├── AssemblyInfo.cs
│   │   ├── Resources.Designer.cs / Resources.resx
│   │   ├── Settings.Designer.cs           # Theme persistence accessors
│   │   └── Settings.settings             # User-scoped theme + accent settings
│   │
│   ├── App.config
│   ├── packages.config
│   ├── Som3a_WPF_UI.csproj
│   └── Som3a_WPF_UI.csproj.user
│
├── Tests/                                # [Unit Test Project - .NET Framework 4.8]
│   ├── Som3a_WPF_UI.Tests.csproj
│   ├── ServiceContainerTests.cs
│   ├── EventBusTests.cs
│   ├── ViewModelBaseTests.cs
│   ├── Run-VSTOTests.ps1
│   └── VSTOResults.xml
│
├── WpfApp2.Modules.Sample/               # [Sample Plugin Module - .NET Framework 4.8]
│   ├── WpfApp2.Modules.Sample.csproj
│   ├── SampleModule.cs
│   └── module.json
│
├── Som3a_WPF_UI/                         # [Standalone WPF App - .NET 10.0] (experimental)
│   ├── MainWindow.xaml/.cs
│   ├── Som3a_WPF_UI.csproj
│   └── Som3a_WPF_UI.csproj.user
│
├── Som3a_WPF_UId/                        # [Legacy WPF App - .NET Framework 4.7.2]
│   ├── MainWindow.xaml/.cs
│   ├── Class1.cs
│   ├── Properties/AssemblyInfo.cs
│   └── Som3a_WPF_UI.csproj
│
├── WpfApp1/                              # [Old/Legacy WPF App - .NET Framework 4.7.2]
│   ├── MainWindow.xaml/.cs
│   ├── App.config
│   ├── Properties/
│   └── Som3a_WPF_UI.csproj
│
├── Docs/                                  # Documentation
│   ├── PROJECT_GUIDE.md
│   ├── QUICK_REFERENCE.md
│   ├── SESSION_STATUS.md
│   ├── fix.md
│   ├── session_5b_settings.md
│   ├── session_prompts.md
│   ├── session_prompts -2.md
│   └── Architecture/
│       ├── AGENT_RULES.md                 # AI execution rules
│       ├── UI_GUIDELINES.md               # Windows 11 Fluent design rules
│       ├── TOKEN_RULES.md                 # Naming conventions + layers
│       ├── SHADOW_SYSTEM.md               # Centralized effects docs
│       ├── POPUP_ARCHITECTURE.md          # ComboBox popup architecture
│       ├── EXCEL_RENDERING_RULES.md       # VSTO-safe rendering rules
│       ├── MVVM_RULES.md                  # MVVM separation standards
│       ├── ACCESSIBILITY_RULES.md         # Keyboard nav + screen reader prep
│       ├── PERFORMANCE_RULES.md           # ≤200ms animations, virtualization
│       ├── REVIEW_CHECKLIST.md            # Mandatory review gates
│       ├── AUDIT_REPORT.md                # Phase 0 architecture audit
│       ├── EXCEL_TEST_CHECKLIST.md        # VSTO test scenarios
│       ├── MVVM_COMPLIANCE.md             # Per-file MVVM audit
│       ├── BRANCH_NAMING.md               # Branch naming standards
│       ├── BRANCH_PROTECTION.md           # Branch protection rules
│       ├── README.md
│       ├── PERFORMANCE_AUDIT_REPORT.md    # Phase 10: perf baselines
│       ├── ACCESSIBILITY_AUDIT_REPORT.md  # Phase 10: keyboard + contrast
│       ├── DPI_AUDIT_REPORT.md            # Phase 10: DPI validation
│       ├── EXCEL_STABILITY_REPORT.md      # Phase 10: VSTO stability
│       ├── LOCALIZATION_READINESS.md      # Phase 10: i18n architecture
│       └── ENTERPRISE_POLISH_CHECKLIST.md # Phase 10: master checklist
│
├── specs/                                 # Feature specifications (13 features)
│   ├── feature-planning-guide.md
│   ├── guide-files-path-and-wbs.md
│   ├── 001-fluent-theme-engine/           # Theme engine + effects + control standardization
│   ├── 001-github-governance-workflow/    # Git workflow + review gates
│   ├── 001-diagnostics-stability-platform/ # Diagnostics + logging + validation
│   ├── 001-settings-personalization-ux/   # Settings UI + panels
│   ├── 002-themes-manager/                # ThemeManager fixes + hardcoded color elimination
│   ├── 004-design-system-core/            # Token architecture + primitive/semantic/component
│   ├── 005-rendering-infrastructure/      # Excel-safe rendering + DPI
│   ├── 006-phase-3-spec/                  # Phase 3 integration spec
│   ├── 007-control-standardization/       # Control template audit + refactor
│   ├── 008-navigation-shell-platform/     # Shell + sidebar + navigation service
│   ├── 009-mvvm-architecture-cleanup/     # DI container + event bus + module registry
│   ├── 011-legacy-window-migration/       # Window → Page migration (Phase 11)
│   ├── 012-plugin-feature-platform/       # Plugin loader + module system
│   └── 013-enterprise-polish/             # Performance, accessibility, DPI, stability (Phase 10)
│
├── .opencode/                             # OpenCode AI agent configuration
│   ├── commands/                          # Speckit commands (14 .md files)
│   ├── plans/                             # OpenCode plans (10 phase plans)
│   ├── skills/image-to-md/                # Image-to-markdown skill
│   └── package.json
│
├── .specify/                              # SpecKit governance framework
│   ├── memory/
│   │   ├── constitution.md                # v1.2.0 (16 principles)
│   │   └── constitution-v2.md            # v2.0.0 (with AI rules + branch standards)
│   ├── templates/                         # Plan, spec, tasks, checklist templates
│   ├── scripts/powershell/                # Setup + prerequisite scripts
│   ├── integrations/                      # OpenCode + Speckit manifests
│   ├── extensions/git/                    # Git extension (commands + scripts)
│   └── workflows/                         # Workflow registry
│
├── .github/                               # GitHub configuration
│   ├── CODEOWNERS
│   ├── pull_request_template.md
│   └── workflows/
│
├── packages/                              # NuGet offline cache
├── scripts/
│   └── Validate-Tokens.ps1
├── Git/
│   └── Uploadgit.md
├── README.md
├── AGENTS.md                              # AI agent context instructions
├── implementation_plan.md                 # Enterprise UI Transformation Master Plan (1948 lines)
├── extraction_1779466152081.md
├── .gitattributes
└── .gitignore
```

### File Count by Project
```
Som3a Addin 2026:       18 files (Source + Config + Properties)
Som3a.Shared:           41 files (Models + Core Services + Controllers + Interop + Utils)
WpfApp2:                150+ files (UI + ViewModels + Services + Theme + Pages + Shell)
Tests:                    5 files (3 test classes + scripts)
WpfApp2.Modules.Sample:   3 files
Legacy projects:         10 files
Specs/Docs:              70+ files
Config/Workflow:         40+ files
────────────────────────────────────
Total:                  340+ files
```

---

## 2.1 Available Modules & Features

| Module | Type | Entry Point |
|--------|------|-------------|
| **Primavera Comparison** | Service + Window/Page | PrimaveraCompareWindow, CompareController |
| **Primavera Results** | Window/Page | PrimaveraResultsWindow |
| **XER Editor** | Window/Page + Service | XerEditorWindow, XerParser, XerExportService |
| **Assign Trade Codes** | Window/Page + Service | AssignTradeCodesWindow |
| **Links Manager** | Window/Page + Service | LinksManagerWindow, LinksManagerService |
| **Sub Daily Report** | Window/Page + Service | SubDailyReportWindow, SubDlyReportService |
| **Float Path Analysis** | Window/Page + Service | Float_path, FloatPathService, PathFinder |
| **Fix Pie Colors** | Window/Page + Service | Fixpiecolors, FixPieColorsService |
| **Unmerge Fill Down** | Window/Page + Service | UnmergeFillDownWindow |
| **Excel Compare** | Service | ExcelCompareService |
| **Project Analysis** | Window/Page + Service | ProjectAnalysisWindow |
| **WBS Styling** | Service | WbsBuilder, WbsStyleFactory, WbsColoringService |
| **Workbook Clone** | Service | WorkbookCloneService |
| **Settings** | Window/Page | SettingsWindow (Appearance, Accessibility, Performance, Excel, Plugins, Diagnostics panels) |
| **Plugin Diagnostics** | View | PluginDiagnosticsViewModel, DiagnosticsPanel |
| **Toast Notifications** | Service + Control | ToastService, ToastWindow |

### Theme System (Fluent Design Tokens)
- **Dark Theme**: Default dark (#0E1720 background, Slate palette)
- **Light Theme**: Fluent white theme
- **Custom Theme**: 8 accent color presets
- **Design Tokens**: 9 base token files (Colors, Typography, Spacing, Radius, Elevation, Motion, Opacity, ZIndex, ComponentTokens)
- **22 control style files**: All controls styled with VSM states
- **3 effects files**: Shadows (7 + safe), Glow (6 variants), Animations (≤200ms)
- **3 theme variants**: Dark, Light, Custom (each with Colors + merged dictionary)
- **ThemeManager**: Singleton with ApplyTheme(), debouncing, accent color application, persistence via Settings.settings

### Shell Navigation
- **ShellWindow**: Main container (ModernWindow-based) with Sidebar + Workspace
- **SidebarControl**: Navigation sidebar with icons + labels
- **CommandPalette**: Ctrl+K searchable command palette
- **NavigationService**: Singleton page navigation with history
- **14 Pages**: All feature windows available as Shell-hosted Pages

### Plugin/Module System
- **ModuleRegistry**: Priority-based module initialization
- **PluginLoader**: Discover modules from assemblies + module.json manifests
- **ModuleLoadOrchestrator**: Manage module lifecycle + lazy loading
- **LazyModuleCommand**: Deferred command execution
- **PluginRegistry**: Module metadata tracking
- **ModuleDiagnosticsService**: Plugin stability diagnostics

### Diagnostics Platform
- **DiagnosticsService**: Render mode, active theme, memory, popup diagnostics
- **LoggingService**: File-based logging (5MB rollover, 3-file rotation, AppData path)
- **ValidationEngine**: Token integrity check, hardcoded color detection, missing token scan

---

## 3. Architecture & Design

### Layered Architecture
```
┌────────────────────────────────────────────────┐
│         VSTO Excel Add-in Layer                  │
│  (Ribbon, ThisAddIn, Bridge, Dialog Hosting)    │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│         WPF Shell Layer                         │
│  (ShellWindow, Sidebar, Pages, CommandPalette)  │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│         WPF ViewModels Layer                    │
│  (ViewModelBase, ICommand, DI injection)       │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│     Business Logic Layer (Shared Services)      │
│  (Comparison, Loading, Database, WBS, XER)     │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│     DI & Infrastructure Layer                   │
│  (ServiceContainer, EventBus, ModuleRegistry)  │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│     Data Access Layer                           │
│  (Database Connectivity, DTO Mapping)          │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│    External Data Sources                        │
│  (Primavera DB: Oracle/SQL Server)             │
└────────────────────────────────────────────────┘
```

### Design Patterns Used

| Pattern | Component | Purpose |
|---------|-----------|---------|
| MVVM | Windows/Pages & ViewModels | Separation of UI and logic |
| Dependency Injection | ServiceContainer | Loose coupling, testability |
| Service Layer | Core services (Db, Comparison) | Business logic encapsulation |
| Singleton | ThemeManager, NavigationService | Global runtime state |
| Event Bus | EventBus (typed, weak refs) | Decoupled cross-module communication |
| Module/Plugin | IModule, PluginLoader | Extensible plugin architecture |
| Repository | IPrimaveraDbService | Database abstraction |
| DTO | ProjectDto, ActivityDto, etc. | Clean data contracts |
| Async/Await | All I/O operations | Non-blocking operations |
| Observer | INotifyPropertyChanged | UI updates from ViewModels |

### Key Architectural Decisions

1. **Async/Await Throughout**: All database and I/O operations use async/await
2. **Dictionary-based Comparison**: O(1) lookups for performance
3. **DynamicResource Only**: All themeable properties use DynamicResource (no StaticResource)
4. **Centralized Effects**: No inline DropShadowEffect — use `{DynamicResource Shadow.*}`
5. **Excel-Safe Rendering**: WindowRenderModeDetector auto-detects VSTO hosting, activates FallbackSafe mode
6. **Incrementally Migrated Pages**: Original XAML preserved until each Page validates in Excel VSTO
7. **Module-Based Extensibility**: Features register pages, commands, and ribbon actions via IModule
8. **2 Branch Strategy**: main, develop, feature/phase-NN-name branches with CodeRabbit review gates

---

## 4. Installation & Setup

### Prerequisites
- Visual Studio Community 2026 (or 2022+)
- .NET Framework 4.8 Developer Pack
- SQL Server or Oracle (for Primavera database)
- Administrator privileges
- Microsoft Office 2016+ (Excel)

### Installation Steps

#### Step 1: Clone/Open Solution
```powershell
cd C:\Users\mohamedabdelsamea\source\repos\
git clone <repository-url> "Som3a Addin 2026"
cd "Som3a Addin 2026"
```

#### Step 2: Open Solution File
```powershell
Start-Process "Som3a Addin 2026.slnx"
```

#### Step 3: Restore NuGet Packages
```
Visual Studio Menu: Tools → NuGet Package Manager → Manage NuGet Packages for Solution
Click: Restore All Packages
```

#### Step 4: Build Solution
```
Visual Studio Menu: Build → Build Solution (Ctrl+Shift+B)
```

Expected Output:
```
========== Build: 3 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
```

#### Step 5: Set Startup Project
```
Right-click on "Som3a Addin 2026" → Set as Startup Project
```

#### Step 6: Configure Debug Settings
```
Project Properties → Debug → Start external program:
C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE
```

---

## 4.1 Build Instructions

### Quick Build (PowerShell)
```powershell
cd "C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026"
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "WpfApp2\Som3a_WPF_UI.csproj" /p:Configuration=Debug
```

### Full Solution Build
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "Som3a Addin 2026.slnx" /p:Configuration=Release /t:Build
```

### Clean and Rebuild
```powershell
msbuild "Som3a Addin 2026.slnx" /t:Clean
msbuild "Som3a Addin 2026.slnx" /t:Rebuild /p:Configuration=Release
```

### Run Unit Tests
```powershell
cd Tests
vstest.console.exe Som3a_WPF_UI.Tests.csproj
```

---

## 5. Configuration

### Database Connection Configuration

#### SQL Server Connection
```csharp
var connectionString =
    "Server=localhost;Database=primavera_p6;User Id=sa;Password=YourPassword;";

var isConnected = await dbService.TestConnectionAsync(connectionString, "SqlServer");
```

**Connection String Format**:
```
Server=<hostname>;Database=<database_name>;User Id=<username>;Password=<password>;
```

**Required Tables**:
- MSP_PROJECTS
- MSP_TASKS
- MSP_TASKPREDS
- MSP_TASKRSRCS
- MSP_WBSES
- MSP_RESOURCES

#### Oracle Connection (Ready)
```csharp
var connectionString =
    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=hostname)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=service)));User Id=username;Password=password;";
```

### Application Configuration (Som3a Addin 2026/app.config)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <appSettings>
        <add key="DefaultDatabaseType" value="SqlServer"/>
        <add key="CommandTimeout" value="300"/>
        <add key="EnableLogging" value="true"/>
    </appSettings>
    <connectionStrings>
        <add name="PrimaveraDb"
             connectionString="Server=localhost;Database=primavera_p6;User Id=sa;Password=YourPassword;"
             providerName="System.Data.SqlClient"/>
    </connectionStrings>
</configuration>
```

### Theme Settings (WpfApp2/Properties/Settings.settings)
- **SelectedTheme**: User-scoped string (Dark, Light, Custom)
- **AccentColor**: User-scoped string (hex color, e.g. #3A86FF)
- **AccentPresetIndex**: User-scoped int (0-7 for 8 preset swatches)

### Diagnostics Configuration (WpfApp2/Services/LoggingService.cs)
- Log path: `%AppData%/Som3a/Logs/`
- Rollover: 5MB per file
- Rotation: 3 files max
- Snapshot coverage: All windows, all render modes

---

## 6. Implementation Phases

### Phase 0: Governance Foundation ✅ COMPLETE
**Branch**: `feature/phase-00-governance-foundation`

- Resolved constitution merge conflicts
- Created architecture audit report (AUDIT_REPORT.md)
- Created 10 governance documents in Docs/Architecture/
- Created constitution v2.0.0 (with AI rules + branch standards)
- Token naming standards established (Primitive.*, Brush.*, Spacing.*, etc.)
- Branch naming convention: `feature/phase-NN-short-name`
- Updated AGENTS.md with Phase 0-2 file paths

### Phase 1: Design System Core ✅ COMPLETE
**Branch**: `feature/phase-01-design-system-core`

- Primitive token system: Primitive.Blue.500, Primitive.Slate.*, Primitive.White.*
- Semantic token system: Brush.Background.*, Brush.Text.*, Brush.Accent.*, etc.
- Component token system: Component.Button.*, Component.Card.*, etc.
- Fix all inline color violations (ButtonStyles, DataGridStyles, etc.)
- New token files: Elevation.xaml, Motion.xaml, Opacity.xaml, ZIndex.xaml, ComponentTokens.xaml
- Standardized spacing (Spacing.XSmall through XXLarge, Padding.*)
- Standardized typography (Style.Text.Title, Subtitle, Body, Caption, Button)

### Phase 2: Rendering & Window Infrastructure ✅ COMPLETE
**Branch**: `feature/phase-02-rendering-infrastructure`

- Audited all 14 windows for ModernWindow compliance
- Extended ModernWindow with RenderMode, IsSafeMode, DpiScale DPs
- Created RenderModeService for session-scoped render caching
- Safe-mode shadow variants (Shadow.Window.Safe, Shadow.Card.Safe, Shadow.Popup.Safe)
- Fallback rendering: disable animations, use safe shadows, disable backdrops
- DPI infrastructure: GetCurrentDpiScale, ScaleValue, IsHighDpi
- Accessibility preparation: FocusVisualStyle, AutomationProperties on all elements
- Excel stability test checklist created

### Phase 3: Theme Engine 2.0 ✅ COMPLETE (15 validation tasks remaining)
**Branch**: `002-fluent-theme-engine` (merged)

**Workstream A — Fluent Theme Engine** (58 tasks):
- Effects library: Shadows.xaml (7 variants), Glow.xaml (6 variants), Animations.xaml (all ≤200ms)
- Theme dictionaries: Dark, Light, Custom (3 themes, each with Colors + merged dictionary)
- ThemeManager singleton: ApplyTheme(), LoadThemeFromSettings(), debouncing, events
- SettingsWindow: 3 theme cards + 8 accent swatches with glow/scale animations
- Control standardization: Button, ComboBox, CheckBox, RadioButton, ToggleButton, TextBox, DataGrid, ScrollViewer
- DPI-aware sizing for theme cards, swatches, popups
- WindowRenderModeDetector integration

**Workstream B — Themes Manager Bug Fixes** (64 tasks):
- Accent persistence fix, early-return fix, safe dictionary fallback
- Thread safety (Dispatcher.InvokeAsync), debouncing (150ms)
- Hardcoded color elimination across all 12 windows
- Progress bar gradient replacement (Brush.Accent.ProgressFill)
- Window background conformance (Brush.Background.Root)

### Phase 4: Control Standardization ✅ COMPLETE
**Branch**: `feature/phase-04-control-standardization`

- ComboBox popup architecture: AllowsTransparency=False, PlacementTarget, width fix
- Standardized all control templates: 22 control style files
- TreeViewStyles.xaml, PasswordBoxStyles.xaml, SettingsPanelStyles.xaml added
- Virtualization enabled on DataGrid and ListView
- Smooth scrolling implemented on ScrollViewer

### Phase 5: Navigation Shell Platform ✅ COMPLETE
**Branch**: `feature/phase-05-navigation-shell`

- ShellWindow: Main container with Sidebar + Workspace host
- SidebarControl: Icon + label navigation sidebar
- WorkspaceHost: Frame-based page container
- NavigationService: Singleton with NavigateTo(), GoBack(), history stack
- CommandPalette: Ctrl+K searchable command palette
- ShellState: Shell navigation state management
- Navigation animations: Fade transitions ≤200ms
- **Scope restriction**: Shell initially applied to NEW windows only

### Phase 6: MVVM & Architecture Cleanup ✅ COMPLETE
**Branch**: `feature/phase-06-mvvm-cleanup`

- ServiceContainer: DI container (Singleton/Transient/Scoped lifetimes, circular detection)
- EventBus: Typed events with weak-reference subscribers, subscriber isolation
- ModuleRegistry: Priority-based initialization, duplicate detection
- ViewModelBase: INotifyPropertyChanged base with SetProperty<T>
- CompositionRoot: Centralized DI registration
- Services registered: ThemeManager, NavigationService, all Shared services
- ViewModels relocated to ViewModels/ directory, constructor-injected via DI

### Phase 7: Settings & Personalization UX ✅ COMPLETE
**Branch**: `feature/phase-07-settings-ux`

- Settings categories: Appearance, Performance, Accessibility, Diagnostics, Excel, Plugins
- SettingsWindow extended with tabbed panels
- Panel files: AccessibilityPanel, AppearancePanel, DiagnosticsPanel, ExcelPanel, PerformancePanel, PluginsPanel
- SettingsViewModel with categories + SettingsPersistenceService
- Live theme preview, accent picker

### Phase 8: Diagnostics & Stability Platform ✅ COMPLETE
**Branch**: `feature/phase-08-diagnostics`

- DiagnosticsService: Render mode, active theme, memory, popup diagnostics
- LoggingService: File-based logging (5MB rollover, 3-file rotation, AppData/Som3a/Logs/)
- ValidationEngine: Token integrity check, hardcoded color detection (regex-based)
- Diagnostics panel UX with loading/error/empty states
- Crash-safe loading for corrupt theme settings

### Phase 9: Plugin & Feature Platform ✅ COMPLETE
**Branch**: `feature/phase-09-plugin-platform`

- Plugin contracts: IModule, IModuleInitializationContext, IModuleRegistry
- PluginLoader: Discover modules from assemblies + module.json manifests
- Lazy loading: Load modules only when needed via LazyModuleCommand
- Module registration: Pages, commands, ribbon actions
- Plugin diagnostics: PluginDiagnosticsViewModel, ModuleDiagnosticsService
- Sample module: WpfApp2.Modules.Sample

### Phase 10: Enterprise Polish 🔄 IN PROGRESS (Validation Phase)
**Branch**: `013-enterprise-polish`

**7 Workstreams**:
- **WS-A**: Performance hardening — memory profiling, rendering optimization, Freeze() on resources, animation budget
- **WS-B**: Accessibility compliance — keyboard nav audit, WCAG 2.1 AA contrast, screen reader prep, reduced motion
- **WS-C**: DPI & multi-monitor validation — 100%/125%/150%/200%, per-monitor DPI, mixed DPI
- **WS-D**: Excel host stability — close 15 Phase 3 validation tasks, full VSTO pass, stress tests
- **WS-E**: Tech debt cleanup — ToastWindow → ModernWindow, hardcoded-value sweep, MVVM_COMPLIANCE audit
- **WS-F**: Diagnostics finalization — snapshot coverage, logging validation, crash recovery
- **WS-G**: Documentation — 6 audit reports (Performance, Accessibility, DPI, Excel Stability, Localization, Enterprise Polish)

**Key deliverables**: 6 audit reports in Docs/Architecture/ created

### Phase 11: Legacy Window Migration 🔄 IN PROGRESS
**Branch**: `011-legacy-window-migration`

- 14 standalone windows being migrated to Pages within unified Shell
- Incremental validation — original XAML preserved until each Page validates
- 4 migration priority tiers
- 14 Page files created in Pages/ directory
- Ribbon launchers updated to use NavigationService.NavigateTo()
- Migration patterns documented in MIGRATION_PATTERNS.md

---

## 7. Core Components

### DI Container (ServiceContainer)
```csharp
// Registration
container.RegisterSingleton<ThemeManager>(ThemeManager.Instance);
container.RegisterSingleton<INavigationService>(NavigationService.Instance);
container.RegisterTransient<IPrimaveraDbService, PrimaveraDbService>();
container.RegisterScoped<...>();

// Resolution
var themeManager = container.Resolve<ThemeManager>();
```

### Event Bus
```csharp
// Typed pub/sub with weak references
eventBus.Subscribe<ThemeChangedEvent>(OnThemeChanged);
eventBus.Publish(new ThemeChangedEvent("Dark"));

// Subscriber isolation — one subscriber's exception doesn't block others
```

### Module Registry
```csharp
// Self-registering modules
public class SampleModule : IModule
{
    public string ModuleId => "sample";
    public int Priority => 10;

    public void Initialize(IModuleInitializationContext context)
    {
        context.RegisterPage("main", typeof(MainPage));
        context.RegisterCommand("export", ExportCommand);
    }
}
```

### Primitive Token System (Theme/Base/Colors.xaml)
```xaml
<!-- Primitive tokens -->
<Color x:Key="Primitive.Blue.500">#3A86FF</Color>
<Color x:Key="Primitive.Slate.900">#0E1720</Color>
<Color x:Key="Primitive.White.95">#F2FFFFFF</Color>

<!-- Semantic tokens (reference primitives) -->
<SolidColorBrush x:Key="Brush.Background.Primary" Color="{StaticResource Primitive.Slate.900}"/>
<SolidColorBrush x:Key="Brush.Text.Primary" Color="{StaticResource Primitive.White.95}"/>
<SolidColorBrush x:Key="Brush.Accent.Primary" Color="{StaticResource Primitive.Blue.500}"/>

<!-- Component tokens -->
<Thickness x:Key="Component.Button.Padding" Value="{DynamicResource ButtonPadding}"/>
<CornerRadius x:Key="Component.Button.Radius" Value="{DynamicResource MediumRadius}"/>
```

### Resource Loading Order (ThemeResources.xaml)
```text
1. Base/Colors.xaml (Primitives + Semantics)
2. Base/Typography.xaml
3. Base/Spacing.xaml
4. Base/Radius.xaml
5. Base/Elevation.xaml
6. Base/Motion.xaml
7. Base/ZIndex.xaml
8. Base/Opacity.xaml
9. Base/ComponentTokens.xaml
10. Effects/Shadows.xaml
11. Effects/Glow.xaml
12. Effects/Animations.xaml
13. Controls/*.xaml (22 files)
14. ModernWindow.xaml
15. ShellStyles.xaml
16. WindowAnimations.xaml
17. Theme Overrides (Dark/Light/Custom — swapped at runtime)
```

---

## 8. Usage Guide

### Basic Workflow

#### 1. Initialize Services (via DI container)
```csharp
var dbService = App.Container.Resolve<IPrimaveraDbService>();
var loaderService = App.Container.Resolve<IPrimaveraDataLoaderService>();
var comparisonService = App.Container.Resolve<IPrimaveraComparisonService>();
```

#### 2. Test Database Connection
```csharp
var connectionString = "Server=localhost;Database=primavera_p6;User Id=sa;Password=YourPassword;";
bool isConnected = await dbService.TestConnectionAsync(connectionString, "SqlServer");
```

#### 3. Load Project Data with Progress
```csharp
var progress = new Progress<int>(percent => progressBar.Value = percent);
var projectData = await loaderService.LoadProjectDataAsync(
    connString, "SqlServer", projectId, progress);
```

#### 4. Compare Projects
```csharp
var result = await comparisonService.CompareProjectsAsync(project1, project2);
Console.WriteLine(result.Summary);
```

### Launching from Excel Ribbon

```csharp
// In Ribbon1.cs — button handlers use NavigationService with namespaced keys
private void BtnPrimaveraCompare_Click(object sender, RibbonControlEventArgs e)
{
    NavigationService.Instance.NavigateTo("planning.primavera.compare");
}

// Or via NavigationService
private void BtnXerEditor_Click(object sender, RibbonControlEventArgs e)
{
    NavigationService.Instance.NavigateTo("planning.xereditor");
}
```

### Using Toast Notifications
```csharp
ToastService.Success("Data saved successfully");
ToastService.Warning("Some items could not be processed");
ToastService.Error("Failed to connect to database");
```

### Using Theme System
```csharp
// Switch themes
ThemeManager.Instance.ApplyTheme("Dark");
ThemeManager.Instance.ApplyTheme("Light");
ThemeManager.Instance.ApplyTheme("Custom", "#FF5722"); // Orange accent

// Change accent color on current theme
ThemeManager.Instance.ApplyAccentColor("#3A86FF");

// Save/Load preferences (automatic via Settings.settings)
ThemeManager.Instance.SaveCurrentTheme();
```

### Using Shell Navigation
```csharp
// Navigate to a page
NavigationService.Instance.NavigateTo("settings");

// Navigate with parameters
NavigationService.Instance.NavigateTo("primavera-compare", new { projectId = 123 });

// Go back
NavigationService.Instance.GoBack();

// Register a page
NavigationService.Instance.RegisterPage("settings", typeof(SettingsPage));
```

---

## 9. API Reference

### Primavera Services (Som3a.Shared)

| Service | Key Methods |
|---------|-------------|
| **IPrimaveraDbService** | `TestConnectionAsync`, `GetProjectsAsync`, `GetActivitiesAsync`, `GetRelationshipsAsync`, `GetResourcesAsync`, `GetProjectByIdAsync` |
| **IPrimaveraDataLoaderService** | `LoadProjectDataAsync`, `LoadMultipleProjectsAsync`, `CancelLoading` |
| **IPrimaveraComparisonService** | `CompareProjectsAsync`, `CompareActivitiesAsync`, `CompareRelationshipsAsync`, `CompareResourcesAsync` |

### WPF Services (WpfApp2)

| Service | Key Methods |
|---------|-------------|
| **ThemeManager** | `ApplyTheme(name, accentColor)`, `LoadThemeFromSettings()`, `SaveCurrentTheme()`, `ApplyAccentColor(hex)` |
| **NavigationService** | `NavigateTo(pageKey)`, `GoBack()`, `RegisterPage(key, type)` |
| **ToastService** | `Show(message)`, `Success(msg)`, `Warning(msg)`, `Error(msg)` |
| **ServiceContainer** | `RegisterSingleton<T>`, `RegisterTransient<T>`, `RegisterScoped<T>`, `Resolve<T>()`, `CreateScope()` |
| **EventBus** | `Publish<TEvent>`, `Subscribe<TEvent>(action)`, `Subscribe<TEvent>(action, filter)` |
| **DiagnosticsService** | `GetSnapshot()`, `GetRenderMode()`, `GetMemoryUsage()` |
| **LoggingService** | `LogInfo()`, `LogWarning()`, `LogError()`, `Flush()` |
| **ValidationEngine** | `ValidateTokens()`, `ScanForInlineColors()` |
| **WindowRenderModeDetector** | `Detect()`, `GetSafeModeRequired()` |
| **PluginLoader** | `DiscoverModules()`, `LoadModule()`, `GetLoadedModules()` |

---

## 10. Troubleshooting

### Common Issues & Solutions

#### Issue 1: "Connection Refused"
Verify Primavera database is running, check connection string format, check firewall.

#### Issue 2: "No Projects Found"
Verify SELECT permission on MSP_PROJECTS, check database contains projects.

#### Issue 3: "Timeout During Load"
Increase CommandTimeout in app.config, check network performance.

#### Issue 4: "Out of Memory"
Load projects sequentially, process in chunks, reduce comparison columns.

#### Issue 5: "WPF Binding Errors"
Verify ViewModel inherits from INotifyPropertyChanged/ViewModelBase, check DataContext, use Output window for details.

#### Issue 6: "Build Errors"
Clean + rebuild, restore NuGet packages, verify .NET Framework 4.8 is installed.

#### Issue 7: "WebView2 Runtime Missing"
Install WebView2 Runtime from Microsoft, or add NuGet package.

#### Issue 8: "Excel COM Interop Errors"
Ensure proper COM release via `ComRelease.SafeRelease()`, check Excel is responsive.

#### Issue 9: "Theme Resources Not Found"
Ensure App.xaml includes Theme resource dictionaries, check build includes Theme/*.xaml, clean + rebuild WpfApp2.

#### Issue 10: "XAML Tag Mismatch Errors"
Check closing tags match opening tags (e.g., `</controls:ModernWindow>` not `</Window>`), ensure Grid nesting is correct.

#### Issue 11: "Missing Roslyn Dependencies"
ModernWindowAnalyzer.cs is excluded from build — ensure it's removed from .csproj Compile includes.

#### Issue 12: "StringComparison.Contains Not Available in .NET 4.8"
Use `IndexOf(string, StringComparison)` >= 0 instead of `Contains(string, StringComparison)`.

#### Issue 13: "WindowChrome.UseWindowChrome Not Available"
Use attached property syntax: `window.SetValue(WindowChrome.WindowChromeProperty, chrome)`.

#### Issue 14: "WindowAttribute Not Found"
Remove any reference to non-existent WPF WindowAttribute — use reflection on Window properties instead.

#### Issue 15: "Clone() Method Not Found"
Window doesn't have Clone() — use `this` reference directly.

#### Issue 16: "Exception in RaisePropertyChanged"
Ensure property names match, use `nameof()` for compile-time safety, check `!= null` or `?.Invoke()`.

---

## 11. Development Workflow

### Start of Day
```powershell
cd "C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026"
git pull origin main
# Open in Visual Studio
Start-Process "Som3a Addin 2026.slnx"
```

### Code Review Checklist
- [ ] Code compiles with zero errors AND zero warnings
- [ ] All public methods have XML documentation
- [ ] All error paths are handled
- [ ] Async operations use await properly
- [ ] No hardcoded connection strings
- [ ] No inline colors/shadows/margins outside Theme/Base/
- [ ] DynamicResource used for themeable properties
- [ ] MVVM — no business logic in code-behind
- [ ] Events unsubscribed in cleanup/dispose
- [ ] No duplicate resource dictionaries
- [ ] Unit tests pass
- [ ] CodeRabbit review green

### Commit Flow
```
Edit → Build → Test → Commit → Push → Create PR → CodeRabbit Review → Merge to develop
```

---

## 12. Testing Strategy

### Unit Tests (Tests/ Project)

**Test files**:
- `ServiceContainerTests.cs` — DI container lifecycle, circular detection, scoped resolution
- `EventBusTests.cs` — Typed pub/sub, subscriber isolation, weak references
- `ViewModelBaseTests.cs` — INotifyPropertyChanged, SetProperty<T>

**Run tests**:
```powershell
cd Tests
dotnet test Som3a_WPF_UI.Tests.csproj
# or
vstest.console.exe Som3a_WPF_UI.Tests.csproj
```

### Manual Testing (Excel VSTO Host)

1. Open each window from Excel Ribbon — verify no black rendering
2. Verify ComboBox popup renders correctly with shadow
3. Test DPI scaling at 100%, 125%, 150%, 200%
4. Test theme switching (10x rapid) — no crash, all windows update
5. Test DataGrid scrolling with 1000+ rows
6. Test safe mode activation on problematic systems
7. Test window move, minimize, close without Excel freezing
8. Test keyboard navigation (Tab, Enter, Escape) on all controls

---

## 13. Deployment Guide

### Build Release
```powershell
msbuild "Som3a Addin 2026.slnx" /p:Configuration=Release
```

### Deploy to Excel
```
Excel: File → Options → Trust Center → Trust Center Settings → Trusted Add-in Catalogs
Add network location of deployed add-in

Excel: File → Options → Add-ins → Manage: COM Add-ins → Browse
Select deployed .vsto file → OK
```

### Post-Deployment Verification
- [ ] Ribbon buttons appear in Excel
- [ ] All 14 windows/Pages open
- [ ] Database connections work
- [ ] Theme switching works
- [ ] Navigation works
- [ ] Settings persist across restarts

---

## 14. Future Roadmap

### Short Term (Current — Phase 10-11)
- [x] Close 15 Phase 3 validation tasks
- [x] All 6 enterprise polish audit reports
- [x] ToastWindow migration to ModernWindow
- [x] Hardcoded value elimination
- [x] All 14 windows migrate to Pages
- [ ] Final production validation

### Medium Term (3-6 Months)
- [ ] Oracle database full support
- [ ] Advanced filtering & sorting
- [ ] Report generation engine
- [ ] Performance optimization for large datasets

### Long Term (6-12 Months)
- [ ] REST API for remote access
- [ ] Web-based UI companion
- [ ] Real-time synchronization
- [ ] Machine learning insights
- [ ] Multi-language localization

---

## 15. Specification & Governance

### Constitution
- **Active**: `.specify/memory/constitution.md` v1.2.0 (16 principles, ratified 2026-05-21)
- **Next-gen**: `.specify/memory/constitution-v2.md` v2.0.0 (includes AI execution rules, branch naming, token standards, review gate requirements, Phase 11 scope)

### Master Implementation Plan
- `implementation_plan.md` (1948 lines) — Phases 0-11 with full task breakdown
- Status: Implementation complete across Phases 0-9 (one branch per phase via develop merges)
- Phase 10 (Enterprise Polish) + Phase 11 (Legacy Window Migration): Validation tasks in progress

### CodeRabbit Review Gates
Every phase requires:
- Build passes
- Excel host test passes
- PR approved
- CodeRabbit clean
- Architecture review passed
- No rendering, popup, or theme regressions

### AI Agent Rules (from AGENTS.md + Docs/Architecture/AGENT_RULES.md)
- **BEFORE** creating any new token/control/dictionary: inspect current implementation
- **NEVER**: inline colors, inline shadows, inline margins, inline font sizes, duplicate styles, business logic in code-behind, direct service creation in views
- **ALWAYS**: DynamicResource, semantic tokens, MVVM separation, centralized effects, Excel-safe rendering, accessibility-ready controls, review before merge

---

## Appendix

### A. Key Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+B` | Build Solution |
| `Ctrl+R, A` | Run All Tests |
| `F5` | Start Debugging |
| `Shift+F5` | Stop Debugging |
| `Ctrl+Alt+O` | Debug Output Window |
| `Ctrl+K` | Command Palette (in Shell) |

### B. File Naming Conventions
```
Services:        {Feature}Service.cs
Interfaces:      I{Feature}Service.cs
ViewModels:      {Feature}ViewModel.cs
Windows:         {Feature}Window.xaml / {Feature}Window.xaml.cs
Pages:           {Feature}Page.xaml / {Feature}Page.xaml.cs
Controls:        {Feature}Control.xaml / {Feature}Control.xaml.cs
Models/DTOs:     {Name}Dto.cs
Test Files:      {Feature}Tests.cs
Theme Tokens:    Primitive.{Color}.{Shade}, Brush.{Category}.{State}
```

### C. Common Commands
```powershell
# Build WPF project
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Build full solution
msbuild "Som3a Addin 2026.slnx" /p:Configuration=Debug

# Clean
msbuild "Som3a Addin 2026.slnx" /t:Clean

# Rebuild
msbuild "Som3a Addin 2026.slnx" /t:Rebuild /p:Configuration=Release

# Run tests
cd Tests; dotnet test Som3a_WPF_UI.Tests.csproj
```

### D. Token Layers Reference
```text
Primitive Tokens → Semantic Tokens → Component Tokens → Control Templates
```

### E. Branch Naming Convention
```
feature/phase-NN-short-description
Examples:
  feature/phase-00-governance-foundation
  feature/phase-11-legacy-migration
```

---

**Document Version**: 3.0
**Last Updated**: May 25, 2026
**Author**: Development Team
**Status**: Complete — All Phases 0-11 Documented
