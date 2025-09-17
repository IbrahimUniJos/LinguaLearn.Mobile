using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class SectionTypeToSkipVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sectionType)
        {
            // Show skip button for non-interactive sections
            return sectionType switch
            {
                "reading" => true,
                "listening" => true,
                "pronunciation" => true,
                _ => false
            };
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}