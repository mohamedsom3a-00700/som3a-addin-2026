using System.Globalization;
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
        private readonly Dictionary<string, string> _fontMappings = new()
        {
            ["en-US"] = "Segoe UI",
            ["ar-SA"] = "Cairo"
        };

        private double _fontScalingFactor = 1.0;

        private static readonly List<LanguageInfo> _supportedLanguages = new()
        {
            new LanguageInfo { Code = "en-US", DisplayName = "English", NativeName = "English", IsRTL = false },
            new LanguageInfo { Code = "ar-SA", DisplayName = "Arabic", NativeName = "العربية", IsRTL = true }
        };

        private static readonly List<string> _availableArabicFonts = new()
        {
            "Cairo", "IBM Plex Sans Arabic", "Segoe UI", "Noto Naskh Arabic", "Amiri"
        };

        private static readonly List<string> _availableEnglishFonts = new()
        {
            "Segoe UI", "Arial", "Calibri", "Times New Roman", "Consolas"
        };

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        public string CurrentLanguageCode => _currentLanguageCode;

        public bool IsRTL => _cultureManager.IsRTL();

        public double FontScalingFactor
        {
            get => _fontScalingFactor;
            set => _fontScalingFactor = Math.Clamp(value, 0.8, 1.5);
        }

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
            if (value == null)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalizationService] MISSING KEY: '{key}' not found in any resource file");
                return key;
            }

            System.Diagnostics.Debug.WriteLine($"[LocalizationService] MISSING KEY: '{key}' not found for culture '{_currentLanguageCode}', falling back to English");
            return value;
        }

        public string GetString(string key, params object[] formatArgs)
        {
            var value = GetString(key);
            if (formatArgs == null || formatArgs.Length == 0)
                return value;
            try
            {
                return string.Format(value, formatArgs);
            }
            catch (FormatException)
            {
                return value;
            }
        }

        public string GetStringOrDefault(string key, string defaultValue)
        {
            var value = GetString(key);
            return value == key ? defaultValue : value;
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

        public string GetFontFamily(string locale)
        {
            if (_fontMappings.TryGetValue(locale, out var font))
                return font;
            return "Segoe UI";
        }

        public bool SetFontFamily(string locale, string fontFamily)
        {
            if (string.IsNullOrWhiteSpace(locale) || string.IsNullOrWhiteSpace(fontFamily))
                return false;
            _fontMappings[locale] = fontFamily;
            return true;
        }

        public IReadOnlyList<string> GetAvailableArabicFonts()
        {
            return _availableArabicFonts.AsReadOnly();
        }

        public IReadOnlyList<string> GetAvailableEnglishFonts()
        {
            return _availableEnglishFonts.AsReadOnly();
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
