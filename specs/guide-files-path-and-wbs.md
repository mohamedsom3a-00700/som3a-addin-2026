# Som3a Add-in 2026 — Full Files Path Guide & WBS for ChatGPT

**Purpose**: This document is a complete reference of every file path in the project, organized in multiple ways (by project, by function, by feature), plus a comprehensive Work Breakdown Structure (WBS). Give this to ChatGPT as system context so it can understand the full codebase.

---

## Part 1: Complete Solution File Tree (Every File, Hierarchical)

### 1.1. Solution Root

```
Som3a Addin 2026.slnx                        # Solution file (Visual Studio)
README.md                                      # Project overview
AGENTS.md                                      # AI agent instructions
COMPLETE_SOLUTION_GUIDE.md                     # Full solution guide (1595 lines)
.gitattributes                                 # Git LFS/attributes config
.gitignore                                     # Git ignore rules
```

### 1.2. VSTO Add-in: `Som3a Addin 2026/`

```
Som3a Addin 2026/
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs
│   └── Settings.settings
├── Resources/                              # Icons & images for ribbon/UI
│   ├── 1302070.png
│   ├── 150-1504985_open-split-icon-png.png
│   ├── 199-1998376_email-marketing-icon-daily-report-icon-png.png
│   ├── 325-3255215_refresh-clipart-reload-icon-png.png
│   ├── 3502688.png
│   ├── 3781852.png
│   ├── 3892368.png
│   ├── 6915924.png
│   ├── 7171906.png
│   ├── 800a0207-9614-4cb2-8c0b-8daf79416ee0.png
│   ├── 8276278.png
│   ├── 9604087.png
│   ├── audit-data-analysis-icon-financial-assessment-vector-335920716.jpg
│   ├── cf4c1cb044c54bfc856f2f370df8e395.png
│   ├── colour-wheel.png
│   ├── daily-report.png
│   ├── editable-unmerge-table-cell-vector-260nw-2568331085.jpg
│   ├── links.png
│   ├── pie.png
│   ├── png-clipart-computer-icons-copying-icon-design-transfer-icon-text-logo.png
│   ├── png-transparent-computer-icons-code-symbol-coding-miscellaneous-angle-text-thumbnail.png
│   ├── settings.jpg
│   ├── stock-vector-comparison-icon-and-symbol-438145465.jpg
│   ├── unmerage.png
│   └── updates.png
├── Ui/
│   ├── LinksManagerLauncher.cs              # Launch links manager from Excel
│   └── WpfDialogHost.cs                     # Host WPF dialogs in Excel
├── app.config                                # Connection strings & app settings
├── ExcelWindowHandle.cs                     # Get Excel window handle for Win32 interop
├── Oracle.DataAccess.Common.Configuration.Section.xsd
├── Oracle.ManagedDataAccess.Client.Configuration.Section.xsd
├── packages.config                           # NuGet packages (legacy)
├── Ribbon1.cs                                # Excel ribbon button handlers
├── Ribbon1.Designer.cs                       # Ribbon designer code
├── Ribbon1.resx                              # Ribbon resources
├── Som3a Addin 2026.csproj                  # Project file (.NET Framework 4.8)
├── Som3a Addin 2026.csproj.user
├── Som3a Addin 2026_TemporaryKey.pfx         # Signing key
├── Som3aAddinBridge.cs                      # Bridge between Excel and WPF
├── ThisAddIn.cs                              # VSTO add-in entry point
├── ThisAddIn.Designer.cs
├── ThisAddIn.Designer.xml
└── WpfDialogHost.cs                          # (duplicate? same as Ui/WpfDialogHost.cs)
```

### 1.3. Shared Library: `Som3a.Shared/`

