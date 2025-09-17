using Google.Cloud.Firestore;

namespace LinguaLearn.Mobile.Models;

/// <summary>
/// Activity item for displaying recent user activities on homepage
/// </summary>
[FirestoreData]
public class ActivityItem
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty("type")]
    public ActivityType Type { get; set; }
    
    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [FirestoreProperty("icon")]
    public string Icon { get; set; } = string.Empty;
    
    [FirestoreProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of activities that can be tracked
/// </summary>
public enum ActivityType
{
    LessonCompleted,
    QuizCompleted,
    BadgeEarned,
    StreakMilestone,
    LevelUp,
    WeeklyGoalReached,
    PronunciationPractice,
    VocabularyReview
}

/// <summary>
/// Leaderboard entry for homepage preview
/// </summary>
[FirestoreData]
public class LeaderboardEntry
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [FirestoreProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [FirestoreProperty("rank")]
    public int Rank { get; set; }
    
    [FirestoreProperty("xp")]
    public int XP { get; set; }
    
    [FirestoreProperty("weeklyXP")]
    public int WeeklyXP { get; set; }
    
    [FirestoreProperty("streakCount")]
    public int StreakCount { get; set; }
    
    [FirestoreProperty("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Helper class for activity creation
/// </summary>
public static class ActivityHelper
{
    public static ActivityItem CreateLessonCompletedActivity(string userId, string lessonTitle, int xpEarned)
    {
        return new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.LessonCompleted,
            Title = "Lesson Completed",
            Description = $"Completed '{lessonTitle}' and earned {xpEarned} XP",
            Icon = "📚",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["lessonTitle"] = lessonTitle,
                ["xpEarned"] = xpEarned
            }
        };
    }
    
    public static ActivityItem CreateBadgeEarnedActivity(string userId, string badgeTitle)
    {
        return new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.BadgeEarned,
            Title = "Badge Earned",
            Description = $"Earned the '{badgeTitle}' badge",
            Icon = "🏆",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["badgeTitle"] = badgeTitle
            }
        };
    }
    
    public static ActivityItem CreateStreakMilestoneActivity(string userId, int streakCount)
    {
        return new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.StreakMilestone,
            Title = "Streak Milestone",
            Description = $"Reached a {streakCount}-day streak!",
            Icon = "🔥",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["streakCount"] = streakCount
            }
        };
    }
    
    public static ActivityItem CreateLevelUpActivity(string userId, int newLevel)
    {
        return new ActivityItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Type = ActivityType.LevelUp,
            Title = "Level Up!",
            Description = $"Reached level {newLevel}",
            Icon = "⭐",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["newLevel"] = newLevel
            }
        };
    }
}