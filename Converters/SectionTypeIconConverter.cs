using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class SectionTypeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string sectionType)
        {
            return sectionType.ToLower() switch
            {
                "vocabulary" => "📚",
                "grammar" => "📝",
                "pronunciation" => "🎤",
                "quiz" => "🧠",
                "reading" => "📖",
                "listening" => "👂",
                _ => "📄"
            };
        }
        return "📄";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}