```
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
│       ├── ComparisonModels.cs              # Diff models (ActivityDiff, etc.)
│       ├── IPrimaveraComparisonService.cs    # Comparison service contract
│       ├── IPrimaveraDataLoaderService.cs    # Data loader contract
│       ├── IPrimaveraDbService.cs            # Database service contract
│       ├── PrimaveraComparisonService.cs     # Compare two P6 projects
│       ├── PrimaveraDataLoaderService.cs     # Load P6 project data
│       ├── PrimaveraDbService.cs             # Database access (SQL Server)
│       └── ProjectFullData.cs               # Container for all project data
├── Excel Class/
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

```
WpfApp2/
├── Analyzers/
│   ├── ModernWindowAnalyzer.cs               # Roslyn analyzer (excluded from build)
│   └── README.md
├── Behaviors/
│   ├── DragMoveBehavior.cs                   # Drag window on mouse move
│   └── EscapeCloseBehavior.cs                # Close window on Escape
├── Commands/
├── Controls/
│   ├── LoadingOverlay.xaml                    # Loading spinner overlay
│   ├── LoadingOverlay.xaml.cs
│   ├── ModernWindow.cs                       # Base custom chrome window
│   └── Toast/
│       ├── ToastModel.cs                     # Toast notification model
│       ├── ToastWindow.xaml                  # Toast notification popup
│       └── ToastWindow.xaml.cs
├── Converters/
│   ├── DifferenceTypeToColorConverter.cs     # Diff type → color (green/red/yellow)
│   ├── SharedConverters.cs                   # Bool→Visibility, etc.
│   └── WindowConverters.cs                   # Window state converters
├── Helpers/
│   ├── AsyncRelayCommand.cs                  # Async command implementation
│   ├── DpiHelper.cs                          # DPI detection & scaling
│   ├── ExcelOwnerHelper.cs                   # Set WPF owner from Excel hwnd
│   ├── NotifyBase.cs                         # INotifyPropertyChanged base
│   ├── RelayCommand.cs                       # ICommand implementation
│   ├── WindowBehaviorHelper.cs               # Window behavior reflection helper
│   ├── WindowChromeHelper.cs                 # Apply WindowChrome
│   └── WindowValidationHelper.cs             # Validate window properties
├── Models/
│   ├── RelayCommand.cs                       # (duplicate?)
│   ├── SubDailyReportViewModel.cs            # (mislocated VM in Models)
│   ├── UnmergeFillDownViewModel.cs            # (mislocated VM in Models)
│   └── XerEditorVM.cs                        # (mislocated VM in Models)
├── Properties/
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Resources.resx
│   ├── Settings.Designer.cs                  # Theme persistence accessors
│   └── Settings.settings                     # User-scoped theme + accent settings
├── Services/
│   ├── DialogService.cs                      # Show dialogs from ViewModels
│   ├── ExcelProjectAnalysisService.cs        # Analyze Excel project data
│   ├── ThemeManager.cs                       # Singleton runtime theme switcher
│   ├── ThemeSettings.cs                      # Legacy JSON theme persistence
│   ├── ToastService.cs                       # Show toast notifications
│   └── WindowRenderModeDetector.cs           # Auto-detect Excel rendering fallback
├── Theme/                                    # Fluent Design system
│   ├── ModernWindow.xaml                     # ModernWindow control template
│   ├── ThemeResources.xaml                   # Aggregator (ordered merge dict)
│   ├── WindowAnimations.xaml                 # Window open/close animations
│   ├── Base/
│   │   ├── Colors.xaml                       # Primitive + semantic color tokens
│   │   ├── Radius.xaml                       # Corner radius tokens
│   │   ├── Spacing.xaml                      # Margin/padding/height tokens
│   │   └── Typography.xaml                   # Font size/weight tokens
│   ├── Controls/
│   │   ├── AccentSwatchStyles.xaml           # Accent color circle swatches
│   │   ├── ButtonStyles.xaml                 # Button variants (Primary, Ghost, etc.)
│   │   ├── CheckBoxStyles.xaml               # Fluent CheckBox
│   │   ├── ComboBoxItemStyles.xaml           # ComboBox item container
│   │   ├── ComboBoxStyles.xaml               # ComboBox with popup fix
│   │   ├── DataGridStyles.xaml               # DataGrid with virtualization
│   │   ├── GroupBoxStyles.xaml               # GroupBox styling
│   │   ├── LabelStyles.xaml                  # Label styling
│   │   ├── ListViewItemStyles.xaml           # ListView item container
│   │   ├── ListViewStyles.xaml               # ListView styling
│   │   ├── ProgressBarStyles.xaml            # Progress bar styling
│   │   ├── RadioButtonStyles.xaml            # Fluent RadioButton
│   │   ├── ScrollBarStyles.xaml              # ScrollBar styling
│   │   ├── ScrollViewerStyles.xaml           # Modern thin scrollviewer
│   │   ├── TextBoxStyles.xaml                # TextBox with states
│   │   ├── ThemeCardStyles.xaml              # Theme selection cards
│   │   ├── ToggleButtonStyles.xaml           # Toggle button with slide
│   │   ├── WindowButtonStyles.xaml           # Window close/minimize/maximize
│   │   └── WindowStyles.xaml                 # ModernWindow chrome styles
│   ├── Custom/
│   │   ├── CustomColors.xaml                 # Custom theme + 8 accent presets
│   │   └── CustomTheme.xaml                  # Custom merged dictionary
│   ├── Dark/
│   │   ├── DarkColors.xaml                   # Dark semantic token overrides
│   │   └── DarkTheme.xaml                    # Dark merged dictionary
│   ├── Effects/
│   │   ├── Animations.xaml                   # Storyboards (hover, focus, popup)
│   │   ├── Glow.xaml                         # Glow effects (focus, selection, accent)
│   │   └── Shadows.xaml                      # DropShadowEffect definitions
│   ├── Fluent/
│   │   ├── FluentEffects.xaml                # Legacy Fluent effects
│   │   └── FluentWhite.xaml                  # Legacy light theme
│   └── Light/
│       ├── LightColors.xaml                   # Light semantic token overrides
│       └── LightTheme.xaml                    # Light merged dictionary
├── UI/
│   ├── ProjectAnalysisWindow.xaml            # Project analysis window
│   └── ProjectAnalysisWindow.xaml.cs
├── UIHost/
│   └── WpfWindowManager.cs                   # Manage WPF windows in Excel host
├── ViewModels/
│   ├── FloatPathViewModel.cs                 # Float path analysis VM
│   ├── LinksManagerViewModel.cs              # Links manager VM
│   ├── ProjectAnalysisViewModel.cs           # Project analysis VM
│   └── Primavera/
│       ├── PrimaveraCompareViewModel.cs      # Primavera comparison VM
│       └── PrimaveraResultsViewModel.cs      # Comparison results VM
├── Views/
│   ├── SettingsWindow.xaml                   # Theme + accent settings UI
│   └── SettingsWindow.xaml.cs
├── Windows/
│   └── PrimaveraComparison/
│       ├── PrimaveraCompareWindow.xaml       # Compare configuration window
│       ├── PrimaveraCompareWindow.xaml.cs
│       ├── PrimaveraResultsWindow.xaml       # Comparison results display
│       └── PrimaveraResultsWindow.xaml.cs
├── App.config
├── App.xaml                                   # Application resources
├── AssignTradeCodesWindow.xaml                # Trade codes assignment UI
├── AssignTradeCodesWindow.xaml.cs
├── Fixpiecolors.xaml                          # Pie chart color fix window
├── Fixpiecolors.xaml.cs
├── Float_path.xaml                            # Float path analysis window
├── Float_path.xaml.cs
├── LinksManagerWindow.xaml                    # Links manager window
├── LinksManagerWindow.xaml.cs
├── MainWindow.xaml                            # Main window (comparison)
├── MainWindow.xaml.cs
├── packages.config
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

