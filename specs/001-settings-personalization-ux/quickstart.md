# Quickstart: Settings & Personalization UX

## Key Files

| Purpose | Path |
|---------|------|
| Existing Settings window | `WpfApp2/Views/SettingsWindow.xaml` |
| Existing ThemeManager | `WpfApp2/Services/ThemeManager.cs` |
| Existing RenderModeService | `WpfApp2/Services/RenderModeService.cs` |
| New ViewModel | `WpfApp2/ViewModels/SettingsViewModel.cs` |
| New Persistence Service | `WpfApp2/Services/SettingsPersistenceService.cs` |
| New Data Model | `WpfApp2/Models/UserSettings.cs` |
| New Panel Styles | `WpfApp2/Theme/Controls/SettingsPanelStyles.xaml` |

## Implementation Order

1. **Data models**: Create `UserSettings`, `SettingsCategory`, `SettingsExport` - pure data classes
2. **Persistence service**: Implement `SettingsPersistenceService` (Load/Save/Import/Export)
3. **ViewModel**: Create `SettingsViewModel` with category navigation, preview state machine, commands
4. **Sidebar navigation**: Refactor `SettingsWindow.xaml` to add sidebar + ContentControl
5. **Panel UserControls**: Create 6 panel views (Appearance, Performance, Accessibility, Diagnostics, Excel, Plugins)
6. **Appearance panel**: Theme cards, accent swatches, background picker, live preview area, Apply/Cancel
7. **Performance panel**: Animation speed selector, density radio buttons
8. **Accessibility panel**: High contrast toggle, focus indicator toggle
9. **Diagnostics panel**: Export/Import buttons, system info display
10. **Excel panel**: Render mode dropdown, safe mode toggle, DPI info
11. **Plugins panel**: Placeholder with "No plugins installed" message
12. **Styles**: `SettingsPanelStyles.xaml` for sidebar and panel consistency

## Build & Test

```powershell
# Build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug

# Manual test checklist
# 1. Open Settings → verify 6 categories visible in sidebar
# 2. Click each category → verify panel loads correctly
# 3. Appearance: click theme card → verify live preview updates → Apply → verify main theme changes
# 4. Performance: set animation Off → verify animations stop
# 5. Accessibility: toggle high contrast → verify contrast changes
# 6. Diagnostics: export settings → modify settings → import → verify restore
# 7. Excel: toggle safe mode → verify render mode changes
```

## Constitutional Rules Checklist

- [ ] No StaticResource for themeable properties
- [ ] Theme mutations through ThemeManager only
- [ ] All new controls use DynamicResource for backgrounds, foregrounds, borders
- [ ] No inline colors, shadows, or effects
- [ ] SettingsWindow inherits from ModernWindow
- [ ] SnapsToDevicePixels=True and UseLayoutRounding=True on all new XAML
