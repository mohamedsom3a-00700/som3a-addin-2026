# Quickstart: Localization for Developers

## Getting a Localized String

### In ViewModels (Preferred)

```csharp
public class MyViewModel
{
    private readonly ILocalizationService _loc;
    
    public MyViewModel(ILocalizationService loc)
    {
        _loc = loc;
        _loc.LanguageChanged += (_, _) => OnPropertyChanged(nameof(Title));
    }
    
    public string Title => _loc.GetString("MyPage.Title");
}
```

### In XAML (Fallback)

When a ViewModel property isn't available, use the `Loc` markup extension:

```xaml
<TextBlock Text="{l:Loc MyPage.Title}" />
```

This automatically refreshes on language change.

## Adding a New String

1. Add the key to BOTH `Strings.en-US.resx` and `Strings.ar-SA.resx` in `Som3a.Localization/Resources/`
2. Follow the naming convention: `{Area}.{SubArea}.{Element}` (e.g., `Settings.Font.Arabic.Label`)
3. Use the key in ViewModel: `_loc.GetString("Settings.Font.Arabic.Label")`
4. If also needed in XAML directly: `Text="{l:Loc Settings.Font.Arabic.Label}"`

## Language Switch Flow

```
User clicks toggle (shell or settings)
    ↓
ILocalizationService.SetLanguage("ar-SA")
    ↓
CultureInfo.CurrentUICulture updated
    ↓
LanguageChanged event fires
    ↓
All ViewModels refresh string properties → UI updates via bindings
ShellRTLManager applies RTL layout
FontService switches fonts per locale
CultureAwareFormattingService refreshes formats
```

## Testing

```powershell
# Run existing tests
dotnet test Tests/Som3a_WPF_UI.Tests.csproj

# Run localization-specific tests
dotnet test Tests/Som3a.Localization.Tests.csproj

# Manual VSTO smoke test
# 1. Build with MSBuild
# 2. Launch Excel → verify ribbon loads
# 3. Switch EN→AR→EN across 3+ pages
# 4. Verify DataGrid, scrollbars, fonts, navigation
```

## Migration Checklist (per page)

When migrating a page from `TranslationSource` to `ILocalizationService`:

- [ ] Replace `{Binding Source={x:Static services:TranslationSource.Instance}, Path=[Key]}` with ViewModel property or `Loc` extension
- [ ] Add `FlowDirection` handling if page doesn't inherit from ModernWindow
- [ ] Verify fonts render correctly in Arabic mode
- [ ] Test with `dotnet test`

## Common Pitfalls

- **Missing key in Arabic RESX**: Falls back to English; check diagnostics log for missing key warnings
- **Format string mismatch**: Ensure `string.Format` parameters match between EN and AR versions
- **DataGrid columns in RTL**: Columns reverse order automatically; verify header alignment
- **Mixed content alignment**: Use `FlowDirection="LeftToRight"` on individual elements containing English text in Arabic UI