### 1.5. Standalone WPF Apps

```
Som3a_WPF_UI/                                 # Legacy standalone
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Som3a_WPF_UI.csproj
└── Som3a_WPF_UI.csproj.user

Som3a_WPF_UId/                                # Duplicate/experimental
├── Properties/AssemblyInfo.cs
├── Class1.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── Som3a_WPF_UI.csproj

WpfApp1/                                      # Older WPF UI project
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

### 1.6. Specification & Documentation Files

```
specs/
├── feature-planning-guide.md                  # ChatGPT feature planning guide
└── 001-fluent-theme-engine/
    ├── data-model.md                          # Entity definitions
    ├── plan.md                                # Implementation plan
    ├── quickstart.md                          # Developer onboarding
    ├── research.md                            # Technology research
    ├── spec.md                                # Feature specification
    ├── tasks.md                               # Task breakdown (58 tasks)
    ├── checklists/
    │   └── requirements.md                   # Specification quality checklist
    └── contracts/
        └── theme-api.md                      # ThemeManager service contract

Docs/
├── fix.md                                     # Fix documentation
├── PROJECT_GUIDE.md                           # Project guide (355 lines)
├── QUICK_REFERENCE.md                         # Quick reference
├── session_5b_settings.md                     # Session notes
├── session_prompts.md                         # Session prompts
├── session_prompts -2.md                      # Session prompts (copy)
└── SESSION_STATUS.md                          # Session status tracking

Git/
└── Uploadgit.md                               # Git upload instructions
```

### 1.7. Configuration & Workflow (`.opencode/`, `.specify/`)

```
.opencode/
└── plans/
    ├── README.md                              # Plan index (8 phases, 36 tasks)
    ├── phase-0-preflight-checklist.md
    ├── phase-1-combobox-refactor.md
    ├── phase-2-shadow-system.md
    ├── phase-3-theme-engine.md
    ├── phase-4-settings-ux.md
    ├── phase-5-control-standardization.md
    ├── phase-6-animation-system.md
    ├── phase-7-runtime-theme-manager.md
    ├── phase-8-advanced-polish.md
    └── gitignore.md

