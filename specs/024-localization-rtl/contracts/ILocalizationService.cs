/// <summary>
/// Contract for dynamic language switching and resource loading.
/// Implemented by Som3a.Localization.Services.LocalizationService.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Current language culture code (e.g., "en-US", "ar-SA").</summary>
    string CurrentLanguageCode { get; }

    /// <summary>Fires when language changes, allowing UI to refresh.</summary>
    event EventHandler<LanguageChangedEventArgs> LanguageChanged;

    /// <summary>Switch the application UI language.</summary>
    /// <param name="cultureCode">BCP-47 culture code.</param>
    /// <returns>True if switch succeeded.</returns>
    bool SetLanguage(string cultureCode);

    /// <summary>Get a localized string by resource key.</summary>
    /// <param name="key">Resource key in format "Area.Component.Identifier".</param>
    /// <returns>Localized string, or English fallback if translation missing.</returns>
    string GetString(string key);

    /// <summary>Get all supported languages.</summary>
    IReadOnlyList<LanguageInfo> GetSupportedLanguages();

    /// <summary>Persist the current language preference.</summary>
    void SaveLanguagePreference();

    /// <summary>Load language preference from persisted settings.</summary>
    void LoadLanguagePreference();
}

public class LanguageChangedEventArgs : EventArgs
{
    public string PreviousLanguageCode { get; init; }
    public string NewLanguageCode { get; init; }
    public bool IsRTL { get; init; }
}

public class LanguageInfo
{
    public string Code { get; init; }
    public string DisplayName { get; init; }
    public string NativeName { get; init; }
    public bool IsRTL { get; init; }
}
