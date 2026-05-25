using System.Globalization;
using System.Resources;
using Som3a.Localization.RTL;

namespace Som3a.Localization.Services
{
    public class LocalizationService
    {
        private readonly CultureManager _cultureManager;
        private readonly ResourceManager _resourceManager;

        public LocalizationService(CultureManager cultureManager)
        {
            _cultureManager = cultureManager;
            _resourceManager = new ResourceManager("Som3a.Localization.Resources.Strings",
                typeof(LocalizationService).Assembly);
        }

        public string GetString(string key)
        {
            return _resourceManager.GetString(key, _cultureManager.CurrentCulture) ?? key;
        }

        public string GetString(string key, string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            return _resourceManager.GetString(key, culture) ?? key;
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
