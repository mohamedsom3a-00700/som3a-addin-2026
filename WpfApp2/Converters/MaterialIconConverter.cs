using System;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace Som3a_WPF_UI.Converters
{
    public class MaterialIconConverter : IValueConverter
    {
        private const int MaxIconNameLength = 30;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return PackIconKind.HelpCircleOutline;

            var iconName = value.ToString();
            if (!string.IsNullOrEmpty(iconName) && iconName.Length < MaxIconNameLength)
            {
                if (Enum.TryParse(iconName, true, out PackIconKind kind))
                    return kind;
                System.Diagnostics.Debug.WriteLine($"[MaterialIconConverter] Unrecognized icon name: '{iconName}'");
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
