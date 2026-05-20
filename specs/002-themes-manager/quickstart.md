# Quickstart: Update Themes Manager

**Feature**: 002-themes-manager  
**Branch**: `002-fluent-theme-engine`

---

## Prerequisites

- Visual Studio 2019+ with .NET Framework 4.8 targeting pack
- Excel (for VSTO runtime testing)
- PowerShell (for build verification)

## Build

```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## Running the Feature

1. Open `Som3a_WPF_UI.sln` in Visual Studio.
2. Set the Excel VSTO add-in project as the startup project.
3. Press **F5** to launch Excel with the add-in loaded.
4. Open any window (e.g., **Links Manager**).
5. Open **Settings** from the ribbon.
6. Click the **Dark**, **Light**, or **Custom** theme card.
7. All open windows should update within 200ms.
8. Select an accent color swatch — buttons, progress bars, and glow effects should reflect the new color immediately.

## Key Files for Developers

| File | Purpose |
|------|---------|
| `Services/ThemeManager.cs` | Singleton service. Start here for theme/accent logic. |
| `Theme/Base/Colors.xaml` | Add new semantic tokens here using `Brush.*` namespace. |
| `Theme/Dark/DarkColors.xaml` | Dark theme overrides for new tokens. |
| `Theme/Light/LightColors.xaml` | Light theme overrides for new tokens. |
| `Theme/Custom/CustomColors.xaml` | Custom theme overrides for new tokens. |
| `Theme/Effects/Shadows.xaml` | Add new centralized shadow effects here. |
| `Theme/ThemeResources.xaml` | Aggregator dictionary — ensure loading order is correct. |

## Adding a New Theme-Aware Color to a Window

1. Define the token in `Theme/Base/Colors.xaml` using the semantic namespace:
   ```xml
   <SolidColorBrush x:Key="Brush.MyCategory.MyRole" Color="#RRGGBB"/>
   ```
2. Add per-theme overrides in `DarkColors.xaml`, `LightColors.xaml`, and `CustomColors.xaml`.
3. Reference it in the window XAML:
   ```xml
   <Border Background="{DynamicResource Brush.MyCategory.MyRole}"/>
   ```
4. Do NOT use `StaticResource` for themeable properties.
5. Do NOT define the token with a legacy flat name (e.g., `MyCategoryBrush`).

## Testing Checklist

- [ ] Build compiles without errors (`msbuild`)
- [ ] SettingsWindow opens without XAML parse exceptions
- [ ] Dark → Light → Custom switches update all open windows
- [ ] Accent color persists across theme switches
- [ ] Rapid clicking on theme cards does not crash (debounce works)
- [ ] Progress bar fill uses accent color, not hardcoded teal-green
- [ ] Progress bar percentage text is readable in both Dark and Light themes
- [ ] No `#HEX` colors remain in window `.xaml` files (verify with grep)

## Common Pitfalls

- **Adding a resource key that already exists** in `App.xaml` or another dictionary → XAML parse exception on load. Check `ThemeResources.xaml` first.
- **Using `StaticResource` for a themeable brush** → the value will not update when the theme changes. Always use `DynamicResource`.
- **Defining `AccentColorValue` in a theme dictionary** → ThemeManager will overwrite it at runtime, but it may cause confusion. Keep accent as runtime-mutable only.
- **Forgetting to add Light/Custom overrides** for a new token → the token will fall back to the Base value (dark), causing visual issues in Light theme.
