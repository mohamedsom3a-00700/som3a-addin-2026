<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at: specs/031-ctk-mvvm-refactor/plan.md

Also refer to the master implementation plans:
- future-plan-fluent-ui-migration.md — Fluent UI & Modern Platform Migration (Phases 1A-12)
- implementation_plan.md — Full execution plan for Phases 0-11
- enterprise_planning_platform_plan.md — Enterprise Planning Platform Phases 14-27

Current phase plan:
- specs/031-ctk-mvvm-refactor/plan.md — Phase 2: CommunityToolkit.Mvvm ViewModel Refactor
<!-- SPECKIT END -->

# Som3a Add-in 2026 — Implementation Notes

## Theme Engine (Feature: 001-fluent-theme-engine)

### Architecture

```
WpfApp2/Theme/
├── Base/
│   ├── Colors.xaml              # Primitive + Semantic tokens (default dark)
│   ├── Typography.xaml
│   ├── Spacing.xaml             # ThemeCardWidth/Height, AccentSwatchSize tokens
│   └── Radius.xaml
├── Dark/
│   ├── DarkColors.xaml          # Semantic token overrides
│   └── DarkTheme.xaml           # Merged dictionary
├── Light/
│   ├── LightColors.xaml
│   └── LightTheme.xaml
├── Custom/
│   ├── CustomColors.xaml         # 8 accent swatch presets
│   └── CustomTheme.xaml
├── Controls/
│   ├── ButtonStyles.xaml
│   ├── ComboBoxStyles.xaml       # Popup: AllowsTransparency=False + PlacementTarget
│   ├── DataGridStyles.xaml       # EnableRowVirtualization=True
│   ├── CheckBoxStyles.xaml        # New
│   ├── RadioButtonStyles.xaml    # New
│   ├── ToggleButtonStyles.xaml   # New
│   ├── ScrollViewerStyles.xaml   # New
│   ├── ThemeCardStyles.xaml      # New
│   ├── AccentSwatchStyles.xaml   # New
│   └── ... (existing)
├── Effects/
│   ├── Shadows.xaml              # Centralized DropShadowEffect definitions
│   ├── Glow.xaml                # DynamicResource AccentColorValue
│   └── Animations.xaml           # Storyboards (all ≤200ms)
└── ThemeResources.xaml          # Aggregator with documented loading order
```

### Key Conventions

- **DynamicResource only** — all theme colors use `{DynamicResource Brush.*}`, no StaticResource for themeable properties
- **No inline DropShadowEffect** — use `{DynamicResource Shadow.*}` from Effects/Shadows.xaml
- **Glow color is dynamic** — Glow.xaml uses `Color="{DynamicResource AccentColorValue}"`, updated by ThemeManager
- **AccentColorValue Color resource** — set in CustomColors.xaml, updated at runtime by ThemeManager.ApplyAccentColor()
- **WindowRenderModeDetector** — auto-detects Excel VSTO hosting and activates FallbackSafe mode

### Services

- `Services/ThemeManager.cs` — singleton, ApplyTheme(string, string), LoadThemeFromSettings(), SaveCurrentTheme(), ThemeChanged event
- `Services/WindowRenderModeDetector.cs` — detects VSTO/DPI/rendering issues, returns FallbackSafe or WindowChrome mode
- `Services/ThemeSettings.cs` — legacy persistence (JSON file), still used by some parts
- `Controls/ModernWindow.cs` — calls LoadThemeFromSettings() on init, uses WindowRenderModeDetector

### Settings Persistence

- Primary: `Properties/Settings.settings` (ApplicationSettingsBase) — SelectedTheme + AccentColor
- Secondary: `Services/ThemeSettings.cs` (JSON file at AppData/Som3a/theme.json)

### Theme Switching Flow

1. User clicks theme card → `ThemeManager.Instance.ApplyTheme("Dark")`
2. ThemeManager finds + removes existing theme dictionary from MergedDictionaries
3. Creates new theme dictionary from DarkTheme.xaml (loads in try/catch first)
4. Adds new theme dictionary to MergedDictionaries
5. If accentColor provided → `ApplyAccentColor()` → updates AccentColorValue + AccentColorBrush + glow effects
6. Fires `ThemeChanged` event
7. SettingsWindow listens → updates card/swatches selection

