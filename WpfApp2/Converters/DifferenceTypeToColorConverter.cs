using Som3a.Shared.Core.Primavera;

using System;
using System.Globalization;
using System.Windows;
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
                        return Application.Current?.TryFindResource("Brush.Accent.Success") as Brush ?? Brushes.Green;

                    case DifferenceType.Deleted:
                        return Application.Current?.TryFindResource("Brush.Accent.Danger") as Brush ?? Brushes.Red;

                    case DifferenceType.Modified:
                        return Application.Current?.TryFindResource("Brush.Accent.Warning") as Brush ?? Brushes.Orange;
                }
            }

            return Application.Current?.TryFindResource("Brush.Text.Primary") as Brush ?? Brushes.White;
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