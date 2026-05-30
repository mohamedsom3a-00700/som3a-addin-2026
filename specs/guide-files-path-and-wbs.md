# Som3a Add-in 2026 — Full Files Path Guide & WBS for ChatGPT

**Purpose**: This document is a complete reference of every file path in the project, organized in multiple ways (by project, by function, by feature), plus a comprehensive Work Breakdown Structure (WBS). Give this to ChatGPT as system context so it can understand the full codebase.

**Last Updated**: May 30, 2026
**Solution Status**: Phases 0-9 complete, Phases 10-11 in validation, Phases 14-27 in planning (specs finalized), Post-Phase 26 Fluent UI Migration planned

---

## Part 1: Complete Solution File Tree (Every File, Hierarchical)

### 1.1. Solution Root

```text
Som3a Addin 2026.slnx                        # Solution file (Visual Studio, 3 projects)
README.md                                      # Project overview (80 lines)
AGENTS.md                                      # AI agent context instructions (~170 lines)
COMPLETE_SOLUTION_GUIDE.md                     # Full solution guide (~1700 lines)
implementation_plan.md                         # Enterprise UI Transformation Master Plan (1948 lines)
enterprise_planning_platform_plan.md           # Enterprise Planning Platform Phases 14-27 (1760 lines)
extraction_1779466152081.md                    # Data extraction notes
.gitattributes                                 # Git LFS/attributes config
.gitignore                                     # Git ignore rules
```

### 1.2. VSTO Add-in: `Som3a Addin 2026/`

```text
Som3a Addin 2026/
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs
│   └── Settings.settings
├── Resources/                              # Icons & images for ribbon/UI
│   ├── btnWorkspace.png
│   ├── button1.png / button2.png / button22.png
│   ├── btnUnmergeFillDown.png
│   ├── btnProjectAnalysis.png
│   ├── btnSafeClone.png
│   ├── btnDailyReport.png
│   ├── btnLinksManager.png
│   ├── Xer_Editor.png
│   ├── Float_Path.png
│   ├── addin_setting.png
│   ├── color_setting.jpg
│   ├── coloringwbs.png
│   ├── comparision.jpg
│   ├── Com_Xer.png
│   ├── ExportPaletteHex.png
│   ├── GroupWbs.png
│   ├── 325-3255215_refresh-clipart-reload-icon-png.png
│   └── updates.png
├── Ui/
│   ├── LinksManagerLauncher.cs              # Launch links manager from Excel
│   └── WpfDialogHost.cs                     # Host WPF dialogs in Excel
├── AddInAutomation.cs                        # IAddInAutomation interface
├── IAddInAutomation.cs                       # Add-in automation contract
├── app.config                                # Connection strings & app settings
├── ExcelWindowHandle.cs                     # Get Excel window handle for Win32 interop
├── Oracle.DataAccess.Common.Configuration.Section.xsd
├── Oracle.ManagedDataAccess.Client.Configuration.Section.xsd
├── packages.config                           # NuGet packages (legacy)
├── Ribbon1.cs                                # Excel ribbon — 14 buttons + dynamic modules group
├── Ribbon1.Designer.cs                       # Ribbon designer code
├── Ribbon1.resx                              # Ribbon resources
├── Som3a Addin 2026.csproj                  # Project file (.NET Framework 4.8, VSTO)
├── Som3a Addin 2026.csproj.user
├── Som3a Addin 2026_TemporaryKey.pfx         # ClickOnce signing key
├── Som3aAddinBridge.cs                      # Bridge between Excel and WPF
├── ThisAddIn.cs                              # VSTO add-in entry point (creates WPF App, registers DI)
├── ThisAddIn.Designer.cs
├── ThisAddIn.Designer.xml
└── WpfDialogHost.cs
```

### 1.3. Shared Library: `Som3a.Shared/`

```text
Som3a.Shared/
├── Controllers/
│   └── CompareController.cs                 # Orchestrates comparison flow
├── Core/                                     # Business logic services
│   ├── AssignTradeCodesService.cs            # Assign trade codes to Excel activities
│   ├── ExcelCompareService.cs                # Compare Excel worksheets
│   ├── ExcelLinkTextHelper.cs               # Link text manipulation helpers
│   ├── FixPieColorsResult.cs                # Pie chart fix result model
│   ├── FixPieColorsService.cs               # Fix Excel pie chart segment colors
│   ├── FloatPathService.cs                  # Calculate float paths in schedules
│   ├── GraphBuilder.cs                      # Build graph from activity relationships
│   ├── GraphService.cs                      # Graph traversal & analysis
│   ├── LinksManagerService.cs               # Manage workbook external links
│   ├── PathFinder.cs                         # Find critical path in schedule network
│   ├── SubDlyReportService.cs               # Generate subcontractor daily reports
│   ├── UnmergeFillDownService.cs            # Unmerge cells & fill down
│   ├── WbsBuilder.cs                         # Build WBS hierarchy from data
│   ├── WbsColoringService.cs                 # Color-code WBS levels in Excel
│   ├── WbsStyleFactory.cs                   # Create WBS cell styles
│   ├── WorkbookCloneService.cs              # Clone Excel workbooks
│   ├── XerExportService.cs                  # Export to XER format
│   ├── XER Parser.cs                        # Parse XER files
│   └── Primavera/
│       ├── ComparisonModels.cs              # Diff models (ActivityDiff, RelationshipDiff, ResourceDiff)
│       ├── IPrimaveraComparisonService.cs    # Comparison service contract
│       ├── IPrimaveraDataLoaderService.cs    # Data loader contract
│       ├── IPrimaveraDbService.cs            # Database service contract
│       ├── PrimaveraComparisonService.cs     # Compare two P6 projects
│       ├── PrimaveraDataLoaderService.cs     # Load P6 project data
│       ├── PrimaveraDbService.cs             # Database access (SQL Server, Oracle-ready)
│       └── ProjectFullData.cs               # Container for all project data
├── Interop/
│   └── ComRelease.cs                         # Safe COM object cleanup
├── Models/
│   ├── AssignTradeCodesViewModel.cs          # Trade codes VM
│   ├── CompareResultItem.cs                  # Comparison result item
│   ├── ExcelColorHelper.cs                  # Color conversion helpers
│   ├── ExcelService.cs                      # Excel automation helpers
│   ├── Float_path.cs                         # Float path model
│   ├── LinkItem.cs                           # Link item model
│   ├── LinkTypeItem.cs                       # Link type model
│   ├── MainViewModel.cs                     # Legacy main VM
│   ├── ProjectAnalysisLogRow.cs             # Project analysis log
│   ├── ProjectAnalysisSummaryRow.cs          # Project analysis summary
│   ├── RelayCommand.cs
│   ├── SheetCheckRow.cs                     # Sheet validation row
│   ├── TableItemVM.cs                       # Generic table item VM
│   ├── WbsItem.cs                            # WBS item model
│   ├── WbsLevelStyle.cs                      # WBS level style definition
│   ├── WorkbookItem.cs                      # Workbook item model
│   ├── XerMapper.cs                          # XER field mapping
│   └── Primavera/
│       ├── ActivityDto.cs                    # P6 activity DTO
│       ├── ProjectDto.cs                    # P6 project DTO
│       ├── RelationshipDto.cs               # P6 relationship DTO
│       └── ResourceDto.cs                   # P6 resource DTO
├── Properties/
│   └── AssemblyInfo.cs
├── Utils/
│   └── RelUtils.cs                           # Relationship utility functions
├── Class1.cs
├── packages.config
└── Som3a.Shared.csproj
```

### 1.4. Main WPF UI: `WpfApp2/`

