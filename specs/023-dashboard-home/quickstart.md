# Quickstart: Dashboard & Home (Phase 23)

**Feature**: 023-dashboard-home
**Date**: 2026-05-28

---

## Prerequisites

Before starting implementation, ensure these phases are complete:
- **Phase 14** (Platform Foundation) — Contracts, Domain, Plugin SDK
- **Phase 15** (Shell Refactor) — Shell navigation, sidebar categories
- **Phase 16** (Dynamic Settings) — Settings registry
- **Phase 18** (AI Core Infrastructure) — AI providers, token tracking

## Build Commands

```powershell
# Build .NET 8.0 contracts (new interfaces)
dotnet build Som3a.Contracts/Som3a.Contracts.csproj

# Build WPF host
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## Key Files to Create

1. **Contracts** (Som3a.Contracts/):
   - `IRecentItemsService.cs`
   - `IPerformanceMonitor.cs`
   - `IChangelogService.cs`

2. **Services** (WpfApp2/Services/):
   - `RecentItemsService.cs`
   - `PerformanceMonitor.cs`
   - `ChangelogService.cs`

3. **ViewModels** (WpfApp2/ViewModels/Dashboard/):
   - `WidgetViewModel.cs` (base class)
   - 9 widget ViewModels (one per widget)
   - `HomeViewModel.cs`

4. **Pages** (WpfApp2/Pages/):
   - `HomePage.xaml` + `.xaml.cs`
   - `DiagnosticsPage.xaml` + `.xaml.cs`

5. **Controls** (WpfApp2/Controls/):
   - `WidgetCard.cs`
   - `WidgetCardStyles.xaml`

## Key Files to Modify

1. **CompositionRoot.cs** — Register new services
2. **SidebarRegistrationService.cs** — Register HomePage and DiagnosticsPage
3. **NavigationService.cs** — Add "Home" category (order 0)
4. **ShellWindow.xaml.cs** — Set `WelcomePageType = typeof(HomePage)`
5. **ThemeResources.xaml** — Merge `WidgetCardStyles.xaml`

## Testing Checklist

- [ ] HomePage renders as default landing page
- [ ] All 9 widgets display data
- [ ] Quick Actions navigate correctly
- [ ] Recent Tools/Projects persist across sessions
- [ ] Diagnostics page updates every 5 seconds
- [ ] AI Provider Status shows online/offline states
- [ ] Plugin Status reflects module states
- [ ] No `StaticResource` for themeable properties
- [ ] No inline `DropShadowEffect`
- [ ] Renders correctly in FallbackSafe mode (Excel VSTO)
- [ ] Loads in <1 second

## Architecture Reference

- **MVVM**: All logic in ViewModels, code-behind is UI-only
- **DI**: Constructor injection via `IServiceContainer`
- **Events**: Subscribe to `ThemeManager.ThemeChanged`, `NavigationService.NavigationChanged`
- **Persistence**: JSON file at `%AppData%/Som3a/recent-items.json`
- **Refresh**: `DispatcherTimer` with 5-second interval for diagnostics widgets

## Common Pitfalls

1. **Don't use `StaticResource`** for colors/brushes — use `DynamicResource` only
2. **Don't add inline `DropShadowEffect`** — use centralized effects from `Effects/Shadows.xaml`
3. **Don't create standalone windows** — all new UI must be Pages within Shell
4. **Don't block UI thread** — use `async/await` for all data loading
5. **Don't forget FallbackSafe mode** — test in Excel VSTO host early

## Next Steps

1. Run `/speckit.tasks` to generate task breakdown
2. Start with Task 1: Create data models and contracts
3. Implement services (Tasks 2-3)
4. Build widget ViewModels (Tasks 6-14)
5. Create pages (Tasks 15-20)
6. Integrate navigation (Tasks 21-23)
7. Test in Excel VSTO host (Task 26)
