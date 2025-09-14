namespace LinguaLearn.Mobile.Models;

public record UserProfile(
    string Id,
    string Email,
    string DisplayName,
    string? NativeLanguage = null,
    string? TargetLanguage = null,
    int XP = 0,
    int Level = 1,
    int StreakCount = 0,
    DateTime? LastActiveDate = null,
    bool HasCompletedOnboarding = false
);

public record LanguageOption(
    string Code,
    string Name,
    string FlagEmoji
);

public static class Languages
{
    public static readonly LanguageOption[] Available = [
        new("en", "English", "????"),
        new("es", "Spanish", "????"),
        new("fr", "French", "????"),
        new("de", "German", "????"),
        new("it", "Italian", "????"),
        new("pt", "Portuguese", "????"),
        new("ja", "Japanese", "????"),
        new("ko", "Korean", "????"),
        new("zh", "Chinese", "????"),
        new("ar", "Arabic", "????")
    ];
}