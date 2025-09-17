using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Main user profile document stored in Firestore users/{userId}
/// Following Firestore best practices: flat structure, camelCase naming, UTC timestamps
/// </summary>
[FirestoreData]
public class UserProfile
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [FirestoreProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [FirestoreProperty("nativeLanguage")]
    public string? NativeLanguage { get; set; }
    
    [FirestoreProperty("targetLanguage")]
    public string? TargetLanguage { get; set; }
    
    // Gamification Stats
    [FirestoreProperty("xp")]
    public int XP { get; set; } = 0;
    
    [FirestoreProperty("level")]
    public int Level { get; set; } = 1;
    
    [FirestoreProperty("streakCount")]
    public int StreakCount { get; set; } = 0;
    
    [FirestoreProperty("lastActiveDate")]
    public DateTime? LastActiveDate { get; set; }
    
    [FirestoreProperty("streakFreezeTokens")]
    public int StreakFreezeTokens { get; set; } = 0;
    
    // Onboarding & Preferences
    [FirestoreProperty("hasCompletedOnboarding")]
    public bool HasCompletedOnboarding { get; set; } = false;
    
    [FirestoreProperty("preferences")]
    public UserPreferences Preferences { get; set; } = new();
    
    // Earned Badges (denormalized for quick access)
    [FirestoreProperty("badges")]
    public List<UserBadge> Badges { get; set; } = new();
    
    // Metadata
    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty("version")]
    public int Version { get; set; } = 1;
}

/// <summary>
/// User preferences for app behavior and notifications
/// </summary>
[FirestoreData]
public class UserPreferences
{
    [FirestoreProperty("soundEnabled")]
    public bool SoundEnabled { get; set; } = true;
    
    [FirestoreProperty("vibrationEnabled")]
    public bool VibrationEnabled { get; set; } = true;
    
    [FirestoreProperty("dailyReminderEnabled")]
    public bool DailyReminderEnabled { get; set; } = true;
    
    [FirestoreProperty("dailyReminderTime")]
    public TimeSpan DailyReminderTime { get; set; } = new(19, 0, 0); // 7 PM
    
    [FirestoreProperty("weeklyGoal")]
    public int WeeklyGoal { get; set; } = 5; // 5 lessons per week
    
    [FirestoreProperty("difficultyPreference")]
    public DifficultyLevel DifficultyPreference { get; set; } = DifficultyLevel.Adaptive;
    
    [FirestoreProperty("pronunciationSensitivity")]
    public PronunciationSensitivity PronunciationSensitivity { get; set; } = PronunciationSensitivity.Medium;
    
    [FirestoreProperty("theme")]
    public AppTheme Theme { get; set; } = AppTheme.System;
}

/// <summary>
/// Badge earned by user with timestamp
/// </summary>
[FirestoreData]
public class UserBadge
{
    [FirestoreProperty("badgeId")]
    public string BadgeId { get; set; } = string.Empty;
    
    [FirestoreProperty("earnedAt")]
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Streak snapshot for detailed streak tracking
/// Stored in users/{userId}/streaks/{snapshotId} subcollection
/// </summary>
[FirestoreData]
public class StreakSnapshot
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty("startDate")]
    public DateTime StartDate { get; set; }
    
    [FirestoreProperty("endDate")]
    public DateTime? EndDate { get; set; }
    
    [FirestoreProperty("currentCount")]
    public int CurrentCount { get; set; }
    
    [FirestoreProperty("maxCount")]
    public int MaxCount { get; set; }
    
    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; } = true;
    
    [FirestoreProperty("freezeTokensUsed")]
    public int FreezeTokensUsed { get; set; } = 0;
    
    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User statistics aggregated for display
/// Computed from progress records and cached in Firestore
/// </summary>
[FirestoreData]
public class UserStats
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty("totalLessonsCompleted")]
    public int TotalLessonsCompleted { get; set; } = 0;
    
    [FirestoreProperty("totalQuizzesCompleted")]
    public int TotalQuizzesCompleted { get; set; } = 0;
    
    [FirestoreProperty("totalXPEarned")]
    public int TotalXPEarned { get; set; } = 0;
    
    [FirestoreProperty("averageQuizAccuracy")]
    public double AverageQuizAccuracy { get; set; } = 0.0;
    
    [FirestoreProperty("totalStudyTimeMinutes")]
    public int TotalStudyTimeMinutes { get; set; } = 0;
    
    [FirestoreProperty("longestStreak")]
    public int LongestStreak { get; set; } = 0;
    
    [FirestoreProperty("badgesEarned")]
    public int BadgesEarned { get; set; } = 0;
    
    [FirestoreProperty("weeklyProgress")]
    public WeeklyProgress CurrentWeek { get; set; } = new();
    
    [FirestoreProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Weekly progress tracking for goals
/// </summary>
[FirestoreData]
public class WeeklyProgress
{
    [FirestoreProperty("weekStartDate")]
    public DateTime WeekStartDate { get; set; }
    
    [FirestoreProperty("lessonsCompleted")]
    public int LessonsCompleted { get; set; } = 0;
    
    [FirestoreProperty("xpEarned")]
    public int XPEarned { get; set; } = 0;
    
    [FirestoreProperty("goal")]
    public int Goal { get; set; } = 5;
    
    [FirestoreProperty("dailyActivity")]
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
[FirestoreData]
public class LanguageOption
{
    [FirestoreProperty("code")]
    public string Code { get; set; } = string.Empty;
    
    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [FirestoreProperty("nativeName")]
    public string NativeName { get; set; } = string.Empty;
    
    [FirestoreProperty("flagEmoji")]
    public string FlagEmoji { get; set; } = string.Empty;
    
    [FirestoreProperty("isAvailable")]
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
        new() { Code = "en", Name = "English", NativeName = "English", FlagEmoji = "🇺🇸", IsAvailable = true },
        new() { Code = "es", Name = "Spanish", NativeName = "Español", FlagEmoji = "🇪🇸", IsAvailable = true },
        new() { Code = "fr", Name = "French", NativeName = "Français", FlagEmoji = "🇫🇷", IsAvailable = true },
        new() { Code = "de", Name = "German", NativeName = "Deutsch", FlagEmoji = "🇩🇪", IsAvailable = true },
        new() { Code = "it", Name = "Italian", NativeName = "Italiano", FlagEmoji = "🇮🇹", IsAvailable = true },
        new() { Code = "pt", Name = "Portuguese", NativeName = "Português", FlagEmoji = "🇵🇹", IsAvailable = true },
        new() { Code = "ja", Name = "Japanese", NativeName = "日本語", FlagEmoji = "🇯🇵", IsAvailable = true },
        new() { Code = "ko", Name = "Korean", NativeName = "한국어", FlagEmoji = "🇰🇷", IsAvailable = true },
        new() { Code = "zh", Name = "Chinese", NativeName = "中文", FlagEmoji = "🇨🇳", IsAvailable = true },
        new() { Code = "ar", Name = "Arabic", NativeName = "العربية", FlagEmoji = "🇸🇦", IsAvailable = true }
    };
    
    public static LanguageOption? GetByCode(string code) 
        => All.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
}