```text
WpfApp2/
├── Analyzers/
│   ├── ModernWindowAnalyzer.cs               # Roslyn analyzer (excluded from build)
│   └── README.md
├── Behaviors/
│   ├── DragMoveBehavior.cs                   # Drag window on mouse move
│   └── EscapeCloseBehavior.cs                # Close window on Escape
├── Contracts/                                # Plugin & module interfaces
│   ├── ICommandRegistrar.cs
│   ├── IModule.cs
│   ├── IModuleInitializationContext.cs
│   ├── IModuleRegistry.cs
│   ├── INavigationRegistrar.cs
│   ├── IPluginLoader.cs
│   ├── IRibbonRegistrar.cs
│   ├── ModuleInfo.cs
│   └── ModuleManifest.cs
├── Controls/
│   ├── LoadingOverlay.xaml                    # Loading spinner overlay
│   ├── LoadingOverlay.xaml.cs
│   ├── ModuleLoadingOverlay.xaml              # Module lazy-load overlay
│   ├── ModuleLoadingOverlay.xaml.cs
│   ├── ModernWindow.cs                       # Base custom chrome window (VSTO-safe, DPI-aware)
│   ├── Shell/
│   │   ├── CommandPalette.xaml                # Ctrl+K searchable command palette
│   │   ├── CommandPalette.xaml.cs
│   │   ├── NavigationDestination.cs           # Navigation target definition
│   │   ├── NavigationEventArgs.cs             # Navigation event args
│   │   ├── NavigationPage.cs                  # Page base class
│   │   ├── ShellState.cs                     # Shell navigation state
│   │   ├── ShellWindow.xaml                   # Main Shell container (Sidebar + Workspace)
│   │   ├── ShellWindow.xaml.cs
│   │   ├── SidebarControl.xaml               # Icon + label navigation sidebar
│   │   ├── SidebarControl.xaml.cs
│   │   └── WorkspaceHost.cs                   # Frame-based Page host
│   └── Toast/
│       ├── ToastModel.cs                     # Toast notification model
│       ├── ToastWindow.xaml                  # Toast notification popup
│       └── ToastWindow.xaml.cs
├── Converters/
│   ├── DifferenceTypeToColorConverter.cs     # Diff type → color (green/red/yellow)
│   ├── SharedConverters.cs                   # Bool→Visibility, Null→Visibility, etc.
│   └── WindowConverters.cs                   # Window state converters
├── Helpers/
│   ├── AsyncRelayCommand.cs                  # Async command with cancellation
│   ├── DpiHelper.cs                          # Monitor DPI detection & scaling
│   ├── ExcelOwnerHelper.cs                   # Set WPF owner from Excel hwnd
│   ├── NotifyBase.cs                         # INotifyPropertyChanged base
│   ├── RelayCommand.cs                       # ICommand implementation
│   ├── WindowBehaviorHelper.cs               # Window behavior reflection helper
│   ├── WindowChromeHelper.cs                 # Apply WindowChrome
│   └── WindowValidationHelper.cs             # Validate window properties
├── Models/
│   ├── DiagnosticsModels.cs                  # Diagnostic snapshot models
│   ├── RelayCommand.cs
│   ├── SettingsCategory.cs                   # Settings category model
│   ├── SettingsExport.cs                     # Settings export model
│   ├── SubDailyReportViewModel.cs            # (mislocated VM in Models)
│   ├── UnmergeFillDownViewModel.cs            # (mislocated VM in Models)
│   ├── UserSettings.cs                       # User settings POCO
│   └── XerEditorVM.cs                        # (mislocated VM in Models)
├── Pages/                                    # Shell-hosted pages (Phase 11 migration)
│   ├── PageBase.cs                           # Base Page with lifecycle hooks (Title, CanNavigateAway, etc.)
│   ├── AssignTradeCodesPage.xaml
│   ├── AssignTradeCodesPage.xaml.cs
│   ├── FixPieColorsPage.xaml
│   ├── FixPieColorsPage.xaml.cs
│   ├── FloatPathPage.xaml
│   ├── FloatPathPage.xaml.cs
│   ├── LinksManagerPage.xaml
│   ├── LinksManagerPage.xaml.cs
│   ├── MainPage.xaml                         # Migrated from MainWindow
│   ├── MainPage.xaml.cs
│   ├── PrimaveraComparePage.xaml             # Migrated from PrimaveraCompareWindow
│   ├── PrimaveraComparePage.xaml.cs
│   ├── PrimaveraResultsPage.xaml            # Migrated from PrimaveraResultsWindow
│   ├── PrimaveraResultsPage.xaml.cs
│   ├── ProjectAnalysisPage.xaml             # Migrated from ProjectAnalysisWindow
│   ├── ProjectAnalysisPage.xaml.cs
│   ├── SettingsPage.xaml                     # Migrated from SettingsWindow
│   ├── SettingsPage.xaml.cs
│   ├── StyleSelectorPage.xaml
│   ├── StyleSelectorPage.xaml.cs
│   ├── SubDailyReportPage.xaml
│   ├── SubDailyReportPage.xaml.cs
│   ├── UnmergeFillDownPage.xaml
│   ├── UnmergeFillDownPage.xaml.cs
│   ├── WelcomePage.xaml                      # Shell welcome page
│   ├── WelcomePage.xaml.cs
│   ├── XerEditorPage.xaml
│   └── XerEditorPage.xaml.cs
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs                  # Theme persistence accessors
│   └── Settings.settings                     # User-scoped theme + accent settings
├── Services/
│   ├── CommandRegistrar.cs                   # Register commands for palette
│   ├── DiagnosticsService.cs                 # Render/theme/memory diagnostics
│   ├── DialogService.cs                      # Show MessageBox dialogs from VMs
│   ├── EventBus.cs                           # Typed event pub/sub (weak references)
│   ├── ExcelProjectAnalysisService.cs
│   ├── LazyModuleCommand.cs                  # Deferred module command
│   ├── LoggingService.cs                     # File logging (5MB rollover, 3-file rotation)
│   ├── ModuleDiagnosticsService.cs           # Plugin stability diagnostics
│   ├── ModuleInitializationContext.cs        # Per-module init context
│   ├── ModuleLoadOrchestrator.cs             # Module lifecycle management
│   ├── ModuleRegistry.cs                     # Priority-based module initialization
│   ├── ModuleRibbonActionsAvailableEvent.cs
│   ├── NavigationRegistrar.cs                # Register pages with nav service
│   ├── NavigationService.cs                 # Shell navigation singleton
│   ├── PluginLoader.cs                       # Discover + load plugin assemblies
│   ├── PluginRegistry.cs                     # Plugin metadata registry
│   ├── RenderModeService.cs                  # Render mode cache + events
│   ├── RibbonRegistrar.cs                    # Register ribbon actions
│   ├── ServiceContainer.cs                   # DI container (Singleton/Transient/Scoped)
│   ├── SettingsPersistenceService.cs         # Settings serialization
│   ├── ShellNavigationHelper.cs              # Shell nav utilities
│   ├── ThemeManager.cs                       # Singleton runtime theme switcher
│   ├── ThemeSettings.cs                      # Legacy JSON theme persistence
│   ├── ToastService.cs                       # Show toast notifications
│   ├── ValidationEngine.cs                   # Token integrity + hardcoded color detection
│   └── WindowRenderModeDetector.cs           # Auto-detect Excel VSTO rendering mode
├── Theme/
│   ├── ModernWindow.xaml                     # ModernWindow control template
│   ├── ShellStyles.xaml                      # Shell-specific styles
│   ├── ThemeResources.xaml                   # Aggregator (documented loading order)
│   ├── WindowAnimations.xaml                 # Window open/close fade animations
│   ├── Base/
│   │   ├── Colors.xaml                       # Primitive + semantic color tokens
│   │   ├── ComponentTokens.xaml              # Component-specific tokens
│   │   ├── Elevation.xaml                    # Elevation level tokens (Card, Popup, Window)
│   │   ├── Motion.xaml                       # Animation duration & easing tokens
│   │   ├── Opacity.xaml                      # Standard opacity values (Disabled, Hover, Pressed)
│   │   ├── Radius.xaml                       # Corner radius tokens
│   │   ├── Spacing.xaml                      # Margin/padding/height tokens
│   │   ├── Typography.xaml                   # Font size/weight tokens
│   │   └── ZIndex.xaml                       # Z-index layering constants
│   ├── Controls/                             # 22 control style XAML files
│   │   ├── AccentSwatchStyles.xaml
│   │   ├── ButtonStyles.xaml
│   │   ├── CheckBoxStyles.xaml
│   │   ├── ComboBoxItemStyles.xaml
│   │   ├── ComboBoxStyles.xaml
│   │   ├── DataGridStyles.xaml
│   │   ├── GroupBoxStyles.xaml
│   │   ├── LabelStyles.xaml
│   │   ├── ListViewItemStyles.xaml
│   │   ├── ListViewStyles.xaml
│   │   ├── PasswordBoxStyles.xaml
│   │   ├── ProgressBarStyles.xaml
│   │   ├── RadioButtonStyles.xaml
│   │   ├── ScrollBarStyles.xaml
│   │   ├── ScrollViewerStyles.xaml
│   │   ├── SettingsPanelStyles.xaml
│   │   ├── TextBoxStyles.xaml
│   │   ├── ThemeCardStyles.xaml
│   │   ├── ToggleButtonStyles.xaml
│   │   ├── TreeViewStyles.xaml
│   │   ├── WindowButtonStyles.xaml
│   │   └── WindowStyles.xaml
│   ├── Custom/
│   │   ├── CustomColors.xaml                 # Custom theme + 8 accent presets
│   │   └── CustomTheme.xaml                  # Custom merged dictionary
│   ├── Dark/
│   │   ├── DarkColors.xaml                   # Dark semantic token overrides
│   │   └── DarkTheme.xaml                    # Dark merged dictionary
│   ├── Effects/
│   │   ├── Animations.xaml                   # Storyboards (hover, focus, popup — all ≤200ms)
│   │   ├── Glow.xaml                         # Glow effects (focus, selection, accent, 6 variants)
│   │   └── Shadows.xaml                      # Centralized DropShadowEffect (7 + safe variants)
│   ├── Fluent/
│   │   ├── FluentEffects.xaml                # Legacy Fluent effects (excluded from build)
│   │   └── FluentWhite.xaml                  # Legacy light theme (excluded from build)
│   └── Light/
│       ├── LightColors.xaml                   # Light semantic token overrides
│       └── LightTheme.xaml                    # Light merged dictionary
├── UI/
│   ├── ProjectAnalysisWindow.xaml            # Project analysis window
│   └── ProjectAnalysisWindow.xaml.cs
├── UIHost/
│   └── WpfWindowManager.cs                   # Manage WPF windows in Excel host
├── ViewModels/
│   ├── CommandPaletteViewModel.cs             # Searchable command palette VM
│   ├── DiagnosticsViewModel.cs               # Diagnostics panel VM
│   ├── FixPieColorsViewModel.cs
│   ├── FloatPathViewModel.cs                 # Float path analysis VM
│   ├── LinksManagerViewModel.cs              # Links manager VM
│   ├── PluginDiagnosticsViewModel.cs         # Plugin diagnostics VM
│   ├── ProjectAnalysisViewModel.cs           # Project analysis VM
│   ├── SettingsViewModel.cs                  # Settings (Appearance, Accessibility, etc.) VM
│   ├── SubDailyReportViewModel.cs
│   ├── ToastViewModel.cs
│   ├── UnmergeFillDownViewModel.cs
│   ├── ViewModelBase.cs                      # INotifyPropertyChanged base with SetProperty<T>
│   ├── WbsStyleSelectorViewModel.cs
│   ├── XerEditorViewModel.cs
│   └── Primavera/
│       ├── PrimaveraCompareViewModel.cs      # Primavera comparison VM
│       └── PrimaveraResultsViewModel.cs      # Comparison results VM
├── Views/
│   ├── AccessibilityPanel.xaml               # Accessibility settings tab
│   ├── AccessibilityPanel.xaml.cs
│   ├── AppearancePanel.xaml                  # Appearance settings tab
│   ├── AppearancePanel.xaml.cs
│   ├── DiagnosticsPanel.xaml                 # Diagnostics settings tab
│   ├── DiagnosticsPanel.xaml.cs
│   ├── ExcelPanel.xaml                       # Excel settings tab
│   ├── ExcelPanel.xaml.cs
│   ├── PerformancePanel.xaml                 # Performance settings tab
│   ├── PerformancePanel.xaml.cs
│   ├── PluginsPanel.xaml                     # Plugin settings tab
│   ├── PluginsPanel.xaml.cs
│   ├── SettingsWindow.xaml                   # Theme + accent settings (ModernWindow)
│   └── SettingsWindow.xaml.cs
├── Windows/
│   └── PrimaveraComparison/
│       ├── PrimaveraCompareWindow.xaml       # Compare configuration window
│       ├── PrimaveraCompareWindow.xaml.cs
│       ├── PrimaveraResultsWindow.xaml       # Comparison results display
│       └── PrimaveraResultsWindow.xaml.cs
├── App.config
├── App.xaml                                   # Application resources (ThemeResources + DarkTheme)
├── App.xaml.cs
├── AssignTradeCodesWindow.xaml                # Trade codes assignment UI
├── AssignTradeCodesWindow.xaml.cs
├── CompositionRoot.cs                        # Centralized DI registration (76 lines)
├── Fixpiecolors.xaml                          # Pie chart color fix window
├── Fixpiecolors.xaml.cs
├── Float_path.xaml                            # Float path analysis window
├── Float_path.xaml.cs
├── LinksManagerWindow.xaml                    # Links manager window
├── LinksManagerWindow.xaml.cs
├── MainWindow.xaml                            # Main comparison window
├── MainWindow.xaml.cs
├── packages.config
├── scripts/
│   └── Validate-Tokens.ps1                   # PowerShell token validation script
├── Som3a_WPF_UI.csproj                       # Project file (.NET Framework 4.8)
├── Som3a_WPF_UI.csproj.user
├── StyleSelectorWindow.xaml                   # Style selector window
├── StyleSelectorWindow.xaml.cs
├── SubDailyReportWindow.xaml                  # Subcontractor daily report
├── SubDailyReportWindow.xaml.cs
├── UnmergeFillDownWindow.xaml                 # Unmerge & fill down window
├── UnmergeFillDownWindow.xaml.cs
├── XerEditorWindow.xaml                       # XER file editor window
└── XerEditorWindow.xaml.cs
```

### 1.5. Unit Tests: `Tests/`

```text
Tests/
├── Som3a_WPF_UI.Tests.csproj                  # Test project (.NET Framework 4.8, MSTest)
├── ServiceContainerTests.cs                   # DI container lifecycle, circular detection, scopes
├── EventBusTests.cs                           # Typed pub/sub, subscriber isolation, weak references
├── ViewModelBaseTests.cs                      # INotifyPropertyChanged, SetProperty<T>
├── Run-VSTOTests.ps1                         # VSTO test execution script
└── VSTOResults.xml                            # VSTO test results
```

### 1.6. Sample Module: `WpfApp2.Modules.Sample/`

```text
WpfApp2.Modules.Sample/
├── WpfApp2.Modules.Sample.csproj              # Plugin module project (.NET Framework 4.8)
├── SampleModule.cs                            # IModule implementation example
└── module.json                                # Module manifest descriptor
```

### 1.7. Legacy/Ancillary Projects

```text
Som3a_WPF_UI/                                 # Standalone .NET 10.0 WPF App (experimental)
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Som3a_WPF_UI.csproj
└── Som3a_WPF_UI.csproj.user

Som3a_WPF_UId/                                # Legacy .NET 4.7.2 WPF App
├── Properties/AssemblyInfo.cs
├── Class1.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── Som3a_WPF_UI.csproj

WpfApp1/                                      # Old legacy .NET 4.7.2 WPF App
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs
│   └── Settings.settings
├── App.config
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── Som3a_WPF_UI.csproj
```

### 1.8. Specification & Documentation Files

