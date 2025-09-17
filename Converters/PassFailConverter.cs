using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

/// <summary>
/// Converts boolean pass/fail status to text
/// </summary>
public class PassFailConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isPassed)
        {
            return isPassed ? "PASSED" : "FAILED";
        }
        
        return "UNKNOWN";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}