using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

/// <summary>
/// Converts boolean values to emoji representations
/// </summary>
public class BoolToEmojiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "✅" : "❌";
        }
        
        return "❓";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}