```text
specs/
├── feature-planning-guide.md                          # ChatGPT feature planning guide (466 lines)
├── guide-files-path-and-wbs.md                        # THIS FILE — comprehensive file reference + WBS (1626 lines)
├── future-plan-fluent-ui-migration.md                 # Post-Phase 26 Fluent UI migration plan (377 lines)
│
├── [PHASE 0] 001-github-governance-workflow/          # Git workflow + review gates + governance
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (merge-gate-contract.md, review-checklist-contract.md)
│
├── [PHASE 1] 004-design-system-core/                  # Primitive/Semantic/Component token architecture
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/token-api.md
│
├── [PHASE 2] 005-rendering-infrastructure/            # Excel-safe rendering + DPI + ModernWindow
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/render-service.md
│
├── [PHASE 3] 001-fluent-theme-engine/                 # Fluent Runtime Theme Engine (Dark/Light/Custom)
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/theme-api.md
├── [PHASE 3] 002-themes-manager/                      # ThemeManager fixes + hardcoded color elimination
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
├── [PHASE 3] 006-phase-3-spec/                        # Theme Engine 2.0 integration spec
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/theme-api.md
│
├── [PHASE 4] 007-control-standardization/             # Control template audit + 22 control styles + popup fix
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
├── [PHASE 5] 008-navigation-shell-platform/           # ShellWindow + Sidebar + NavigationService
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (INavigationService.md, IPageHost.md, ISidebarModel.md)
│
├── [PHASE 6] 009-mvvm-architecture-cleanup/           # DI container + EventBus + ModuleRegistry
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IEventBus.md, IModule.md, IServiceContainer.md)
│
├── [PHASE 7] 001-settings-personalization-ux/         # Settings UI with 6 categorized panels
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── contracts/settings-persistence.md
│
├── [PHASE 8] 001-diagnostics-stability-platform/      # Diagnostics panel + validation + logging
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/service-interfaces.md
│
├── [PHASE 9] 012-plugin-feature-platform/             # Plugin loader + module system + sample
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (ICommandRegistrar.cs, IModule.cs, IModuleInitializationContext.cs,
│                    IModuleRegistry.cs, INavigationRegistrar.cs, IPluginLoader.cs,
│                    IRibbonRegistrar.cs, ModuleInfo.cs, ModuleManifest.cs)
│
├── [PHASE 10] 013-enterprise-polish/                  # 7 workstreams: perf/accessibility/DPI/stability
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
├── [PHASE 11] 011-legacy-window-migration/            # Window → Page migration (14 windows)
│   ├── data-model.md / MIGRATION_PATTERNS.md / plan.md / quickstart.md
│   ├── research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
├── [PHASE 14] 014-platform-foundation/                # .NET 8.0 Domain/Contracts/AI/Export/PluginSDK
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IPlugin.cs, IAIProvider.cs, IExportEngine.cs, ISettingsModule.cs, IPromptProvider.cs, IDiagnosticsProvider.cs)
│
├── [PHASE 15] 015-shell-refactor/                     # Categorized sidebar + dynamic nav + legacy cleanup
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (INavigationItem.cs, ISidebarCategory.cs, ICategoryProvider.cs)
│
├── [PHASE 16] 016-dynamic-settings-platform/          # Plugin registry settings + ISettingsModule
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (ISettingsModule.cs, ISettingsSection.cs, IValidationRule.cs)
│
├── [PHASE 17] 017-theme-expansion/                    # Material Design + background blur + fonts
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
├── [PHASE 18] 018-ai-core-infrastructure/             # 6 AI providers + orchestration + prompt governance
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IAIProviderAdapter.cs, IPromptTemplate.cs, IOutputParser.cs)
│
├── [PHASE 19] 019-wbs-engine/                         # AI-powered WBS generation + templates
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IWBSGenerator.cs, IWBSTemplateProvider.cs)
│
├── [PHASE 20] 020-boq-activity-generator/            # AI BOQ → Activity generation
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IBOQParser.cs, IActivityGenerator.cs)
│
├── [PHASE 21] 001-relationship-generator-plugin/      # AI relationship generation (FS/SS/FF/SF)
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IRelationshipGenerator.cs, IRelationshipValidator.cs)
│
├── [PHASE 22] 021-duration-estimator-plugin/          # Duration estimation from quantities + rates
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IDurationEstimator.cs, IProductivityProvider.cs)
│
├── [PHASE 23] 023-dashboard-home/                     # Home dashboard with 9 live widgets
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
├── [PHASE 24] 024-localization-rtl/                   # Arabic + English RTL localization
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (ILocalizationProvider.cs, ITranslationProvider.cs)
│
├── [PHASE 25] 025-platform-rebranding/                # Som3a → Planova visual identity rebranding
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   ├── checklists/requirements.md
│   └── contracts/ (IBrandingProvider.cs, IThemeVariantProvider.cs)
│
├── [PHASE 26] 026-release-candidate-packaging/        # MSI installer + validation + code signing
│   ├── data-model.md / plan.md / quickstart.md / research.md / spec.md / tasks.md
│   └── checklists/requirements.md
│
└── [PHASE 27] 027-persistence-infrastructure/        # SQLite persistence + repository pattern + migrations + backup
    ├── plan.md                                        # Implementation plan (108 lines)
    ├── spec.md                                        # Feature specification (181 lines, 5 user stories)
    ├── research.md                                    # Technology research (90 lines, 11 decisions)
    ├── data-model.md                                  # Entity data model (251 lines, 10 entities)
    ├── quickstart.md                                  # Quickstart guide (133 lines)
    ├── tasks.md                                       # Task breakdown (269 lines, 45 tasks)
    ├── checklists/                                    # Requirements checklists
    └── contracts/                                     # 6 contract interface definitions
        ├── ISettingsRepository.cs.md
        ├── IAIRepository.cs.md
        ├── IPluginRepository.cs.md
        ├── ILogRepository.cs.md
        ├── ITemplateRepository.cs.md
        └── IUnitOfWork.cs.md

Docs/
├── fix.md / PROJECT_GUIDE.md / QUICK_REFERENCE.md
├── SESSION_STATUS.md
├── session_5b_settings.md / session_prompts.md / session_prompts -2.md
└── Architecture/
    ├── AGENT_RULES.md / UI_GUIDELINES.md / TOKEN_RULES.md
    ├── SHADOW_SYSTEM.md / POPUP_ARCHITECTURE.md
    ├── EXCEL_RENDERING_RULES.md / EXCEL_TEST_CHECKLIST.md
    ├── MVVM_RULES.md / MVVM_COMPLIANCE.md
    ├── ACCESSIBILITY_RULES.md / PERFORMANCE_RULES.md
    ├── REVIEW_CHECKLIST.md / AUDIT_REPORT.md
    ├── BRANCH_NAMING.md / BRANCH_PROTECTION.md
    ├── README.md
    ├── PERFORMANCE_AUDIT_REPORT.md              # Phase 10: perf baselines + optimizations
    ├── ACCESSIBILITY_AUDIT_REPORT.md            # Phase 10: keyboard nav + contrast
    ├── DPI_AUDIT_REPORT.md                      # Phase 10: DPI per-level + multi-monitor
    ├── EXCEL_STABILITY_REPORT.md                # Phase 10: VSTO stability results
    ├── LOCALIZATION_READINESS.md                # Phase 10: i18n architecture
    └── ENTERPRISE_POLISH_CHECKLIST.md           # Phase 10: master validation
```

### 1.9. Configuration & Workflow (`.opencode/`, `.specify/`, `.github/`)

```text
.opencode/
├── commands/                                   # 14 speckit command .md files
│   ├── speckit.analyze.md / speckit.checklist.md / speckit.clarify.md
│   ├── speckit.constitution.md / speckit.implement.md / speckit.plan.md
│   ├── speckit.specify.md / speckit.tasks.md / speckit.taskstoissues.md
│   └── speckit.git.*.md (commit, feature, initialize, remote, validate)
├── plans/                                      # 10 OpenCode phase plans
│   ├── README.md / gitignore.md
│   ├── phase-0-preflight-checklist.md
│   ├── phase-1-combobox-refactor.md / phase-2-shadow-system.md
│   ├── phase-3-theme-engine.md / phase-4-settings-ux.md
│   ├── phase-5-control-standardization.md / phase-6-animation-system.md
│   ├── phase-7-runtime-theme-manager.md / phase-8-advanced-polish.md
├── skills/image-to-md/                          # Image-to-markdown conversion skill
│   ├── SKILL.md / package.json / requirements.txt
│   └── scripts/image_to_md.py
└── package.json / package-lock.json

.specify/
├── extensions.yml / feature.json / init-options.json / integration.json
├── memory/
│   ├── constitution.md                          # v1.2.0 (16 principles, ratified 2026-05-21)
│   └── constitution-v2.md                      # v2.0.0 (AI rules + branch naming + token standards)
├── templates/
│   ├── checklist-template.md / constitution-template.md
│   ├── plan-template.md / spec-template.md / tasks-template.md
├── scripts/powershell/
│   ├── check-prerequisites.ps1 / common.ps1
│   ├── create-new-feature.ps1 / setup-plan.ps1 / setup-tasks.ps1
├── workflows/
│   ├── workflow-registry.json
│   └── speckit/workflow.yml
├── integrations/
│   ├── opencode.manifest.json / speckit.manifest.json
└── extensions/
    ├── .registry
    └── git/
        ├── README.md / config-template.yml / extension.yml / git-config.yml
        ├── commands/ (speckit.git.commit.md, .feature.md, .initialize.md, .remote.md, .validate.md)
        └── scripts/
            ├── bash/ (auto-commit.sh, create-new-feature.sh, git-common.sh, initialize-repo.sh)
            └── powershell/ (auto-commit.ps1, create-new-feature.ps1, git-common.ps1, initialize-repo.ps1)

.github/
├── CODEOWNERS
├── pull_request_template.md
└── workflows/
```

### 1.10. Other Directories

```text
packages/                                     # NuGet offline cache
├── CommunityToolkit.Mvvm.8.4.2/
├── Microsoft.Web.WebView2.1.0.3912.50/
├── Newtonsoft.Json.13.0.4/
├── System.Collections.Immutable.10.0.3/
└── ... (other packages)

scripts/
└── Validate-Tokens.ps1                        # Token validation script

Git/
└── Uploadgit.md                               # Git upload instructions
```

---

## Part 2: Files Organized by Function

### 2.1. Business Logic Services (Core)

| Service | Project | File |
|---------|---------|------|
| Assign Trade Codes | Som3a.Shared | `Core/AssignTradeCodesService.cs` |
| Excel Compare | Som3a.Shared | `Core/ExcelCompareService.cs` |
| Fix Pie Colors | Som3a.Shared | `Core/FixPieColorsService.cs` + `FixPieColorsResult.cs` |
| Float Path | Som3a.Shared | `Core/FloatPathService.cs` |
| Graph Builder | Som3a.Shared | `Core/GraphBuilder.cs` |
| Graph Service | Som3a.Shared | `Core/GraphService.cs` |
| Links Manager | Som3a.Shared | `Core/LinksManagerService.cs` |
| Path Finder | Som3a.Shared | `Core/PathFinder.cs` |
| Sub Daily Report | Som3a.Shared | `Core/SubDlyReportService.cs` |
| Unmerge Fill Down | Som3a.Shared | `Core/UnmergeFillDownService.cs` |
| WBS Builder | Som3a.Shared | `Core/WbsBuilder.cs` |
| WBS Coloring | Som3a.Shared | `Core/WbsColoringService.cs` |
| WBS Style Factory | Som3a.Shared | `Core/WbsStyleFactory.cs` |
| Workbook Clone | Som3a.Shared | `Core/WorkbookCloneService.cs` |
| XER Export | Som3a.Shared | `Core/XerExportService.cs` |
| XER Parser | Som3a.Shared | `Core/XER Parser.cs` |
| Primavera DB | Som3a.Shared | `Core/Primavera/PrimaveraDbService.cs` |
| Primavera Data Loader | Som3a.Shared | `Core/Primavera/PrimaveraDataLoaderService.cs` |
| Primavera Comparison | Som3a.Shared | `Core/Primavera/PrimaveraComparisonService.cs` |
| Compare Controller | Som3a.Shared | `Controllers/CompareController.cs` |

### 2.2. WPF Core Services (DI, Events, Modules)

| Service | File | Description |
|---------|------|-------------|
| ServiceContainer | `WpfApp2/Services/ServiceContainer.cs` | DI container (Singleton/Transient/Scoped) |
| EventBus | `WpfApp2/Services/EventBus.cs` | Typed pub/sub with weak references |
| ModuleRegistry | `WpfApp2/Services/ModuleRegistry.cs` | Priority-based module initialization |
| ModuleLoadOrchestrator | `WpfApp2/Services/ModuleLoadOrchestrator.cs` | Module lifecycle management |
| PluginLoader | `WpfApp2/Services/PluginLoader.cs` | Discover + load plugin assemblies |
| PluginRegistry | `WpfApp2/Services/PluginRegistry.cs` | Plugin metadata tracking |
| NavigationService | `WpfApp2/Services/NavigationService.cs` | Shell page navigation singleton |
| RibbonRegistrar | `WpfApp2/Services/RibbonRegistrar.cs` | Register ribbon actions |
| CommandRegistrar | `WpfApp2/Services/CommandRegistrar.cs` | Register commands for palette |
| NavigationRegistrar | `WpfApp2/Services/NavigationRegistrar.cs` | Register pages with nav service |
| LazyModuleCommand | `WpfApp2/Services/LazyModuleCommand.cs` | Deferred module command |

### 2.3. WPF Theme & UI Services