### Control Templates

- ComboBox popup: `AllowsTransparency="False"`, `Placement="Bottom"`, `PlacementTarget` binding
- DataGrid: `EnableRowVirtualization="True"`, `VirtualizationMode="Recycling"`
- All controls: `SnapsToDevicePixels="True"`, `UseLayoutRounding="True"`
- No BlurEffect on any scrollable container

### Governance Documents

```text
Docs/Architecture/
├── UI_GUIDELINES.md
├── TOKEN_RULES.md
├── POPUP_ARCHITECTURE.md
├── SHADOW_SYSTEM.md
├── ACCESSIBILITY_RULES.md
├── EXCEL_RENDERING_RULES.md
├── MVVM_RULES.md
├── REVIEW_CHECKLIST.md
├── AGENT_RULES.md
├── PERFORMANCE_RULES.md
├── AUDIT_REPORT.md
├── EXCEL_TEST_CHECKLIST.md
├── MVVM_COMPLIANCE.md
├── PERFORMANCE_AUDIT_REPORT.md      # Phase 10: baseline + optimizations
├── ACCESSIBILITY_AUDIT_REPORT.md    # Phase 10: keyboard nav + contrast
├── DPI_AUDIT_REPORT.md              # Phase 10: per-level + multi-monitor
├── EXCEL_STABILITY_REPORT.md        # Phase 10: VSTO stability results
├── LOCALIZATION_READINESS.md        # Phase 10: i18n architecture
└── ENTERPRISE_POLISH_CHECKLIST.md   # Phase 10: master validation
```

### Constitution

- `.specify/memory/constitution.md` — Current active constitution (v1.2.0)
- `.specify/memory/constitution-v2.md` — Next-generation constitution (v2.0.0)

## Architecture Cleanup (Feature: 009-mvvm-architecture-cleanup)

### Service Container

- `Services/ServiceContainer.cs` — `IServiceContainer`, `ServiceContainer`, `IServiceScope`, `ServiceScope`
- Singleton: `App.Container` (static), registered in `CompositionRoot.RegisterServices()`
- Lifetimes: `RegisterSingleton<T>`, `RegisterTransient<T>`, `RegisterScoped<T>`
- Resolution: `Resolve<T>()`, `Resolve(Type)`, `CreateScope()`
- Error handling: Circular dependency detection (throws `InvalidOperationException` with chain), unregistered service detection (throws `InvalidOperationException` with type name)
- Diagnostics: `ServiceResolved` and `ServiceRegistered` events

### Event Bus

- `Services/EventBus.cs` — `IEventBus`, `EventBus`, `SubscriptionToken`
- Typed events: `Publish<TEvent>(TEvent)`, `Subscribe<TEvent>(Action<TEvent>)`, `Subscribe<TEvent>(Action<TEvent>, Func<TEvent, bool>)`
- Weak reference subscriber storage — dead subscribers auto-pruned on next publish
- Subscriber isolation — one subscriber's exception doesn't block others; errors reported via `SubscriberError` event
- Diagnostics: `EventPublished`, `EventSubscribed`, `SubscriberError` events

### Module Registry

- `Services/ModuleRegistry.cs` — `IModule`, `IModuleRegistry`, `ModuleRegistry`
- Modules self-register services and event subscriptions in `Initialize(IServiceContainer, IEventBus)`
- Priority-based initialization order (lower = first)
- Duplicate `ModuleId` throws `InvalidOperationException`
- One module failure doesn't block remaining modules

### MVVM ViewModels

- `ViewModels/ViewModelBase.cs` — base class with `INotifyPropertyChanged`, `SetProperty<T>` helper
- ViewModels relocated to `ViewModels/` directory with `Som3a_WPF_UI.ViewModels` namespace
- Constructor injection via `IServiceContainer` for all service dependencies
- Commands (`ICommand`) for user actions instead of code-behind event handlers

### Composition Root

- `CompositionRoot.cs` — centralized registration point called from `App.xaml.cs`
- `RegisterServices(IServiceContainer)` — registers all core services
- `InitializeModules(IModuleRegistry)` — initializes all modules at startup

