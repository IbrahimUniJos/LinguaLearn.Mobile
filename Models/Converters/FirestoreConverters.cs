using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models.Converters;

/// <summary>
/// Firestore converter for DifficultyLevel enum
/// </summary>
public class DifficultyLevelConverter : IFirestoreConverter<DifficultyLevel>
{
    public DifficultyLevel FromFirestore(object value)
    {
        if (value is string stringValue && Enum.TryParse<DifficultyLevel>(stringValue, true, out var result))
        {
            return result;
        }
        return DifficultyLevel.Adaptive; // Default fallback
    }

    public object ToFirestore(DifficultyLevel value)
    {
        return value.ToString();
    }
}

/// <summary>
/// Firestore converter for PronunciationSensitivity enum
/// </summary>
public class PronunciationSensitivityConverter : IFirestoreConverter<PronunciationSensitivity>
{
    public PronunciationSensitivity FromFirestore(object value)
    {
        if (value is string stringValue && Enum.TryParse<PronunciationSensitivity>(stringValue, true, out var result))
        {
            return result;
        }
        return PronunciationSensitivity.Medium; // Default fallback
    }

    public object ToFirestore(PronunciationSensitivity value)
    {
        return value.ToString();
    }
}

/// <summary>
/// Firestore converter for AppTheme enum
/// </summary>
public class AppThemeConverter : IFirestoreConverter<AppTheme>
{
    public AppTheme FromFirestore(object value)
    {
        if (value is string stringValue && Enum.TryParse<AppTheme>(stringValue, true, out var result))
        {
            return result;
        }
        return AppTheme.System; // Default fallback
    }

    public object ToFirestore(AppTheme value)
    {
        return value.ToString();
    }
}

/// <summary>
/// Firestore converter for TimeSpan
/// </summary>
public class TimeSpanConverter : IFirestoreConverter<TimeSpan>
{
    public TimeSpan FromFirestore(object value)
    {
        if (value is string stringValue && TimeSpan.TryParse(stringValue, out var result))
        {
            return result;
        }
        return new TimeSpan(19, 0, 0); // Default to 7 PM
    }

    public object ToFirestore(TimeSpan value)
    {
        return value.ToString(@"hh\:mm\:ss");
    }
}