| Service | File | Description |
|---------|------|-------------|
| ThemeManager | `WpfApp2/Services/ThemeManager.cs` | Runtime theme switching singleton |
| WindowRenderModeDetector | `WpfApp2/Services/WindowRenderModeDetector.cs` | Auto-detect Excel fallback mode |
| RenderModeService | `WpfApp2/Services/RenderModeService.cs` | Render mode cache + events |
| DialogService | `WpfApp2/Services/DialogService.cs` | Show MessageBox dialogs |
| ToastService | `WpfApp2/Services/ToastService.cs` | Toast notifications |
| ThemeSettings | `WpfApp2/Services/ThemeSettings.cs` | Legacy JSON persistence |
| ExcelProjectAnalysisService | `WpfApp2/Services/ExcelProjectAnalysisService.cs` | Analyze Excel project data |

### 2.4. WPF Diagnostics & Validation Services

| Service | File | Description |
|---------|------|-------------|
| DiagnosticsService | `WpfApp2/Services/DiagnosticsService.cs` | Render mode, theme, memory snapshots |
| LoggingService | `WpfApp2/Services/LoggingService.cs` | File logging (5MB rollover, 3-file rotation) |
| ValidationEngine | `WpfApp2/Services/ValidationEngine.cs` | Token integrity + hardcoded color scan |
| ModuleDiagnosticsService | `WpfApp2/Services/ModuleDiagnosticsService.cs` | Plugin stability diagnostics |

### 2.5. WPF Windows (by domain)

#### Primavera Comparison
| File | Type |
|------|------|
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml` | Window (View) |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml.cs` | Code-behind |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml` | Window (View) |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml.cs` | Code-behind |
| `WpfApp2/ViewModels/Primavera/PrimaveraCompareViewModel.cs` | ViewModel |
| `WpfApp2/ViewModels/Primavera/PrimaveraResultsViewModel.cs` | ViewModel |
| `WpfApp2/Pages/PrimaveraComparePage.xaml` | Page (Shell) |
| `WpfApp2/Pages/PrimaveraResultsPage.xaml` | Page (Shell) |

#### Project Analysis
| File | Type |
|------|------|
| `WpfApp2/UI/ProjectAnalysisWindow.xaml` | Window (View) |
| `WpfApp2/Pages/ProjectAnalysisPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/ProjectAnalysisViewModel.cs` | ViewModel |

#### Settings
| File | Type |
|------|------|
| `WpfApp2/Views/SettingsWindow.xaml` | Window (ModernWindow) |
| `WpfApp2/Pages/SettingsPage.xaml` | Page (Shell) |
| `WpfApp2/Views/AccessibilityPanel.xaml` | Settings tab |
| `WpfApp2/Views/AppearancePanel.xaml` | Settings tab |
| `WpfApp2/Views/DiagnosticsPanel.xaml` | Settings tab |
| `WpfApp2/Views/ExcelPanel.xaml` | Settings tab |
| `WpfApp2/Views/PerformancePanel.xaml` | Settings tab |
| `WpfApp2/Views/PluginsPanel.xaml` | Settings tab |
| `WpfApp2/ViewModels/SettingsViewModel.cs` | ViewModel |

#### Float Path
| File | Type |
|------|------|
| `WpfApp2/Float_path.xaml` | Window |
| `WpfApp2/Pages/FloatPathPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/FloatPathViewModel.cs` | ViewModel |

#### Links Manager
| File | Type |
|------|------|
| `WpfApp2/LinksManagerWindow.xaml` | Window |
| `WpfApp2/Pages/LinksManagerPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/LinksManagerViewModel.cs` | ViewModel |

#### XER Editor
| File | Type |
|------|------|
| `WpfApp2/XerEditorWindow.xaml` | Window |
| `WpfApp2/Pages/XerEditorPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/XerEditorViewModel.cs` | ViewModel |
| `WpfApp2/Models/XerEditorVM.cs` | ViewModel (mislocated) |

#### Trade Codes
| File | Type |
|------|------|
| `WpfApp2/AssignTradeCodesWindow.xaml` | Window |
| `WpfApp2/Pages/AssignTradeCodesPage.xaml` | Page (Shell) |

#### Daily Report
| File | Type |
|------|------|
| `WpfApp2/SubDailyReportWindow.xaml` | Window |
| `WpfApp2/Pages/SubDailyReportPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/SubDailyReportViewModel.cs` | ViewModel |

#### Unmerge Fill Down
| File | Type |
|------|------|
| `WpfApp2/UnmergeFillDownWindow.xaml` | Window |
| `WpfApp2/Pages/UnmergeFillDownPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/UnmergeFillDownViewModel.cs` | ViewModel |

#### Fix Pie Colors
| File | Type |
|------|------|
| `WpfApp2/Fixpiecolors.xaml` | Window |
| `WpfApp2/Pages/FixPieColorsPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/FixPieColorsViewModel.cs` | ViewModel |

#### Style Selector
| File | Type |
|------|------|
| `WpfApp2/StyleSelectorWindow.xaml` | Window |
| `WpfApp2/Pages/StyleSelectorPage.xaml` | Page (Shell) |
| `WpfApp2/ViewModels/WbsStyleSelectorViewModel.cs` | ViewModel |

#### MainWindow
| File | Type |
|------|------|
| `WpfApp2/MainWindow.xaml` | Window |
| `WpfApp2/Pages/MainPage.xaml` | Page (Shell) |

### 2.6. Theme System (Fluent Design Tokens)

| Layer | Files | Purpose |
|-------|-------|---------|
| **Base Tokens** | `Theme/Base/Colors.xaml` | Primitive + semantic color tokens |
| | `Theme/Base/Typography.xaml` | Font size/weight tokens |
| | `Theme/Base/Spacing.xaml` | Margin/padding/size tokens |
| | `Theme/Base/Radius.xaml` | Corner radius tokens |
| | `Theme/Base/Elevation.xaml` | Elevation level tokens |
| | `Theme/Base/Motion.xaml` | Animation duration/easing tokens |
| | `Theme/Base/Opacity.xaml` | Standard opacity values |
| | `Theme/Base/ZIndex.xaml` | Z-index layering constants |
| | `Theme/Base/ComponentTokens.xaml` | Component-specific tokens |
| **Effects** | `Theme/Effects/Shadows.xaml` | Centralized DropShadowEffect (7 + safe variants) |
| | `Theme/Effects/Glow.xaml` | Centralized glow effects (6 variants) |
| | `Theme/Effects/Animations.xaml` | Storyboards (all ≤200ms) |
| **Dark Theme** | `Theme/Dark/DarkColors.xaml`, `DarkTheme.xaml` | Dark theme overrides |
| **Light Theme** | `Theme/Light/LightColors.xaml`, `LightTheme.xaml` | Light theme overrides |
| **Custom Theme** | `Theme/Custom/CustomColors.xaml`, `CustomTheme.xaml` | Custom + 8 accent swatches |
| **Control Styles** | `Theme/Controls/*.xaml` (22 files) | All control templates with VSM states |
| **Shell Styles** | `Theme/ShellStyles.xaml` | Shell-specific styles |
| **Aggregator** | `Theme/ThemeResources.xaml` | Ordered merge dictionary (16 layers) |
| **Window Template** | `Theme/ModernWindow.xaml` | ModernWindow chrome template |
| **Window Anim** | `Theme/WindowAnimations.xaml` | Window fade transitions |

### 2.7. Shell System

| File | Type | Purpose |
|------|------|---------|
| `Controls/Shell/ShellWindow.xaml/.cs` | Window | Main Shell container (ModernWindow-based) |
| `Controls/Shell/SidebarControl.xaml/.cs` | UserControl | Icon + label navigation sidebar |
| `Controls/Shell/WorkspaceHost.cs` | Class | Frame-based Page host |
| `Controls/Shell/ShellState.cs` | Class | Shell navigation state management |
| `Controls/Shell/CommandPalette.xaml/.cs` | UserControl | Ctrl+K searchable command palette |
| `Controls/Shell/NavigationDestination.cs` | Class | Navigation target definition |
| `Controls/Shell/NavigationEventArgs.cs` | Class | Navigation event arguments |
| `Controls/Shell/NavigationPage.cs` | Class | Page base class for navigation |
| `Services/NavigationService.cs` | Singleton | Page navigation with history stack |
| `Services/NavigationRegistrar.cs` | Service | Register pages with nav service |
| `Services/ShellNavigationHelper.cs` | Helper | Shell navigation utilities |
| `Pages/PageBase.cs` | Class | Base Page with lifecycle hooks |

### 2.8. Plugins & Modules System

| File | Type | Purpose |
|------|------|---------|
| `Contracts/ICommandRegistrar.cs` | Interface | Command registration contract |
| `Contracts/IModule.cs` | Interface | Module lifecycle contract |
| `Contracts/IModuleInitializationContext.cs` | Interface | Per-module init context |
| `Contracts/IModuleRegistry.cs` | Interface | Module registry contract |
| `Contracts/INavigationRegistrar.cs` | Interface | Navigation registration contract |
| `Contracts/IPluginLoader.cs` | Interface | Plugin discovery contract |
| `Contracts/IRibbonRegistrar.cs` | Interface | Ribbon registration contract |
| `Contracts/ModuleInfo.cs` | Class | Module metadata |
| `Contracts/ModuleManifest.cs` | Class | JSON module manifest |
| `Services/ModuleRegistry.cs` | Service | Priority-based module init |
| `Services/ModuleLoadOrchestrator.cs` | Service | Module lifecycle management |
| `Services/ModuleInitializationContext.cs` | Service | Per-module init context |
| `Services/ModuleRibbonActionsAvailableEvent.cs` | Event | Ribbon actions ready |
| `Services/PluginLoader.cs` | Service | Discover + load modules |
| `Services/PluginRegistry.cs` | Service | Plugin metadata tracking |
| `Services/LazyModuleCommand.cs` | Command | Deferred module command |
| `Services/RibbonRegistrar.cs` | Service | Register ribbon actions |
| `Services/CommandRegistrar.cs` | Service | Register palette commands |
| `Services/ModuleDiagnosticsService.cs` | Service | Plugin stability diagnostics |
| `ViewModels/PluginDiagnosticsViewModel.cs` | ViewModel | Plugin diagnostics VM |
| `WpfApp2.Modules.Sample/` | Project | Sample plugin module |

### 2.9. Converters

| Converter | File | Function |
|-----------|------|----------|
| Bool→Visibility | `WpfApp2/Converters/SharedConverters.cs` | BoolToVisibility, NullToVisibility, InverseBool |
| Diff Type→Color | `WpfApp2/Converters/DifferenceTypeToColorConverter.cs` | Added=Green, Deleted=Red, Modified=Yellow |
| Window Converters | `WpfApp2/Converters/WindowConverters.cs` | Window state sizing, maximize/restore |

### 2.10. Behaviors

| Behavior | File | Function |
|----------|------|----------|
| Drag Move | `WpfApp2/Behaviors/DragMoveBehavior.cs` | Drag window on header |
| Escape Close | `WpfApp2/Behaviors/EscapeCloseBehavior.cs` | Close on Escape key |

### 2.11. Helpers

| Helper | File | Function |
|--------|------|----------|
| AsyncRelayCommand | `WpfApp2/Helpers/AsyncRelayCommand.cs` | Async command with cancellation |
| DpiHelper | `WpfApp2/Helpers/DpiHelper.cs` | Monitor DPI detection + scaling |
| ExcelOwnerHelper | `WpfApp2/Helpers/ExcelOwnerHelper.cs` | Set WPF owner window from Excel handle |
| NotifyBase | `WpfApp2/Helpers/NotifyBase.cs` | INotifyPropertyChanged base class |
| RelayCommand | `WpfApp2/Helpers/RelayCommand.cs` | ICommand implementation |
| WindowBehaviorHelper | `WpfApp2/Helpers/WindowBehaviorHelper.cs` | Window property reflection |
| WindowChromeHelper | `WpfApp2/Helpers/WindowChromeHelper.cs` | Apply custom chrome |
| WindowValidationHelper | `WpfApp2/Helpers/WindowValidationHelper.cs` | Validate window properties |
| ComRelease | `Som3a.Shared/Interop/ComRelease.cs` | Safe COM cleanup |
| RelUtils | `Som3a.Shared/Utils/RelUtils.cs` | Relationship parsing |

### 2.12. Models/DTOs

