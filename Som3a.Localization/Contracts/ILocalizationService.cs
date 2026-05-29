using System.Globalization;

namespace Som3a.Localization.Contracts
{
    public interface ILocalizationService
    {
        string CurrentLanguageCode { get; }
        event EventHandler<LanguageChangedEventArgs> LanguageChanged;
        bool SetLanguage(string cultureCode);
        string GetString(string key);
        IReadOnlyList<LanguageInfo> GetSupportedLanguages();
        void SaveLanguagePreference();
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
}
