using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Som3a.Localization.Contracts;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels.Settings
{
    public sealed partial class LanguagePageViewModel : ViewModelBase
    {
        private readonly ILocalizationService _localization;
        private readonly FontService _fontService;

        public ObservableCollection<LanguageInfo> SupportedLanguages { get; } = new();
        public ObservableCollection<string> ArabicFonts { get; } = new();
        public ObservableCollection<string> EnglishFonts { get; } = new();

        [ObservableProperty]
        private LanguageInfo _selectedLanguage;

        [ObservableProperty]
        private string _selectedArabicFont = "Cairo";

        [ObservableProperty]
        private string _selectedEnglishFont = "Segoe UI";

        [ObservableProperty]
        private double _fontScalingFactor = 1.0;

        [ObservableProperty]
        private string _arabicPreviewText = "مرحباً بكم في سومة";

        public LanguagePageViewModel()
        {
            _localization = App.Container.Resolve<ILocalizationService>();
            _fontService = App.Container.Resolve<FontService>();

            LoadSupportedLanguages();
            LoadFonts();

            _fontScalingFactor = _localization.FontScalingFactor;

            var currentCode = _localization.CurrentLanguageCode;
            foreach (var lang in SupportedLanguages)
            {
                if (lang.Code == currentCode)
                {
                    _selectedLanguage = lang;
                    break;
                }
            }
        }

        partial void OnSelectedLanguageChanged(LanguageInfo value)
        {
            if (value == null || value.Code == _localization.CurrentLanguageCode)
                return;

            if (_localization.SetLanguage(value.Code))
            {
                _localization.SaveLanguagePreference();
            }
        }

        partial void OnSelectedArabicFontChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            _fontService.SetArabicFont(value);
        }

        partial void OnSelectedEnglishFontChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            _fontService.SetEnglishFont(value);
        }

        partial void OnFontScalingFactorChanged(double value)
        {
            _localization.FontScalingFactor = value;
        }

        private void LoadSupportedLanguages()
        {
            SupportedLanguages.Clear();
            foreach (var lang in _localization.GetSupportedLanguages())
                SupportedLanguages.Add(lang);
        }

        private void LoadFonts()
        {
            ArabicFonts.Clear();
            foreach (var font in _fontService.GetAvailableArabicFonts())
                ArabicFonts.Add(font);

            EnglishFonts.Clear();
            foreach (var font in _fontService.GetAvailableEnglishFonts())
                EnglishFonts.Add(font);
        }
    }
}
