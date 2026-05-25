# Localization Readiness

**Project**: Som3a Add-in 2026
**Phase**: 10 (Enterprise Polish)
**Date**: 2026-05-25
**Status**: Architecture assessment

## Current State

The Som3a Add-in currently has no localization framework. All user-visible strings are hardcoded in C# code-behind and XAML files.

## String Externalization Points

### Resource Extraction Patterns

Strings appear in two locations:

1. **XAML files** — `Text`, `ToolTip`, `Header`, `Content` attributes on controls
   - Pattern: `Text="My string"` or `Header="My string"`
   - Candidate for: `x:Uid` + `.resx` resource files

2. **C# files** — String literals in ViewModels and Services
   - Pattern: `"Some message"`, `$"Interpolated {value}"`
   - Candidate for: ResourceManager calls

### Recommended Approach

1. Add `.resx` resource files per locale (e.g., `Resources.resx`, `Resources.ar-SA.resx`)
2. Use `x:Uid` attributes in XAML for automatic resource lookup
3. Replace hardcoded C# strings with `ResourceManager.GetString()` calls
4. Use `System.Globalization.CultureInfo.CurrentUICulture` for locale detection

### RTL Considerations

- WPF has good RTL support via `FlowDirection` property
- All containers should check `FlowDirection` for RTL locales
- Text alignment, image flipping, and layout mirroring may be needed
- `ModernWindow` title bar buttons positioned on right side — may need Left side for RTL

## Inventory of String Locations

*Pending — full string inventory needed before localization implementation.*

## Recommendations
1. Create base `.resx` resource file for English (US)
2. Extract all hardcoded strings from XAML and C# files
3. Create Arabic (Saudi Arabia) `.resx` as first additional locale
4. Implement culture-aware resource loading in `App.xaml.cs`
5. Add locale switcher in Settings panel
