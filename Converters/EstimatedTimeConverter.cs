using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class EstimatedTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int estimatedMinutes)
        {
            if (estimatedMinutes < 60)
            {
                return $"{estimatedMinutes} min";
            }
            else
            {
                var hours = estimatedMinutes / 60;
                var minutes = estimatedMinutes % 60;
                return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
            }
        }
        return "0 min";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}