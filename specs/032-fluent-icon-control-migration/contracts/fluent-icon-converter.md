# Contract: FluentIconConverter

**Type**: WPF Value Converter
**Namespace**: `Som3a_WPF_UI.Converters`
**Implements**: `IValueConverter`

## Purpose

Converts a string icon name (e.g., `"Home"`, `"Settings"`) to a `FluentIcon` control instance for use in XAML data bindings.

## Interface

```csharp
public class FluentIconConverter : IValueConverter
{
    // Converts string icon name → FluentIcon control
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    // Not supported (one-way binding only)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}
```

## Conversion Rules

| Input | Output | Behavior |
|-------|--------|----------|
| Valid `Symbol` name (e.g., `"Home"`) | `FluentIcon` with that Symbol | Normal resolution |
| `null` or empty string | `FluentIcon` with `Symbol.Error` | Fallback with warning |
| Unknown string | `FluentIcon` with `Symbol.ErrorCircleHalfFilled` | Fallback with warning |
| Non-string type | `FluentIcon` with `Symbol.Error` | Type mismatch fallback |

## XAML Usage

```xml
<Window.Resources>
    <converters:FluentIconConverter x:Key="FluentIconConverter" />
</Window.Resources>

<!-- Static usage -->
<fluentIcons:FluentIcon Symbol="{Binding IconName, Converter={StaticResource FluentIconConverter}}"
                        IconSize="Small" />

<!-- In DataTemplate -->
<DataTemplate>
    <fluentIcons:FluentIcon Symbol="{Binding Icon, Converter={StaticResource FluentIconConverter}}"
                            IconSize="Small"
                            Foreground="{DynamicResource Brush.TextPrimary}" />
</DataTemplate>
```

## Error Handling

- All conversion failures produce a visible fallback icon (never blank/crash)
- Diagnostic warning logged via `System.Diagnostics.Trace` for unknown icon names
- No exceptions thrown from converter (fails silently with fallback)