.specify/
├── extensions.yml
├── feature.json
├── init-options.json
├── integration.json
├── extensions/
│   ├── .registry
│   └── git/
│       ├── config-template.yml
│       ├── extension.yml
│       ├── git-config.yml
│       ├── README.md
│       └── commands/
│           ├── speckit.git.commit.md
│           ├── speckit.git.feature.md
│           ├── speckit.git.initialize.md
│           ├── speckit.git.remote.md
│           └── speckit.git.validate.md
│       └── scripts/
│           ├── bash/    (auto-commit.sh, create-new-feature.sh, etc.)
│           └── powershell/  (auto-commit.ps1, create-new-feature.ps1, etc.)
├── integrations/
│   ├── opencode.manifest.json
│   └── speckit.manifest.json
├── memory/
│   └── constitution.md                        # Architecture constitution
├── scripts/
│   └── powershell/
│       ├── check-prerequisites.ps1
│       ├── common.ps1
│       ├── create-new-feature.ps1
│       ├── setup-plan.ps1
│       └── setup-tasks.ps1
├── templates/
│   ├── checklist-template.md
│   ├── constitution-template.md
│   ├── plan-template.md
│   ├── spec-template.md
│   └── tasks-template.md
└── workflows/
    ├── workflow-registry.json
    └── speckit/
        └── workflow.yml
```

---

## Part 2: Files Organized by Function

### 2.1. Business Logic Services (Core)

| Service | Project | File |
|---------|---------|------|
| Assign Trade Codes | Som3a.Shared | `Core/AssignTradeCodesService.cs` |
| Excel Compare | Som3a.Shared | `Core/ExcelCompareService.cs` |
| Fix Pie Colors | Som3a.Shared | `Core/FixPieColorsService.cs` |
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

### 2.2. WPF Services

| Service | File | Description |
|---------|------|-------------|
| ThemeManager | `WpfApp2/Services/ThemeManager.cs` | Runtime theme switching singleton |
| WindowRenderModeDetector | `WpfApp2/Services/WindowRenderModeDetector.cs` | Auto-detect Excel fallback mode |
| DialogService | `WpfApp2/Services/DialogService.cs` | Show MessageBox dialogs |
| ToastService | `WpfApp2/Services/ToastService.cs` | Toast notifications |
| ThemeSettings | `WpfApp2/Services/ThemeSettings.cs` | Legacy JSON persistence |
| ExcelProjectAnalysisService | `WpfApp2/Services/ExcelProjectAnalysisService.cs` | Analyze Excel project data |

### 2.3. WPF Windows (by domain)

#### Primavera Comparison
| File | Type |
|------|------|
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml` | Window (View) |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraCompareWindow.xaml.cs` | Code-behind |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml` | Window (View) |
| `WpfApp2/Windows/PrimaveraComparison/PrimaveraResultsWindow.xaml.cs` | Code-behind |
| `WpfApp2/ViewModels/Primavera/PrimaveraCompareViewModel.cs` | ViewModel |
| `WpfApp2/ViewModels/Primavera/PrimaveraResultsViewModel.cs` | ViewModel |

#### Project Analysis
| File | Type |
|------|------|
| `WpfApp2/UI/ProjectAnalysisWindow.xaml` | Window (View) |
| `WpfApp2/UI/ProjectAnalysisWindow.xaml.cs` | Code-behind |
| `WpfApp2/ViewModels/ProjectAnalysisViewModel.cs` | ViewModel |

#### Settings
| File | Type |
|------|------|
| `WpfApp2/Views/SettingsWindow.xaml` | Window (ModernWindow) |
| `WpfApp2/Views/SettingsWindow.xaml.cs` | Code-behind |

#### Float Path
| File | Type |
|------|------|
| `WpfApp2/Float_path.xaml` | Window |
| `WpfApp2/Float_path.xaml.cs` | Code-behind |
| `WpfApp2/ViewModels/FloatPathViewModel.cs` | ViewModel |

#### Links Manager
| File | Type |
|------|------|
| `WpfApp2/LinksManagerWindow.xaml` | Window |
| `WpfApp2/LinksManagerWindow.xaml.cs` | Code-behind |
| `WpfApp2/ViewModels/LinksManagerViewModel.cs` | ViewModel |

#### XER Editor
| File | Type |
|------|------|
| `WpfApp2/XerEditorWindow.xaml` | Window |
| `WpfApp2/XerEditorWindow.xaml.cs` | Code-behind |
| `WpfApp2/Models/XerEditorVM.cs` | ViewModel (mislocated) |

#### Trade Codes
| File | Type |
|------|------|
| `WpfApp2/AssignTradeCodesWindow.xaml` | Window |
| `WpfApp2/AssignTradeCodesWindow.xaml.cs` | Code-behind |

#### Daily Report
| File | Type |
|------|------|
| `WpfApp2/SubDailyReportWindow.xaml` | Window |
| `WpfApp2/SubDailyReportWindow.xaml.cs` | Code-behind |
| `WpfApp2/Models/SubDailyReportViewModel.cs` | ViewModel (mislocated) |

