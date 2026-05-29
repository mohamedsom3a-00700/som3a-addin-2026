# Quickstart: Localization & RTL

## Adding a New Translated String

1. Open the appropriate .resx file in `Som3a.Localization/Resources/`:
   - `Strings.en-US.resx` — English (default)
   - `Strings.ar-SA.resx` — Arabic
2. Add a new entry with key following the naming convention: `{Area}.{Component}.{Identifier}`
   - Example: `Settings.Appearance.LanguageLabel`
3. Provide the translated value in both .resx files
4. If Arabic translation is not yet available, leave the Arabic value empty — the system will fall back to English automatically

## Consuming a Localized String

```csharp
// In any ViewModel or service:
var label = LocalizationService.Instance.GetString("Settings.Appearance.LanguageLabel");

// In XAML (requires a markup extension or binding):
// Will be implemented as a Localize markup extension
<TextBlock Text="{helpers:Localize Settings.Appearance.LanguageLabel}" />
```

## Switching Language Programmatically

```csharp
bool success = LocalizationService.Instance.SetLanguage("ar-SA");
if (success)
{
    // UI automatically refreshes via LanguageChanged event
    // RTL layout applies automatically via ShellRTLManager
    // Culture formatting updates automatically
}
```

## Registering a UI Element for RTL Updates

```csharp
// In code-behind of a Page or UserControl:
RTLManager.Instance.RegisterFlowElement(myPanel, "MyPanelId");

// Unregister when disposing:
RTLManager.Instance.UnregisterFlowElement("MyPanelId");
```

## Formatting Values with Current Culture

```csharp
// In XAML:
<TextBlock Text="{Binding Quantity, Converter={StaticResource CultureNumberConverter}}" />

// In code-behind:
string formatted = CultureAwareFormattingService.Instance.FormatNumber(123456.78m);
```

## Adding a New Language

1. Create a new .resx file: `Strings.{cultureCode}.resx`
2. Register the language in `LocalizationService.GetSupportedLanguages()`
3. Add a `FontProfile` entry for the new language
4. No code changes needed — the architecture is language-agnostic

## Arabic Font Bundle

| Font | File | License |
|------|------|---------|
| Cairo | Cairo-Regular.ttf, Cairo-Bold.ttf | SIL Open Font License 1.1 |
| IBM Plex Sans Arabic | IBM-Plex-Sans-Arabic-* | SIL Open Font License 1.1 |

Font files should be placed in `WpfApp2/Assets/Fonts/` and set to "Resource" build action.

## Testing Checklist

- [ ] Switch from English to Arabic — all Shell UI translates
- [ ] Switch from Arabic to English — all text reverts
- [ ] RTL layout: sidebar on right, text right-aligned
- [ ] LTR layout: sidebar on left, text left-aligned
- [ ] Number formatting: Arabic-Indic digits in Arabic mode
- [ ] Date formatting: Arabic date convention in Arabic mode
- [ ] Font switch: Arabic font in Arabic mode, Segoe UI in English mode
- [ ] Excel ribbon remains LTR in both modes
- [ ] Plugin with no Arabic translation shows English fallback
- [ ] Language preference persists across app restart
- [ ] Language switch <1s, RTL switch <500ms
