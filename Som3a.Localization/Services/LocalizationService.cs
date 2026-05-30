using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using Som3a.Localization.Contracts;
using Som3a.Localization.RTL;

namespace Som3a.Localization.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly CultureManager _cultureManager;
        private readonly ResourceManager _resourceManager;
        private string _currentLanguageCode;

        private static readonly List<LanguageInfo> _supportedLanguages = new()
        {
            new LanguageInfo { Code = "en-US", DisplayName = "English", NativeName = "English", IsRTL = false },
            new LanguageInfo { Code = "ar-SA", DisplayName = "Arabic", NativeName = "العربية", IsRTL = true }
        };

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        public string CurrentLanguageCode => _currentLanguageCode;

        public LocalizationService(CultureManager cultureManager)
        {
            _cultureManager = cultureManager;
            _resourceManager = new ResourceManager("Som3a.Localization.Resources.Strings",
                typeof(LocalizationService).Assembly);
            _currentLanguageCode = _cultureManager.CurrentCulture.Name;
        }

        public bool SetLanguage(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
                return false;

            if (_currentLanguageCode == cultureCode)
                return true;

            if (!_supportedLanguages.Any(l => l.Code == cultureCode))
                return false;

            var previousCode = _currentLanguageCode;

            try
            {
                _cultureManager.SetCulture(cultureCode);
                _currentLanguageCode = cultureCode;
                var isRTL = _cultureManager.IsRTL();

                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs
                {
                    PreviousLanguageCode = previousCode,
                    NewLanguageCode = cultureCode,
                    IsRTL = isRTL
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalizationService] SetLanguage({cultureCode}) failed: {ex.Message}");
                return false;
            }
        }

        public string GetString(string key)
        {
            var value = _resourceManager.GetString(key, _cultureManager.CurrentCulture);
            if (!string.IsNullOrEmpty(value))
                return value;

            value = _resourceManager.GetString(key, new CultureInfo("en-US"));
            return value ?? key;
        }

        public string GetString(string key, string cultureName)
        {
            CultureInfo culture;
            try
            {
                culture = string.IsNullOrWhiteSpace(cultureName)
                    ? new CultureInfo("en-US")
                    : new CultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                culture = new CultureInfo("en-US");
            }

            var value = _resourceManager.GetString(key, culture);
            if (!string.IsNullOrEmpty(value))
                return value;

            value = _resourceManager.GetString(key, new CultureInfo("en-US"));
            return value ?? key;
        }

        public IReadOnlyList<LanguageInfo> GetSupportedLanguages()
        {
            return _supportedLanguages.AsReadOnly();
        }

        public void SaveLanguagePreference()
        {
            try
            {
                var path = GetPreferenceFilePath();
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, _currentLanguageCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalizationService] SaveLanguagePreference failed: {ex.Message}");
            }
        }

        public void LoadLanguagePreference()
        {
            try
            {
                var path = GetPreferenceFilePath();
                if (File.Exists(path))
                {
                    var savedCode = File.ReadAllText(path).Trim();
                    if (_supportedLanguages.Any(l => l.Code == savedCode))
                    {
                        SetLanguage(savedCode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalizationService] LoadLanguagePreference failed: {ex.Message}");
            }
        }

        private static string GetPreferenceFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Som3a", "language-preference.txt");
        }
    }

    public class CultureManager
    {
        public CultureInfo CurrentCulture { get; private set; }

        public CultureManager()
        {
            CurrentCulture = CultureInfo.CurrentUICulture;
        }

        public void SetCulture(string cultureName)
        {
            CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = CurrentCulture;
        }

        public bool IsRTL()
        {
            return RTLHelper.IsRTLCulture(CurrentCulture.Name);
        }
    }
}