### Build

WpfApp2 (SDK-style, .NET 8.0):
```powershell
dotnet build WpfApp2\Som3a_WPF_UI.csproj -p:Configuration=Debug
```

Som3a.Shared (.NET Framework 4.8, COM interop — requires full MSBuild):
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Som3a.Shared\Som3a.Shared.csproj /p:Configuration=Debug
```

WpfApp2 with full MSBuild (resolves all dependency projects):
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

Run tests:
```powershell
dotnet test Tests\Som3a_WPF_UI.Tests.csproj
dotnet test Tests\Som3a.Infrastructure.Tests\Som3a.Infrastructure.Tests.csproj
```

## Platform Foundation (Phase 14)

### Build Commands

Build all .NET 8.0 libraries:
```powershell
dotnet build Som3a.Bridge/Som3a.Bridge.csproj
dotnet build Som3a.Contracts/Som3a.Contracts.csproj
dotnet build Som3a.Domain/Som3a.Domain.csproj
dotnet build Som3a.Plugin.SDK/Som3a.Plugin.SDK.csproj
dotnet build Som3a.AI/Som3a.AI.csproj
dotnet build Som3a.Exporting/Som3a.Exporting.csproj
dotnet build Som3a.Localization/Som3a.Localization.csproj
dotnet build Som3a.Validation/Som3a.Validation.csproj
dotnet build Som3a.Diagnostics/Som3a.Diagnostics.csproj
dotnet build Som3a.Infrastructure/Som3a.Infrastructure.csproj
```

Build WPF host with Bridge reference:
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

### Project Dependency Graph

```
text
Som3a.Contracts (no dependencies)
    |
Som3a.Domain → Som3a.Contracts
Som3a.Plugin.SDK → Som3a.Contracts
Som3a.AI → Som3a.Contracts, Som3a.Domain
Som3a.Exporting → Som3a.Contracts, Som3a.Domain
Som3a.Validation → Som3a.Contracts, Som3a.Domain
Som3a.Diagnostics → Som3a.Contracts, Som3a.Plugin.SDK
Som3a.Infrastructure → Som3a.Contracts
Som3a.Localization → Som3a.Contracts
Som3a.Bridge (.NET Standard 2.0) — standalone
WpfApp2 (.NET 8.0-windows) → Som3a.Bridge (.NET Standard 2.0)
```

### Project Structure (Phase 14)

```text
Som3a Addin 2026/
├── Som3a.Bridge/            # .NET Standard 2.0 interop bridge
│   ├── InteropContracts.cs
│   └── DiagnosticsChannel.cs
├── Som3a.Contracts/         # .NET 8.0 contract interfaces
│   ├── IPlugin.cs
│   ├── IAIProvider.cs
│   ├── IExportEngine.cs
│   ├── ISettingsModule.cs
│   ├── IPromptProvider.cs
│   └── IDiagnosticsProvider.cs
├── Som3a.Domain/            # .NET 8.0 domain entities
│   ├── BOQ/
│   ├── Activities/
│   ├── WBS/
│   ├── Relationships/
│   ├── Calendars/
│   ├── Constraints/
│   ├── Resources/
│   ├── Export/
│   └── Serialization/
├── Som3a.Plugin.SDK/        # .NET 8.0 plugin framework
│   ├── Attributes/
│   ├── Discovery/
│   ├── Validation/
│   └── Hosting/
├── Som3a.AI/                # .NET 8.0 AI abstraction
│   ├── Providers/
│   ├── Orchestration/
│   ├── Prompts/
│   ├── Parsing/
│   └── Tracking/
├── Som3a.Exporting/         # .NET 8.0 export engine
│   ├── Pipeline/
│   ├── Excel/
│   ├── Primavera/
│   └── Formats/
├── Som3a.Localization/      # .NET 8.0 i18n
├── Som3a.Validation/        # .NET 8.0 validation
├── Som3a.Diagnostics/       # .NET 8.0 diagnostics
├── Som3a.Infrastructure/    # .NET 8.0 security/config
└── WpfApp2/                 # .NET 8.0-windows WPF host
```
