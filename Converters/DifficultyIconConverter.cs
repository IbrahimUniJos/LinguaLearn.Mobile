using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class DifficultyIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string difficulty)
        {
            return difficulty.ToLower() switch
            {
                "beginner" => "🟢",
                "elementary" => "🟡",
                "intermediate" => "🟠",
                "advanced" => "🔴",
                _ => "⚪"
            };
        }
        return "⚪";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}