#### Unmerge Fill Down
| File | Type |
|------|------|
| `WpfApp2/UnmergeFillDownWindow.xaml` | Window |
| `WpfApp2/UnmergeFillDownWindow.xaml.cs` | Code-behind |
| `WpfApp2/Models/UnmergeFillDownViewModel.cs` | ViewModel (mislocated) |

#### Fix Pie Colors
| File | Type |
|------|------|
| `WpfApp2/Fixpiecolors.xaml` | Window |
| `WpfApp2/Fixpiecolors.xaml.cs` | Code-behind |

#### Style Selector
| File | Type |
|------|------|
| `WpfApp2/StyleSelectorWindow.xaml` | Window |
| `WpfApp2/StyleSelectorWindow.xaml.cs` | Code-behind |

### 2.4. Theme System (Fluent Design Tokens)

| Layer | Files | Purpose |
|-------|-------|---------|
| **Base Tokens** | `Theme/Base/Colors.xaml`, `Typography.xaml`, `Spacing.xaml`, `Radius.xaml` | Primitive values + semantic aliases |
| **Effects** | `Theme/Effects/Shadows.xaml`, `Glow.xaml`, `Animations.xaml` | DropShadow, glow, storyboards |
| **Dark Theme** | `Theme/Dark/DarkColors.xaml`, `DarkTheme.xaml` | Dark theme overrides |
| **Light Theme** | `Theme/Light/LightColors.xaml`, `LightTheme.xaml` | Light theme overrides |
| **Custom Theme** | `Theme/Custom/CustomColors.xaml`, `CustomTheme.xaml` | Custom + 8 accent swatches |
| **Control Styles** | `Theme/Controls/*.xaml` (18 files) | All control templates |
| **Aggregator** | `Theme/ThemeResources.xaml` | Ordered merge dictionary |
| **Aggregator** | `Theme/ModernWindow.xaml` | ModernWindow chrome template |
| **Window Anim** | `Theme/WindowAnimations.xaml` | Window transitions |

### 2.5. Converters

| Converter | File | Function |
|-----------|------|----------|
| Bool→Visibility | `WpfApp2/Converters/SharedConverters.cs` | BoolToVisibility, NullToVisibility |
| Diff Type→Color | `WpfApp2/Converters/DifferenceTypeToColorConverter.cs` | Added=Green, Deleted=Red, Modified=Yellow |
| Window Converters | `WpfApp2/Converters/WindowConverters.cs` | Window state sizing |

### 2.6. Behaviors

| Behavior | File | Function |
|----------|------|----------|
| Drag Move | `WpfApp2/Behaviors/DragMoveBehavior.cs` | Drag window on header |
| Escape Close | `WpfApp2/Behaviors/EscapeCloseBehavior.cs` | Close on Escape key |

### 2.7. Helpers

| Helper | File | Function |
|--------|------|----------|
| AsyncRelayCommand | `WpfApp2/Helpers/AsyncRelayCommand.cs` | Async command with cancellation |
| DpiHelper | `WpfApp2/Helpers/DpiHelper.cs` | Monitor DPI detection |
| ExcelOwnerHelper | `WpfApp2/Helpers/ExcelOwnerHelper.cs` | Set WPF owner window from Excel handle |
| NotifyBase | `WpfApp2/Helpers/NotifyBase.cs` | INotifyPropertyChanged base class |
| RelayCommand | `WpfApp2/Helpers/RelayCommand.cs` | ICommand implementation |
| WindowBehaviorHelper | `WpfApp2/Helpers/WindowBehaviorHelper.cs` | Window property reflection |
| WindowChromeHelper | `WpfApp2/Helpers/WindowChromeHelper.cs` | Apply custom chrome |
| WindowValidationHelper | `WpfApp2/Helpers/WindowValidationHelper.cs` | Validate window properties |
| ComRelease | `Som3a.Shared/Interop/ComRelease.cs` | Safe COM cleanup |
| RelUtils | `Som3a.Shared/Utils/RelUtils.cs` | Relationship parsing |

### 2.8. Models/DTOs

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

### 2.9. VSTO/Excel Integration

| File | Project | Function |
|------|---------|----------|
| `ThisAddIn.cs` | Som3a Addin 2026 | VSTO startup |
| `Ribbon1.cs` | Som3a Addin 2026 | Ribbon buttons |
| `Som3aAddinBridge.cs` | Som3a Addin 2026 | Add-in bridge |
| `ExcelWindowHandle.cs` | Som3a Addin 2026 | Get Excel HWND |
| `WpfDialogHost.cs` | Som3a Addin 2026 | Host WPF as Excel dialog |
| `WpfWindowManager.cs` | WpfApp2 | Window lifecycle in Excel |
| `ExcelOwnerHelper.cs` | WpfApp2 | Parent WPF to Excel window |
| `ExcelService.cs` | Som3a.Shared | Excel object model helpers |

