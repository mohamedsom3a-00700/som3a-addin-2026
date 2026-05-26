using System;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace Som3a_WPF_UI.Converters
{
    public class MaterialIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return PackIconKind.HelpCircleOutline;

            var iconName = value.ToString();
            if (!string.IsNullOrEmpty(iconName) && iconName.Length < 30)
            {
                try
                {
                    if (Enum.TryParse(iconName, true, out PackIconKind kind))
                        return kind;
                }
                catch { }
            }
            return PackIconKind.HelpCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is PackIconKind kind)
                return kind.ToString();
            return null;
        }
    }
}