| Model | Project | File |
|-------|---------|------|
| ProjectDto | Som3a.Shared | `Models/Primavera/ProjectDto.cs` |
| ActivityDto | Som3a.Shared | `Models/Primavera/ActivityDto.cs` |
| RelationshipDto | Som3a.Shared | `Models/Primavera/RelationshipDto.cs` |
| ResourceDto | Som3a.Shared | `Models/Primavera/ResourceDto.cs` |
| ComparisonModels | Som3a.Shared | `Core/Primavera/ComparisonModels.cs` |
| ProjectFullData | Som3a.Shared | `Core/Primavera/ProjectFullData.cs` |
| CompareResultItem | Som3a.Shared | `Models/CompareResultItem.cs` |
| LinkItem | Som3a.Shared | `Models/LinkItem.cs` |
| WbsItem | Som3a.Shared | `Models/WbsItem.cs` |
| WbsLevelStyle | Som3a.Shared | `Models/WbsLevelStyle.cs` |
| WorkbookItem | Som3a.Shared | `Models/WorkbookItem.cs` |
| ToastModel | WpfApp2 | `Controls/Toast/ToastModel.cs` |
| DiagnosticsModels | WpfApp2 | `Models/DiagnosticsModels.cs` |
| SettingsCategory | WpfApp2 | `Models/SettingsCategory.cs` |
| UserSettings | WpfApp2 | `Models/UserSettings.cs` |
| ModuleInfo | WpfApp2 | `Contracts/ModuleInfo.cs` |
| ModuleManifest | WpfApp2 | `Contracts/ModuleManifest.cs` |

### 2.13. VSTO/Excel Integration

| File | Project | Function |
|------|---------|----------|
| `ThisAddIn.cs` | Som3a Addin 2026 | VSTO startup (creates WPF App, registers DI) |
| `Ribbon1.cs` | Som3a Addin 2026 | Ribbon with 14 buttons + dynamic modules |
| `Som3aAddinBridge.cs` | Som3a Addin 2026 | Add-in bridge |
| `ExcelWindowHandle.cs` | Som3a Addin 2026 | Get Excel HWND |
| `WpfDialogHost.cs` | Som3a Addin 2026 | Host WPF as Excel dialog |
| `LinksManagerLauncher.cs` | Som3a Addin 2026 | Launch Links Manager from Excel |
| `AddInAutomation.cs` | Som3a Addin 2026 | Add-in automation interface |
| `WpfWindowManager.cs` | WpfApp2 | Window lifecycle in Excel |
| `ExcelOwnerHelper.cs` | WpfApp2 | Parent WPF to Excel window |
| `ExcelService.cs` | Som3a.Shared | Excel object model helpers |

---

## Part 3: Files by Feature Domain

### 3.1. Primavera Comparison (complete feature)

```text
Som3a.Shared/Core/Primavera/
├── ComparisonModels.cs                          # Diff models
├── IPrimaveraComparisonService.cs               # Service contract
├── IPrimaveraDataLoaderService.cs               # Service contract
├── IPrimaveraDbService.cs                       # Service contract
├── PrimaveraComparisonService.cs                # Implementation
├── PrimaveraDataLoaderService.cs                # Implementation
├── PrimaveraDbService.cs                        # Database access
└── ProjectFullData.cs                           # Data container

Som3a.Shared/Models/Primavera/
├── ActivityDto.cs
├── ProjectDto.cs
├── RelationshipDto.cs
└── ResourceDto.cs

Som3a.Shared/Controllers/
└── CompareController.cs                         # Orchestration

WpfApp2/Windows/PrimaveraComparison/
├── PrimaveraCompareWindow.xaml                  # UI
├── PrimaveraCompareWindow.xaml.cs
├── PrimaveraResultsWindow.xaml                  # UI
└── PrimaveraResultsWindow.xaml.cs

WpfApp2/Pages/                                   # Shell-hosted variants
├── PrimaveraComparePage.xaml
├── PrimaveraResultsPage.xaml

WpfApp2/ViewModels/Primavera/
├── PrimaveraCompareViewModel.cs                 # ViewModel
└── PrimaveraResultsViewModel.cs                 # ViewModel

WpfApp2/Converters/
└── DifferenceTypeToColorConverter.cs            # Converter

Som3a Addin 2026/Ribbon1.cs                      # Launch from Excel
```

### 3.2. Theme Engine (complete feature)

```text
WpfApp2/Theme/
├── Base/                                        # 9 token files
│   ├── Colors.xaml / Typography.xaml / Spacing.xaml / Radius.xaml
│   ├── Elevation.xaml / Motion.xaml / Opacity.xaml / ZIndex.xaml
│   └── ComponentTokens.xaml
├── Dark/                                        # Dark theme (2 files)
├── Light/                                       # Light theme (2 files)
├── Custom/                                      # Custom theme + 8 accent presets (2 files)
├── Controls/                                    # 22 control style files
├── Effects/                                     # 3 effect files (Shadows, Glow, Animations)
├── ThemeResources.xaml                          # Aggregator (16-layer loading order)
├── ModernWindow.xaml                            # Chrome template
├── WindowAnimations.xaml                        # Window transitions
└── ShellStyles.xaml                             # Shell-specific styles

WpfApp2/Services/
├── ThemeManager.cs                              # Singleton runtime switcher
├── ThemeSettings.cs                             # Legacy JSON persistence
├── WindowRenderModeDetector.cs                  # VSTO fallback detection
└── RenderModeService.cs                         # Render mode cache

WpfApp2/Properties/
├── Settings.settings                            # SelectedTheme + AccentColor
└── Settings.Designer.cs                         # Accessor

WpfApp2/Controls/
└── ModernWindow.cs                              # Base window with theme integration

WpfApp2/Views/
├── SettingsWindow.xaml                          # Theme card UI
└── SettingsWindow.xaml.cs
```

### 3.3. Shell & Navigation Platform

```text
WpfApp2/Controls/Shell/
├── ShellWindow.xaml/.cs                         # Main container
├── SidebarControl.xaml/.cs                      # Navigation sidebar
├── WorkspaceHost.cs                             # Page host
├── ShellState.cs                                # Navigation state
├── CommandPalette.xaml/.cs                      # Command palette
├── NavigationDestination.cs                     # Destination definition
├── NavigationEventArgs.cs                       # Navigation events
└── NavigationPage.cs                            # Page base

WpfApp2/Services/
├── NavigationService.cs                         # Navigation singleton
├── NavigationRegistrar.cs                       # Page registration
└── ShellNavigationHelper.cs                     # Nav utilities

WpfApp2/Pages/                                   # 14 shell-hosted pages
├── PageBase.cs / WelcomePage.xaml
├── MainPage / SettingsPage / ProjectAnalysisPage
├── FloatPathPage / AssignTradeCodesPage
├── FixPieColorsPage / LinksManagerPage
├── StyleSelectorPage / SubDailyReportPage
├── UnmergeFillDownPage / XerEditorPage
├── PrimaveraComparePage / PrimaveraResultsPage
```

### 3.4. Module & Plugin System

```text
WpfApp2/Contracts/
├── ICommandRegistrar.cs / IModule.cs
├── IModuleInitializationContext.cs / IModuleRegistry.cs
├── INavigationRegistrar.cs / IPluginLoader.cs / IRibbonRegistrar.cs
├── ModuleInfo.cs / ModuleManifest.cs

WpfApp2/Services/
├── ModuleRegistry.cs                            # Priority-based init
├── ModuleLoadOrchestrator.cs                    # Lifecycle management
├── ModuleInitializationContext.cs               # Per-module context
├── PluginLoader.cs                              # Discover + load
├── PluginRegistry.cs                            # Metadata tracking
├── LazyModuleCommand.cs                         # Deferred commands
├── RibbonRegistrar.cs / CommandRegistrar.cs
├── ModuleDiagnosticsService.cs
└── ModuleRibbonActionsAvailableEvent.cs

WpfApp2.Modules.Sample/
├── SampleModule.cs                              # IModule example
└── module.json                                  # Manifest
```

### 3.5. DI Container & Event Bus

```text
WpfApp2/Services/
├── ServiceContainer.cs                          # DI container (Singleton/Transient/Scoped)
├── EventBus.cs                                  # Typed pub/sub with weak references
└── CompositionRoot.cs (root)                    # Centralized DI registration
```

### 3.6. Diagnostics & Validation

```text
WpfApp2/Services/
├── DiagnosticsService.cs                        # Snapshot diagnostics
├── LoggingService.cs                            # File logging
├── ValidationEngine.cs                          # Token + inline color validation
└── ModuleDiagnosticsService.cs                  # Plugin diagnostics

WpfApp2/ViewModels/
├── DiagnosticsViewModel.cs
└── PluginDiagnosticsViewModel.cs

WpfApp2/Views/
├── DiagnosticsPanel.xaml/.cs
└── PluginsPanel.xaml/.cs
```

### 3.7. Toast Notification System

```text
WpfApp2/Controls/Toast/
├── ToastModel.cs
├── ToastWindow.xaml
└── ToastWindow.xaml.cs

WpfApp2/Services/
└── ToastService.cs

WpfApp2/ViewModels/
└── ToastViewModel.cs
```

### 3.8. XER File Handling

```text
Som3a.Shared/Core/
├── XER Parser.cs
└── XerExportService.cs

Som3a.Shared/Models/
└── XerMapper.cs

WpfApp2/
├── XerEditorWindow.xaml/.cs
├── Pages/XerEditorPage.xaml/.cs
├── ViewModels/XerEditorViewModel.cs
└── Models/XerEditorVM.cs
```

### 3.9. Float Path / Critical Path

```text
Som3a.Shared/Core/
├── FloatPathService.cs
├── PathFinder.cs
├── GraphBuilder.cs
└── GraphService.cs

Som3a.Shared/Models/
└── Float_path.cs

WpfApp2/
├── Float_path.xaml/.cs
├── Pages/FloatPathPage.xaml/.cs
├── ViewModels/FloatPathViewModel.cs
└── Utils/RelUtils.cs
```

### 3.10. WBS (Work Breakdown Structure)

```text
Som3a.Shared/Core/
├── WbsBuilder.cs
├── WbsStyleFactory.cs
└── WbsColoringService.cs

Som3a.Shared/Models/
├── WbsItem.cs
└── WbsLevelStyle.cs
```

---

## Part 4: Work Breakdown Structure (WBS)

### WBS Level 1: Solution

```text
1.0  Som3a Add-in 2026 — Complete VSTO Excel Add-in Enterprise Platform
```

### WBS Level 2: Projects

```text
1.1  Som3a Addin 2026 (VSTO Host)        # Excel add-in project
1.2  Som3a.Shared (Business Logic)       # Shared services & models
1.3  WpfApp2 (WPF UI)                    # Main WPF UI + Theme + Shell + Services
1.4  Tests (Unit Testing)                # MSTest unit tests
1.5  WpfApp2.Modules.Sample (Plugin)     # Sample plugin module
1.6  Som3a.Infrastructure (Planned)      # .NET 8.0 Persistence library (Phase 27)
1.7  Som3a.Domain (Planned)              # .NET 8.0 Domain entities (Phase 14)
1.8  Som3a.Contracts (Planned)           # .NET 8.0 Contract interfaces (Phase 14)
1.9  Som3a.AI (Planned)                  # .NET 8.0 AI abstraction (Phase 14/18)
1.10 Som3a.Plugin.SDK (Planned)          # .NET 8.0 Plugin framework (Phase 14)
1.11 Som3a.Exporting (Planned)           # .NET 8.0 Export engine (Phase 14)
1.12 Som3a.Localization (Planned)        # .NET 8.0 Localization (Phase 14/24)
1.13 Som3a.Validation (Planned)          # .NET 8.0 Validation (Phase 14)
1.14 Som3a.Diagnostics (Planned)         # .NET 8.0 Diagnostics (Phase 14)
1.15 Som3a.Bridge (Planned)              # .NET Standard 2.0 Bridge (Phase 14)
```

### WBS Level 3: Feature Domains

