using System;
using Som3a.Localization.Contracts;

namespace Som3a_WPF_UI.Services
{
    public static class LocalizationBridge
    {
        private static ILocalizationService GetService()
        {
            try
            {
                return App.Container.Resolve<ILocalizationService>();
            }
            catch
            {
                return null;
            }
        }

        public static string CurrentLanguageCode =>
            GetService()?.CurrentLanguageCode ?? "en-US";

        public static bool IsRTL =>
            GetService()?.IsRTL ?? false;

        public static bool SetLanguage(string cultureCode)
        {
            var service = GetService();
            return service?.SetLanguage(cultureCode) ?? false;
        }

        public static void SaveLanguagePreference()
        {
            GetService()?.SaveLanguagePreference();
        }
    }
}
