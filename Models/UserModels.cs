using System.Text.Json.Serialization;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Main user profile document stored in Firestore users/{userId}
/// Following Firestore best practices: flat structure, camelCase naming, UTC timestamps
/// </summary>
public class UserProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("nativeLanguage")]
    public string? NativeLanguage { get; set; }
    
    [JsonPropertyName("targetLanguage")]
    public string? TargetLanguage { get; set; }
    
    // Gamification Stats
    [JsonPropertyName("xp")]
    public int XP { get; set; } = 0;
    
    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;
    
    [JsonPropertyName("streakCount")]
    public int StreakCount { get; set; } = 0;
    
    [JsonPropertyName("lastActiveDate")]
    public DateTime? LastActiveDate { get; set; }
    
    [JsonPropertyName("streakFreezeTokens")]
    public int StreakFreezeTokens { get; set; } = 0;
    
    // Onboarding & Preferences
    [JsonPropertyName("hasCompletedOnboarding")]
    public bool HasCompletedOnboarding { get; set; } = false;
    
    [JsonPropertyName("preferences")]
    public UserPreferences Preferences { get; set; } = new();
    
    // Earned Badges (denormalized for quick access)
    [JsonPropertyName("badges")]
    public List<UserBadge> Badges { get; set; } = new();
    
    // Metadata
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
}

/// <summary>
/// User preferences for app behavior and notifications
/// </summary>
public class UserPreferences
{
    [JsonPropertyName("soundEnabled")]
    public bool SoundEnabled { get; set; } = true;
    
    [JsonPropertyName("vibrationEnabled")]
    public bool VibrationEnabled { get; set; } = true;
    
    [JsonPropertyName("dailyReminderEnabled")]
    public bool DailyReminderEnabled { get; set; } = true;
    
    [JsonPropertyName("dailyReminderTime")]
    public TimeSpan DailyReminderTime { get; set; } = new(19, 0, 0); // 7 PM
    
    [JsonPropertyName("weeklyGoal")]
    public int WeeklyGoal { get; set; } = 5; // 5 lessons per week
    
    [JsonPropertyName("difficultyPreference")]
    public DifficultyLevel DifficultyPreference { get; set; } = DifficultyLevel.Adaptive;
    
    [JsonPropertyName("pronunciationSensitivity")]
    public PronunciationSensitivity PronunciationSensitivity { get; set; } = PronunciationSensitivity.Medium;
    
    [JsonPropertyName("theme")]
    public AppTheme Theme { get; set; } = AppTheme.System;
}

/// <summary>
/// Badge earned by user with timestamp
/// </summary>
public class UserBadge
{
    [JsonPropertyName("badgeId")]
    public string BadgeId { get; set; } = string.Empty;
    
    [JsonPropertyName("earnedAt")]
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streak snapshot for detailed streak tracking
/// Stored in users/{userId}/streaks/{snapshotId} subcollection
/// </summary>
public class StreakSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
    
    [JsonPropertyName("currentCount")]
    public int CurrentCount { get; set; }
    
    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
    
    [JsonPropertyName("freezeTokensUsed")]
    public int FreezeTokensUsed { get; set; } = 0;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User statistics aggregated for display
/// Computed from progress records and cached in Firestore
/// </summary>
public class UserStats
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("totalLessonsCompleted")]
    public int TotalLessonsCompleted { get; set; } = 0;
    
    [JsonPropertyName("totalQuizzesCompleted")]
    public int TotalQuizzesCompleted { get; set; } = 0;
    
    [JsonPropertyName("totalXPEarned")]
    public int TotalXPEarned { get; set; } = 0;
    
    [JsonPropertyName("averageQuizAccuracy")]
    public double AverageQuizAccuracy { get; set; } = 0.0;
    
    [JsonPropertyName("totalStudyTimeMinutes")]
    public int TotalStudyTimeMinutes { get; set; } = 0;
    
    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; set; } = 0;
    
    [JsonPropertyName("badgesEarned")]
    public int BadgesEarned { get; set; } = 0;
    
    [JsonPropertyName("weeklyProgress")]
    public WeeklyProgress CurrentWeek { get; set; } = new();
    
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Weekly progress tracking for goals
/// </summary>
public class WeeklyProgress
{
    [JsonPropertyName("weekStartDate")]
    public DateTime WeekStartDate { get; set; }
    
    [JsonPropertyName("lessonsCompleted")]
    public int LessonsCompleted { get; set; } = 0;
    
    [JsonPropertyName("xpEarned")]
    public int XPEarned { get; set; } = 0;
    
    [JsonPropertyName("goal")]
    public int Goal { get; set; } = 5;
    
    [JsonPropertyName("dailyActivity")]
    public Dictionary<string, bool> DailyActivity { get; set; } = new();
}

/// <summary>
/// User session for in-memory state management
/// Not stored in Firestore - used for app state only
/// </summary>
public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string IdToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(IdToken) && DateTime.UtcNow < ExpiresAt;
}

/// <summary>
/// Available languages for learning
/// </summary>
public class LanguageOption
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("nativeName")]
    public string NativeName { get; set; } = string.Empty;
    
    [JsonPropertyName("flagEmoji")]
    public string FlagEmoji { get; set; } = string.Empty;
    
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; } = true;
}

// Enums for user preferences
public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Adaptive
}

public enum PronunciationSensitivity
{
    Low,
    Medium,
    High,
    Strict
}

public enum AppTheme
{
    Light,
    Dark,
    System
}

/// <summary>
/// Static data for available languages
/// </summary>
public static class AvailableLanguages
{
    public static readonly List<LanguageOption> All = new()
    {
        new() { Code = "en", Name = "English", NativeName = "English", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "es", Name = "Spanish", NativeName = "Español", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "fr", Name = "French", NativeName = "Français", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "de", Name = "German", NativeName = "Deutsch", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "it", Name = "Italian", NativeName = "Italiano", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "pt", Name = "Portuguese", NativeName = "Português", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "ja", Name = "Japanese", NativeName = "???", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "ko", Name = "Korean", NativeName = "???", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "zh", Name = "Chinese", NativeName = "??", FlagEmoji = "????", IsAvailable = true },
        new() { Code = "ar", Name = "Arabic", NativeName = "???????", FlagEmoji = "????", IsAvailable = true }
    };
    
    public static LanguageOption? GetByCode(string code) 
        => All.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}