---

## Part 3: Files by Feature Domain

### 3.1. Primavera Comparison (complete feature)

```
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

WpfApp2/ViewModels/Primavera/
├── PrimaveraCompareViewModel.cs                 # ViewModel
└── PrimaveraResultsViewModel.cs                 # ViewModel

WpfApp2/Converters/
└── DifferenceTypeToColorConverter.cs            # Converter

Som3a Addin 2026/Ribbon1.cs                      # Launch from Excel
```

### 3.2. Theme Engine (complete feature)

```
WpfApp2/Theme/
├── Base/
│   ├── Colors.xaml                # Primitive + semantic token definitions
│   ├── Radius.xaml                # Corner radius tokens
│   ├── Spacing.xaml               # Layout spacing tokens
│   └── Typography.xaml            # Font tokens
├── Dark/
│   ├── DarkColors.xaml            # Dark semantic overrides
│   └── DarkTheme.xaml             # Dark merged dictionary
├── Light/
│   ├── LightColors.xaml           # Light semantic overrides
│   └── LightTheme.xaml            # Light merged dictionary
├── Custom/
│   ├── CustomColors.xaml          # Custom + 8 accent presets
│   └── CustomTheme.xaml           # Custom merged dictionary
├── Controls/                      # 18 control style files
├── Effects/
│   ├── Shadows.xaml               # Centralized DropShadowEffect
│   ├── Glow.xaml                  # Centralized glow effects
│   └── Animations.xaml            # Storyboards (≤200ms)
├── ThemeResources.xaml            # Aggregator
├── ModernWindow.xaml              # Chrome template
└── WindowAnimations.xaml          # Window transitions

WpfApp2/Services/
├── ThemeManager.cs                # Singleton runtime switcher
├── ThemeSettings.cs               # Legacy JSON persistence
└── WindowRenderModeDetector.cs    # Excel fallback detection

WpfApp2/Properties/
├── Settings.settings              # User-scoped theme + accent
└── Settings.Designer.cs           # Accessor

WpfApp2/Controls/
└── ModernWindow.cs                # Base window with theme integration

WpfApp2/Helpers/
├── WindowChromeHelper.cs          # Apply WindowChrome
└── DpiHelper.cs                   # DPI detection

WpfApp2/Views/
├── SettingsWindow.xaml            # Theme card UI
└── SettingsWindow.xaml.cs
```

### 3.3. Toast Notification System

```
WpfApp2/Controls/Toast/
├── ToastModel.cs                  # Data model
├── ToastWindow.xaml               # Toast popup UI
└── ToastWindow.xaml.cs            # Code-behind

WpfApp2/Services/
└── ToastService.cs                # Show/hide toasts
```

### 3.4. Loading Overlay

```
WpfApp2/Controls/
├── LoadingOverlay.xaml            # Spinner overlay UI
└── LoadingOverlay.xaml.cs         # Code-behind
```

### 3.5. XER File Handling

```
Som3a.Shared/Core/
├── XER Parser.cs                  # Parse XER format
└── XerExportService.cs            # Export to XER

WpfApp2/
├── XerEditorWindow.xaml           # Editor UI
├── XerEditorWindow.xaml.cs
└── Models/XerEditorVM.cs          # ViewModel
```

### 3.6. Float Path / Critical Path

```
Som3a.Shared/Core/
├── FloatPathService.cs            # Float path calculation
├── PathFinder.cs                  # Critical path search
├── GraphBuilder.cs                # Network graph construction
└── GraphService.cs                # Graph algorithms

Som3a.Shared/Models/
└── Float_path.cs                  # Data model

WpfApp2/
├── Float_path.xaml                # Analysis UI
├── Float_path.xaml.cs
├── ViewModels/FloatPathViewModel.cs
```

### 3.7. WBS (Work Breakdown Structure)

```
Som3a.Shared/Core/
├── WbsBuilder.cs                  # Build WBS hierarchy
├── WbsStyleFactory.cs             # Cell style factory
└── WbsColoringService.cs          # Color by WBS level

Som3a.Shared/Models/
├── WbsItem.cs                     # WBS item
└── WbsLevelStyle.cs               # Level style definition
```

---

## Part 4: Work Breakdown Structure (WBS)

### WBS Level 1: Solution

```
1.0  Som3a Add-in 2026                   # Complete VSTO Excel Add-in
```

### WBS Level 2: Projects

```
1.1  Som3a Addin 2026 (VSTO Host)        # Excel add-in project
1.2  Som3a.Shared (Business Logic)       # Shared services & models
1.3  WpfApp2 (WPF UI)                    # Main WPF UI library
```

