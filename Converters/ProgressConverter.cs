using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class ProgressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int completedSections && parameter is int totalSections)
        {
            if (totalSections == 0) return 0.0;
            return (double)completedSections / totalSections;
        }
        
        // If no parameter provided, assume it's already a percentage
        if (value is double percentage)
        {
            return percentage / 100.0;
        }
        
        if (value is int intValue)
        {
            return intValue / 100.0;
        }
        
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}