```text
1.1  VSTO Host (Excel Integration)
     1.1.1  Ribbon UI                    # Ribbon1.cs, main ribbon (14 buttons + modules)
     1.1.2  Add-in Bridge                # ThisAddIn.cs, bridge, automation
     1.1.3  Dialog Hosting               # WPF dialogs in Excel
     1.1.4  Excel Automation             # Excel interop + COM release

1.2  Business Logic (Shared)
     1.2.1  Primavera Services           # DB, Data Loader, Comparison
     1.2.2  XER Services                 # Parser, Export
     1.2.3  Excel Services               # Compare, Links, Clone
     1.2.4  WBS Services                 # Builder, Coloring, Styling
     1.2.5  Schedule Analysis            # Float Path, Critical Path, Graph
     1.2.6  Utility Services             # Trade Codes, Reports, Unmerge

1.3  WPF UI Layer
     1.3.1  Theme Engine                 # Design tokens, themes, effects
     1.3.2  Control Library              # Custom controls, behaviors
     1.3.3  Shell & Navigation           # ShellWindow, Sidebar, NavigationService
     1.3.4  Module & Plugin System       # ModuleRegistry, PluginLoader, Contracts
     1.3.5  DI & Event Infrastructure    # ServiceContainer, EventBus
     1.3.6  Diagnostics & Validation     # Logging, Diagnostics, ValidationEngine
     1.3.7  Feature Windows/Pages        # All 14 WPF windows + Page variants
     1.3.8  Services                     # ThemeManager, Toast, Dialog, Settings
     1.3.9  Infrastructure               # Helpers, Converters, ViewModels

1.4  Tests
     1.4.1  DI Container Tests           # ServiceContainerTests
     1.4.2  Event Bus Tests              # EventBusTests
     1.4.3  ViewModel Tests              # ViewModelBaseTests

1.5  Plugin Sample
      1.5.1  Sample Module                # SampleModule.cs + module.json

1.6  Persistence Infrastructure (Phase 27 — Planned)
      1.6.1  SQLite Infrastructure        # DatabaseContext, DatabaseFactory, ConnectionManager, SQLiteConfiguration
      1.6.2  Migration Engine             # MigrationEngine, 001_initial_schema.sql, version tracking, rollback
      1.6.3  Unit of Work                 # IUnitOfWork, UnitOfWork (BEGIN IMMEDIATE, atomic commit/rollback)
      1.6.4  Settings Repository          # ISettingsRepository, SettingsRepository (category+name+pluginId upsert)
      1.6.5  AI Repository                # IAIRepository, AIRepository (provider/date/status filtering, pagination)
      1.6.6  Plugin Repository            # IPluginRepository, PluginRepository (version history, health tracking)
      1.6.7  Diagnostics Repository       # ILogRepository, DiagnosticsRepository (severity filtering, batch writes)
      1.6.8  Template Repository          # ITemplateRepository, TemplateRepository (type+name+category search)
      1.6.9  Backup & Restore             # BackupService (VACUUM INTO), RestoreService, BackupManifest
      1.6.10 Security                     # DataProtection (DPAPI encrypt/decrypt)
      1.6.11 Data Retention               # DataRetentionConfiguration, DataRetentionService (chunked cleanup)
      1.6.12 Validation & Health          # EntityValidator, DatabaseHealthCheck (PRAGMA integrity_check)
      1.6.13 Composition Root             # ServiceRegistration in Som3a.Infrastructure
      1.6.14 Tests                        # xUnit project: repository contracts, migration, backup integration

1.7  Platform Foundation (Phase 14 — Planned)
      1.7.1  Domain Library               # BOQ, Activities, WBS, Relationships, Calendars, Resources, Constraints
      1.7.2  Contracts Library            # IPlugin, IAIProvider, IExportEngine, ISettingsModule, IPromptProvider
      1.7.3  Plugin SDK                   # Attributes, Discovery, Validation, Hosting infrastructure
      1.7.4  AI Abstraction Layer         # IAIProvider + OpenAI/Anthropic SDK adapters
      1.7.5  Export Engine                # Pipeline: Excel, CSV, JSON, XML, Primavera XER
      1.7.6  Localization Library         # i18n interfaces + resource management
      1.7.7  Validation Library           # Entity validation + cross-field rules
      1.7.8  Diagnostics Library          # Structured logging + performance counters
      1.7.9  Bridge (.NET Standard 2.0)   # InteropContracts, DiagnosticsChannel

1.8  Shell Refactor (Phase 15 — Planned)
      1.8.1  Categorized Sidebar          # Planning, Analysis, Excel, AI, Settings groups
      1.8.2  Dynamic Navigation           # [NavigationItem] attribute discovery
      1.8.3  Sidebar Collapse             # Auto-hide icon strip + expand/collapse
      1.8.4  Keyboard Navigation          # Arrow keys + skip link
      1.8.5  Unsaved Changes Warning      # CanNavigateAway guard
      1.8.6  Legacy Window Removal        # Remove 12 legacy standalone Window classes

1.9  Dynamic Settings Platform (Phase 16 — Planned)
      1.9.1  Settings Registry            # ISettingsModule registration
      1.9.2  Dynamic UI Builder           # Auto-render settings controls from declarations
      1.9.3  Hot-Reload via EventBus      # SettingsChangedEvent propagation
      1.9.4  DPAPI Encryption             # Encrypted API key storage
      1.9.5  Per-Plugin JSON Files        # AppData/Som3a/Plugins/ persistence
      1.9.6  Import/Export                # JSON bundle import/export

1.10 Theme Expansion (Phase 17 — Planned)
      1.10.1 Material Design Icons        # PackIcon integration
      1.10.2 Background Blur              # DWM blur on Shell workspace
      1.10.3 Font Switching               # English + Arabic font stacks
      1.10.4 Accent Color Picker          # Color wheel + hex + variant generation
      1.10.5 WCAG Validation              # Contrast compliance checks

1.11 AI Core Infrastructure (Phase 18 — Planned)
      1.11.1 Provider Adapters            # OpenAI, Claude, DeepSeek, GLM, Kimi, Codex
      1.11.2 Orchestration Engine         # Routing, streaming, failover
      1.11.3 Prompt Governance            # Draft→Published→Deprecated lifecycle
      1.11.4 Output Parsers               # Activity, WBS, Relationship, Duration, Validation
      1.11.5 Token Tracking               # Usage monitoring per provider/session

1.12 WBS Engine (Phase 19 — Planned)
      1.12.1 AI WBS Generation            # From project description or template
      1.12.2 Template Library             # 15+ templates across 5 categories
      1.12.3 Tree Editor                  # Manual edit + auto-code generation
      1.12.4 Multi-Format Export          # Excel, JSON, XML

1.13 BOQ Activity Generator (Phase 20 — Planned)
      1.13.1 BOQ Parsing                  # From Excel worksheets
      1.13.2 AI Context Building          # BOQ items → structured activities
      1.13.3 Activity Grid                # Editable, sortable, filterable grid
      1.13.4 Excel Export                 # Generated activities to Excel

1.14 Relationship Generator (Phase 21 — Planned)
      1.14.1 AI Relationship Logic        # FS/SS/FF/SF generation
      1.14.2 Cross-Trade Detection        # Trade sequence + space constraints
      1.14.3 Topological Validation       # Cycle detection + dangling links
      1.14.4 Critical Path                # PathFinder + GraphService integration

1.15 Duration Estimator (Phase 22 — Planned)
      1.15.1 Productivity Benchmarks      # Built-in library by trade category
      1.15.2 Calendar-Aware Scheduling    # Work days, holidays, shifts
      1.15.3 Three-Point Variance         # Optimistic, likely, pessimistic
      1.15.4 AI Suggestions               # Anomaly detection + rate recommendations

1.16 Dashboard Home (Phase 23 — Planned)
      1.16.1 Live Widgets                 # 9 fixed widgets (AI, DB, theme, memory, etc.)
      1.16.2 Diagnostics Page             # Real-time system metrics
      1.16.3 WelcomePage Replacement      # HomePage as default landing page

1.17 Localization & RTL (Phase 24 — Planned)
      1.17.1 Arabic Translation           # 100% Shell/Settings/Dashboard coverage
      1.17.2 RTL Mirroring                # FlowDirection switching without restart
      1.17.3 Culture-Aware Formatting     # Date, number, currency per locale

1.18 Platform Rebranding (Phase 25 — Planned)
      1.18.1 New Brand Colors             # Dark Engineering + Light Engineering-White
      1.18.2 Animated Splash              # ≤3s branded startup screen
      1.18.3 Shell Branding               # Logo + app name in sidebar
      1.18.4 Ribbon Icons                 # Updated Planova design system

1.19 Release Candidate Packaging (Phase 26 — Planned)
      1.19.1 Release Pipeline             # Validation → optimization → packaging → installer → QA
      1.19.2 MSI Installer                # WiX Toolset v4 + code signing
      1.19.3 Documentation                # CHM help file + PDF user guide
      1.19.4 Final Validation             # UI, plugin, AI, Excel regression suites
```

### WBS Level 4: Detailed Work Packages

