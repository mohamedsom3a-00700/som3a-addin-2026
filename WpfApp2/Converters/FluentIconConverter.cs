using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using FluentIcons.Common;
using FluentIcons.Wpf;

namespace Som3a_WPF_UI.Converters
{
    public class FluentIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = ResolveIcon(value);

            return new FluentIcon
            {
                Icon = icon,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("FluentIconConverter is one-way only.");
        }

        private static Icon ResolveIcon(object value)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                if (Enum.TryParse<Icon>(str, ignoreCase: true, out var icon))
                {
                    return icon;
                }

                Trace.TraceWarning($"[FluentIconConverter] Unknown icon name: '{str}'. Falling back to Info.");
                return Icon.Info;
            }

            if (value != null)
            {
                Trace.TraceWarning($"[FluentIconConverter] Non-string value type: {value.GetType().Name}. Falling back to Info.");
            }

            return Icon.Info;
        }
    }
}
