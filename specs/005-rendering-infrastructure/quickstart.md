# Quickstart: Rendering & Window Infrastructure

## Files to Create

| File | Action |
|------|--------|
| `WpfApp2/Services/RenderModeService.cs` | New service wrapping WindowRenderModeDetector |
| `WpfApp2/Helpers/DpiHelper.cs` | New/extended DPI utilities |
| `Docs/Architecture/EXCEL_TEST_CHECKLIST.md` | New manual test checklist |

## Files to Modify

| File | Action |
|------|--------|
| `WpfApp2/Controls/ModernWindow.cs` | Add 4 DPs: RenderMode, IsSafeMode, WindowBackdrop, DpiScale |
| `WpfApp2/Theme/Effects/Shadows.xaml` | Add 3 safe variants: Shadow.Window.Safe, Shadow.Card.Safe, Shadow.Popup.Safe |
| `WpfApp2/Theme/WindowAnimations.xaml` | Standardize durations, add safe-mode skip |
| All 14 windows | Apply ModernWindow template, safe-mode settings, DynamicResource backgrounds |

## Build & Test

```powershell
msbuild WpfApp2\Som3a_WPF_UI.csproj /p:Configuration=Debug
```

## Order of Implementation

1. **RenderModeService** — first, since all other components depend on render mode detection
2. **Shadows.xaml safe variants** — effects must exist before they can be referenced
3. **ModernWindow v2 DPs** — exposes render mode and safe mode to all windows
4. **Window refactoring** — apply safe-mode and ModernWindow changes per window
5. **DPI helper** — DPI-aware scaling for correct rendering
6. **WindowAnimations standardization** — ensure compliance with FR-009
7. **Accessibility annotations** — FocusVisualStyle, AutomationProperties, keyboard nav
8. **EXCEL_TEST_CHECKLIST.md** — document manual test procedures

## Key Bindings

- **ThemeResource keys**: `Brush.Background.Root`, `CardStrokeBrush`, `Shadow.*`, `Shadow.*.Safe`
- **Animation durations**: open ≤200ms, close ≤150ms (using `Motion.*` tokens)
- **Safe mode**: disables fade animations, uses safe shadows, disables backdrops, AllowsTransparency=False