### WBS Level 3: Feature Domains

```
1.1  VSTO Host (Excel Integration)
     1.1.1  Ribbon UI                    # Ribbon1.cs, buttons
     1.1.2  Add-in Bridge                # ThisAddIn.cs, bridge
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
     1.3.3  Feature Windows              # All WPF windows
     1.3.4  Services                     # ThemeManager, Toast, Dialog
     1.3.5  Infrastructure               # Helpers, Converters, VMs
```

### WBS Level 4: Detailed Work Packages

```
1.1.1  Ribbon UI
      1.1.1.1  Ribbon tabs & groups      # Ribbon1.Designer.cs
      1.1.1.2  Button click handlers      # Ribbon1.cs (10+ buttons)
      1.1.1.3  Ribbon images & icons      # Resources/*.png

1.1.2  Add-in Bridge
      1.1.2.1  ThisAddIn startup          # ThisAddIn.cs
      1.1.2.2  Add-in shutdown cleanup    # ThisAddIn.Designer.cs
      1.1.2.3  Bridge to WPF layer        # Som3aAddinBridge.cs

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
      1.3.1.1  Base design tokens         # Colors, Typography, Spacing, Radius
      1.3.1.2  Theme variants             # Dark, Light, Custom (6 files)
      1.3.1.3  Effects library            # Shadows, Glow, Animations
      1.3.1.4  Control styles             # 18 style XAML files
      1.3.1.5  Aggregator                 # ThemeResources.xaml
      1.3.1.6  ThemeManager service       # ThemeManager.cs
      1.3.1.7  Theme persistence          # Settings.settings, Settings.Designer.cs
      1.3.1.8  Fallback detection         # WindowRenderModeDetector.cs
      1.3.1.9  Window chrome              # ModernWindow.cs, WindowChromeHelper.cs
      1.3.1.10 DPI support                # DpiHelper.cs

1.3.2  Control Library
      1.3.2.1  ModernWindow               # ModernWindow.cs + ModernWindow.xaml
      1.3.2.2  LoadingOverlay             # LoadingOverlay.xaml + .xaml.cs
      1.3.2.3  Toast system               # ToastModel.cs, ToastWindow.xaml/.cs
      1.3.2.4  DragMove behavior          # DragMoveBehavior.cs
      1.3.2.5  EscapeClose behavior       # EscapeCloseBehavior.cs

1.3.3  Feature Windows
      1.3.3.1  MainWindow                 # Main (comparison hub)
      1.3.3.2  SettingsWindow             # Theme + accent settings
      1.3.3.3  PrimaveraCompareWindow     # Project comparison config
      1.3.3.4  PrimaveraResultsWindow     # Comparison results display
      1.3.3.5  ProjectAnalysisWindow      # Excel project analysis
      1.3.3.6  XerEditorWindow            # XER file editing
      1.3.3.7  AssignTradeCodesWindow     # Trade code assignment
      1.3.3.8  LinksManagerWindow         # Workbook links
      1.3.3.9  SubDailyReportWindow       # Subcontractor reports
      1.3.3.10 Float_path                 # Float path analysis
      1.3.3.11 UnmergeFillDownWindow      # Unmerge + fill down
      1.3.3.12 Fixpiecolors               # Pie chart color fix
      1.3.3.13 StyleSelectorWindow        # Style selection

1.3.4  Services
      1.3.4.1  ThemeManager              # Theme switching singleton
      1.3.4.2  ToastService              # Toast notifications
      1.3.4.3  DialogService             # MessageBox dialogs
      1.3.4.4  ThemeSettings             # Legacy JSON persistence

1.3.5  Infrastructure
      1.3.5.1  ViewModel base            # NotifyBase.cs
      1.3.5.2  Relay commands            # RelayCommand.cs, AsyncRelayCommand.cs
      1.3.5.3  Value converters          # SharedConverters.cs, WindowConverters.cs
      1.3.5.4  Window helpers            # WindowBehaviorHelper.cs, WindowValidationHelper.cs
      1.3.5.5  ViewModels                # 5 ViewModels (all feature VMs)
```

### WBS Format: Numeric Code Key

```
WBS Format:  X.Y.Z.W
  X = Project (1=Host, 2=Shared, 3=WPF UI)
  Y = Domain (1-6)
  Z = Work Package (1-10)
  W = Task (optional, 1-5)

Examples:
  1.3.2.1  = VSTO Host > Dialog Hosting > WPF dialog in Excel > WpfDialogHost.cs
  2.1.3.2  = Shared > Primavera > Comparison service > CompareActivities method  
  3.1.4.3  = WPF UI > Theme Engine > Control styles > ComboBoxStyles.xaml
```

