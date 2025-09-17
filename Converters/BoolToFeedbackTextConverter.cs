using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

/// <summary>
/// Converts boolean values to feedback text
/// </summary>
public class BoolToFeedbackTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Correct!" : "Incorrect";
        }
        
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}