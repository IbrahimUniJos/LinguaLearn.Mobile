using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class SectionTypeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string sectionType)
        {
            return sectionType.ToLower() switch
            {
                "vocabulary" => "Vocabulary",
                "grammar" => "Grammar",
                "pronunciation" => "Pronunciation",
                "quiz" => "Quiz",
                "reading" => "Reading",
                "listening" => "Listening",
                _ => "Lesson"
            };
        }
        return "Lesson";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}