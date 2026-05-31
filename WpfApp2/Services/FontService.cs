using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Som3a.Localization.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class FontService
    {
        private readonly ILocalizationService _localization;
        private string _arabicFont = "Cairo";
        private string _englishFont = "Segoe UI";

        public string ArabicFont => _arabicFont;
        public string EnglishFont => _englishFont;

        public FontService(ILocalizationService localization)
        {
            _localization = localization;
            _localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            ApplyLanguageFont(e.NewLanguageCode);
        }

        public void SetArabicFont(string fontFamily)
        {
            _arabicFont = fontFamily;
            if (_localization.CurrentLanguageCode == "ar-SA")
                ApplyFontToResources(fontFamily);
        }

        public void SetEnglishFont(string fontFamily)
        {
            _englishFont = fontFamily;
            if (_localization.CurrentLanguageCode != "ar-SA")
                ApplyFontToResources(fontFamily);
        }

        public void ApplyLanguageFont(string languageCode)
        {
            var font = languageCode == "ar-SA" ? _arabicFont : _englishFont;
            ApplyFontToResources(font);
        }

        private static void ApplyFontToResources(string fontFamilyName)
        {
            try
            {
                var fontFamily = new FontFamily(fontFamilyName);
                Application.Current.Resources["CustomFontFamily"] = fontFamily;
                Application.Current.Resources["FontFamilyPrimary"] = fontFamily;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FontService] ApplyFont failed: {ex.Message}");
            }
        }

        public IReadOnlyList<string> GetAvailableArabicFonts()
        {
            return new List<string>
            {
                "Cairo", "IBM Plex Sans Arabic", "Segoe UI", "Noto Naskh Arabic", "Amiri"
            };
        }

        public IReadOnlyList<string> GetAvailableEnglishFonts()
        {
            return new List<string>
            {
                "Segoe UI", "Arial", "Calibri", "Times New Roman", "Consolas"
            };
        }
    }
}

