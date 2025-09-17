using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

/// <summary>
/// Converts boolean values to colors (correct/incorrect)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (Application.Current?.Resources != null)
            {
                var colorKey = boolValue ? "Primary" : "Error";
                if (Application.Current.Resources.TryGetValue(colorKey, out var color))
                {
                    return color;
                }
            }
            
            // Fallback colors
            return boolValue ? Colors.Green : Colors.Red;
        }
        
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}