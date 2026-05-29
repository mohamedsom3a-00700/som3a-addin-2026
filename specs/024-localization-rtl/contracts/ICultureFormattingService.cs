/// <summary>
/// Contract for culture-aware number, date, and currency formatting.
/// Implemented by WpfApp2.Converters.CultureAwareFormattingConverter.
/// </summary>
public interface ICultureFormattingService
{
    /// <summary>Format a numeric value per current culture.</summary>
    string FormatNumber(decimal value);

    /// <summary>Format a date per current culture.</summary>
    string FormatDate(DateTime date);

    /// <summary>Format a currency value per current culture.</summary>
    string FormatCurrency(decimal amount);

    /// <summary>Format a number with specified decimal places per current culture.</summary>
    string FormatNumber(decimal value, int decimalPlaces);
}
