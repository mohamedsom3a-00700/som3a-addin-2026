using System;
using System.Globalization;
using System.Windows.Data;

namespace Som3a_WPF_UI.Converters
{
    public class CultureAwareFormattingConverter
    {
        private static readonly Lazy<CultureAwareFormattingConverter> _instance =
            new Lazy<CultureAwareFormattingConverter>(() => new CultureAwareFormattingConverter());

        public static CultureAwareFormattingConverter Instance => _instance.Value;

        private CultureInfo _currentCulture;

        public CultureAwareFormattingConverter()
        {
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        public void SetCulture(string cultureCode)
        {
            try
            {
                _currentCulture = new CultureInfo(cultureCode);
            }
            catch
            {
                _currentCulture = CultureInfo.CurrentUICulture;
            }
        }

        public string FormatNumber(decimal value)
        {
            return value.ToString("N", _currentCulture);
        }

        public string FormatNumber(decimal value, int decimalPlaces)
        {
            var format = "N" + decimalPlaces;
            return value.ToString(format, _currentCulture);
        }

        public string FormatDate(DateTime date)
        {
            return date.ToString("D", _currentCulture);
        }

        public string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", _currentCulture);
        }
    }

    public class CultureNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return CultureAwareFormattingConverter.Instance.FormatNumber(d);
            if (value is double db)
                return CultureAwareFormattingConverter.Instance.FormatNumber((decimal)db);
            if (value is int i)
                return CultureAwareFormattingConverter.Instance.FormatNumber((decimal)i);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CultureDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return CultureAwareFormattingConverter.Instance.FormatDate(dt);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CultureCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return CultureAwareFormattingConverter.Instance.FormatCurrency(d);
            if (value is double db)
                return CultureAwareFormattingConverter.Instance.FormatCurrency((decimal)db);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