```text
1.1.1  Ribbon UI
       1.1.1.1  Ribbon tabs & groups      # Ribbon1.Designer.cs
       1.1.1.2  Button click handlers      # Ribbon1.cs (14 buttons + dynamic modules)
       1.1.1.3  Ribbon images & icons      # Resources/ (25+ icons)
       1.1.1.4  Dynamic modules group      # ModuleLoadOrchestrator population

1.1.2  Add-in Bridge
       1.1.2.1  ThisAddIn startup          # ThisAddIn.cs (WPF App, DI, modules, plugins)
       1.1.2.2  Add-in shutdown cleanup    # ThisAddIn.Designer.cs
       1.1.2.3  Bridge to WPF layer        # Som3aAddinBridge.cs
       1.1.2.4  Add-in automation          # AddInAutomation.cs, IAddInAutomation.cs

1.1.3  Dialog Hosting
       1.1.3.1  WPF dialog in Excel        # WpfDialogHost.cs
       1.1.3.2  Window manager service      # WpfWindowManager.cs
       1.1.3.3  Excel HWND detection        # ExcelWindowHandle.cs

1.1.4  Excel Automation
       1.1.4.1  COM object cleanup          # ComRelease.cs
       1.1.4.2  Excel object helpers        # ExcelService.cs
       1.1.4.3  Owner window binding        # ExcelOwnerHelper.cs

1.2.1  Primavera Services
       1.2.1.1  Database service            # IPrimaveraDbService + PrimaveraDbService
       1.2.1.2  Data loader service         # IPrimaveraDataLoaderService + PrimaveraDataLoaderService
       1.2.1.3  Comparison service          # IPrimaveraComparisonService + PrimaveraComparisonService
       1.2.1.4  Data transfer objects       # ProjectDto, ActivityDto, RelationshipDto, ResourceDto
       1.2.1.5  Data container              # ProjectFullData
       1.2.1.6  Comparison models           # ComparisonModels.cs
       1.2.1.7  Flow controller             # CompareController.cs

1.2.2  XER Services
       1.2.2.1  XER file parser            # XER Parser.cs
       1.2.2.2  XER export service         # XerExportService.cs
       1.2.2.3  XER field mappings         # XerMapper.cs

1.2.3  Excel Services
       1.2.3.1  Worksheet comparison        # ExcelCompareService.cs
       1.2.3.2  Workbook links management   # LinksManagerService.cs
       1.2.3.3  Workbook cloning            # WorkbookCloneService.cs
       1.2.3.4  Pie chart color fix         # FixPieColorsService.cs + FixPieColorsResult.cs
       1.2.3.5  Link text helper            # ExcelLinkTextHelper.cs

1.2.4  WBS Services
       1.2.4.1  WBS hierarchy builder      # WbsBuilder.cs
       1.2.4.2  WBS level coloring         # WbsColoringService.cs
       1.2.4.3  WBS cell style factory     # WbsStyleFactory.cs
       1.2.4.4  WBS data models            # WbsItem.cs, WbsLevelStyle.cs

1.2.5  Schedule Analysis
       1.2.5.1  Float path calculation      # FloatPathService.cs
       1.2.5.2  Critical path finder       # PathFinder.cs
       1.2.5.3  Graph construction         # GraphBuilder.cs
       1.2.5.4  Graph analysis             # GraphService.cs
       1.2.5.5  Float path model           # Float_path.cs
       1.2.5.6  Relationship utilities     # RelUtils.cs

1.2.6  Utility Services
       1.2.6.1  Trade code assignment      # AssignTradeCodesService.cs
       1.2.6.2  Sub daily report gen       # SubDlyReportService.cs
       1.2.6.3  Unmerge & fill down        # UnmergeFillDownService.cs
       1.2.6.4  Excel project analysis     # ExcelProjectAnalysisService.cs

1.3.1  Theme Engine
       1.3.1.1  Base design tokens         # 9 token files (Colors, Typography, Spacing, Radius, Elevation, Motion, Opacity, ZIndex, Component)
       1.3.1.2  Theme variants             # Dark, Light, Custom (6 files total)
       1.3.1.3  Effects library            # Shadows (7+safe), Glow (6), Animations (all ≤200ms)
       1.3.1.4  Control styles             # 22 control style XAML files
       1.3.1.5  Aggregator                 # ThemeResources.xaml (16-layer loading order)
       1.3.1.6  Shell styles               # ShellStyles.xaml
       1.3.1.7  ThemeManager service       # ThemeManager.cs (singleton, debounce, accent, persistence)
       1.3.1.8  Theme persistence          # Settings.settings, Settings.Designer.cs
       1.3.1.9  Fallback detection         # WindowRenderModeDetector.cs, RenderModeService.cs
       1.3.1.10 Window chrome              # ModernWindow.cs, ModernWindow.xaml, WindowChromeHelper.cs
       1.3.1.11 DPI support                # DpiHelper.cs
       1.3.1.12 Window animations          # WindowAnimations.xaml

1.3.2  Control Library
       1.3.2.1  ModernWindow               # ModernWindow.cs + ModernWindow.xaml
       1.3.2.2  LoadingOverlay             # LoadingOverlay.xaml + .xaml.cs
       1.3.2.3  ModuleLoadingOverlay       # ModuleLoadingOverlay.xaml + .xaml.cs
       1.3.2.4  Toast system               # ToastModel.cs, ToastWindow.xaml/.cs
       1.3.2.5  DragMove behavior          # DragMoveBehavior.cs
       1.3.2.6  EscapeClose behavior       # EscapeCloseBehavior.cs

1.3.3  Shell & Navigation
       1.3.3.1  Shell window               # ShellWindow.xaml/.cs
       1.3.3.2  Sidebar control            # SidebarControl.xaml/.cs
       1.3.3.3  Workspace host             # WorkspaceHost.cs
       1.3.3.4  Shell state                # ShellState.cs
       1.3.3.5  Command palette            # CommandPalette.xaml/.cs
       1.3.3.6  Navigation service         # NavigationService.cs
       1.3.3.7  Navigation registrar       # NavigationRegistrar.cs
       1.3.3.8  Shell nav helper           # ShellNavigationHelper.cs
       1.3.3.9  Page base                  # PageBase.cs, NavigationPage.cs
       1.3.3.10 Navigation types           # NavigationDestination.cs, NavigationEventArgs.cs

1.3.4  Module & Plugin System
       1.3.4.1  Module contracts           # IModule, IModuleRegistry, IModuleInitializationContext (Contracts/)
       1.3.4.2  Plugin contracts           # IPluginLoader (Contracts/)
       1.3.4.3  Registration contracts     # ICommandRegistrar, INavigationRegistrar, IRibbonRegistrar
       1.3.4.4  Module registry            # ModuleRegistry.cs
       1.3.4.5  Module orchestrator        # ModuleLoadOrchestrator.cs
       1.3.4.6  Module init context        # ModuleInitializationContext.cs
       1.3.4.7  Plugin loader              # PluginLoader.cs
       1.3.4.8  Plugin registry            # PluginRegistry.cs
       1.3.4.9  Lazy module command        # LazyModuleCommand.cs
       1.3.4.10 Ribbon actions event        # ModuleRibbonActionsAvailableEvent.cs
       1.3.4.11 Module diagnostics          # ModuleDiagnosticsService.cs

1.3.5  DI & Event Infrastructure
       1.3.5.1  DI container               # ServiceContainer.cs (Singleton/Transient/Scoped, circular detection)
       1.3.5.2  Event bus                  # EventBus.cs (typed pub/sub, weak refs, subscriber isolation)
       1.3.5.3  Composition root           # CompositionRoot.cs (centralized DI registration)

1.3.6  Diagnostics & Validation
       1.3.6.1  Diagnostics service        # DiagnosticsService.cs
       1.3.6.2  Logging service            # LoggingService.cs (5MB rollover, 3-file rotation)
       1.3.6.3  Validation engine          # ValidationEngine.cs (token integrity, inline color scan)
       1.3.6.4  Diagnostics VM             # DiagnosticsViewModel.cs
       1.3.6.5  Plugin diagnostics VM      # PluginDiagnosticsViewModel.cs
       1.3.6.6  Diagnostics panel          # DiagnosticsPanel.xaml/.cs
       1.3.6.7  Plugins panel              # PluginsPanel.xaml/.cs

1.3.7  Feature Windows/Pages
       1.3.7.1  MainWindow/MainPage        # Comparison hub
       1.3.7.2  SettingsWindow/SettingsPage # Theme + accent settings
       1.3.7.3  PrimaveraCompare window    # Project comparison config
       1.3.7.4  PrimaveraResults window    # Comparison results display
       1.3.7.5  ProjectAnalysis window     # Excel project analysis
       1.3.7.6  XerEditor window           # XER file editing
       1.3.7.7  AssignTradeCodes window    # Trade code assignment
       1.3.7.8  LinksManager window        # Workbook links
       1.3.7.9  SubDailyReport window      # Subcontractor reports
       1.3.7.10 FloatPath window           # Float path analysis
       1.3.7.11 UnmergeFillDown window     # Unmerge + fill down
       1.3.7.12 FixPieColors window        # Pie chart color fix
       1.3.7.13 StyleSelector window       # Style selection
       1.3.7.14 Welcome page               # Shell welcome

1.3.8  Services
       1.3.8.1  ThemeManager               # Theme switching singleton
       1.3.8.2  ToastService               # Toast notifications
       1.3.8.3  DialogService              # MessageBox dialogs
       1.3.8.4  ThemeSettings              # Legacy JSON persistence
       1.3.8.5  SettingsPersistenceService # Settings serialization
       1.3.8.6  ExcelProjectAnalysisService

1.3.9  Infrastructure
        1.3.9.1  ViewModel base             # ViewModelBase.cs, NotifyBase.cs
        1.3.9.2  Relay commands             # RelayCommand.cs, AsyncRelayCommand.cs
        1.3.9.3  Value converters           # SharedConverters.cs, WindowConverters.cs, DifferenceTypeToColorConverter.cs
        1.3.9.4  Window helpers             # WindowBehaviorHelper.cs, WindowValidationHelper.cs, WindowChromeHelper.cs
        1.3.9.5  DPI helper                 # DpiHelper.cs
        1.3.9.6  ViewModels                 # 17 ViewModels across VMs directory
        1.3.9.7  Settings panels            # 6 settings panel files (Appearance, Accessibility, Performance, Excel, Diagnostics, Plugins)

1.6  Persistence Infrastructure
        1.6.1.1  SQLiteConfiguration        # Data directory, filename, connection string builder
        1.6.1.2  ConnectionManager          # Reader/writer split, SemaphoreSlim(1,1) write gate
        1.6.1.3  DatabaseContext            # PRAGMA setup (WAL, synchronous=NORMAL, busy_timeout=5000, foreign_keys=ON)
        1.6.1.4  DatabaseFactory            # Singleton factory, initialization orchestration
        1.6.2.1  MigrationEngine            # PRAGMA user_version tracking, numbered SQL scripts, rollback
        1.6.2.2  001_initial_schema.sql     # All 10 entity tables with GUID PKs, indexes, constraints
        1.6.3.1  UnitOfWork                 # BEGIN IMMEDIATE, CommitAsync, RollbackAsync, repository accessors
        1.6.4.1  Settings repository        # Upsert (INSERT OR REPLACE), category+name+pluginId unique
        1.6.5.1  AI repository              # Date/provider/status filtering, pagination, runtime aggregation
        1.6.6.1  Plugin repository          # Version history FK chain, health tracking, enable/disable
        1.6.7.1  Diagnostics repository     # Severity filtering, batched writes, export history queries
        1.6.8.1  Template repository        # Type+name+category unique, partial match search, version increment
        1.6.9.1  BackupService              # VACUUM INTO, SHA-256 checksum, PRAGMA integrity_check
        1.6.9.2  RestoreService             # Close connections, replace file, re-run migrations
        1.6.10.1 DataProtection             # DPAPI (ProtectedData) encrypt/decrypt for sensitive columns
        1.6.11.1 DataRetentionService       # Chunked batch deletes (500 rows), per-category config
        1.6.12.1 EntityValidator            # GUID non-empty, string max length, enum validation
        1.6.12.2 DatabaseHealthCheck        # PRAGMA integrity_check on startup, recovery from backup
        1.6.13.1 ServiceRegistration        # DI registration of DatabaseFactory, repositories, services
        1.6.14.1 Repository contract tests  # SettingsRepository, AIRepository via in-memory SQLite
        1.6.14.2 Migration integration test # Apply all migrations, verify schema version, rollback
        1.6.14.3 Backup integration test    # Backup checksum, restore, data integrity verification
        ```

### WBS Format: Numeric Code Key

```text
WBS Format:  X.Y.Z.W
  X = Project (1=VSTO Host, 2=Shared, 3=WPF UI, 4=Tests, 5=Plugin Sample, 6=Infrastructure)
  Y = Feature Domain (1-9)
  Z = Work Package (1-14)
  W = Task (optional, 1-12)

Examples:
  1.1.1.2  = VSTO Host > Ribbon UI > Button handlers > Ribbon1.cs
  2.2.4.1  = Shared > Schedule Analysis > Float path > FloatPathService.cs
  3.3.3.6  = WPF UI > Shell > Navigation service > NavigationService.cs
  3.1.4.6  = WPF UI > Theme Engine > Control styles > ComboBoxStyles.xaml
```

### WBS Task Estimation

| WBS Code | Work Package | Phase | Status | Est. Effort |
|----------|-------------|-------|--------|-------------|
| 1.1.1 | Ribbon UI | 7 | ✅ Complete | 2d |
| 1.1.2 | Add-in Bridge | 9 | ✅ Complete | 1d |
| 1.1.3 | Dialog Hosting | 2,5 | ✅ Complete | 2d |
| 1.1.4 | Excel Automation | 2 | ✅ Complete | 3d |
| 1.2.1 | Primavera Services | 1,3,4 | ✅ Complete | 10d |
| 1.2.2 | XER Services | 5 | ✅ Complete | 5d |
| 1.2.3 | Excel Services | 1 | ✅ Complete | 5d |
| 1.2.4 | WBS Services | 1 | ✅ Complete | 3d |
| 1.2.5 | Schedule Analysis | 1 | ✅ Complete | 5d |
| 1.2.6 | Utility Services | 1 | ✅ Complete | 3d |
| 1.3.1 | Theme Engine | 0-3,10 | 🔄 Validation | 15d+ |
| 1.3.2 | Control Library | 4 | ✅ Complete | 3d |
| 1.3.3 | Shell & Navigation | 5,8 | ✅ Complete | 10d |
| 1.3.4 | Module & Plugin System | 6,9 | ✅ Complete | 8d |
| 1.3.5 | DI & Event Infrastructure | 6 | ✅ Complete | 5d |
| 1.3.6 | Diagnostics & Validation | 8 | ✅ Complete | 5d |
| 1.3.7 | Feature Windows/Pages | 3,5,11 | 🔄 Migration | 15d+ |
| 1.3.8 | Services | 3 | ✅ Complete | 3d |
| 1.3.9 | Infrastructure | 3 | ✅ Complete | 5d |
| 1.4 | Tests | 6 | ✅ Complete | 3d |
| 1.5 | Plugin Sample | 9 | ✅ Complete | 1d |
| 1.6 | Persistence Infrastructure | 27 | 📋 Spec Complete | 15d |
| 1.7 | Platform Foundation (7 libs) | 14 | 📋 Planned | 20d |
| 1.8 | Shell Refactor | 15 | 📋 Planned | 10d |
| 1.9 | Dynamic Settings Platform | 16 | 📋 Planned | 10d |
| 1.10 | Theme Expansion | 17 | 📋 Planned | 8d |
| 1.11 | AI Core Infrastructure | 18 | 📋 Planned | 15d |
| 1.12 | WBS Engine | 19 | 📋 Planned | 12d |
| 1.13 | BOQ Activity Generator | 20 | 📋 Planned | 10d |
| 1.14 | Relationship Generator | 21 | 📋 Planned | 8d |
| 1.15 | Duration Estimator | 22 | 📋 Planned | 10d |
| 1.16 | Dashboard Home | 23 | 📋 Planned | 8d |
| 1.17 | Localization & RTL | 24 | 📋 Planned | 10d |
| 1.18 | Platform Rebranding | 25 | 📋 Planned | 8d |
| 1.19 | Release Candidate Packaging | 26 | 📋 Planned | 10d |
| | **Total Estimated** | | | **~244 days** |

