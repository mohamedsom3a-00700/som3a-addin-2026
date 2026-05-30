using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace Som3a_WPF_UI.Services
{
    public class LocalizationBridgeService
    {
        private static readonly Lazy<LocalizationBridgeService> _instance =
            new Lazy<LocalizationBridgeService>(() => new LocalizationBridgeService());

        public static LocalizationBridgeService Instance => _instance.Value;

        private string _currentLanguageCode = "en-US";
        private readonly Dictionary<string, Dictionary<string, string>> _resourceCache = new();

        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        public string CurrentLanguageCode => _currentLanguageCode;

        public bool IsRTL
        {
            get
            {
                var code = _currentLanguageCode;
                if (code == "ar-SA" || code == "ar" || code.StartsWith("ar-"))
                    return true;
                try { return new CultureInfo(code).TextInfo.IsRightToLeft; }
                catch { return false; }
            }
        }

        private static readonly List<LanguageInfo> _supportedLanguages = new()
        {
            new LanguageInfo { Code = "en-US", DisplayName = "English", NativeName = "English", IsRTL = false },
            new LanguageInfo { Code = "ar-SA", DisplayName = "Arabic", NativeName = "العربية", IsRTL = true }
        };

        private LocalizationBridgeService()
        {
            LoadResourcesIntoCache("en-US");
            LoadResourcesIntoCache("ar-SA");
        }

        private void LoadResourcesIntoCache(string cultureCode)
        {
            if (_resourceCache.ContainsKey(cultureCode))
                return;

            try
            {
                Stream stream = null;
                var asm = Assembly.GetExecutingAssembly();

                if (cultureCode == "en-US" || cultureCode == "en")
                {
                    stream = asm.GetManifestResourceStream("Som3a_WPF_UI.Resources.Strings.resources");
                }
                else
                {
                    stream = asm.GetManifestResourceStream("Som3a_WPF_UI.Resources.StringsArabic.resources");
                }

                if (stream != null)
                {
                    var dict = new Dictionary<string, string>();
                    using (var reader = new ResourceReader(stream))
                    {
                        var en = reader.GetEnumerator();
                        while (en.MoveNext())
                            dict[(string)en.Key] = (string)en.Value;
                    }
                    _resourceCache[cultureCode] = dict;
                }
            }
            catch
            {
            }

            if (!_resourceCache.ContainsKey(cultureCode))
                _resourceCache[cultureCode] = new Dictionary<string, string>();
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
                _currentLanguageCode = cultureCode;
                CultureInfo.CurrentUICulture = new CultureInfo(cultureCode);

                var isRTL = IsRTL;

                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs
                {
                    PreviousLanguageCode = previousCode,
                    NewLanguageCode = cultureCode,
                    IsRTL = isRTL
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetString(string key)
        {
            try
            {
                if (_resourceCache.TryGetValue(_currentLanguageCode, out var dict))
                {
                    if (dict.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                        return val;
                }

                if (_resourceCache.TryGetValue("en-US", out var enDict))
                {
                    if (enDict.TryGetValue(key, out var enVal) && !string.IsNullOrEmpty(enVal))
                        return enVal;
                }

                return key;
            }
            catch
            {
                return key;
            }
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
            catch
            {
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
            catch
            {
            }
        }

        private static string GetPreferenceFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Som3a", "language-preference.txt");
        }
    }

    public class LanguageChangedEventArgs : EventArgs
    {
        public string PreviousLanguageCode { get; set; }
        public string NewLanguageCode { get; set; }
        public bool IsRTL { get; set; }
    }

    public class LanguageInfo
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string NativeName { get; set; }
        public bool IsRTL { get; set; }
    }
}
