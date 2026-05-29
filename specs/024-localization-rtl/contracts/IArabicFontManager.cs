/// <summary>
/// Contract for managing Arabic font selection and fallback chains.
/// Implemented by WpfApp2.Theme.Fonts.ArabicFontManager.
/// </summary>
public interface IArabicFontManager
{
    /// <summary>Available Arabic font families.</summary>
    IReadOnlyList<FontOption> AvailableFonts { get; }

    /// <summary>Currently selected Arabic font.</summary>
    string CurrentArabicFont { get; }

    /// <summary>Switch to a different Arabic font.</summary>
    void SetArabicFont(string fontFamily);

    /// <summary>Get the full fallback chain for a language (primary + fallbacks).</summary>
    string[] GetFontFallbackChain(string languageCode);
}

public class FontOption
{
    public string DisplayName { get; init; }
    public string FontFamily { get; init; }
    public string PreviewText { get; init; }
    public bool IsBundled { get; init; }
}