---

## Part 5: ChatGPT Prompt Template

Copy this entire block when you need ChatGPT to work with the codebase:

````markdown
## Project Context

You are working on **Som3a Addin 2026**, a professional Excel VSTO Add-in built with:
- **C# (LangVersion 14.0) / .NET Framework 4.8** (WPF/VSTO) + **.NET 8.0** (Domain/AI/Contracts/Infrastructure)
- **WPF** for UI (ModernWindow, WindowChrome, ResourceDictionaries)
- **VSTO** for Excel integration (Ribbon1.cs, ThisAddIn.cs)
- **CommunityToolkit.Mvvm 8.4.2** for MVVM
- **Oracle/SQL Server** for Primavera P6 database access
- **SQLite (Microsoft.Data.Sqlite 8.0+)** for platform persistence (Phase 27)

### Solution Structure (3 main projects + 1 test + 1 sample + planned .NET 8.0 libraries)
1. **Som3a Addin 2026/** — Excel host (Ribbon, Bridge, Dialog Hosting)
2. **Som3a.Shared/** — Business logic (Primavera, XER, Excel, WBS, Graph)
3. **WpfApp2/** — WPF UI (all windows, Pages, ViewModels, Theme, Shell, Services)
4. **Tests/** — Unit tests (MSTest)
5. **WpfApp2.Modules.Sample/** — Sample plugin module
6. **Som3a.Infrastructure/** (Planned) — .NET 8.0 persistence library for Phase 27: SQLite, repositories, migrations, backup/restore, DPAPI encryption

### Architecture Layers
```
VSTO → WPF Shell → ViewModels → Business Logic (Shared) → DI Infrastructure → Data Access
```

### Persistence Layer (Phase 27)
```
Application Layer → Repository Interfaces (ISettingsRepository, IAIRepository, IPluginRepository, ILogRepository, ITemplateRepository)
                → Unit of Work (atomic transactions)
                → Database Infrastructure (DatabaseFactory, ConnectionManager, MigrationEngine, BackupService)
                → SQLite (platform.db, WAL mode, DPAPI, GUID PKs)
```

### Key Architecture Rules
1. **DynamicResource only** for themeable properties (no StaticResource)
2. **No inline DropShadowEffect** — use `{DynamicResource Shadow.*}` from Effects/Shadows.xaml
3. **No third-party UI frameworks** — native WPF only
4. **MVVM** — minimal code-behind, business logic in ViewModels, constructor DI via ServiceContainer
5. **WindowChrome** preferred, auto-fallback to AllowsTransparency via WindowRenderModeDetector
6. **All animations ≤200ms** — from Motion tokens, with reduced-motion support
7. **Theme tokens**: Primitive.* > Brush.* > Component.* > Control Templates
8. **Never**: inline colors, inline shadows, inline margins, duplicate styles, business logic in code-behind
9. **Excel-safe rendering**: AllowsTransparency=False for popups, FallbackSafe mode detection
10. **Async persistence**: All DB operations async with cancellation tokens (Phase 27)
11. **GUID PKs** for all persistence entities (Phase 27 — future sync readiness)
12. **DPAPI encryption** for sensitive data at rest (Phase 27)

### Build Commands
```powershell
# WPF .NET Framework 4.8 project
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Full solution
msbuild "Som3a Addin 2026.slnx" /p:Configuration=Debug

# .NET 8.0 libraries (planned)
dotnet build Som3a.Infrastructure/Som3a.Infrastructure.csproj
```

### Theme Resource Loading Order (ThemeResources.xaml)
1. Base/Colors.xaml → 2. Typography → 3. Spacing → 4. Radius → 5. Elevation → 6. Motion
7. ZIndex → 8. Opacity → 9. ComponentTokens → 10. Effects/ (Shadows, Glow, Animations)
11. Controls/ (22 style files) → 12. ModernWindow.xaml → 13. ShellStyles.xaml
14. WindowAnimations.xaml → 15. Theme Overrides (Dark/Light/Custom — swapped at runtime)
16. Runtime Overrides (generated brushes, accent variants)

### Persistence Architecture (Phase 27)
```
Storage: AppData/Som3a/platform.db
Engine:  SQLite (Microsoft.Data.Sqlite 8.0+)
Journal: WAL (Write-Ahead Logging)
Keys:    GUID for all entities
Encrypt: DPAPI (ProtectedData) for sensitive columns
Backup:  VACUUM INTO + SHA-256 integrity check
Migrate: PRAGMA user_version + numbered SQL scripts + rollback
Retain:  AI=1yr, Diagnostics=90d, Crash=2yr, Export=1yr (configurable)
Test:    In-memory SQLite (:memory:) for unit tests, file-based for integration
```

### Full File Tree Reference
See specs/guide-files-path-and-wbs.md for complete file inventory (410+ files).
````

---

## Part 6: Quick Reference Tables

### 6.1. File Count Summary

| Project | Source Files | XAML Files | Config/Other | Total |
|---------|-------------|------------|--------------|-------|
| Som3a Addin 2026 | 12 | 0 | 8 | 20 |
| Som3a.Shared | 41 | 0 | 3 | 44 |
| WpfApp2 | 100+ | 55+ | 8 | 163+ |
| Tests | 3 | 0 | 3 | 6 |
| WpfApp2.Modules.Sample | 2 | 0 | 1 | 3 |
| Som3a_WPF_UI (.NET 10.0) | 2 | 1 | 2 | 5 |
| Som3a_WPF_UId (legacy) | 3 | 1 | 1 | 5 |
| WpfApp1 (legacy) | 3 | 1 | 6 | 10 |
| Specs/Docs | 0 | 0 | 170+ (28 spec dirs + architecture + plans) | 170+ |
| Config/Workflow (.opencode, .specify, .github) | 0 | 0 | 50+ | 50+ |
| packages/ | 0 | 0 | 9 packages | 9 |
| **Total** | **~166** | **~58** | **~261** | **~485** |

### 6.2. Project Dependencies

```text
Som3a Addin 2026 (VSTO)
  ├── references WpfApp2 (WPF UI)
  ├── references Som3a.Shared (Business Logic)
  └── NuGet: CommunityToolkit.Mvvm, WebView2, Oracle MDA

WpfApp2 (WPF UI)
  ├── references Som3a.Shared (Business Logic)
  └── NuGet: CommunityToolkit.Mvvm, WebView2, Newtonsoft.Json

Tests
  ├── references WpfApp2 (WPF UI)
  └── packages: MSTest

WpfApp2.Modules.Sample (Plugin)
  └── references WpfApp2 (WPF UI for IModule contracts)

Som3a.Shared (Business Logic)
  └── NuGet: Oracle MDA, WebView2, Newtonsoft.Json (no project references)

Som3a.Infrastructure (Phase 27 — .NET 8.0 Persistence) [PLANNED]
  └── NuGet: Microsoft.Data.Sqlite 8.0+, System.Text.Json (built-in)
  └── Depends on: Project reference: Som3a.Contracts
```

### 6.3. Theme Control Styles (22 files)

```text
Controls/
├── ButtonStyles.xaml         # Primary, Ghost, Icon, Danger variants (VSM states + glow)
├── ComboBoxStyles.xaml       # Popup fix (AllowsTransparency=False, PlacementTarget)
├── ComboBoxItemStyles.xaml   # Item container with hover/selected states
├── TextBoxStyles.xaml        # VSM states with accent border transitions
├── PasswordBoxStyles.xaml    # Password input with reveal button
├── CheckBoxStyles.xaml       # VSM states + custom checkmark
├── RadioButtonStyles.xaml    # VSM states + dot indicator
├── ToggleButtonStyles.xaml   # VSM states + slide animation
├── DataGridStyles.xaml       # Virtualization, hover/selection via DynamicResource
├── ListViewStyles.xaml       # Item container + virtualization
├── ListViewItemStyles.xaml   # ListView item hover/selected states
├── TreeViewStyles.xaml       # Tree node expand/collapse + hover/selected
├── ScrollViewerStyles.xaml   # Modern thin scrollbar (4px track, 8px thumb)
├── ScrollBarStyles.xaml      # ScrollBar styling
├── ProgressBarStyles.xaml    # Accent gradient fill + ProgressGlow
├── GroupBoxStyles.xaml       # GroupBox border + header styling
├── LabelStyles.xaml          # Label styling
├── ThemeCardStyles.xaml      # Theme card with glow + scale animations
├── AccentSwatchStyles.xaml   # Accent swatch circles (8 presets)
├── WindowStyles.xaml         # ModernWindow chrome styles
├── WindowButtonStyles.xaml   # Close/minimize/maximize button styles
└── SettingsPanelStyles.xaml  # Settings panel tab + content styles
```

### 6.4. Implementation Phase Summary

| Phase | Name | Branch | Status | Key Artifact |
|-------|------|--------|--------|-------------|
| 0 | Governance Foundation | `feature/phase-00-*` | ✅ Complete | 10 governance docs + Constitution v2 |
| 1 | Design System Core | `feature/phase-01-*` | ✅ Complete | Primitive + Semantic + Component tokens |
| 2 | Rendering Infrastructure | `feature/phase-02-*` | ✅ Complete | ModernWindow v2 + RenderModeService |
| 3 | Theme Engine 2.0 | `002-fluent-theme-engine` | ✅ Complete (15 valid. tasks remain) | 22 control styles + 3 themes + effects |
| 4 | Control Standardization | `feature/phase-04-*` | ✅ Complete | ComboBox popup fix + all 22 styles |
| 5 | Navigation Shell | `feature/phase-05-*` | ✅ Complete | ShellWindow + Sidebar + NavigationService |
| 6 | MVVM Cleanup | `feature/phase-06-*` | ✅ Complete | ServiceContainer + EventBus + ModuleRegistry |
| 7 | Settings UX | `feature/phase-07-*` | ✅ Complete | 6 settings panels + SettingsPersistence |
| 8 | Diagnostics Platform | `feature/phase-08-*` | ✅ Complete | LoggingService + ValidationEngine + Diagnostics |
| 9 | Plugin Platform | `feature/phase-09-*` | ✅ Complete | PluginLoader + ModuleOrchestrator + Sample |
| 10 | Enterprise Polish | `013-enterprise-polish` | 🔄 Validation | 6 audit reports + 37 tasks (7 workstreams) |
| 11 | Legacy Window Migration | `011-legacy-window-migration` | 🔄 Migration | 14 Pages + MIGRATION_PATTERNS.md |
| 14 | Platform Foundation | `feature/phase-14-*` | 📋 Planned | 7 .NET 8.0 class libraries + Bridge |
| 15 | Shell Refactor | `015-shell-refactor` | 📋 Planned | Categorized sidebar + dynamic nav + legacy window cleanup |
| 16 | Dynamic Settings Platform | `feature/phase-16-*` | 📋 Planned | ISettingsModule + registry + DPAPI encryption |
| 17 | Theme Expansion | `017-theme-expansion` | 📋 Planned | Material Design + background blur + font switching |
| 18 | AI Core Infrastructure | `018-ai-core-infrastructure` | 📋 Planned | 6 AI providers + orchestration + prompt governance |
| 19 | WBS Engine | `019-wbs-engine` | 📋 Planned | AI WBS generation + 15+ templates + tree editor |
| 20 | BOQ Activity Generator | `020-boq-activity-generator` | 📋 Planned | AI BOQ→Activity generation |
| 21 | Relationship Generator | `021-relationship-generator-plugin` | 📋 Planned | AI relationship generation (FS/SS/FF/SF) |
| 22 | Duration Estimator | `022-duration-estimator-plugin` | 📋 Planned | Duration calc from quantities + productivity benchmarks |
| 23 | Dashboard Home | `023-dashboard-home` | 📋 Planned | 9 live widgets replacing WelcomePage |
| 24 | Localization & RTL | `025-localization-rtl` | 📋 Planned | Arabic + English RTL without restart |
| 25 | Platform Rebranding | `026-platform-rebranding` | 📋 Planned | Som3a→Planova brand + splash + iconography |
| 26 | Release Candidate | `feature/phase-26-*` | 📋 Planned | MSI installer + code signing + validation |
| 27 | Persistence Infrastructure | `027-persistence-infrastructure` | 📋 Spec Complete | SQLite — 45 tasks, 10 entities, 5 repos, migrations, backup/restore, DPAPI |

---

*This document is a living reference. Update when new files are added or the project structure changes.*
