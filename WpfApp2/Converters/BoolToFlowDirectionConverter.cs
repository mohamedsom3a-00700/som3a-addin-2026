using System;
using System.Windows;
using System.Windows.Data;

namespace Som3a_WPF_UI.Converters
{
    [ValueConversion(typeof(bool), typeof(FlowDirection))]
    public class BoolToFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isRTL)
                return isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FlowDirection fd)
                return fd == FlowDirection.RightToLeft;
            return false;
        }
    }
}
