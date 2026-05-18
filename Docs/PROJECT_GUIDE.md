# Som3a Addin 2026 - Project Guide

## Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Design Tokens System](#design-tokens-system)
5. [Control Styles](#control-styles)
6. [Windows & Views](#windows--views)
7. [Services & ViewModels](#services--viewmodels)
8. [Build & Run](#build--run)

---

## Project Overview

**Som3a Addin 2026** is a WPF desktop application designed as an Excel Add-in (VSTO) for project management tasks. It provides UI tools for comparing P6 activities, managing trade codes, analyzing XER files, and more.

**Framework:** WPF (.NET Framework 4.8 + .NET 10.0)
**IDE:** Visual Studio 2026
**Architecture:** MVVM Pattern
**Last Updated:** May 2026

---

## Architecture

### Layer Structure

```
┌─────────────────────────────────────────┐
│           WPF Windows (Views)            │
├─────────────────────────────────────────┤
│           ViewModels (MVVM)             │
├─────────────────────────────────────────┤
│    Services (Business Logic)            │
├─────────────────────────────────────────┤
│    Models (Data Structures)             │
├─────────────────────────────────────────┤
│    Theme System (Design Tokens)         │
└─────────────────────────────────────────┘
```

### Key Principles

- **Design Tokens**: All colors, sizes, and styles defined in XAML ResourceDictionaries
- **DynamicResource**: Used for runtime theme changes (vs StaticResource for compile-time)
- **Global Styles**: Control styles centralized in Theme/Controls/
- **No Hardcoded Colors**: All colors come from the token system

---

## Project Structure

```
Som3a Addin 2026/
├── WpfApp2/                          # Main WPF Application
│   ├── App.xaml                      # Application resources & theme loading
│   ├── MainWindow.xaml               # Main comparison window
│   ├── Controls/                     # Custom controls
│   │   ├── ModernWindow.cs           # Base window class with custom chrome
│   │   ├── LoadingOverlay.xaml       # Loading indicator
│   │   └── Toast/                    # Toast notifications
│   ├── Converters/                   # Value converters
│   │   └── SharedConverters.cs       # Shared converters
│   ├── Helpers/                      # Helper classes
│   │   ├── RelayCommand.cs           # ICommand implementation
│   │   └── NotifyBase.cs             # INotifyPropertyChanged base
│   ├── Models/                       # Data models
│   ├── Services/                     # Business logic
│   │   ├── ThemeManager.cs           # Theme switching & accent color
│   │   ├── ToastService.cs           # Toast notifications
│   │   └── ThemeSettings.cs          # Theme configuration
│   ├── Theme/                        # Theme system
│   │   ├── Base/                     # Design tokens
│   │   │   ├── Colors.xaml           # Color palette
│   │   │   ├── Typography.xaml       # Font sizes & weights
│   │   │   ├── Spacing.xaml          # Margins, padding, heights
│   │   │   └── Radius.xaml           # Corner radius values
│   │   ├── Controls/                 # Global control styles
│   │   │   ├── ButtonStyles.xaml     # All button variants
│   │   │   ├── TextBoxStyles.xaml    # TextBox styling
│   │   │   ├── ComboBoxStyles.xaml   # ComboBox styling
│   │   │   ├── DataGridStyles.xaml   # DataGrid styling
│   │   │   ├── ListViewStyles.xaml   # ListView styling
│   │   │   ├── ScrollBarStyles.xaml  # ScrollBar styling
│   │   │   ├── GroupBoxStyles.xaml   # GroupBox styling
│   │   │   ├── LabelStyles.xaml      # Label styling
│   │   │   ├── ListViewItemStyles.xaml
│   │   │   ├── ComboBoxItemStyles.xaml
│   │   │   ├── ProgressBarStyles.xaml
│   │   │   └── WindowStyles.xaml     # ModernWindow template
│   │   └── Fluent/                   # Fluent effects
│   │       ├── FluentEffects.xaml    # Shadows & glow effects
│   │       └── FluentWhite.xaml      # Light theme override
│   ├── ViewModels/                   # MVVM ViewModels
│   └── Views/                        # Additional windows
│       └── SettingsWindow.xaml       # Theme settings UI
├── Som3a Addin 2026/                 # VSTO Add-in project
│   ├── ThisAddIn.cs                  # Excel add-in entry point
│   └── Ribbon*.cs                   # Excel ribbon configuration
└── Docs/                            # Documentation
```

---

## Design Tokens System

### Color Tokens (Theme/Base/Colors.xaml)

| Token Name | Value | Usage |
|------------|-------|-------|
| AccentColor | #3A86FF | Primary action color |
| BackgroundColor | #0E1720 | Window background |
| CardColor | #15202B | Card/panel background |
| SurfaceColor | #1C2B3A | Elevated surfaces |
| TextMainColor | #F2FFFFFF | Primary text (white, 95% opacity) |
| TextSubColor | #BFFFFFFF | Secondary text |
| SuccessColor | #2ED573 | Success states |
| WarningColor | #FFA502 | Warning states |
| DangerColor | #FF4757 | Error/danger states |
| BorderColor | #33FFFFFF | Borders |

### Brush Tokens

All colors have corresponding Brush tokens:
- `AccentBrush`, `BackgroundBrush`, `CardBrush`, etc.

### Typography (Theme/Base/Typography.xaml)

| Token | Value |
|-------|-------|
| TitleFontSize | 20 |
| HeaderFontSize | 16 |
| SubHeaderFontSize | 14 |
| BodyFontSize | 13 |
| CaptionFontSize | 11 |

### Spacing (Theme/Base/Spacing.xaml)

| Token | Value |
|-------|-------|
| ButtonPadding | 12,6 |
| CardPadding | 16,12 |
| InputPadding | 10,6 |
| ControlHeight | 30 |
| ButtonHeight | 32 |
| HeaderHeight | 48 |

### Radius (Theme/Base/Radius.xaml)

| Token | Value |
|-------|-------|
| SmallRadius | 6 |
| MediumRadius | 10 |
| CardRadius | 12 |
| WindowRadius | 16 |
| PillRadius | 20 |

### Using Tokens in XAML

```xml
<!-- Correct: Use DynamicResource for runtime theme changes -->
<Border Background="{DynamicResource CardBrush}"
        BorderBrush="{DynamicResource CardStrokeBrush}"
        CornerRadius="{DynamicResource CardRadius}">
    <TextBlock Text="Hello" Foreground="{DynamicResource TextMainBrush}"/>
</Border>

<!-- Also valid: StaticResource for compile-time (styles) -->
<Style TargetType="{x:Type Button}">
    <Setter Property="Background" Value="{DynamicResource AccentBrush}"/>
</Style>
```

---

## Control Styles

### Button Styles

| Style Key | Description |
|-----------|-------------|
| BaseButton | Default button with border |
| PrimaryButton | Accent color filled button |
| DangerButton | Red filled button |
| SuccessButton | Green filled button |
| GhostButton | Transparent with border |
| IconButton | Square icon-only button |

### Usage

```xml
<Button Content="Primary" Style="{DynamicResource PrimaryButton}"/>
<Button Content="Default" Style="{DynamicResource BaseButton}"/>
<Button Content="Danger" Style="{DynamicResource DangerButton}"/>
<Button Content="Ghost" Style="{DynamicResource GhostButton}"/>
```

### Other Controls

- **TextBox**: Default style applied automatically
- **ReadOnlyTextBox**: `Style="{DynamicResource ReadOnlyTextBox}"`
- **RoundComboBox**: `Style="{DynamicResource RoundComboBox}"`
- **ListViewItem**: Applied automatically
- **GridViewColumnHeader**: Applied automatically

---

## Windows & Views

### MainWindow (MainWindow.xaml)
- **Purpose**: P6 Activity Comparison
- **Type**: Custom chrome (WindowStyle=None, AllowsTransparency=True)
- **Features**: Sheet selection, ListView comparison, progress bar

### XerEditorWindow (XerEditorWindow.xaml)
- **Purpose**: XER file editor
- **Type**: Standard Window
- **Features**: Load/Export XER, table selection

### AssignTradeCodesWindow (AssignTradeCodesWindow.xaml)
- **Purpose**: Assign trade codes to activities
- **Type**: Custom chrome
- **Features**: Sheet selection, trade code assignment, progress

### LinksManagerWindow (Views/LinksManagerWindow.xaml)
- **Purpose**: Manage workbook links
- **Type**: Custom chrome
- **Features**: Link tracking, refresh, progress

### SubDailyReportWindow (SubDailyReportWindow.xaml)
- **Purpose**: Daily report generation
- **Type**: Custom chrome
- **Features**: File selection, report generation

### SettingsWindow (Views/SettingsWindow.xaml)
- **Purpose**: Theme & accent color settings
- **Type**: ModernWindow
- **Features**: Theme toggle (Dark/White), accent color picker

---

## Services & ViewModels

### ThemeManager (Services/ThemeManager.cs)

```csharp
// Change accent color at runtime
ThemeManager.ChangeAccent("#FF5722");  // Orange
ThemeManager.ChangeAccent("#3A86FF");  // Blue (default)

// Switch theme
ThemeManager.ApplyTheme(ThemeType.FluentDarkBlue);
ThemeManager.ApplyTheme(ThemeType.FluentWhite);

// Save/Load settings
ThemeManager.SaveSettings();
ThemeManager.LoadSettings();
```

### ToastService (Services/ToastService.cs)

```csharp
ToastService.Show("Message", ToastType.Info);
ToastService.Success("Operation completed");
ToastService.Warning("Warning message");
ToastService.Error("Error occurred");
```

---

## Build & Run

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Office Developer Tools (for VSTO)

### Build Command
```powershell
dotnet build "Som3a Addin 2026.slnx"
```

### Running the Application
1. Open the solution in Visual Studio
2. Set WpfApp2 as startup project
3. Press F5 to run

### For Excel Add-in
1. Build the solution
2. Publish the VSTO project
3. Install the add-in in Excel

---

## Migration Guide (From Legacy)

If updating from the old theme system:

1. **Replace StaticResource with DynamicResource**
   ```xml
   <!-- Before -->
   <Border Background="{StaticResource AccentBrush}"/>

   <!-- After -->
   <Border Background="{DynamicResource AccentBrush}"/>
   ```

2. **Use new token names**
   - `TextMain` → `TextMainBrush`
   - `TextSub` → `TextSubBrush`
   - `CardStroke` → `CardStrokeBrush`
   - `ControlBg` → `ControlBgBrush`
   - `ControlStroke` → `ControlStrokeBrush`

3. **Update button styles**
   - `RoundButton` → `BaseButton`
   - `PrimaryButton` stays the same

4. **Remove local Window.Resources**
   - Delete local style definitions
   - Use global styles from Theme/Controls/

---

## Best Practices

1. **Always use DynamicResource for brushes** - enables runtime theme switching
2. **Use StaticResource for BasedOn** - required for style inheritance
3. **No hardcoded colors** - use tokens from Theme/Base/
4. **Keep Window.Resources clean** - use global styles
5. **Test in both themes** - verify accent color changes work

---

## File Paths Reference

| Component | Path |
|-----------|------|
| Colors | `WpfApp2/Theme/Base/Colors.xaml` |
| Typography | `WpfApp2/Theme/Base/Typography.xaml` |
| Spacing | `WpfApp2/Theme/Base/Spacing.xaml` |
| Radius | `WpfApp2/Theme/Base/Radius.xaml` |
| Button Styles | `WpfApp2/Theme/Controls/ButtonStyles.xaml` |
| TextBox Styles | `WpfApp2/Theme/Controls/TextBoxStyles.xaml` |
| ComboBox Styles | `WpfApp2/Theme/Controls/ComboBoxStyles.xaml` |
| App.xaml | `WpfApp2/App.xaml` |
| ThemeManager | `WpfApp2/Services/ThemeManager.cs` |
| ModernWindow | `WpfApp2/Controls/ModernWindow.cs` |

---

*Last Updated: May 2026*
*Som3a Addin 2026 - WPF UI Architecture v2.0*