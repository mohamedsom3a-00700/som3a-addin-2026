using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Som3a_WPF_UI.Converters
{
    public class TextFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text || string.IsNullOrEmpty(text))
                return FlowDirection.LeftToRight;

            if (text.Length > 0 && IsRtlCharacter(text[0]))
                return FlowDirection.RightToLeft;

            return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static bool IsRtlCharacter(char c)
        {
            return (c >= 0x0590 && c <= 0x05FF)
                || (c >= 0x0600 && c <= 0x06FF)
                || (c >= 0x0750 && c <= 0x077F)
                || (c >= 0x08A0 && c <= 0x08FF)
                || (c >= 0xFB1D && c <= 0xFDFF)
                || (c >= 0xFE70 && c <= 0xFEFF)
                || (c >= 0x1EE00 && c <= 0x1EEFF);
        }
    }
}
