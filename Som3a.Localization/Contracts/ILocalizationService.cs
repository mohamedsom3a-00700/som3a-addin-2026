using System.Globalization;

namespace Som3a.Localization.Contracts
{
    public interface ILocalizationService
    {
        string CurrentLanguageCode { get; }
        bool IsRTL { get; }
        event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        bool SetLanguage(string cultureCode);
        string GetString(string key);
        string GetString(string key, params object[] formatArgs);
        string GetStringOrDefault(string key, string defaultValue);
        IReadOnlyList<LanguageInfo> GetSupportedLanguages();
        void SaveLanguagePreference();
        void LoadLanguagePreference();

        string GetFontFamily(string locale);
        bool SetFontFamily(string locale, string fontFamily);
        IReadOnlyList<string> GetAvailableArabicFonts();
        IReadOnlyList<string> GetAvailableEnglishFonts();
        double FontScalingFactor { get; set; }
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
}
