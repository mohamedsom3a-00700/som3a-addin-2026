# Future Plan: Fluent UI & Modern MVVM Migration

**Status**: Planned (not started)
**Target Phase**: Post Phase 26 (Release Candidate)

## Scope

Migrate ribbon icons, control styles, and MVVM infrastructure to modern
.NET libraries for improved maintainability, icon consistency, and
developer ergonomics.

## Components

### 1. Ribbon Icons — Fluent UI Icon Set
- Replace all existing ribbon icons (T025) with **Fluent UI System Icons**
- Available formats: SVG, PNG — at 24x24 and 32x32 for ribbon toolbar
- Source: https://github.com/microsoft/fluentui-system-icons
- Placement: `WpfApp2/Assets/Branding/Ribbon/`
- Must support dark/light theme compatibility

### 2. Fluent Icons NuGet Package
- Package: `FluentIcons.WPF` or `FluentIcons.FluentSystemIcons`
- Provides reusable `FluentIcon` control analogous to `materialDesign:PackIcon`
- Onyx/Mono-color variant for toolbar use

### 3. WPF UI Library (Modern Controls)
- Package: `WPF-UI` (https://github.com/lepoco/wpfui)
- Evaluate for replacing custom controls (e.g., `ModernWindow`, sidebar)
- Provides Fluent design language control templates natively
- Potential to reduce custom theme maintenance burden

### 4. CommunityToolkit.Mvvm
- Package: `CommunityToolkit.Mvvm`
- Replace manual `INotifyPropertyChanged`, `RelayCommand`, `ICommand` implementations
- Use `[ObservableProperty]`, `[RelayCommand]` source generators
- Apply to `ViewModelBase`, `SettingsViewModel`, all page VMs
- Reduces boilerplate and improves nullability handling

## Migration Order

1. **Phase A**: Add `CommunityToolkit.Mvvm` — refactor ViewModels with source generators
2. **Phase B**: Add `FluentIcons.WPF` — replace ribbon icons + sidebar icons
3. **Phase C**: Evaluate `WPF-UI` — pilot on a single page, then roll out
4. **Phase D**: Remove `MaterialDesignThemes` dependency (if WPF-UI fully replaces it)

## Risks

- MaterialDesignThemes is deeply integrated (PackIcon, MaterialDesign controls, theme bridge)
- WPF-UI may conflict with existing `ModernWindow` Pattern
- CommunityToolkit.Mvvm source generators require C# 10+ / .NET SDK (already on LangVersion 14.0 — safe)
- Fluent icons need dark/light variant testing in VSTO host

## Dependencies

- .NET Framework 4.8 compatibility of all NuGet packages
- WPF-UI supports .NET Framework 4.8 (verify before proceeding)
- CommunityToolkit.Mvvm 8.x supports .NET Framework 4.8 via Microsoft.Bcl.AsyncInterfaces
