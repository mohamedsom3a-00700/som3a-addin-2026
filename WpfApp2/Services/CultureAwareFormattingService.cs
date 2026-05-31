using System;
using System.Globalization;
using Som3a.Localization.Contracts;
using Som3a_WPF_UI.Converters;

namespace Som3a_WPF_UI.Services
{
    public class CultureAwareFormattingService
    {
        private static readonly Lazy<CultureAwareFormattingService> _instance =
            new Lazy<CultureAwareFormattingService>(() => new CultureAwareFormattingService());

        public static CultureAwareFormattingService Instance => _instance.Value;

        private readonly CultureAwareFormattingConverter _converter;

        public CultureAwareFormattingService()
        {
            _converter = CultureAwareFormattingConverter.Instance;
        }

        public void RefreshCulture(string cultureCode)
        {
            _converter.SetCulture(cultureCode);
        }

        public string FormatNumber(decimal value)
        {
            return _converter.FormatNumber(value);
        }

        public string FormatNumber(decimal value, int decimalPlaces)
        {
            return _converter.FormatNumber(value, decimalPlaces);
        }

        public string FormatDate(DateTime date)
        {
            return _converter.FormatDate(date);
        }

        public string FormatCurrency(decimal amount)
        {
            return _converter.FormatCurrency(amount);
        }

        public CultureInfo GetCurrentCulture()
        {
            try
            {
                var localizationService = App.Container.Resolve<ILocalizationService>();
                var code = localizationService?.CurrentLanguageCode ?? "en-US";
                return new CultureInfo(code);
            }
            catch
            {
                return CultureInfo.CurrentUICulture;
            }
        }
    }
}
