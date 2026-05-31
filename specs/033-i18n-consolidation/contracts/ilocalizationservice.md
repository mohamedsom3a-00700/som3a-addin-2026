# Contract: ILocalizationService

**Purpose**: Single service contract for all localization consumers. Replaces `LocalizationBridgeService` and `TranslationSource`.

**Location**: `Som3a.Localization/Contracts/ILocalizationService.cs` (existing — extend as needed)

## Interface

```csharp
public interface ILocalizationService
{
    // -- Language Management --
    string CurrentLanguageCode { get; }
    bool IsRTL { get; }
    IReadOnlyList<LanguageInfo> GetSupportedLanguages();
    bool SetLanguage(string cultureCode);
    
    // -- String Resolution --
    string GetString(string key);
    string GetString(string key, params object[] formatArgs);
    string GetStringOrDefault(string key, string defaultValue);
    
    // -- Font Management --
    string GetFontFamily(string locale);
    bool SetFontFamily(string locale, string fontFamily);
    IReadOnlyList<string> GetAvailableArabicFonts();
    IReadOnlyList<string> GetAvailableEnglishFonts();
    double FontScalingFactor { get; set; }
    
    // -- Persistence --
    void SaveLanguagePreference();
    void LoadLanguagePreference();
    
    // -- Events --
    event EventHandler<LanguageChangedEventArgs> LanguageChanged;
}
```

## XAML Binding Contract

### Pattern 1: ViewModel Binding (Preferred)

ViewModels consume `ILocalizationService` via constructor injection and expose string properties:

```csharp
public class MyViewModel
{
    private readonly ILocalizationService _localization;
    
    public MyViewModel(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LanguageChanged += OnLanguageChanged;
    }
    
    public string Title => _localization.GetString("MyPage.Title");
    
    private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Title));
    }
}
```

XAML:
```xaml
<TextBlock Text="{Binding Title}" />
```

### Pattern 2: Loc Markup Extension (Optional, for direct XAML use)

When ViewModel exposure is impractical (template selectors, data triggers), a `Loc` markup extension can be used:

```xaml
<TextBlock Text="{l:Loc MyPage.Title}" />
```

This requires:
- A `LocExtension` class implementing `MarkupExtension`
- Subscription to `ILocalizationService.LanguageChanged` for refresh

## LanguageChangedEventArgs

```csharp
public class LanguageChangedEventArgs : EventArgs
{
    public string PreviousCultureCode { get; }
    public string NewCultureCode { get; }
    public bool IsRTL { get; }
}
```

## Contract Consumers

| Consumer | Contract Used | Pattern |
|----------|--------------|---------|
| ViewModels | `GetString(key)` | Constructor injection |
| XAML pages | ViewModel properties or `Loc` extension | Binding |
| ShellRTLManager | `LanguageChanged` event | Event subscription |
| FontService | `GetFontFamily(locale)`, `FontScalingFactor` | Direct call |
| LanguagePageViewModel | All members | Constructor injection |
