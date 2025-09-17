using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

/// <summary>
/// Converts seconds to formatted time string
/// </summary>
public class SecondsToTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            
            if (timeSpan.TotalHours >= 1)
            {
                return $"Time: {timeSpan:h\\:mm\\:ss}";
            }
            else
            {
                return $"Time: {timeSpan:mm\\:ss}";
            }
        }
        
        return "Time: --:--";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}