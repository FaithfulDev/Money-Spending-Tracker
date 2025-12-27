using System.Diagnostics;
using System.Globalization;

namespace Money_Spending_Tracker.Features.Transactions;

public class NullFallbackConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        Debug.WriteLine("NullFallbackConverter invoked with values: " + string.Join(", ", values.Select(v => v?.ToString() ?? "null")));

        object? firstNonNull = values.FirstOrDefault(v => v != null);
        return firstNonNull;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