---

## Part 5: ChatGPT Prompt Template

Copy this entire block when you need ChatGPT to work with the codebase:

```
## Project Context

You are working on **Som3a Addin 2026**, a professional Excel VSTO Add-in built with:
- **C# 8.0 / .NET Framework 4.8**
- **WPF** for UI (ModernWindow, WindowChrome, ResourceDictionaries)
- **VSTO** for Excel integration (Ribbon1.cs, ThisAddIn.cs)
- **CommunityToolkit.Mvvm** for MVVM
- **Oracle/SQL Server** for Primavera P6 database access

### Solution Structure (3 main projects)
1. **Som3a Addin 2026/** — Excel host (Ribbon, Bridge, Dialog Hosting)
2. **Som3a.Shared/** — Business logic (Primavera services, XER, Excel, WBS, Graph)
3. **WpfApp2/** — WPF UI (all windows, ViewModels, Theme system, Services)

### WpfApp2 Architecture (primary UI project)
```
WpfApp2/
├── Theme/          # Fluent Design system (Base/, Dark/, Light/, Custom/, Controls/, Effects/)
├── Services/       # ThemeManager, ToastService, DialogService
├── Controls/       # ModernWindow, LoadingOverlay, Toast
├── ViewModels/     # MVVM (Primavera/, FloatPath, LinksManager, ProjectAnalysis)
├── Views/          # SettingsWindow
├── Windows/        # PrimaveraCompareWindow, PrimaveraResultsWindow
├── Helpers/        # DpiHelper, WindowChromeHelper, RelayCommand, NotifyBase
├── Converters/     # SharedConverters, DifferenceTypeToColorConverter
├── Behaviors/      # DragMoveBehavior, EscapeCloseBehavior
└── [root]          # MainWindow, XerEditorWindow, AssignTradeCodesWindow, ...
```

### Key Architecture Rules
1. **DynamicResource only** for themeable properties (no StaticResource)
2. **No inline DropShadowEffect** — use `{DynamicResource Shadow.*}` from Effects/Shadows.xaml
3. **No third-party UI frameworks** — native WPF only
4. **MVVM** — minimal code-behind, business logic in ViewModels
5. **WindowChrome** preferred, auto-fallback to AllowsTransparency

### Build Command
```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

### Full File Tree Reference
[Include relevant sections from spec/guide-files-path-and-wbs.md]
```

---

## Part 6: Quick Reference Tables

### 6.1. File Count Summary

| Project | Source Files | XAML Files | Config Files | Total |
|---------|-------------|------------|--------------|-------|
| Som3a Addin 2026 | 12 | 0 | 5 | 17 |
| Som3a.Shared | 35 | 0 | 2 | 37 |
| WpfApp2 | 53 | 32 | 3 | 88 |
| WpfApp1 (legacy) | 3 | 1 | 2 | 6 |
| Som3a_WPF_UI (legacy) | 1 | 1 | 1 | 3 |
| Som3a_WPF_UId (legacy) | 2 | 1 | 0 | 3 |
| Specs/Docs | 0 | 0 | 15 | 15 |
| Config/Workflow | 0 | 0 | 30+ | 30+ |
| **Total** | **~106** | **~35** | **~58** | **~200** |

### 6.2. Project Dependencies

```
Som3a Addin 2026 (VSTO)
  └── references WpfApp2 (WPF UI) + Som3a.Shared (Business Logic)

WpfApp2 (WPF UI)
  └── references Som3a.Shared (Business Logic)

Som3a.Shared (Business Logic)
  └── no internal references (leaf project)
```

### 6.3. WBS Task Estimation

| WBS Code | Work Package | Est. Effort | Dependencies |
|----------|-------------|-------------|--------------|
| 1.1.1 | Ribbon UI | 2d | None |
| 1.1.2 | Add-in Bridge | 1d | None |
| 1.1.3 | Dialog Hosting | 2d | None |
| 1.1.4 | Excel Automation | 3d | None |
| 1.2.1 | Primavera Services | 10d | None |
| 1.2.2 | XER Services | 5d | None |
| 1.2.3 | Excel Services | 5d | None |
| 1.2.4 | WBS Services | 3d | None |
| 1.2.5 | Schedule Analysis | 5d | 1.2.1 |
| 1.2.6 | Utility Services | 3d | None |
| 1.3.1 | Theme Engine | 15d | None |
| 1.3.2 | Control Library | 3d | None |
| 1.3.3 | Feature Windows | 15d | 1.3.1, 1.3.2 |
| 1.3.4 | Services | 3d | None |
| 1.3.5 | Infrastructure | 5d | None |
| | **Total Estimated** | **~80 days** | |

---

*This document is a living reference. Update when new files are added or the project structure changes.*
