using System.Globalization;

namespace LinguaLearn.Mobile.Converters;

public class ActionButtonTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool showFeedback)
        {
            return showFeedback ? "Continue" : "Submit Answer";
        }
        return "Continue";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}