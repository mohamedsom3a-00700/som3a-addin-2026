using Som3a.Shared.Core.Primavera;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Som3a_WPF_UI.Converters
{
    public class DifferenceTypeToColorConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is DifferenceType type)
            {
                switch (type)
                {
                    case DifferenceType.Added:
                        return Brushes.LightGreen;

                    case DifferenceType.Deleted:
                        return Brushes.LightCoral;

                    case DifferenceType.Modified:
                        return Brushes.Orange;
                }
            }

            return Brushes.White;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}